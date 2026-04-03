// src/configuration.ts
import * as vscode from 'vscode';
import * as path from 'path';

/*
 *   ___ _      _        _    ___ ___        __ _      
 *  | __(_)__ _| |   ___| |_ / __/ _ \ _ _  / _(_)__ _ 
 *  | _|| / _` | |__/ -_)  _| (_| (_) | ' \|  _| / _` |
 *  |_| |_\__, |____\___|\__|\___\___/|_||_|_| |_\__, |
 *        |___/                                  |___/ 
 */
export interface FigletConfig {
    fontDirectory: string;
    defaultFont: string;
    layoutMode: 'full' | 'kerning' | 'smush';
    defaultWidth?: number;
}

/*
 *    ___           __ _                    _   _          __  __                             
 *   / __|___ _ _  / _(_)__ _ _  _ _ _ __ _| |_(_)___ _ _ |  \/  |__ _ _ _  __ _ __ _ ___ _ _ 
 *  | (__/ _ \ ' \|  _| / _` | || | '_/ _` |  _| / _ \ ' \| |\/| / _` | ' \/ _` / _` / -_) '_|
 *   \___\___/_||_|_| |_\__, |\_,_|_| \__,_|\__|_\___/_||_|_|  |_\__,_|_||_\__,_\__, \___|_|  
 *                      |___/                                                   |___/         
 */
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
