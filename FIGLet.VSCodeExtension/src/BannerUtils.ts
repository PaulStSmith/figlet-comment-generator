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
     * Inserts a commented banner at the specified line, or at the current cursor
     * position when no explicit line is given.
     * @param editor        The active text editor
     * @param figletText    The FIGlet banner text
     * @param languageId    The language identifier (defaults to the document's language)
     * @param insertionLine Optional 0-based line number at which to insert the banner
     */
    static async insertBanner(
        editor: vscode.TextEditor,
        figletText: string,
        languageId?: string,
        insertionLine?: number
    ): Promise<void> {
        const targetLine = insertionLine ?? editor.selection.active.line;
        const position = new vscode.Position(targetLine, 0);
        const indentation = this.getIndentation(editor, position);

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

    /**
     * Returns true if the given (already left-trimmed) line is a documentation
     * comment or code-decoration line for the specified language.
     *
     * Mirrors `IsDocOrDecorationLine` in the VS extension's FIGLetCommentCommand.cs.
     */
    static isDocOrDecorationLine(line: string, languageId: string): boolean {
        line = line.trimStart();
        if (!line) return false;

        switch (languageId.toLowerCase().trim()) {
            // Triple-slash documentation style (C#, F#, Rust)
            case 'csharp': case 'fsharp': case 'rust':
                return line.startsWith('///') || line.startsWith('[');

            // Doxygen / doc-comment style (C, C++, D, Objective-C)
            case 'c': case 'cpp': case 'c/c++': case 'd': case 'objective-c':
                return line.startsWith('///') || line.startsWith('//!') || line.startsWith('@');

            // Triple-quote documentation (VB / Basic)
            case 'vb': case 'basic':
                return line.startsWith("'''") || line.startsWith('<');

            // Hash-based documentation (Python, Ruby, Perl, YAML, shells, etc.)
            case 'python': case 'ruby': case 'perl': case 'yaml':
            case 'shellscript': case 'shell': case 'sh': case 'bash':
            case 'zsh': case 'fish': case 'powershell': case 'ps1':
                return line.startsWith('#') || line.startsWith('@');

            // R — Roxygen2
            case 'r':
                return line.startsWith("#'");

            // Fortran
            case 'fortran':
                return line.startsWith('!>') || line.startsWith('!<');

            // Lisp-family
            case 'lisp': case 'scheme':
                return line.startsWith(';;;') || line.startsWith(';;');

            // SQL-family
            case 'sql': case 'mysql': case 'pgsql': case 'plsql':
            case 'tsql': case 'sqlite':
                return line.startsWith('--');

            // Pascal — PasDoc treats any // comment before a declaration as documentation
            case 'pascal':
                return line.startsWith('//');

            // Batch / DOS
            case 'bat': case 'cmd': case 'dos': case 'batch':
                return line.startsWith('::') || line.toLowerCase().startsWith('rem');

            // XML-style markup
            case 'html': case 'xml': case 'xaml': case 'svg': case 'aspx':
                return line.startsWith('<!--');

            // Annotation-only languages (doc comments are block-based)
            case 'java': case 'javascript': case 'typescript': case 'css':
            case 'go': case 'swift': case 'php': case 'kotlin': case 'scala':
                return line.startsWith('@');

            default:
                return false;
        }
    }

    /**
     * Finds the 0-based line number at which a banner should be inserted above a
     * code element that begins at `symbolStartLine`, walking upward past any
     * documentation comments and block comments that immediately precede it.
     *
     * Mirrors `FindInsertionPoint` in the VS extension's FIGLetCommentCommand.cs,
     * and reuses the `blockCommentStyles` collection from `LanguageCommentStyles`.
     *
     * @param document        The open text document
     * @param symbolStartLine 0-based line of the code element's declaration
     * @param languageId      VS Code language identifier
     * @returns               0-based line number before which the banner should be inserted
     */
    static findInsertionPoint(
        document: vscode.TextDocument,
        symbolStartLine: number,
        languageId: string
    ): number {
        if (symbolStartLine === 0) return 0;

        let insertLine = symbolStartLine;
        let checkLine  = symbolStartLine - 1;

        // Step 1 — walk upward past doc-comment and decoration lines.
        while (checkLine >= 0 && BannerUtils.isDocOrDecorationLine(document.lineAt(checkLine).text, languageId)) {
            insertLine = checkLine;
            checkLine--;
        }

        // Step 2 — walk upward past a block comment (checks every delimiter pair
        //           registered for this language, e.g. both { } and (* *) for Pascal).
        const commentStyle = LanguageCommentStyles.getCommentStyle(languageId);
        if (commentStyle) {
            for (const [blockStart, blockEnd] of commentStyle.blockCommentStyles) {
                if (checkLine < 0 || !document.lineAt(checkLine).text.includes(blockEnd)) continue;

                while (checkLine >= 0 && !document.lineAt(checkLine).text.includes(blockStart)) {
                    checkLine--;
                }
                if (checkLine >= 0) {
                    insertLine = checkLine; // include the opening delimiter line
                }
                break; // only one block comment can sit immediately above the symbol
            }
        }

        return insertLine;
    }
}