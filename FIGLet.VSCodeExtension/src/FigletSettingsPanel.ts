import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import { FIGLetFontManager } from './FIGLetFontManager.js';

type LayoutKey = 'full' | 'kerning' | 'smush';

interface ReadyMessage       { type: 'ready'; }
interface BrowseFontDirMsg   { type: 'browseFontDir'; }
interface SaveSettingsMessage { type: 'saveSettings'; settings: { fontDirectory: string; defaultFont: string; layoutMode: LayoutKey }; }
interface CloseMessage        { type: 'close'; }
type FromWebview = ReadyMessage | BrowseFontDirMsg | SaveSettingsMessage | CloseMessage;

export class FigletSettingsPanel {
    private static _instance: FigletSettingsPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];
    private _context: vscode.ExtensionContext;

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

    private constructor(panel: vscode.WebviewPanel, context: vscode.ExtensionContext) {
        this._panel    = panel;
        this._extensionUri = context.extensionUri;
        this._context  = context;
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        this._panel.webview.onDidReceiveMessage(
            (msg: FromWebview) => this._handleMessage(msg),
            null,
            this._disposables
        );
    }

    private async _init(): Promise<void> {
        this._panel.webview.html = this._getHtml();
    }

    private async _sendInitData(): Promise<void> {
        const config = vscode.workspace.getConfiguration('figlet');
        const fontDirectory = config.get<string>('fontDirectory') || '';
        const defaultFont   = config.get<string>('defaultFont')   || 'small';
        const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
        const rawLayout  = config.get<string>('layoutMode') ?? 'smush';
        const layoutMode = (VALID_LAYOUT_KEYS.has(rawLayout) ? rawLayout : 'smush') as LayoutKey;

        await FIGLetFontManager.setFontDirectory(fontDirectory || null);

        const fonts = await this._loadFonts();

        await this._panel.webview.postMessage({
            type: 'init',
            settings: { fontDirectory, defaultFont, layoutMode },
            fonts,
        });
    }

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
                const { fontDirectory, defaultFont } = msg.settings;
                const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
                const layoutMode = (VALID_LAYOUT_KEYS.has(msg.settings.layoutMode)
                    ? msg.settings.layoutMode : 'smush') as LayoutKey;
                const config = vscode.workspace.getConfiguration('figlet');
                await config.update('fontDirectory', fontDirectory, vscode.ConfigurationTarget.Global);
                await config.update('defaultFont',   defaultFont,   vscode.ConfigurationTarget.Global);
                await config.update('layoutMode',    layoutMode,    vscode.ConfigurationTarget.Global);
                this.dispose();
                vscode.window.showInformationMessage('FIGLet settings saved.');
                break;
            }

            case 'close':
                this.dispose();
                break;
        }
    }

    private _getHtml(): string {
        const webview   = this._panel.webview;
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'settings-webview.js'));
        const nonce     = getNonce();
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

    public dispose(): void {
        FigletSettingsPanel._instance = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            this._disposables.pop()?.dispose();
        }
    }
}

function getNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    return Array.from({ length: 32 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
}
