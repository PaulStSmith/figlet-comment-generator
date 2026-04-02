import * as vscode from 'vscode';

/**
 * A read-only webview panel shown on first install (and on demand via the
 * "FIGLet: Getting Started" command) that explains how to use the extension.
 */
export class WelcomePanel {
    private static _instance: WelcomePanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private _disposables: vscode.Disposable[] = [];

    /** Show the panel, or reveal it if it is already open. */
    public static createOrShow(): void {
        if (WelcomePanel._instance) {
            WelcomePanel._instance._panel.reveal(vscode.ViewColumn.Active);
            return;
        }
        const panel = vscode.window.createWebviewPanel(
            'figletWelcome',
            'Getting Started — FIGLet Comments',
            vscode.ViewColumn.Active,
            { enableScripts: false }   // purely static — no script surface area
        );
        WelcomePanel._instance = new WelcomePanel(panel);
    }

    private constructor(panel: vscode.WebviewPanel) {
        this._panel = panel;
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        this._panel.webview.html = WelcomePanel._buildHtml();
    }

    public dispose(): void {
        WelcomePanel._instance = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            this._disposables.pop()?.dispose();
        }
    }

    // -----------------------------------------------------------------------
    // HTML
    // -----------------------------------------------------------------------

    private static _buildHtml(): string {
        return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline';">
  <title>Getting Started — FIGLet Comment Generator</title>
  <style>
    *, *::before, *::after { box-sizing: border-box; }

    body {
      font-family: var(--vscode-font-family);
      font-size: var(--vscode-font-size);
      color: var(--vscode-foreground);
      background: var(--vscode-editor-background);
      margin: 0;
      padding: 32px 48px 48px;
      line-height: 1.6;
    }

    .page { max-width: 760px; margin: 0 auto; }

    /* ── hero ────────────────────────────────────────────────── */
    .hero {
      border-bottom: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
      padding-bottom: 24px;
      margin-bottom: 32px;
    }
    .hero pre {
      font-family: var(--vscode-editor-font-family, monospace);
      font-size: 13px;
      color: var(--vscode-textLink-foreground);
      margin: 0 0 12px;
      line-height: 1.3;
    }
    .hero p {
      margin: 0;
      font-size: calc(var(--vscode-font-size) + 1px);
      color: var(--vscode-descriptionForeground, var(--vscode-foreground));
    }

    /* ── sections ────────────────────────────────────────────── */
    h2 {
      font-size: calc(var(--vscode-font-size) + 2px);
      font-weight: 600;
      margin: 32px 0 12px;
      padding-bottom: 4px;
      border-bottom: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
    }
    p  { margin: 0 0 10px; }
    ul { margin: 0 0 10px; padding-left: 24px; }
    li { margin-bottom: 6px; }

    /* ── keyboard shortcuts ──────────────────────────────────── */
    .shortcuts { border-collapse: collapse; width: 100%; margin-bottom: 10px; }
    .shortcuts th, .shortcuts td {
      text-align: left;
      padding: 5px 12px;
      border-bottom: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
      font-size: var(--vscode-font-size);
    }
    .shortcuts th {
      font-weight: 600;
      background: var(--vscode-editor-lineHighlightBackground,
                      var(--vscode-editorWidget-background));
    }

    /* ── inline code / kbd ───────────────────────────────────── */
    kbd, code {
      font-family: var(--vscode-editor-font-family, monospace);
      font-size: 0.9em;
      background: var(--vscode-textBlockQuote-background,
                      var(--vscode-editorWidget-background));
      border: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
      border-radius: 3px;
      padding: 1px 5px;
    }

    /* ── sample banner ───────────────────────────────────────── */
    .sample {
      background: var(--vscode-editor-background);
      border: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
      border-radius: 3px;
      padding: 12px 16px;
      overflow-x: auto;
      margin: 0 0 10px;
    }
    .sample pre {
      margin: 0;
      font-family: var(--vscode-editor-font-family, monospace);
      font-size: var(--vscode-editor-font-size, 12px);
      color: var(--vscode-editor-foreground);
      line-height: 1.3;
    }

    /* ── tip box ─────────────────────────────────────────────── */
    .tip {
      background: var(--vscode-textBlockQuote-background,
                      var(--vscode-editorWidget-background));
      border-left: 3px solid var(--vscode-textLink-foreground);
      padding: 8px 14px;
      border-radius: 0 3px 3px 0;
      margin: 0 0 10px;
    }
    .tip p { margin: 0; }

    /* ── footer ──────────────────────────────────────────────── */
    .footer {
      margin-top: 40px;
      padding-top: 16px;
      border-top: 1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border));
      font-size: calc(var(--vscode-font-size) - 1px);
      color: var(--vscode-descriptionForeground, var(--vscode-foreground));
    }
  </style>
</head>
<body>
<div class="page">

  <!-- ── Hero ──────────────────────────────────────────────────────── -->
  <div class="hero">
    <pre
> _____ ___ ____ _        _    ___                                   _
|  ___|_ _/ ___| |      | |  / __| ___  _ __ ___  _ __ ___   ___ _ __ | |_
| |_   | | |  _| |   ___| |_| |   / _ \\| '_ \` _ \\| '_ \` _ \\ / _ \\ '_ \\| __|
|  _|  | | |_| | |__|___| |_| |__| (_) | | | | | | | | | | |  __/ | | | |_
|_|   |___|\\____|_____|   \\__|\\____\\___/|_| |_| |_|_| |_| |_|\\___|_| |_|\\__|
 ____                           _
/ ___| ___ _ __   ___ _ __ __ _| |_ ___  _ __
| |  _/ _ \\ '_ \\ / _ \\ '__/ _\` | __/ _ \\| '__|
| |_| |  __/ | | |  __/ | | (_| | || (_) | |
 \\____|\___|_| |_|\\___|_|  \\__,_|\\__\\___/|_|</pre>
    <p>Turn any text into eye-catching ASCII-art comment banners — right inside VS Code.</p>
  </div>

  <!-- ── Quick Start ───────────────────────────────────────────────── -->
  <h2>Quick Start</h2>
  <ol>
    <li>Open any source file in the editor.</li>
    <li>Place the cursor where you want the banner inserted.</li>
    <li>Press <kbd>Ctrl+Alt+B</kbd> (<kbd>⌘⌥B</kbd> on macOS) — or right-click and choose
        <strong>FIGlet Comments → Generate FIGlet Banner</strong>.</li>
    <li>Type your banner text, choose a font and layout, then click <strong>OK</strong>.</li>
    <li>The banner is inserted as a comment in the correct format for your language.</li>
  </ol>

  <!-- ── Sample output ─────────────────────────────────────────────── -->
  <h2>Example Output</h2>
  <p>For a C# file, typing <em>Hello</em> with the <em>small</em> font produces:</p>
  <div class="sample"><pre>/*
 *  _   _      _ _
 * | | | | ___| | | ___
 * | |_| |/ _ \\ | |/ _ \\
 * |  _  |  __/ | | (_) |
 * |_| |_|\\___|_|_|\\___/
 */</pre></div>
  <p>The comment style is detected automatically from the active file's language
     (C-style block for C#/TypeScript/Java, <code>#</code> for Python, <code>--</code> for SQL, and so on).</p>

  <!-- ── Ways to open ──────────────────────────────────────────────── -->
  <h2>Three Ways to Open the Panel</h2>
  <table class="shortcuts">
    <thead>
      <tr><th>Method</th><th>How</th></tr>
    </thead>
    <tbody>
      <tr>
        <td>Keyboard shortcut</td>
        <td><kbd>Ctrl+Alt+B</kbd> &nbsp;/&nbsp; <kbd>⌘⌥B</kbd></td>
      </tr>
      <tr>
        <td>Right-click menu</td>
        <td>Right-click in the editor → <strong>FIGlet Comments → Generate FIGlet Banner</strong></td>
      </tr>
      <tr>
        <td>Editor title bar</td>
        <td>Click the <code>$(symbol-string)</code> icon in the top-right of the editor</td>
      </tr>
      <tr>
        <td>Command Palette</td>
        <td><kbd>Ctrl+Shift+P</kbd> → <em>FIGlet Comments: Generate FIGlet Banner</em></td>
      </tr>
    </tbody>
  </table>

  <!-- ── Configuration ─────────────────────────────────────────────── -->
  <h2>Configuration</h2>
  <p>Open the settings panel via <strong>right-click → FIGlet Comments → FIGlet Settings</strong>
     or <kbd>Ctrl+Shift+P</kbd> → <em>FIGlet Comments: FIGlet Settings</em>.</p>
  <ul>
    <li><strong>Font Directory</strong> — point to a folder of <code>.flf</code> font files to
        unlock hundreds of additional fonts. The built-in <em>small</em> font is always available.</li>
    <li><strong>Default Font</strong> — the font pre-selected each time the panel opens.</li>
    <li><strong>Layout Mode</strong> — controls how characters sit next to each other:
      <ul>
        <li><em>Smushing</em> (default) — characters merge at their edges for the tightest look.</li>
        <li><em>Kerning</em> — characters touch but do not merge.</li>
        <li><em>Full Size</em> — characters keep their full bounding box with no overlap.</li>
      </ul>
    </li>
  </ul>

  <div class="tip">
    <p>💡 You can also edit these settings directly in <strong>File → Preferences → Settings</strong>
       and search for <em>FIGlet</em>.</p>
  </div>

  <!-- ── Font files ────────────────────────────────────────────────── -->
  <h2>Getting More Fonts</h2>
  <p>The FIGLet project hosts hundreds of free <code>.flf</code> font files.
     Download the collection, extract it anywhere on disk, then set that folder as your
     <strong>Font Directory</strong> in settings.</p>

  <!-- ── Reopen ────────────────────────────────────────────────────── -->
  <h2>Reopen This Page</h2>
  <p>Run <kbd>Ctrl+Shift+P</kbd> → <em>FIGlet Comments: Getting Started</em> at any time
     to bring this page back.</p>

  <!-- ── Footer ────────────────────────────────────────────────────── -->
  <div class="footer">
    <p>FIGLet Comment Generator &nbsp;·&nbsp; © Paul St. Smith &nbsp;·&nbsp;
    Issues and feedback welcome on the VS Code Marketplace page.</p>
  </div>

</div>
</body>
</html>`;
    }
}
