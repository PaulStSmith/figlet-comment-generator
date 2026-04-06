import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import { FIGLetFontManager } from './FIGLetFontManager.js';

/** Identifies the three supported FIGLet layout modes by name. */
type LayoutKey = 'full' | 'kerning' | 'smush';

/** Message sent by the webview once it has registered its message listener and is ready to receive initialisation data. */
interface ReadyMessage { type: 'ready'; }
/** Message sent by the webview to request that the extension open a folder-picker dialog for the font directory. */
interface BrowseFontDirMsg { type: 'browseFontDir'; }
/** Message sent by the webview when the user clicks Save, carrying the new settings values. */
interface SaveSettingsMessage { type: 'saveSettings'; settings: { fontDirectory: string; layoutMode: LayoutKey }; }
/** Message sent by the webview when the user dismisses the settings panel without saving. */
interface CloseMessage { type: 'close'; }
/** Union of all message types that can be sent from the settings webview to the extension host. */
type FromWebview = ReadyMessage | BrowseFontDirMsg | SaveSettingsMessage | CloseMessage;

/**
 * Manages the FIGLet Settings webview panel.
 *
 * A singleton panel (at most one open at a time) rendered in the active editor
 * column. It displays the current extension configuration (font directory,
 * default font, layout mode) alongside a table of available fonts and a live
 * preview, and persists changes to VS Code's global settings on save.
 */
export class FigletSettingsPanel {
    /** The single active instance; `undefined` when the panel is closed. */
    private static _instance: FigletSettingsPanel | undefined;
    /** The underlying VS Code webview panel. */
    private readonly _panel: vscode.WebviewPanel;
    /** URI of the extension's install directory, used to resolve bundled resources. */
    private readonly _extensionUri: vscode.Uri;
    /** Disposables accumulated during the panel's lifetime, cleaned up on dispose. */
    private _disposables: vscode.Disposable[] = [];
    /** Extension context stored at construction time; provides access to extension paths and settings. */
    private _context: vscode.ExtensionContext;

    /**
     * Opens the settings panel in the active editor column, or reveals it if
     * it is already open.
     *
     * @param context - The extension context used to resolve resource URIs and configuration.
     */
    public static async createOrShow(context: vscode.ExtensionContext): Promise<void> {
        if (FigletSettingsPanel._instance) {
            FigletSettingsPanel._instance._panel.reveal(vscode.ViewColumn.Active);
            return;
        }
        const panel = vscode.window.createWebviewPanel(
            'figletSettings',
            'FIGLet Settings',
            vscode.ViewColumn.Active,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(context.extensionUri, 'media')]
            }
        );
        FigletSettingsPanel._instance = new FigletSettingsPanel(panel, context);
        await FigletSettingsPanel._instance._init();
    }

    /**
     * Creates a new `FigletSettingsPanel` wrapping the given webview panel.
     *
     * Registers dispose and message-receive listeners immediately so that no
     * messages are dropped between construction and `_init`.
     *
     * @param panel   - The VS Code webview panel to manage.
     * @param context - The extension context, stored for later resource resolution.
     */
    private constructor(panel: vscode.WebviewPanel, context: vscode.ExtensionContext) {
        this._panel = panel;
        this._extensionUri = context.extensionUri;
        this._context = context;
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        this._panel.webview.onDidReceiveMessage(
            (msg: FromWebview) => this._handleMessage(msg),
            null,
            this._disposables
        );
    }

    /**
     * Sets the initial HTML content of the webview.
     *
     * Actual settings data is sent only after the webview signals `ready` to
     * avoid a race condition between `postMessage` and listener registration.
     */
    private async _init(): Promise<void> {
        this._panel.webview.html = this._getHtml();
    }

    /**
     * Reads the current workspace configuration and all available font files,
     * then posts an `init` message to the webview containing the settings object
     * and font array.
     */
    private async _sendInitData(): Promise<void> {
        const config = vscode.workspace.getConfiguration('figlet');
        const fontDirectory = config.get<string>('fontDirectory') || '';
        const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
        const rawLayout = config.get<string>('layoutMode') ?? 'smush';
        const layoutMode = (VALID_LAYOUT_KEYS.has(rawLayout) ? rawLayout : 'smush') as LayoutKey;

        await FIGLetFontManager.setFontDirectory(fontDirectory || null);

        const fonts = await this._loadFonts();

        await this._panel.webview.postMessage({
            type: 'init',
            settings: { fontDirectory, layoutMode },
            fonts,
        });
    }

    /**
     * Reads the built-in `small.flf` font and all user-configured fonts from
     * disk, returning an array of `{ name, content }` objects for the webview.
     *
     * Fonts that cannot be read are silently skipped.
     *
     * @returns A promise that resolves to an array of font name/content pairs.
     */
    private async _loadFonts(): Promise<Array<{ name: string; content: string }>> {
        const fonts: Array<{ name: string; content: string }> = [];

        // Built-in default font
        try {
            const p = path.join(this._context.extensionPath, 'resources', 'fonts', 'small.flf');
            fonts.push({ name: 'small', content: await fs.readFile(p, 'utf-8') });
        } catch (e) {
            console.error('[FigletSettingsPanel] Failed to read default font:', e);
        }

        // User-configured fonts
        for (const info of FIGLetFontManager.availableFonts) {
            if (info.name !== 'small' && info.filePath) {
                try {
                    fonts.push({ name: info.name, content: await fs.readFile(info.filePath, 'utf-8') });
                } catch { /* skip unreadable */ }
            }
        }

        return fonts;
    }

    /**
     * Routes incoming webview messages to the appropriate handler.
     *
     * - `ready`            – sends initialisation data to the webview.
     * - `browseFontDir`    – opens a folder-picker dialog and notifies the webview of the result.
     * - `saveSettings`     – validates and persists the settings, then disposes the panel.
     * - `close`            – disposes the panel without saving.
     *
     * @param msg - The message received from the settings webview.
     */
    private async _handleMessage(msg: FromWebview): Promise<void> {
        switch (msg.type) {
            case 'ready':
                await this._sendInitData();
                break;

            case 'browseFontDir': {
                const result = await vscode.window.showOpenDialog({
                    canSelectFiles: false,
                    canSelectFolders: true,
                    canSelectMany: false,
                    openLabel: 'Select Font Directory',
                    title: 'Select FIGLet Font Directory',
                });
                if (result && result.length > 0) {
                    const dir = result[0].fsPath;
                    await FIGLetFontManager.setFontDirectory(dir);
                    const fonts = await this._loadFonts();
                    await this._panel.webview.postMessage({
                        type: 'fontDirectoryUpdated',
                        directory: dir,
                        fonts,
                    });
                }
                break;
            }

            case 'saveSettings': {
                const { fontDirectory } = msg.settings;
                const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
                const layoutMode = (VALID_LAYOUT_KEYS.has(msg.settings.layoutMode)
                    ? msg.settings.layoutMode : 'smush') as LayoutKey;
                const config = vscode.workspace.getConfiguration('figlet');
                await config.update('fontDirectory', fontDirectory, vscode.ConfigurationTarget.Global);
                await config.update('layoutMode', layoutMode, vscode.ConfigurationTarget.Global);
                this.dispose();
                vscode.window.showInformationMessage('FIGLet settings saved.');
                break;
            }

            case 'close':
                this.dispose();
                break;
        }
    }

    /**
     * Builds and returns the HTML document that bootstraps the settings webview
     * React app.
     *
     * A per-instance nonce is embedded in the Content-Security-Policy and the
     * `<script>` tag so that only the bundled settings script is allowed to run.
     *
     * @returns The complete HTML string for the webview's `html` property.
     */
    private _getHtml(): string {
        const webview = this._panel.webview;
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'settings-webview.js'));
        const nonce = getNonce();
        return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none'; script-src 'nonce-${nonce}'; style-src ${webview.cspSource} 'unsafe-inline';">
  <title>FIGLet Settings</title>
  <style>
    *, *::before, *::after { box-sizing: border-box; }
    body {
      padding: 0; margin: 0; overflow: hidden;
      background: var(--vscode-editorWidget-background, var(--vscode-editor-background));
      color: var(--vscode-foreground);
      font-family: var(--vscode-font-family);
      font-size: var(--vscode-font-size);
    }
  </style>
</head>
<body>
  <div id="root"></div>
  <script nonce="${nonce}" src="${scriptUri}"></script>
</body>
</html>`;
    }

    /**
     * Cleans up the singleton reference, disposes the underlying webview panel,
     * and runs all accumulated disposables.
     */
    public dispose(): void {
        FigletSettingsPanel._instance = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            this._disposables.pop()?.dispose();
        }
    }
}

/**
 * Generates a cryptographically-random 32-character alphanumeric nonce string
 * suitable for use in Content-Security-Policy `nonce-` directives.
 *
 * @returns A 32-character nonce string composed of `[A-Za-z0-9]` characters.
 */
function getNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    return Array.from({ length: 32 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
}
