import * as vscode from 'vscode';
import { FigletPanel } from './FigletPanel.js';
import { FigletSettingsPanel } from './FigletSettingsPanel.js';
import { WelcomePanel } from './WelcomePanel.js';
import { findSymbolAtCursor, CLASS_LIKE_KINDS, METHOD_LIKE_KINDS } from './CodeElementDetector.js';
import { BannerUtils } from './BannerUtils.js';

/*
 *     _      _   _          _        _____     _               _          ___         _           _ __  
 *    /_\  __| |_(_)_ ____ _| |_ ___ / / __|_ _| |_ ___ _ _  __(_)___ _ _ / __|___ _ _| |_ _____ _| |\ \ 
 *   / _ \/ _|  _| \ V / _` |  _/ -_) || _|\ \ /  _/ -_) ' \(_-< / _ \ ' \ (__/ _ \ ' \  _/ -_) \ /  _| |
 *  /_/ \_\__|\__|_|\_/\__,_|\__\___| ||___/_\_\\__\___|_||_/__/_\___/_||_\___\___/_||_\__\___/_\_\\__| |
 *                                   \_\                                                             /_/ 
 */
export async function activate(context: vscode.ExtensionContext) {
    /*
     *   _                  _   ___                          ___                              _ 
     *  (_)_ _  ___ ___ _ _| |_| _ ) __ _ _ _  _ _  ___ _ _ / __|___ _ __  _ __  __ _ _ _  __| |
     *  | | ' \(_-</ -_) '_|  _| _ \/ _` | ' \| ' \/ -_) '_| (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  |_|_||_/__/\___|_|  \__|___/\__,_|_||_|_||_\___|_|  \___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *                                                                                          
     */
    let insertBannerCommand = vscode.commands.registerCommand('figlet.insertBanner', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active text editor found');
            return;
        }
        try {
            await FigletPanel.createOrShow(context, editor);
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';
            vscode.window.showErrorMessage(`Error opening FIGLet panel: ${errorMessage}`);
        }
    });

    /*
     *           _        _   ___        _   ___  _      ___                              _ 
     *   ___ ___| |___ __| |_| __|__ _ _| |_|   \(_)_ _ / __|___ _ __  _ __  __ _ _ _  __| |
     *  (_-</ -_) / -_) _|  _| _/ _ \ ' \  _| |) | | '_| (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  /__/\___|_\___\__|\__|_|\___/_||_\__|___/|_|_|  \___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *                                                                                      
     */
    // New font directory selection command
    let selectFontDirCommand = vscode.commands.registerCommand('figlet.selectFontDirectory', async () => {
        const options: vscode.OpenDialogOptions = {
            canSelectFiles: false,
            canSelectFolders: true,
            canSelectMany: false,
            openLabel: 'Select Font Directory',
            title: 'Select FIGlet Font Directory'
        };

        const folders = await vscode.window.showOpenDialog(options);
        
        if (folders && folders.length > 0) {
            const selectedDir = folders[0].fsPath;
            
            try {
                // Verify the directory exists and contains .flf files
                const files = await vscode.workspace.fs.readDirectory(folders[0]);
                const hasFontFiles = files.some(([name]) => name.endsWith('.flf'));
                
                if (!hasFontFiles) {
                    const proceed = await vscode.window.showWarningMessage(
                        'No .flf font files found in the selected directory. Do you want to use this directory anyway?',
                        'Yes', 'No'
                    );
                    
                    if (proceed !== 'Yes') {
                        return;
                    }
                }
                
                // Update the configuration
                const config = vscode.workspace.getConfiguration('figlet');
                await config.update('fontDirectory', selectedDir, vscode.ConfigurationTarget.Global);
                
                vscode.window.showInformationMessage(`Font directory updated to: ${selectedDir}`);
                
            } catch (error) {
                vscode.window.showErrorMessage(`Error accessing directory: ${error instanceof Error ? error.message : 'Unknown error'}`);
            }
        }
    });

    /*
     *                      ___      _   _   _               ___                              _ 
     *   ___ _ __  ___ _ _ / __| ___| |_| |_(_)_ _  __ _ ___/ __|___ _ __  _ __  __ _ _ _  __| |
     *  / _ \ '_ \/ -_) ' \\__ \/ -_)  _|  _| | ' \/ _` (_-< (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  \___/ .__/\___|_||_|___/\___|\__|\__|_|_||_\__, /__/\___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *      |_|                                    |___/                                        
     */
    let openSettingsCommand = vscode.commands.registerCommand('figlet.openSettings', async () => {
        await FigletSettingsPanel.createOrShow(context);
    });

    /*
     *      _              __      __   _                   ___                              _ 
     *   __| |_  _____ __ _\ \    / /__| |__ ___ _ __  ___ / __|___ _ __  _ __  __ _ _ _  __| |
     *  (_-< ' \/ _ \ V  V /\ \/\/ / -_) / _/ _ \ '  \/ -_) (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  /__/_||_\___/\_/\_/  \_/\_/\___|_\__\___/_|_|_\___|\___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *                                                                                         
     */
    let showWelcomeCommand = vscode.commands.registerCommand('figlet.showWelcome', () => {
        WelcomePanel.createOrShow();
    });

    /*
     *   _                      _    ___           __ _       ___                              _ 
     *  (_)_ _  ____ __  ___ __| |_ / __|___ _ _  / _(_)__ _ / __|___ _ __  _ __  __ _ _ _  __| |
     *  | | ' \(_-< '_ \/ -_) _|  _| (__/ _ \ ' \|  _| / _` | (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  |_|_||_/__/ .__/\___\__|\__|\___\___/_||_|_| |_\__, |\___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *            |_|                                  |___/                                     
     */
    // Debug command for inspecting configuration (useful during development)
    let inspectConfigCommand = vscode.commands.registerCommand('figlet.inspectConfig', () => {
        const config = vscode.workspace.getConfiguration('figlet');
        
        const allSettings = {
            fontDirectory: config.get('fontDirectory'),
            layoutMode: config.get('layoutMode'),
            commentStyle: config.get('commentStyle')
        };

        console.log('Current Configuration:', allSettings);

        // Also show in UI for easier viewing during debug
        vscode.window.showInformationMessage(
            `Layout: ${allSettings.layoutMode}, Font dir: ${allSettings.fontDirectory || '(default)'}`
        );
    });

    /*
     *   ___                 _    ___ _               ___
     *  |_ _|_ _  ___ ___ __| |_ / __| |__ _ ______  | _ ) __ _ _ _  _ _  ___ _ _
     *   | || ' \(_-</ -_) _|  _| (__| / _` (_-<_-<  | _ \/ _` | ' \| ' \/ -_) '_|
     *  |___|_||_/__/\___\__|\__|\___\__,_|__/__/__/  |___/\__,_|_||_|_||_\___|_|
     *
     */
    let insertClassBannerCommand = vscode.commands.registerCommand('figlet.insertClassBanner', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active text editor found');
            return;
        }
        try {
            const symbol = await findSymbolAtCursor(editor.document, editor.selection.active, CLASS_LIKE_KINDS);
            if (!symbol) {
                vscode.window.showInformationMessage('No class, interface, or struct found at cursor — inserting banner at cursor position.');
            }
            const insertionLine = symbol
                ? BannerUtils.findInsertionPoint(editor.document, symbol.startLine, editor.document.languageId)
                : undefined;
            await FigletPanel.createOrShow(context, editor, { initialText: symbol?.name, insertionLine });
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';
            vscode.window.showErrorMessage(`Error opening FIGLet panel: ${errorMessage}`);
        }
    });

    /*
     *   ___                 _    __  __     _   _            _   ___
     *  |_ _|_ _  ___ ___ __| |_ |  \/  |___| |_| |_  ___  __| | | _ ) __ _ _ _  _ _  ___ _ _
     *   | || ' \(_-</ -_) _|  _|| |\/| / -_)  _| ' \/ _ \/ _` | | _ \/ _` | ' \| ' \/ -_) '_|
     *  |___|_||_/__/\___\__|\__||_|  |_\___|\__|_||_\___/\__,_| |___/\__,_|_||_|_||_\___|_|
     *
     */
    let insertMethodBannerCommand = vscode.commands.registerCommand('figlet.insertMethodBanner', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active text editor found');
            return;
        }
        try {
            const symbol = await findSymbolAtCursor(editor.document, editor.selection.active, METHOD_LIKE_KINDS);
            if (!symbol) {
                vscode.window.showInformationMessage('No function or method found at cursor — inserting banner at cursor position.');
            }
            const insertionLine = symbol
                ? BannerUtils.findInsertionPoint(editor.document, symbol.startLine, editor.document.languageId)
                : undefined;
            await FigletPanel.createOrShow(context, editor, { initialText: symbol?.name, insertionLine });
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';
            vscode.window.showErrorMessage(`Error opening FIGLet panel: ${errorMessage}`);
        }
    });

    // Register all commands
    context.subscriptions.push(
        insertBannerCommand,
        insertClassBannerCommand,
        insertMethodBannerCommand,
        selectFontDirCommand,
        inspectConfigCommand,
        openSettingsCommand,
        showWelcomeCommand
    );

    // Show the welcome page on first install (not on every update)
    const welcomed = context.globalState.get<boolean>('figlet.welcomed');
    if (!welcomed) {
        await context.globalState.update('figlet.welcomed', true);
        WelcomePanel.createOrShow();
    }

    // Optional: Set up configuration change listener
    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration(event => {
            if (event.affectsConfiguration('figlet')) {
                // Handle configuration changes
                // For example, reload fonts if font directory changes
                const config = vscode.workspace.getConfiguration('figlet');
                console.log('Configuration changed:', {
                    fontDirectory: config.get('fontDirectory'),
                    layoutMode: config.get('layoutMode')
                });
            }
        })
    );
}

export function deactivate() {}