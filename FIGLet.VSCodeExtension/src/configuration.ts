// src/configuration.ts
import * as vscode from 'vscode';
import * as path from 'path';

export interface FigletConfig {
    fontDirectory: string;
    defaultFont: string;
    layoutMode: 'full' | 'kerning' | 'smush';
    defaultWidth?: number;
}

export class ConfigurationManager {
    private static readonly SECTION = 'figlet';

    static getConfiguration(): FigletConfig {
        const config = vscode.workspace.getConfiguration(this.SECTION);
        
        return {
            layoutMode: config.get<'full' | 'kerning' | 'smush'>('layoutMode') || 'smush',
            defaultFont: config.get<string>('defaultFont') || 'small',
            defaultWidth: config.get<number>('defaultWidth') || 80,
            fontDirectory: config.get<string>('fontDirectory') || path.join(this.getExtensionPath(), 'resources', 'fonts'),
        };
    }

    static async updateConfiguration(settings: Partial<FigletConfig>): Promise<void> {
        const config = vscode.workspace.getConfiguration(this.SECTION);
        
        for (const [key, value] of Object.entries(settings)) {
            await config.update(key, value, vscode.ConfigurationTarget.Global);
        }
    }

    private static getExtensionPath(): string {
        const extension = vscode.extensions.getExtension('paulo-santos.figlet');
        return extension ? extension.extensionPath : '';
    }
}
