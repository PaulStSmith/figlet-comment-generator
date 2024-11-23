/**
 * Enum representing different styles of comments.
 */
export enum CommentStyle {
    /**
     * A custom comment style.
     */
    Custom = 0,

    /**
     * C-style block comments (/* ... *\/).
     */
    CStyleBlock,

    /**
     * Single line comments (//).
     */
    DoubleSlashes,

    /**
     * Hash-style single line comments (#).
     */
    Hash,

    /**
     * Semicolon-style single line comments (;).
     */
    Semicolon,

    /**
     * Quote-style single line comments (').
     */
    Quote,

    /**
     * ML-style block comments ((* ... *)).
     */
    MLComment,

    /**
     * HTML-style block comments (<!-- ... -->).
     */
    HTML,

    /**
     * SQL-style single line comments (--).
     */
    SQLLine,

    /**
     * Pascal-style comments ({}).
     */
    Pascal,

    /**
     * PowerShell-style comments (<# ... #>).
     */
    PowerShell,

    /**
     * Bash-style comments (: ' ... ').
     */
    Bash
}

/**
 * Represents information about a comment style.
 */
export class CommentStyleInfo {
    private readonly _primaryStyle: CommentStyle;
    private readonly _blockCommentStart: string | null = null;
    private readonly _blockCommentEnd: string | null = null;
    private readonly _singleLinePrefix: string | null = null;

    /**
     * Gets the primary comment style.
     */
    get primaryStyle(): CommentStyle {
        return this._primaryStyle;
    }

    /**
     * Gets a value indicating whether block comments are supported.
     */
    get supportsBlockComments(): boolean {
        return this._blockCommentStart !== null && this._blockCommentEnd !== null;
    }

    /**
     * Gets the block comment start marker.
     */
    get blockCommentStart(): string | null {
        return this._blockCommentStart;
    }

    /**
     * Gets the block comment end marker.
     */
    get blockCommentEnd(): string | null {
        return this._blockCommentEnd;
    }

    /**
     * Gets the single line comment prefix.
     */
    get singleLinePrefix(): string | null {
        return this._singleLinePrefix;
    }

    /**
     * Initializes a new instance of the CommentStyleInfo class.
     */
    constructor(
        primary: CommentStyle,
        singleLinePrefix: string | null = null,
        blockStart: string | null = null,
        blockEnd: string | null = null
    ) {
        this._primaryStyle = primary;

        switch (primary) {
            case CommentStyle.Custom:
                if (singleLinePrefix) {
                    this._singleLinePrefix = singleLinePrefix;
                }
                if (blockStart && blockEnd) {
                    this._blockCommentStart = blockStart;
                    this._blockCommentEnd = blockEnd;
                }
                if (!singleLinePrefix && (!blockStart || !blockEnd)) {
                    throw new Error("Custom comment style requires a single-line prefix, both block comment markers, or all three.");
                }
                break;

            case CommentStyle.CStyleBlock:
                this._blockCommentStart = blockStart ?? "/*";
                this._blockCommentEnd = blockEnd ?? "*/";
                this._singleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.DoubleSlashes:
                this._singleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.Hash:
                this._singleLinePrefix = singleLinePrefix ?? "#";
                break;

            case CommentStyle.Semicolon:
                this._singleLinePrefix = singleLinePrefix ?? ";";
                break;

            case CommentStyle.Quote:
                this._singleLinePrefix = singleLinePrefix ?? "'";
                break;

            case CommentStyle.MLComment:
                this._blockCommentStart = blockStart ?? "(*";
                this._blockCommentEnd = blockEnd ?? "*)";
                this._singleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.HTML:
                this._blockCommentStart = blockStart ?? "<!--";
                this._blockCommentEnd = blockEnd ?? "-->";
                this._singleLinePrefix = null;
                break;

            case CommentStyle.SQLLine:
                this._blockCommentStart = blockStart ?? "/*";
                this._blockCommentEnd = blockEnd ?? "*/";
                this._singleLinePrefix = singleLinePrefix ?? "--";
                break;

            case CommentStyle.Pascal:
                this._blockCommentStart = blockStart ?? "{";
                this._blockCommentEnd = blockEnd ?? "}";
                this._singleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.PowerShell:
                this._singleLinePrefix = singleLinePrefix ?? "#";
                this._blockCommentStart = blockStart ?? "<#";
                this._blockCommentEnd = blockEnd ?? "#>";
                break;

            case CommentStyle.Bash:
                this._singleLinePrefix = singleLinePrefix ?? "#";
                this._blockCommentStart = blockStart ?? ": '#";
                this._blockCommentEnd = blockEnd ?? "#'";
                break;

            default:
                throw new Error(`Invalid comment style: ${primary}`);
        }
    }

    /**
     * Wraps the given text in block comments.
     */
    wrapInBlockComment(text: string): string {
        if (!this.supportsBlockComments) {
            throw new Error(`Comment style ${CommentStyle[this.primaryStyle]} does not support block comments`);
        }

        const startLen = this._blockCommentStart!.length - 1;
        if (this._blockCommentStart![startLen] === this._blockCommentEnd![0]) {
            const padding = ' '.repeat(startLen) + this._blockCommentEnd![0];
            return `${this._blockCommentStart}\n${padding}${
                text.split(/[\r\n]/)
                    .filter(line => line.length > 0)
                    .join('\n' + padding)
            }\n${padding}${this._blockCommentEnd!.substring(1)}`;
        }

        return `${this._blockCommentStart}\n${text}\n${this._blockCommentEnd}`;
    }

    /**
     * Wraps the given text in single-line comments.
     */
    wrapInSingleLineComments(text: string): string {
        if (!this._singleLinePrefix) {
            throw new Error(`Comment style ${CommentStyle[this.primaryStyle]} does not support single line comments`);
        }

        return text
            .split(/\r?\n/)
            .map(line => `${this._singleLinePrefix} ${line}`)
            .join('\n');
    }
}

/**
 * Provides language-specific comment style information and utilities.
 */
export class LanguageCommentStyles {
    /**
     * Default comment style using double slashes (//).
     */
    static readonly Default = new CommentStyleInfo(CommentStyle.DoubleSlashes);

    /**
     * C-style block comment style (/* ... *\/).
     */
    static readonly CStyleBlock = new CommentStyleInfo(CommentStyle.CStyleBlock);

    /**
     * Double slashes comment style (//).
     */
    static readonly DoubleSlashes = new CommentStyleInfo(CommentStyle.DoubleSlashes);

    /**
     * Hash-style single line comment style (#).
     */
    static readonly Hash = new CommentStyleInfo(CommentStyle.Hash);

    /**
     * Semicolon-style single line comment style (;).
     */
    static readonly Semicolon = new CommentStyleInfo(CommentStyle.Semicolon);

    /**
     * Quote-style single line comment style (').
     */
    static readonly Quote = new CommentStyleInfo(CommentStyle.Quote);

    /**
     * ML-style block comment style ((* ... *)).
     */
    static readonly MLComment = new CommentStyleInfo(CommentStyle.MLComment);

    /**
     * HTML-style block comment style (<!-- ... -->).
     */
    static readonly HTML = new CommentStyleInfo(CommentStyle.HTML);

    /**
     * SQL-style single line comment style (--).
     */
    static readonly SQLLine = new CommentStyleInfo(CommentStyle.SQLLine);

    /**
     * Pascal-style comments ({}).
     */
    static readonly Pascal = new CommentStyleInfo(CommentStyle.Pascal);

    /**
     * PowerShell-style comments (# and <# ... #>).
     */
    static readonly PowerShell = new CommentStyleInfo(CommentStyle.PowerShell);

    /**
     * Bash-style comments (# and : ' ... ').
     */
    static readonly Bash = new CommentStyleInfo(CommentStyle.Bash);

    /**
     * Dictionary mapping language names to their respective comment styles.
     */
    private static readonly languageMap = new Map<string, CommentStyleInfo>([
        // C-style block comments primary
        ['csharp', LanguageCommentStyles.CStyleBlock],
        ['c/c++', LanguageCommentStyles.CStyleBlock],
        ['cpp', LanguageCommentStyles.CStyleBlock],
        ['java', LanguageCommentStyles.CStyleBlock],
        ['javascript', LanguageCommentStyles.CStyleBlock],
        ['typescript', LanguageCommentStyles.CStyleBlock],
        ['css', LanguageCommentStyles.CStyleBlock],
        ['rust', LanguageCommentStyles.CStyleBlock],
        ['go', LanguageCommentStyles.CStyleBlock],
        ['swift', LanguageCommentStyles.CStyleBlock],
        ['php', LanguageCommentStyles.CStyleBlock],
        ['kotlin', LanguageCommentStyles.CStyleBlock],
        ['scala', LanguageCommentStyles.CStyleBlock],
        ['d', LanguageCommentStyles.CStyleBlock],
        ['objective-c', LanguageCommentStyles.CStyleBlock],

        // Single line comments primary
        ['python', LanguageCommentStyles.Hash],
        ['ruby', LanguageCommentStyles.Hash],
        ['perl', LanguageCommentStyles.Hash],
        ['r', LanguageCommentStyles.Hash],
        ['yaml', LanguageCommentStyles.Hash],
        ['shell', LanguageCommentStyles.Hash],
        ['basic', LanguageCommentStyles.Quote],
        ['vb', LanguageCommentStyles.Quote],
        ['fortran', LanguageCommentStyles.Quote],
        ['lisp', LanguageCommentStyles.Semicolon],
        ['scheme', LanguageCommentStyles.Semicolon],
        ['fsharp', LanguageCommentStyles.MLComment],

        // HTML-style comments
        ['html', LanguageCommentStyles.HTML],
        ['xml', LanguageCommentStyles.HTML],
        ['xaml', LanguageCommentStyles.HTML],
        ['svg', LanguageCommentStyles.HTML],
        ['aspx', LanguageCommentStyles.HTML],

        // SQL variants
        ['sql', LanguageCommentStyles.SQLLine],
        ['tsql', LanguageCommentStyles.SQLLine],
        ['mysql', LanguageCommentStyles.SQLLine],
        ['pgsql', LanguageCommentStyles.SQLLine],
        ['plsql', LanguageCommentStyles.SQLLine],
        ['sqlite', LanguageCommentStyles.SQLLine],

        // Pascal-style comments
        ['pascal', LanguageCommentStyles.Pascal],

        // PowerShell
        ['ps1', LanguageCommentStyles.PowerShell],
        ['powershell', LanguageCommentStyles.PowerShell],

        // Bash
        ['sh', LanguageCommentStyles.Bash],
        ['zsh', LanguageCommentStyles.Bash],
        ['bash', LanguageCommentStyles.Bash],
        ['fish', LanguageCommentStyles.Bash],
        ['shellscript', LanguageCommentStyles.Bash],

        // Batch files
        ['bat', new CommentStyleInfo(CommentStyle.Custom, '::', null, null)],
        ['cmd', new CommentStyleInfo(CommentStyle.Custom, '::', null, null)],
        ['dos', new CommentStyleInfo(CommentStyle.Custom, '::', null, null)],
        ['batch', new CommentStyleInfo(CommentStyle.Custom, '::', null, null)]
    ]);

    /**
     * Gets the comment style information for the specified language.
     */
    static getCommentStyle(language: string): CommentStyleInfo {
        return LanguageCommentStyles.languageMap.get(language.toLowerCase()) ?? LanguageCommentStyles.Default;
    }

    /**
     * Wraps the given text in comments appropriate for the specified language.
     */
    static wrapInComments(text: string, language: string, preferBlockComments = true): string {
        const styleInfo = LanguageCommentStyles.getCommentStyle(language);

        // If block comments are preferred and supported, use them
        if (preferBlockComments && styleInfo.supportsBlockComments) {
            return styleInfo.wrapInBlockComment(text);
        }

        // Otherwise use single-line comments
        return styleInfo.wrapInSingleLineComments(text);
    }
}