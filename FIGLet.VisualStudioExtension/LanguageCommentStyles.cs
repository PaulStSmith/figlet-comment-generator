using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteForge.FIGLet.VisualStudioExtension;

/// <summary>
/// Enum representing different styles of comments.
/// </summary>
public enum CommentStyle
{
    /// <summary>
    /// A custom comment style.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// C-style block comments (/* ... */).
    /// </summary>
    CStyleBlock,

    /// <summary>
    /// Single line comments (//).
    /// </summary>
    DoubleSlashes,

    /// <summary>
    /// Hash-style single line comments (#).
    /// </summary>
    Hash,

    /// <summary>
    /// Semicolon-style single line comments (;).
    /// </summary>
    Semicolon,

    /// <summary>
    /// Quote-style single line comments (').
    /// </summary>
    Quote,

    /// <summary>
    /// ML-style block comments ((* ... *)). 
    /// </summary>
    MLComment,

    /// <summary>
    /// HTML-style block comments (<!-- ... -->).
    /// </summary>
    HTML,

    /// <summary>
    /// SQL-style single line comments (--).
    /// </summary>
    SQLLine,

    /// <summary>
    /// Pascal-style comments ({}).
    /// </summary>
    Pascal,

    /// <summary>
    /// PowerShell-style comments (<# ... #>).
    /// </summary>
    PowerShell,

    /// <summary>
    /// Bash-style comments (: ' ... ').
    /// </summary>
    Bash,

    /// <summary>
    /// Lua-style comments (-- and --[[ ... ]]).
    /// </summary>
    Lua,

    /// <summary>
    /// MATLAB-style comments (% and %{ ... %}).
    /// </summary>
    Matlab,

    /// <summary>
    /// Haskell-style comments (-- and {- ... -}).
    /// </summary>
    Haskell,

    /// <summary>
    /// Handlebars-style comments ({{! ... }} and {{!-- ... --}}).
    /// </summary>
    Handlebars,

    /// <summary>
    /// Razor-style comments (@* ... *@).
    /// </summary>
    Razor,

    /// <summary>
    /// Twig-style comments ({# ... #}).
    /// </summary>
    Twig
}

public class CommentStyleInfo
{
    /// <summary>
    /// Gets or sets the primary comment style.
    /// </summary>
    public CommentStyle PrimaryStyle { get; set; }

    /// <summary>
    /// Gets a value indicating whether block comments are supported.
    /// </summary>
    public bool SupportsBlockComments => BlockCommentStart != null && BlockCommentEnd != null;

    /// <summary>
    /// Gets the block comment start marker.
    /// </summary>
    public string BlockCommentStart { get; }

    /// <summary>
    /// Gets the block comment end marker.
    /// </summary>
    public string BlockCommentEnd { get; }

    /// <summary>
    /// Gets the single line comment prefix.
    /// </summary>
    public string SingleLinePrefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentStyleInfo"/> class.
    /// </summary>
    /// <param name="primary">The primary comment style.</param>
    /// <param name="singleLinePrefix">The single line comment prefix.</param>
    /// <param name="blockStart">The block comment start marker.</param>
    /// <param name="blockEnd">The block comment end marker.</param>
    public CommentStyleInfo(CommentStyle primary, string singleLinePrefix = null, string blockStart = null, string blockEnd = null)
    {
        switch (PrimaryStyle = primary)
        {
            case CommentStyle.Custom:
                if (singleLinePrefix != null)
                    SingleLinePrefix = singleLinePrefix;

                if (blockStart != null && blockEnd != null)
                {
                    BlockCommentStart = blockStart;
                    BlockCommentEnd = blockEnd;
                }

                if (SingleLinePrefix == null && (BlockCommentStart == null || BlockCommentEnd == null))
                    throw new ArgumentException("Custom comment style requires a single-line prefix, both block comment markers, or all three.");
                break;
            case CommentStyle.CStyleBlock:
                BlockCommentStart = blockStart ?? "/*";
                BlockCommentEnd = blockEnd ?? "*/";
                SingleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.DoubleSlashes:
                SingleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.Hash:
                SingleLinePrefix = singleLinePrefix ?? "#";
                break;

            case CommentStyle.Semicolon:
                SingleLinePrefix = singleLinePrefix ?? ";";
                break;

            case CommentStyle.Quote:
                SingleLinePrefix = singleLinePrefix ?? "'";
                break;

            case CommentStyle.MLComment:
                BlockCommentStart = blockStart ?? "(*";
                BlockCommentEnd = blockEnd ?? "*)";
                SingleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.HTML:
                BlockCommentStart = blockStart ?? "<!--";
                BlockCommentEnd = blockEnd ?? "-->";
                SingleLinePrefix = null;
                break;

            case CommentStyle.SQLLine:
                BlockCommentStart = blockStart ?? "/*";
                BlockCommentEnd = blockEnd ?? "*/";
                SingleLinePrefix = singleLinePrefix ?? "--";
                break;

            case CommentStyle.Pascal:
                BlockCommentStart = blockStart ?? "{";
                BlockCommentEnd = blockEnd ?? "}";
                SingleLinePrefix = singleLinePrefix ?? "//";
                break;

            case CommentStyle.PowerShell:
                SingleLinePrefix = singleLinePrefix ?? "#";
                BlockCommentStart = blockStart ?? "<#";
                BlockCommentEnd = blockEnd ?? "#>";
                break;

            case CommentStyle.Bash:
                SingleLinePrefix = singleLinePrefix ?? "#";
                BlockCommentStart = blockStart ?? ": '#";
                BlockCommentEnd = blockEnd ?? "#'";     
                break;

            case CommentStyle.Lua:
                SingleLinePrefix = singleLinePrefix ?? "--";
                BlockCommentStart = blockStart ?? "--[[";
                BlockCommentEnd = blockEnd ?? "]]";
                break;

            case CommentStyle.Matlab:
                SingleLinePrefix = singleLinePrefix ?? "%";
                BlockCommentStart = blockStart ?? "%{";
                BlockCommentEnd = blockEnd ?? "%}";
                break;

            case CommentStyle.Haskell:
                SingleLinePrefix = singleLinePrefix ?? "--";
                BlockCommentStart = blockStart ?? "{-";
                BlockCommentEnd = blockEnd ?? "-}";
                break;

            case CommentStyle.Handlebars:
                SingleLinePrefix = singleLinePrefix ?? "{{!";
                BlockCommentStart = blockStart ?? "{{!--";
                BlockCommentEnd = blockEnd ?? "--}}";
                break;

            case CommentStyle.Razor:
                SingleLinePrefix = singleLinePrefix ?? "@*";
                BlockCommentStart = blockStart ?? "@*";
                BlockCommentEnd = blockEnd ?? "*@";
                break;

            case CommentStyle.Twig:
                SingleLinePrefix = singleLinePrefix ?? "{#";
                BlockCommentEnd = blockEnd ?? "#}";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(primary), primary, null);
        }
    }

    /// <summary>
    /// Wraps the given text in block comments.
    /// </summary>
    /// <param name="text">The text to be commented.</param>
    /// <returns>The text wrapped in block comments.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the comment style does not support block comments.</exception>
    public string WrapInBlockComment(string text)
    {
        if (!SupportsBlockComments)
            throw new InvalidOperationException($"Comment style {PrimaryStyle} does not support block comments");

        var a = BlockCommentStart.Length - 1;
        if (BlockCommentStart[a] == BlockCommentEnd[0])
        {
            var c = new string(' ', a) + BlockCommentEnd[0];
            return $"{BlockCommentStart}\n{c}{ string.Join("\n" + c, text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries)) }\n{c}{BlockCommentEnd.Substring(1)}";
        }

        return $"{BlockCommentStart}\n{text}\n{BlockCommentEnd}";
    }

    /// <summary>
    /// Wraps the given text in single-line comments with the specified prefix.
    /// </summary>
    /// <param name="text">The text to be commented.</param>
    /// <param name="commentPrefix">The prefix to use for each comment line.</param>
    /// <returns>The text wrapped in single-line comments.</returns>
    public string WrapInSingleLineComments(string text)
    {
        return string.Join("\n",
            text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => $"{SingleLinePrefix} {line}"));
    }
}

/// <summary>
/// Represents information about a specific programming language.
/// </summary>
/// <param name="key">A unique key identifying the programming language.</param>
/// <param name="name">The name of the programming language.</param>
/// <param name="extenstions">A string containing the most common file extensions associated with the programming language.</param>
/// <param name="style">The <see cref="CommentStyleInfo"/> associated with the programming language.</param>
public readonly struct ProgrammingLanguageInfo(string key, string name, string extenstions, CommentStyleInfo style)
{
    /// <summary>
    /// Gets the unique key identifying the comment style.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Gets the name of the comment style.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets an array of the most common file extensions associated with the programming language.
    /// </summary>
    public string[] Extensions => extenstions.ToLowerInvariant().Split(';').Select(ext => ext.Trim()).ToArray();

    /// <summary>
    /// Gets the <see cref="CommentStyleInfo"/> associated with the comment style.
    /// </summary>
    public CommentStyleInfo Style { get; } = style;

    /// <summary>
    /// Returns the name of the comment style.
    /// </summary>
    /// <returns>The name of the comment style.</returns>
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// Provides comment style information and utilities for various programming languages.
/// </summary>
public static class LanguageCommentStyles
{
    /// <summary>
    /// Default comment style using double slashes (//).
    /// </summary>
    public static readonly CommentStyleInfo Default = DoubleSlashes;

    /// <summary>
    /// C-style block comment style (/* ... */).
    /// </summary>
    public static readonly CommentStyleInfo CStyleBlock = new(CommentStyle.CStyleBlock);

    /// <summary>
    /// Double slashes comment style (//).
    /// </summary>
    public static readonly CommentStyleInfo DoubleSlashes = new(CommentStyle.DoubleSlashes);

    /// <summary>
    /// Hash-style single line comment style (#).
    /// </summary>
    public static readonly CommentStyleInfo Hash = new(CommentStyle.Hash);

    /// <summary>
    /// Semicolon-style single line comment style (;).
    /// </summary>
    public static readonly CommentStyleInfo Semicolon = new(CommentStyle.Semicolon);

    /// <summary>
    /// Quote-style single line comment style (').
    /// </summary>
    public static readonly CommentStyleInfo Quote = new(CommentStyle.Quote);

    /// <summary>
    /// ML-style block comment style ((* ... *)).
    /// </summary>
    public static readonly CommentStyleInfo MLComment = new(CommentStyle.MLComment);

    /// <summary>
    /// HTML-style block comment style (<!-- ... -->).
    /// </summary>
    public static readonly CommentStyleInfo HTML = new(CommentStyle.HTML);

    /// <summary>
    /// SQL-style single line comment style (--).
    /// </summary>
    public static readonly CommentStyleInfo SQLLine = new(CommentStyle.SQLLine);

    /// <summary>
    /// Pascal-style comments ({}).
    /// </summary>
    public static readonly CommentStyleInfo Pascal = new(CommentStyle.Pascal);

    /// <summary>
    /// PowerShell-style comments (# and <# ... #>).
    /// </summary>
    public static readonly CommentStyleInfo PowerShell = new(CommentStyle.PowerShell);

    /// <summary>
    /// Bash-style comments (# and : ' ... ').
    /// </summary>
    public static readonly CommentStyleInfo Bash = new(CommentStyle.Bash);

    /// <summary>
    /// Lua-style comments (-- and --[[ ... ]]).
    /// </summary>
    public static readonly CommentStyleInfo Lua = new(CommentStyle.Lua);

    /// <summary>
    /// MATLAB-style comments (% and %{ ... %}).
    /// </summary>
    public static readonly CommentStyleInfo Matlab = new(CommentStyle.Matlab);

    /// <summary>
    /// Haskell-style comments (-- and {- ... -}).
    /// </summary>
    public static readonly CommentStyleInfo Haskell = new(CommentStyle.Haskell);

    /// <summary>
    /// Handlebars-style comments ({{! ... }} and {{!-- ... --}}).
    /// </summary>
    public static readonly CommentStyleInfo Handlebars = new(CommentStyle.Handlebars);

    /// <summary>
    /// Razor-style comments (@* ... *@).
    /// </summary>
    public static readonly CommentStyleInfo Razor = new(CommentStyle.Razor);

    /// <summary>
    /// Twig-style comments ({# ... #}).
    /// </summary>
    public static readonly CommentStyleInfo Twig = new(CommentStyle.Twig);

    /// <summary>
    /// Dictionary mapping language names to their respective comment styles.
    /// </summary>
    public static readonly Dictionary<string, ProgrammingLanguageInfo> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        // C-style block comments
        { "csharp", new ProgrammingLanguageInfo("csharp", "C#", "cs; csx", CStyleBlock) },
        { "c/c++", new ProgrammingLanguageInfo("c/c++", "C/C++", "c; h; cpp; hpp", CStyleBlock) },
        { "cpp", new ProgrammingLanguageInfo("cpp", "C++", "cpp; hpp; cc; hh; cxx; hxx", CStyleBlock) },
        { "java", new ProgrammingLanguageInfo("java", "Java", "java", CStyleBlock) },
        { "javascript", new ProgrammingLanguageInfo("javascript", "JavaScript", "js; jsx; mjs", CStyleBlock) },
        { "typescript", new ProgrammingLanguageInfo("typescript", "TypeScript", "ts; tsx", CStyleBlock) },
        { "css", new ProgrammingLanguageInfo("css", "CSS", "css", CStyleBlock) },
        { "rust", new ProgrammingLanguageInfo("rust", "Rust", "rs", CStyleBlock) },
        { "go", new ProgrammingLanguageInfo("go", "Go", "go", CStyleBlock) },
        { "swift", new ProgrammingLanguageInfo("swift", "Swift", "swift", CStyleBlock) },
        { "php", new ProgrammingLanguageInfo("php", "PHP", "php; phtml", CStyleBlock) },
        { "kotlin", new ProgrammingLanguageInfo("kotlin", "Kotlin", "kt; kts", CStyleBlock) },
        { "scala", new ProgrammingLanguageInfo("scala", "Scala", "scala", CStyleBlock) },
        { "d", new ProgrammingLanguageInfo("d", "D", "d", CStyleBlock) },
        { "objective-c", new ProgrammingLanguageInfo("objective-c", "Objective-C", "m; mm", CStyleBlock) },
    
        // Single line comments
        { "python", new ProgrammingLanguageInfo("python", "Python", "py; pyw; pyx", Hash) },
        { "ruby", new ProgrammingLanguageInfo("ruby", "Ruby", "rb; rbw", Hash) },
        { "perl", new ProgrammingLanguageInfo("perl", "Perl", "pl; pm", Hash) },
        { "r", new ProgrammingLanguageInfo("r", "R", "r; R", Hash) },
        { "yaml", new ProgrammingLanguageInfo("yaml", "YAML", "yaml; yml", Hash) },
        { "shell", new ProgrammingLanguageInfo("shell", "Shell", "sh", Hash) },
        { "basic", new ProgrammingLanguageInfo("basic", "BASIC", "bas", Quote) },
        { "vb", new ProgrammingLanguageInfo("vb", "Visual Basic", "vb; bas", Quote) },
        { "fortran", new ProgrammingLanguageInfo("fortran", "FORTRAN", "f; f90; f95; f03; f08", Quote) },
        { "lisp", new ProgrammingLanguageInfo("lisp", "Lisp", "lisp; lsp; l", Semicolon) },
        { "scheme", new ProgrammingLanguageInfo("scheme", "Scheme", "scm; ss", Semicolon) },
        { "fsharp", new ProgrammingLanguageInfo("fsharp", "F#", "fs; fsx", MLComment) },
    
        // HTML-style comments
        { "html", new ProgrammingLanguageInfo("html", "HTML", "html; htm", HTML) },
        { "xml", new ProgrammingLanguageInfo("xml", "XML", "xml", HTML) },
        { "xaml", new ProgrammingLanguageInfo("xaml", "XAML", "xaml", HTML) },
        { "svg", new ProgrammingLanguageInfo("svg", "SVG", "svg", HTML) },
        { "aspx", new ProgrammingLanguageInfo("aspx", "ASP.NET", "aspx; ascx", HTML) },
    
        // SQL variants
        { "sql", new ProgrammingLanguageInfo("sql", "SQL", "sql", SQLLine) },
        { "tsql", new ProgrammingLanguageInfo("tsql", "T-SQL", "sql", SQLLine) },
        { "mysql", new ProgrammingLanguageInfo("mysql", "MySQL", "sql", SQLLine) },
        { "pgsql", new ProgrammingLanguageInfo("pgsql", "PostgreSQL", "sql", SQLLine) },
        { "plsql", new ProgrammingLanguageInfo("plsql", "PL/SQL", "sql; pls", SQLLine) },
        { "sqlite", new ProgrammingLanguageInfo("sqlite", "SQLite", "sql", SQLLine) },
    
        // Pascal-style comments
        { "pascal", new ProgrammingLanguageInfo("pascal", "Pascal", "pas; pp", Pascal) },
    
        // PowerShell
        { "ps1", new ProgrammingLanguageInfo("ps1", "PowerShell", "ps1; psm1; psd1", PowerShell) },
        { "powershell", new ProgrammingLanguageInfo("powershell", "PowerShell", "ps1; psm1; psd1", PowerShell) },
    
        // Bash
        { "sh", new ProgrammingLanguageInfo("sh", "Shell Script", "sh", Bash) },
        { "zsh", new ProgrammingLanguageInfo("zsh", "Z Shell", "zsh", Bash) },
        { "bash", new ProgrammingLanguageInfo("bash", "Bash", "sh; bash", Bash) },
        { "fish", new ProgrammingLanguageInfo("fish", "Fish Shell", "fish", Bash) },
    
        // Batch files
        { "dos", new ProgrammingLanguageInfo("dos", "DOS Batch", "bat", new CommentStyleInfo(CommentStyle.Custom, "::", null, null)) },
        { "batch", new ProgrammingLanguageInfo("batch", "Batch File", "bat; cmd", new CommentStyleInfo(CommentStyle.Custom, "::", null, null)) },

        // Unique comment styles
        { "lua", new ProgrammingLanguageInfo("lua", "Lua", "lua", Lua) },
        { "matlab", new ProgrammingLanguageInfo("matlab", "MATLAB", "m", Matlab) },
        { "octave", new ProgrammingLanguageInfo("octave", "Octave", "m", Matlab) },
        { "haskell", new ProgrammingLanguageInfo("haskell", "Haskell", "hs; lhs", Haskell) },
        { "handlebars", new ProgrammingLanguageInfo("handlebars", "Handlebars", "hbs; handlebars", Handlebars) },
        { "razor", new ProgrammingLanguageInfo("razor", "Razor", "cshtml; vbhtml", Razor) },
        { "twig", new ProgrammingLanguageInfo("twig", "Twig", "twig", Twig) },

        // Using existing comment styles
        { "dart", new ProgrammingLanguageInfo("dart", "Dart", "dart", CStyleBlock) },
        { "julia", new ProgrammingLanguageInfo("julia", "Julia", "jl", Hash) },
        { "erlang", new ProgrammingLanguageInfo("erlang", "Erlang", "erl; hrl", new CommentStyleInfo(CommentStyle.Custom, "%", null, null)) },
        { "elixir", new ProgrammingLanguageInfo("elixir", "Elixir", "ex; exs", Hash) },
        { "groovy", new ProgrammingLanguageInfo("groovy", "Groovy", "groovy; gvy", CStyleBlock) },
        { "ini", new ProgrammingLanguageInfo("ini", "INI", "ini", Semicolon) },
        { "toml", new ProgrammingLanguageInfo("toml", "TOML", "toml", Hash) },
        { "dockerfile", new ProgrammingLanguageInfo("dockerfile", "Dockerfile", "dockerfile", Hash) },
        { "makefile", new ProgrammingLanguageInfo("makefile", "Makefile", "makefile; mk", Hash) },
        { "cmake", new ProgrammingLanguageInfo("cmake", "CMake", "cmake", Hash) },
        { "gradle", new ProgrammingLanguageInfo("gradle", "Gradle", "gradle", CStyleBlock) },
        { "autohotkey", new ProgrammingLanguageInfo("autohotkey", "AutoHotkey", "ahk", Semicolon) },
        { "powerquery", new ProgrammingLanguageInfo("powerquery", "Power Query", "pq", CStyleBlock) },    
    };

    /// <summary>
    /// Gets the comment style information for the specified language.
    /// </summary>
    /// <param name="language">The programming language to get the comment style for.</param>
    /// <returns>The <see cref="CommentStyleInfo"/> for the specified language.</returns>
    /// <remarks>
    /// If the language is not found in the predefined list, the default comment style (double slashes) is returned.
    /// </remarks>
    public static CommentStyleInfo GetCommentStyle(string language)
    {
        return SupportedLanguages.TryGetValue(language, out var styleInfo) ? styleInfo.Style : Default;
    }

    /// <summary>
    /// Wraps the given text in comments appropriate for the specified language.
    /// </summary>
    /// <param name="text">The text to be commented.</param>
    /// <param name="language">The programming language to determine the comment style.</param>
    /// <param name="preferBlockComments">Indicates whether to prefer block comments if available.</param>
    /// <returns>The text wrapped in the appropriate comment style.</returns>
    public static string WrapInComments(string text, string language, bool preferBlockComments = true)
    {
        // Default to single-line slashes if language not found
        if (!SupportedLanguages.TryGetValue(language, out var info))
            return WrapInSingleLineComments(text, "//");

        var styleInfo = info.Style;
        // If block comments are preferred and supported, use them
        if (preferBlockComments && styleInfo.SupportsBlockComments)
            return styleInfo.WrapInBlockComment(text);

        // Otherwise use single-line comments
        return styleInfo.WrapInSingleLineComments(text);
    }

    /// <summary>
    /// Wraps the given text in single-line comments with the specified prefix.
    /// </summary>
    /// <param name="text">The text to be commented.</param>
    /// <param name="commentPrefix">The prefix to use for each comment line.</param>
    /// <returns>The text wrapped in single-line comments.</returns>
    private static string WrapInSingleLineComments(string text, string commentPrefix)
    {
        return string.Join("\n",
            text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => $"{commentPrefix} {line}"));
    }
}