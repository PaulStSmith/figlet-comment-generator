using System;
using System.Collections.Generic;
using System.Linq;

namespace FIGLet.VisualStudioExtension;

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
/// Represents information about a specific comment style.
/// </summary>
/// <param name="key">The unique key identifying the comment style.</param>
/// <param name="name">The name of the comment style.</param>
/// <param name="style">The <see cref="CommentStyleInfo"/> associated with the comment style.</param>
public readonly struct CommentInfo(string key, string name, CommentStyleInfo style)
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
    public static readonly Dictionary<string, CommentInfo> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        // C-style block comments
        { "csharp", new CommentInfo("csharp", "C#", CStyleBlock) },
        { "c/c++", new CommentInfo("c/c++", "C/C++", CStyleBlock) },
        { "cpp", new CommentInfo("cpp", "C++", CStyleBlock) },
        { "java", new CommentInfo("java", "Java", CStyleBlock) },
        { "javascript", new CommentInfo("javascript", "JavaScript", CStyleBlock) },
        { "typescript", new CommentInfo("typescript", "TypeScript", CStyleBlock) },
        { "css", new CommentInfo("css", "CSS", CStyleBlock) },
        { "rust", new CommentInfo("rust", "Rust", CStyleBlock) },
        { "go", new CommentInfo("go", "Go", CStyleBlock) },
        { "swift", new CommentInfo("swift", "Swift", CStyleBlock) },
        { "php", new CommentInfo("php", "PHP", CStyleBlock) },
        { "kotlin", new CommentInfo("kotlin", "Kotlin", CStyleBlock) },
        { "scala", new CommentInfo("scala", "Scala", CStyleBlock) },
        { "d", new CommentInfo("d", "D", CStyleBlock) },
        { "objective-c", new CommentInfo("objective-c", "Objective-C", CStyleBlock) },
    
        // Single line comments
        { "python", new CommentInfo("python", "Python", Hash) },
        { "ruby", new CommentInfo("ruby", "Ruby", Hash) },
        { "perl", new CommentInfo("perl", "Perl", Hash) },
        { "r", new CommentInfo("r", "R", Hash) },
        { "yaml", new CommentInfo("yaml", "YAML", Hash) },
        { "shell", new CommentInfo("shell", "Shell", Hash) },
        { "basic", new CommentInfo("basic", "BASIC", Quote) },
        { "vb", new CommentInfo("vb", "Visual Basic", Quote) },
        { "fortran", new CommentInfo("fortran", "FORTRAN", Quote) },
        { "lisp", new CommentInfo("lisp", "Lisp", Semicolon) },
        { "scheme", new CommentInfo("scheme", "Scheme", Semicolon) },
        { "fsharp", new CommentInfo("fsharp", "F#", MLComment) },
    
        // HTML-style comments
        { "html", new CommentInfo("html", "HTML", HTML) },
        { "xml", new CommentInfo("xml", "XML", HTML) },
        { "xaml", new CommentInfo("xaml", "XAML", HTML) },
        { "svg", new CommentInfo("svg", "SVG", HTML) },
        { "aspx", new CommentInfo("aspx", "ASP.NET", HTML) },
    
        // SQL variants
        { "sql", new CommentInfo("sql", "SQL", SQLLine) },
        { "tsql", new CommentInfo("tsql", "T-SQL", SQLLine) },
        { "mysql", new CommentInfo("mysql", "MySQL", SQLLine) },
        { "pgsql", new CommentInfo("pgsql", "PostgreSQL", SQLLine) },
        { "plsql", new CommentInfo("plsql", "PL/SQL", SQLLine) },
        { "sqlite", new CommentInfo("sqlite", "SQLite", SQLLine) },
    
        // Pascal-style comments
        { "pascal", new CommentInfo("pascal", "Pascal", Pascal) },
    
        // PowerShell
        { "ps1", new CommentInfo("ps1", "PowerShell", PowerShell) },
        { "powershell", new CommentInfo("powershell", "PowerShell", PowerShell) },
    
        // Bash
        { "sh", new CommentInfo("sh", "Shell Script", Bash) },
        { "zsh", new CommentInfo("zsh", "Z Shell", Bash) },
        { "bash", new CommentInfo("bash", "Bash", Bash) },
        { "fish", new CommentInfo("fish", "Fish Shell", Bash) },
    
        // Batch files
        { "cmd", new CommentInfo("cmd", "Command Prompt", new CommentStyleInfo(CommentStyle.Custom, "::", null, null)) },
        { "dos", new CommentInfo("dos", "DOS Batch", new CommentStyleInfo(CommentStyle.Custom, "::", null, null)) },
        { "batch", new CommentInfo("batch", "Batch File", new CommentStyleInfo(CommentStyle.Custom, "::", null, null)) },

        // Unique comment styles
        { "lua", new CommentInfo("lua", "Lua", Lua) },
        { "matlab", new CommentInfo("matlab", "MATLAB", Matlab) },
        { "octave", new CommentInfo("octave", "Octave", Matlab) },
        { "haskell", new CommentInfo("haskell", "Haskell", Haskell) },
        { "handlebars", new CommentInfo("handlebars", "Handlebars", Handlebars) },
        { "razor", new CommentInfo("razor", "Razor", Razor) },
        { "twig", new CommentInfo("twig", "Twig", Twig) },

        // Using existing comment styles
        { "dart", new CommentInfo("dart", "Dart", CStyleBlock) },
        { "julia", new CommentInfo("julia", "Julia", Hash) },
        { "erlang", new CommentInfo("erlang", "Erlang", new CommentStyleInfo(CommentStyle.Custom, "%", null, null)) },
        { "elixir", new CommentInfo("elixir", "Elixir", Hash) },
        { "groovy", new CommentInfo("groovy", "Groovy", CStyleBlock) },
        { "ini", new CommentInfo("ini", "INI", Semicolon) },
        { "toml", new CommentInfo("toml", "TOML", Hash) },
        { "dockerfile", new CommentInfo("dockerfile", "Dockerfile", Hash) },
        { "makefile", new CommentInfo("makefile", "Makefile", Hash) },
        { "cmake", new CommentInfo("cmake", "CMake", Hash) },
        { "gradle", new CommentInfo("gradle", "Gradle", CStyleBlock) },
        { "autohotkey", new CommentInfo("autohotkey", "AutoHotkey", Semicolon) },
        { "powerquery", new CommentInfo("powerquery", "Power Query", CStyleBlock) },
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