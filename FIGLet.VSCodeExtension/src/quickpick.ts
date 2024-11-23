import * as vscode from 'vscode';
import { FIGFontInfo } from './FIGFontInfo.js';
import { FIGLetRenderer } from './FIGLet/FIGLetRenderer.js';
import { LayoutMode } from './FIGLet/LayoutMode.js';

interface FontQuickPickItem extends vscode.QuickPickItem {
    fontName: string;
    fontInfo: FIGFontInfo;
    filePath?: string;
    height?: number;
    maxLength?: number;
}

// This simpler version is no longer needed since we're using the dynamic preview version


// Optional: Create a more sophisticated QuickPick with dynamic preview
export async function showFontQuickPickWithDynamicPreview(
    fonts: FIGFontInfo[], 
    sampleText: string = 'Hi'
): Promise<FIGFontInfo | undefined> {
    const quickPick = vscode.window.createQuickPick<FontQuickPickItem>();
    quickPick.placeholder = 'Select a font (type to search, use arrow keys to preview)';
    quickPick.matchOnDescription = true;
    quickPick.matchOnDetail = true;

    // Initial items without preview
    quickPick.items = fonts.map(fontInfo => ({
        label: `$(symbol-string) ${fontInfo.name}`,
        fontName: fontInfo.name,
        fontInfo: fontInfo,
        description: `Height: ${fontInfo.height}, Max Length: ${fontInfo.maxLength}`,
        // Default style for entries before preview is generated
        detail: `$(loading~spin) Loading preview...`,
    }));

    // Make sure we use monospace font for the previews
    quickPick.onDidChangeActive(async (active) => {
        if (active[0]) {
            try {
                const font = active[0].fontInfo.font;
                const renderer = new FIGLetRenderer(font);
                const preview = renderer.render(sampleText, LayoutMode.Smushing);
                
                const items = [...quickPick.items];
                const index = items.findIndex(item => item.fontName === active[0].fontName);
                if (index !== -1) {
                    items[index] = {
                        ...items[index],
                        // Use monospace font and preserve whitespace
                        detail: `\`\`\`\n${preview}\n\`\`\``,
                    };
                    quickPick.items = items;
                }
            } catch (error) {
                console.error('Error generating preview:', error);
                // Show error in preview area
                const items = [...quickPick.items];
                const index = items.findIndex(item => item.fontName === active[0].fontName);
                if (index !== -1) {
                    items[index] = {
                        ...items[index],
                        detail: '$(error) Failed to generate preview'
                    };
                    quickPick.items = items;
                }
            }
        }
    });

    return new Promise<FIGFontInfo | undefined>((resolve) => {
        quickPick.onDidChangeSelection(selection => {
            const selected = selection[0];
            if (selected) {
                resolve(selected.fontInfo);
            }
            quickPick.dispose();
        });

        quickPick.onDidHide(() => {
            quickPick.dispose();
            resolve(undefined);
        });

        quickPick.show();
    });
}