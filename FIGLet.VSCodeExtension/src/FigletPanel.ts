import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import { FIGLetFontManager } from './FIGLetFontManager.js';
import { FIGFont } from './FIGLet/FIGFont.js';
import { FIGLetRenderer } from './FIGLet/FIGLetRenderer.js';
import { LayoutMode } from './FIGLet/LayoutMode.js';
import { BannerUtils } from './BannerUtils.js';

/** Identifies the three supported FIGLet layout modes by name. */
type LayoutKey = 'full' | 'kerning' | 'smush';

/** Message sent by the webview once its `window.addEventListener('message')` is registered and it is ready to receive data. */
interface ReadyMessage { type: 'ready'; }
/** Message sent by the webview when the user confirms the banner; carries the text, font, layout, and target language. */
interface OkMessage { type: 'ok'; text: string; font: string; layoutMode: LayoutKey; language: string; }
/** Message sent by the webview when the user dismisses the panel without inserting a banner. */
interface CancelMessage { type: 'cancel'; }
/** Message sent by the webview to request the raw `.flf` content for a specific font by name. */
interface RequestFontMessage { type: 'requestFont'; name: string; }
/** Message sent by the webview to open a URL in the system browser; only allow-listed hosts are acted upon. */
interface OpenExternalMessage { type: 'openExternal'; url: string; }
/** Union of all message types that can be sent from the webview to the extension host. */
type FromWebview = ReadyMessage | OkMessage | CancelMessage | RequestFontMessage | OpenExternalMessage;

/*
 *   ___ _      _     _   ___               _ 
 *  | __(_)__ _| |___| |_| _ \__ _ _ _  ___| |
 *  | _|| / _` | / -_)  _|  _/ _` | ' \/ -_) |
 *  |_| |_\__, |_\___|\__|_| \__,_|_||_\___|_|
 *        |___/                               
 */
/**
 * Manages the FIGLet Comment Generator webview panel.
 *
 * A singleton panel (at most one open at a time) rendered beside the active
 * editor. It loads available FIGLet fonts, lets the user compose and preview
 * a banner, and then inserts the resulting ASCII-art comment block into the
 * document at the current cursor position.
 */
export class FigletPanel {
    /** The single active instance; `undefined` when the panel is closed. */
    private static _instance: FigletPanel | undefined;
    /** The underlying VS Code webview panel. */
    private readonly _panel: vscode.WebviewPanel;
    /** URI of the extension's install directory, used to resolve bundled resources. */
    private readonly _extensionUri: vscode.Uri;
    /** Disposables accumulated during the panel's lifetime, cleaned up on dispose. */
    private _disposables: vscode.Disposable[] = [];
    /** The text editor that was active when the panel was opened (or last refreshed). */
    private _editor: vscode.TextEditor;
    /** Extension context stored after `_init` runs; provides access to extension paths and workspace state. */
    private _context!: vscode.ExtensionContext;

    /**
     * Opens the panel beside the active editor, or reveals it if it is already open.
     *
     * If a panel already exists the editor reference is updated and the panel is
     * brought to the foreground; otherwise a new panel is created and initialised.
     *
     * @param context - The extension context used to resolve resource URIs and configuration.
     * @param editor  - The text editor into which the banner will eventually be inserted.
     */
    public static async createOrShow(context: vscode.ExtensionContext, editor: vscode.TextEditor): Promise<void> {
        if (FigletPanel._instance) {
            FigletPanel._instance._editor = editor;
            FigletPanel._instance._panel.reveal(vscode.ViewColumn.Beside);
            return;
        }
        const panel = vscode.window.createWebviewPanel(
            'figletBanner',
            'FIGLet Comment Generator',
            vscode.ViewColumn.Beside,
            {
                enableScripts: true,
                localResourceRoots: [vscode.Uri.joinPath(context.extensionUri, 'media')]
            }
        );
        FigletPanel._instance = new FigletPanel(panel, context.extensionUri, editor);
        await FigletPanel._instance._init(context);
    }

    /**
     * Creates a new `FigletPanel` wrapping the given webview panel.
     *
     * Registers the dispose and message-receive listeners immediately so that
     * no messages are lost between construction and `_init`.
     *
     * @param panel        - The VS Code webview panel to manage.
     * @param extensionUri - URI of the extension root, used to build resource URIs.
     * @param editor       - The active text editor at the time the panel is created.
     */
    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri, editor: vscode.TextEditor) {
        this._panel = panel;
        this._extensionUri = extensionUri;
        this._editor = editor;
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        this._panel.webview.onDidReceiveMessage(
            (msg: FromWebview) => this._handleMessage(msg),
            null,
            this._disposables
        );
    }

    /**
     * Stores the extension context and sets the initial HTML content of the webview.
     *
     * Font data is deliberately not sent here; it is sent only after the webview
     * signals `ready` to avoid a race condition where `postMessage` fires before
     * the webview's `window.addEventListener('message')` is registered.
     *
     * @param context - The extension context, persisted for later use in `_sendInitData`.
     */
    private async _init(context: vscode.ExtensionContext): Promise<void> {
        this._context = context;
        this._panel.webview.html = this._getHtml();
        // Font data is sent only after the webview signals 'ready' (see _handleMessage).
        // This avoids a race condition where postMessage fires before the webview's
        // window.addEventListener('message') is registered.
    }

    /**
     * Reads extension configuration and all available font files, then posts an
     * `init` message to the webview with the font content, default font name,
     * default layout mode, and the active document's language identifier.
     */
    private async _sendInitData(): Promise<void> {
        const context = this._context;
        const config = vscode.workspace.getConfiguration('figlet');
        const fontDir = config.get<string>('fontDirectory') || null;
        const defaultFont = config.get<string>('defaultFont') || 'small';
        const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
        const rawLayout = config.get<string>('layoutMode') ?? 'smush';
        const defaultLayout = (VALID_LAYOUT_KEYS.has(rawLayout) ? rawLayout : 'smush') as LayoutKey;
        const language = this._editor.document.languageId;

        await FIGLetFontManager.setFontDirectory(fontDir);

        const fonts: Array<{ name: string; content: string }> = [];

        // Built-in default font
        try {
            const p = path.join(context.extensionPath, 'resources', 'fonts', 'small.flf');
            fonts.push({ name: 'small', content: await fs.readFile(p, 'utf-8') });
        } catch (e) {
            console.error('[FigletPanel] Failed to read default font:', e);
        }

        // User-configured fonts
        for (const info of FIGLetFontManager.availableFonts) {
            if (info.name !== 'small' && info.filePath) {
                try {
                    fonts.push({ name: info.name, content: await fs.readFile(info.filePath, 'utf-8') });
                } catch { /* skip unreadable fonts */ }
            }
        }

        await this._panel.webview.postMessage({ type: 'init', fonts, defaultFont, defaultLayout, language });
    }

    /**
     * Routes incoming webview messages to the appropriate handler.
     *
     * - `ready`        – sends initialisation data to the webview.
     * - `ok`           – inserts the banner and disposes the panel.
     * - `cancel`       – disposes the panel without inserting anything.
     * - `openExternal` – opens a URL in the system browser (allow-listed hosts only).
     * - `requestFont`  – returns the raw `.flf` content for a named font.
     *
     * @param msg - The message received from the webview.
     */
    private async _handleMessage(msg: FromWebview): Promise<void> {
        switch (msg.type) {
            case 'ready':
                await this._sendInitData();
                break;
            case 'ok':
                await this._insertBanner(msg);
                this.dispose();
                break;
            case 'cancel':
                this.dispose();
                break;
            case 'openExternal': {
                const ALLOWED_EXTERNAL_HOSTS = new Set([
                    'marketplace.visualstudio.com',
                    'code.visualstudio.com',
                ]);
                const uri = vscode.Uri.parse(msg.url);
                if (uri.scheme === 'https' && ALLOWED_EXTERNAL_HOSTS.has(uri.authority)) {
                    await vscode.env.openExternal(uri);
                }
                break;
            }
            case 'requestFont': {
                // Fonts are all sent eagerly in _init; handle late requests just in case
                const info = FIGLetFontManager.availableFonts.find(f => f.name === msg.name);
                if (info?.filePath) {
                    try {
                        const content = await fs.readFile(info.filePath, 'utf-8');
                        await this._panel.webview.postMessage({ type: 'fontLoaded', name: msg.name, content });
                    } catch { /* skip */ }
                }
                break;
            }
        }
    }

    /**
     * Renders the FIGLet banner from the `ok` message payload and inserts it
     * into the active (or remembered) text editor wrapped in the appropriate
     * language comment syntax.
     *
     * @param msg - The `OkMessage` containing the text, font name, layout mode, and language id.
     */
    private async _insertBanner(msg: OkMessage): Promise<void> {
        const editor = vscode.window.activeTextEditor || this._editor;
        const fontInfo = FIGLetFontManager.availableFonts.find(f => f.name === msg.font);
        const font: FIGFont = fontInfo?.font ?? await FIGFont.getDefault();

        const layoutMode =
            msg.layoutMode === 'full' ? LayoutMode.FullSize :
                msg.layoutMode === 'kerning' ? LayoutMode.Kerning :
                    LayoutMode.Smushing;

        const figletText = new FIGLetRenderer(font, layoutMode).render(msg.text);
        await BannerUtils.insertBanner(editor, figletText, msg.language);
    }

    /**
     * Builds and returns the HTML document that bootstraps the webview React app.
     *
     * A per-instance nonce is embedded in the Content-Security-Policy and the
     * `<script>` tag so that only this specific inline/bundle script is allowed
     * to execute.
     *
     * @returns The complete HTML string for the webview's `html` property.
     */
    private _getHtml(): string {
        const webview = this._panel.webview;
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'webview.js'));
        const nonce = getNonce();
        return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none'; script-src 'nonce-${nonce}'; style-src ${webview.cspSource} 'unsafe-inline';">
  <title>FIGLet Comment Generator</title>
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
        FigletPanel._instance = undefined;
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
