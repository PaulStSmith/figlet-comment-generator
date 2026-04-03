import * as vscode from 'vscode';
import { FIGLetFontManager } from './FIGLetFontManager.js';
import { FigletPanel } from './FigletPanel.js';
import { FigletSettingsPanel } from './FigletSettingsPanel.js';
import { WelcomePanel } from './WelcomePanel.js';

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
     *           _        _   ___        _    ___                              _ 
     *   ___ ___| |___ __| |_| __|__ _ _| |_ / __|___ _ __  _ __  __ _ _ _  __| |
     *  (_-</ -_) / -_) _|  _| _/ _ \ ' \  _| (__/ _ \ '  \| '  \/ _` | ' \/ _` |
     *  /__/\___|_\___\__|\__|_|\___/_||_\__|\___\___/_|_|_|_|_|_\__,_|_||_\__,_|
     *                                                                           
     */
    // Add the font selection command
    let selectFontCommand = vscode.commands.registerCommand('figlet.selectDefaultFont', async () => {
        try {
            const fontDir = vscode.workspace.getConfiguration('figlet').get<string>('fontDirectory');
            
            if (!fontDir) {
                const result = await vscode.window.showErrorMessage(
                    'Font directory not configured. Would you like to configure it now?',
                    'Yes', 'No'
                );
                if (result === 'Yes') {
                    await vscode.commands.executeCommand('figlet.selectFontDirectory');
                }
                return;
            }
            // Load fonts using the manager
            await FIGLetFontManager.setFontDirectory(fontDir);
            const fonts = FIGLetFontManager.availableFonts;

            // Check if we have any fonts beyond the default
            if (fonts.length <= 1) {
                vscode.window.showErrorMessage('No FIGlet font files (.flf) found in the configured directory.');
                return;
            }

            // Show QuickPick with font options
            const quickPickItems: vscode.QuickPickItem[] = fonts.map((font: { name: string; filePath: string | null; }) => ({
                label: font.name || '<Default>',
                description: font.filePath || ''
            }));

            const selected = await vscode.window.showQuickPick(quickPickItems, {
                placeHolder: 'Select default FIGlet font',
                title: 'Available Fonts'
            });

            if (selected) {
                // Update the configuration
                await vscode.workspace.getConfiguration().update(
                    'figlet.defaultFont',
                    selected.label,
                    vscode.ConfigurationTarget.Global
                );
                
                vscode.window.showInformationMessage(`Default font updated to: ${selected.label}`);
            }
        } catch (error) {
            vscode.window.showErrorMessage('Failed to load fonts: ' + error);
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
            defaultFont: config.get('defaultFont'),
            layoutMode: config.get('layoutMode'),
            commentStyle: config.get('commentStyle')
        };
        
        console.log('Current Configuration:', allSettings);
        
        // Also show in UI for easier viewing during debug
        vscode.window.showInformationMessage(
            `Current font: ${allSettings.defaultFont}, Layout: ${allSettings.layoutMode}`
        );
    });

    // Register all commands
    context.subscriptions.push(
        insertBannerCommand,
        selectFontDirCommand,
        selectFontCommand,
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
                    defaultFont: config.get('defaultFont'),
                    layoutMode: config.get('layoutMode')
                });
            }
        })
    );
}

export function deactivate() {}