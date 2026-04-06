import * as vscode from 'vscode';
import { LanguageCommentStyles } from './LanguageCommentStyles';

/*
 *   ___                         _   _ _   _ _    
 *  | _ ) __ _ _ _  _ _  ___ _ _| | | | |_(_) |___
 *  | _ \/ _` | ' \| ' \/ -_) '_| |_| |  _| | (_-<
 *  |___/\__,_|_||_|_||_\___|_|  \___/ \__|_|_/__/
 *                                                
 */
export class BannerUtils {
    /**
     * Generates the indentation string based on the current line
     * @param editor The active text editor
     * @param position The position to get indentation from
     * @returns The indentation string
     */
    static getIndentation(editor: vscode.TextEditor, position: vscode.Position): string {
        const line = editor.document.lineAt(position);
        const match = line.text.match(/^[\t ]*/);
        return match ? match[0] : '';
    }

    /**
     * Wraps the FIGlet output with appropriate comments for the current language
     * @param figletText The FIGlet banner text
     * @param languageId The language identifier
     * @returns The commented banner text
     */
    static wrapWithComments(figletText: string, languageId: string): string {
        const commentStyle = LanguageCommentStyles.getCommentStyle(languageId.toLowerCase());
        if (!commentStyle) {
            // Default to double slashes if no specific style is found
            return figletText.split('\n').map(line => `// ${line}`).join('\n');
        }

        if (commentStyle.supportsBlockComments) {
            // Use block comments
            const lines = figletText.split('\n');
            const result = [
                commentStyle.blockCommentStart,
                ...lines.map(line => ` * ${line}`),
                ` ${commentStyle.blockCommentEnd}`
            ];
            return result.join('\n');
        } else {
            // Use line comments
            return figletText.split('\n')
                .map(line => `${commentStyle.singleLinePrefix} ${line}`)
                .join('\n');
        }
    }

    /**
     * Inserts a commented banner at the current cursor position
     * @param editor The active text editor
     * @param figletText The FIGlet banner text
     */
    static async insertBanner(editor: vscode.TextEditor, figletText: string, languageId?: string): Promise<void> {
        const currentPosition = editor.selection.active;
        const indentation = this.getIndentation(editor, currentPosition);
        const position = currentPosition.with(currentPosition.line, 0); // Move to the start of the line

        // Wrap the banner with appropriate comments
        const commentedBanner = this.wrapWithComments(figletText, languageId ?? editor.document.languageId);
        
        // Apply indentation to each line
        const indentedBanner = commentedBanner
            .split('\n')
            .map(line => indentation + line)
            .join('\n');

        // Insert the banner
        await editor.edit(editBuilder => {
            editBuilder.insert(position, indentedBanner + '\n');
        });
    }
}