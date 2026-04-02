import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import { FIGLetFontManager } from './FIGLetFontManager.js';
import { FIGFont } from './FIGLet/FIGFont.js';
import { FIGLetRenderer } from './FIGLet/FIGLetRenderer.js';
import { LayoutMode } from './FIGLet/LayoutMode.js';
import { BannerUtils } from './BannerUtils.js';

type LayoutKey = 'full' | 'kerning' | 'smush';

interface ReadyMessage        { type: 'ready'; }
interface OkMessage          { type: 'ok';           text: string; font: string; layoutMode: LayoutKey; language: string; }
interface CancelMessage      { type: 'cancel'; }
interface RequestFontMessage { type: 'requestFont';  name: string; }
interface OpenExternalMessage{ type: 'openExternal'; url: string;  }
type FromWebview = ReadyMessage | OkMessage | CancelMessage | RequestFontMessage | OpenExternalMessage;

export class FigletPanel {
    private static _instance: FigletPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];
    private _editor: vscode.TextEditor;
    private _context!: vscode.ExtensionContext;

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

    private async _init(context: vscode.ExtensionContext): Promise<void> {
        this._context = context;
        this._panel.webview.html = this._getHtml();
        // Font data is sent only after the webview signals 'ready' (see _handleMessage).
        // This avoids a race condition where postMessage fires before the webview's
        // window.addEventListener('message') is registered.
    }

    private async _sendInitData(): Promise<void> {
        const context = this._context;
        const config = vscode.workspace.getConfiguration('figlet');
        const fontDir       = config.get<string>('fontDirectory') || null;
        const defaultFont   = config.get<string>('defaultFont')   || 'small';
        const VALID_LAYOUT_KEYS = new Set<string>(['full', 'kerning', 'smush']);
        const rawLayout    = config.get<string>('layoutMode') ?? 'smush';
        const defaultLayout = (VALID_LAYOUT_KEYS.has(rawLayout) ? rawLayout : 'smush') as LayoutKey;
        const language      = this._editor.document.languageId;

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

    private async _insertBanner(msg: OkMessage): Promise<void> {
        const editor = vscode.window.activeTextEditor || this._editor;
        const fontInfo = FIGLetFontManager.availableFonts.find(f => f.name === msg.font);
        const font: FIGFont = fontInfo?.font ?? await FIGFont.getDefault();

        const layoutMode =
            msg.layoutMode === 'full'    ? LayoutMode.FullSize :
            msg.layoutMode === 'kerning' ? LayoutMode.Kerning  :
                                           LayoutMode.Smushing;

        const figletText = new FIGLetRenderer(font).render(msg.text, layoutMode);
        await BannerUtils.insertBanner(editor, figletText, msg.language);
    }

    private _getHtml(): string {
        const webview   = this._panel.webview;
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'webview.js'));
        const nonce     = getNonce();
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

    public dispose(): void {
        FigletPanel._instance = undefined;
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
