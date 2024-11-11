using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIGLet.VisualStudioExtension
{
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
        CStyleBlock,      // /* ... */

        /// <summary>
        /// Single line comments (//).
        /// </summary>
        DoubleSlashes,  // //

        /// <summary>
        /// Hash-style single line comments (#).
        /// </summary>
        Hash,        // #

        /// <summary>
        /// Semicolon-style single line comments (;).
        /// </summary>
        Semicolon,   // ;

        /// <summary>
        /// Quote-style single line comments (').
        /// </summary>
        Quote,       // '

        /// <summary>
        /// ML-style block comments ((* ... *)).
        /// </summary>
        MLComment,   // (* ... *)

        /// <summary>
        /// HTML-style block comments (<!-- ... -->).
        /// </summary>
        HTML,        // <!-- ... -->

        /// <summary>
        /// SQL-style single line comments (--).
        /// </summary>
        SQLLine      // --
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
                    if (singleLinePrefix == null || (blockStart == null && blockEnd == null))
                        throw new ArgumentException("Custom comment style requires a single-line prefix, both block comment markers, or all three.");
                    BlockCommentStart = blockStart;
                    BlockCommentEnd = blockEnd;
                    SingleLinePrefix = singleLinePrefix;
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
                return $"{BlockCommentStart}\n{ string.Join("\n" + c, text.Split('\n')) }\n{c}{BlockCommentEnd.Substring(1)}";
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

    public static class LanguageCommentStyles
    {
        /// <summary>
        /// Default comment style using double slashes (//).
        /// </summary>
        public static readonly CommentStyleInfo Default = DoubleSlashes;

        /// <summary>
        /// C-style block comment style (/* ... */).
        /// </summary>
        public static readonly CommentStyleInfo CStyleBlock = new CommentStyleInfo(CommentStyle.CStyleBlock);

        /// <summary>
        /// Double slashes comment style (//).
        /// </summary>
        public static readonly CommentStyleInfo DoubleSlashes = new CommentStyleInfo(CommentStyle.DoubleSlashes);

        /// <summary>
        /// Hash-style single line comment style (#).
        /// </summary>
        public static readonly CommentStyleInfo Hash = new CommentStyleInfo(CommentStyle.Hash);

        /// <summary>
        /// Semicolon-style single line comment style (;).
        /// </summary>
        public static readonly CommentStyleInfo Semicolon = new CommentStyleInfo(CommentStyle.Semicolon);

        /// <summary>
        /// Quote-style single line comment style (').
        /// </summary>
        public static readonly CommentStyleInfo Quote = new CommentStyleInfo(CommentStyle.Quote);

        /// <summary>
        /// ML-style block comment style ((* ... *)).
        /// </summary>
        public static readonly CommentStyleInfo MLComment = new CommentStyleInfo(CommentStyle.MLComment);

        /// <summary>
        /// HTML-style block comment style (<!-- ... -->).
        /// </summary>
        public static readonly CommentStyleInfo HTML = new CommentStyleInfo(CommentStyle.HTML);

        /// <summary>
        /// SQL-style single line comment style (--).
        /// </summary>
        public static readonly CommentStyleInfo SQLLine = new CommentStyleInfo(CommentStyle.SQLLine);

        /// <summary>
        /// Dictionary mapping language names to their respective comment styles.
        /// </summary>
        private static readonly Dictionary<string, CommentStyleInfo> LanguageMap = new Dictionary<string, CommentStyleInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // C-style block comments primary
            { "csharp",CStyleBlock},
            { "c/c++", CStyleBlock },
            { "cpp", CStyleBlock },
            { "java", CStyleBlock },
            { "javascript", CStyleBlock },
            { "typescript", CStyleBlock },
            { "css", CStyleBlock },
            { "rust", CStyleBlock },
            { "go", CStyleBlock },
            { "swift", CStyleBlock },
            { "php", CStyleBlock },
            { "kotlin", CStyleBlock },
            { "scala", CStyleBlock },
            { "d", CStyleBlock },
            { "objective-c", CStyleBlock },
            
            // Single line comments primary
            { "python", Hash },
            { "ruby", Hash },
            { "perl", Hash },
            { "powershell", Hash },
            { "r", Hash },
            { "yaml", Hash },
            { "shell", Hash },
            { "basic", Quote },
            { "vb", Quote },
            { "fortran", Quote },
            { "lisp", Semicolon },
            { "scheme", Semicolon },
            { "fsharp", MLComment },
            
            // HTML-style comments
            { "html", HTML },
            { "xml", HTML },
            { "xaml", HTML },
            { "svg", HTML },
            { "aspx", HTML },
            
            // SQL variants
            { "sql", SQLLine },
            { "tsql", SQLLine },
            { "mysql", SQLLine },
            { "pgsql", SQLLine },
            { "plsql", SQLLine }
        };

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
            if (!LanguageMap.TryGetValue(language, out var styleInfo))
                return WrapInSingleLineComments(text, "//");

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
}