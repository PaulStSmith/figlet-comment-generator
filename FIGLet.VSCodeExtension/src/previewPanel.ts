import * as vscode from 'vscode';
import { ConfigurationManager } from './configuration';

// src/previewPanel.ts
export class FigletPreviewPanel {
    public static currentPanel: FigletPreviewPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private _disposables: vscode.Disposable[] = [];

    private constructor(panel: vscode.WebviewPanel) {
        this._panel = panel;
        this._update();
        
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        
        // Handle messages from the WebView
        this._panel.webview.onDidReceiveMessage(
            async (message) => {
                switch (message.command) {
                    case 'updateFont':
                        await ConfigurationManager.updateConfiguration({ defaultFont: message.font });
                        break;
                    case 'updateLayout':
                        await ConfigurationManager.updateConfiguration({ layoutMode: message.layout });
                        break;
                    case 'insertBanner':
                        vscode.commands.executeCommand('figlet.insertBanner', message.text);
                        break;
                }
            },
            null,
            this._disposables
        );
    }

    public static createOrShow() {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (FigletPreviewPanel.currentPanel) {
            FigletPreviewPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'figletPreview',
            'FIGlet Preview',
            column || vscode.ViewColumn.One,
            {
                enableScripts: true,
                retainContextWhenHidden: true
            }
        );

        FigletPreviewPanel.currentPanel = new FigletPreviewPanel(panel);
    }

    private _update() {
        this._panel.webview.html = this._getHtmlContent();
    }

    private _getHtmlContent(): string {
        const config = ConfigurationManager.getConfiguration();
        
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>FIGlet Preview</title>
    <style>
        body { padding: 10px; }
        .preview { 
            font-family: monospace;
            white-space: pre;
            background: var(--vscode-editor-background);
            padding: 10px;
            margin: 10px 0;
        }
        .controls {
            display: grid;
            gap: 10px;
            margin-bottom: 20px;
        }
        select, input, button {
            padding: 5px;
            width: 100%;
        }
    </style>
</head>
<body>
    <div class="controls">
        <input type="text" id="bannerText" placeholder="Enter banner text...">
        <select id="fontSelect">
            <option value="standard">Standard</option>
            <option value="slant">Slant</option>
            <!-- More fonts will be dynamically loaded -->
        </select>
        <select id="layoutMode">
            <option value="full">Full</option>
            <option value="kerning">Kerning</option>
            <option value="smush">Smushing</option>
        </select>
        <button id="insertButton">Insert Banner</button>
    </div>
    <div class="preview" id="preview"></div>
    <script>
        // Initialize with current configuration
        const vscode = acquireVsCodeApi();
        const config = ${JSON.stringify(config)};
        
        // Update preview as user types/changes settings
        document.getElementById('bannerText').addEventListener('input', updatePreview);
        document.getElementById('fontSelect').addEventListener('change', updateFont);
        document.getElementById('layoutMode').addEventListener('change', updateLayout);
        document.getElementById('insertButton').addEventListener('click', insertBanner);
        
        function updatePreview() {
            // In real implementation, this would call FIGlet to generate preview
            const text = document.getElementById('bannerText').value;
            // For now, just show plain text
            document.getElementById('preview').textContent = text;
        }
        
        function updateFont() {
            const font = document.getElementById('fontSelect').value;
            vscode.postMessage({ command: 'updateFont', font });
            updatePreview();
        }
        
        function updateLayout() {
            const layout = document.getElementById('layoutMode').value;
            vscode.postMessage({ command: 'updateLayout', layout });
            updatePreview();
        }
        
        function insertBanner() {
            const text = document.getElementById('bannerText').value;
            vscode.postMessage({ command: 'insertBanner', text });
        }
    </script>
</body>
</html>`;
    }

    public dispose() {
        FigletPreviewPanel.currentPanel = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            const disposable = this._disposables.pop();
            if (disposable) {
                disposable.dispose();
            }
        }
    }
}