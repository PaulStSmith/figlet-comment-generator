using System.Text;
using System.Text.RegularExpressions;

namespace ByteForge.FIGLet;

/*
 *  ___ ___ ___ _        _   ___             _                 
 * | __|_ _/ __| |   ___| |_| _ \___ _ _  __| |___ _ _ ___ _ _ 
 * | _| | | (_ | |__/ -_)  _|   / -_) ' \/ _` / -_) '_/ -_) '_|
 * |_| |___\___|____\___|\__|_|_\___|_||_\__,_\___|_| \___|_|  
 *                                                             
 */

/// <summary>
/// Class for rendering text using FIGFonts.
/// </summary>
public partial class FIGLetRenderer
{
    /// <summary>
    /// Characters used for hierarchy smushing.
    /// </summary>
    private const string HierarchyCharacters = "|/\\[]{}()<>";

    /// <summary>
    /// ANSI color reset code for terminal output.
    /// </summary>
    private const string ANSIColorResetCode = "\u001b[0m";

    /// <summary>
    /// Dictionary of opposite character pairs for smushing.
    /// </summary>
    private static readonly Dictionary<char, char> oppositePairs = new()
        {
            {'[', ']'}, {']', '['},
            {'{', '}'}, {'}', '{'},
            {'(', ')'}, {')', '('},
            {'<', '>'}, {'>', '<'}
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetRenderer"/> class with the default FIGFont.
    /// </summary>
    public FIGLetRenderer() : this(FIGFont.Default) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetRenderer"/> class with the specified FIGFont.
    /// </summary>
    /// <param name="font">The FIGFont to use for rendering text.</param>
    public FIGLetRenderer(FIGFont? font) => Font = font ?? throw new ArgumentNullException(nameof(font));

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetRenderer"/> class with the specified FIGFont, layout mode, line separator, and ANSI color usage.
    /// </summary>
    /// <param name="font">The FIGFont to use for rendering text.</param>
    /// <param name="mode">The layout mode to use for rendering.</param>
    /// <param name="lineSeparator">The line separator to use.</param>
    /// <param name="useANSIColors">Whether to use ANSI colors during rendering.</param>
    /// <param name="paragraphMode">Whether to use paragraph mode for rendering.</param>
    public FIGLetRenderer(FIGFont? font, LayoutMode mode = LayoutMode.Default, string? lineSeparator = null, bool useANSIColors = false, bool paragraphMode = true) : this(font)
    {
        LayoutMode = mode;
        LineSeparator = lineSeparator ?? Environment.NewLine;
        UseANSIColors = useANSIColors;
        ParagraphMode = paragraphMode;
    }

    /// <summary>
    /// Gets the FIGFont used for rendering text.
    /// </summary>
    public FIGFont Font { get; }

    /// <summary>
    /// Gets or sets the layout mode for rendering.
    /// </summary>
    public LayoutMode LayoutMode { get; set; } = LayoutMode.Default;

    /// <summary>
    /// Gets or sets the line separator used for rendering.
    /// </summary>
    public string LineSeparator { get; set; } = Environment.NewLine;

    /// <summary>
    /// Gets or sets a value indicating whether to use ANSI colors during rendering.
    /// </summary>
    public bool UseANSIColors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to use paragraph mode for rendering.
    /// </summary>
    public bool ParagraphMode { get; set; } = true;

    /// <summary>
    /// Renders the specified text using the given FIGFont and layout mode.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="font">The FIGFont to use for rendering the text.</param>
    /// <param name="mode">The layout mode to use for rendering. Default is LayoutMode.Smushing.</param>
    /// <param name="lineSeparator">The line separator to use. Default is "\r\n".</param>
    /// <param name="useANSIColors">Whether to process and preserve ANSI color codes. Default is false.</param>
    /// <param name="paragraphMode">Whether to use paragraph mode for rendering. Default is true.</param>
    /// <returns>The rendered text as a string.</returns>
    public static string Render(string text, FIGFont? font = null, LayoutMode mode = LayoutMode.Default, string? lineSeparator = null, bool useANSIColors = false, bool paragraphMode = true)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        font ??= FIGFont.Default;
        lineSeparator ??= Environment.NewLine;
        var renderer = new FIGLetRenderer(font, mode, lineSeparator, useANSIColors, paragraphMode);
        return renderer.Render(text);
    }

    /// <summary>
    /// Renders the specified text using the current FIGFont and settings.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <returns>The rendered text as a string.</returns>
    public string Render(string text){
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (ParagraphMode)
        {
            // Split the text into paragraphs based on line breaks
            var emptlyLine = (new string('*', Font.Height)).Replace("*", LineSeparator);
            var paragraphs = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var renderedParagraphs = new StringBuilder();
            foreach (var paragraph in paragraphs)
                if (string.IsNullOrWhiteSpace(paragraph))
                    // Add empty lines for paragraph separation
                    renderedParagraphs.Append(emptlyLine);
                else
                    // Render the non-empty paragraph
                    renderedParagraphs.Append(RenderLine(paragraph));
            return renderedParagraphs.ToString();
        }

        text = text.Replace("\r", "").Replace("\n", " ");
        return RenderLine(text);
    }

    /// <summary>
    /// Renders a single line of text using the current FIGFont and settings.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <returns>The rendered line as a string.</returns>
    private string RenderLine(string text) {

        var mode = LayoutMode;
        var lineSeparator = LineSeparator;
        var useANSIColors = UseANSIColors;

        var ol = new StringBuilder[Font.Height];
        for (var i = 0; i < Font.Height; i++)
            ol[i] = new StringBuilder();

        // Create ANSI processor if colors are enabled
        var ansiProcessor = useANSIColors ? new ANSIProcessor() : null;

        // Process each character in the input text
        var charIndex = 0;
        var plainText = new StringBuilder();
        var colorDict = new Dictionary<int, string>();

        if (ansiProcessor != null)
        {
            // First pass: Extract ANSI sequences and build plain text
            for (charIndex = 0; charIndex < text.Length; charIndex++)
            {
                var c = text[charIndex];
                var isAnsiCode = ansiProcessor.ProcessCharacter(c);

                if (!isAnsiCode)
                {
                    if (!string.IsNullOrEmpty(ansiProcessor.CurrentColorSequence))
                    {
                        // Append the current color sequence to the plain text
                        colorDict[plainText.Length] = ansiProcessor.CurrentColorSequence;
                        ansiProcessor.ResetColorState();
                    }

                    // Skip characters not in the font
                    if (Font.Characters.ContainsKey(c))
                        plainText.Append(c);
                }

            }


            // Reset for second pass
            text = plainText.ToString();
        }

        // Reverse the text for RTL fonts while preserving surrogate pairs
        if (Font.PrintDirection == 1)
        {
            // Use StringInfo to properly handle surrogate pairs and combining characters
            var textElements = new System.Globalization.StringInfo(text);
            var textElementCount = textElements.LengthInTextElements;

            // Build the reversed string
            var reversedText = new StringBuilder(text.Length);
            for (int i = textElementCount - 1; i >= 0; i--)
            {
                reversedText.Append(textElements.SubstringByTextElements(i, 1));
            }

            // Also reverse the color codes
            var reversedColorDict = new Dictionary<int, string>();
            foreach (var kvp in colorDict)
            {
                // Calculate the new position based on text elements, not raw chars
                int newPos = plainText.Length - kvp.Key - 1;
                reversedColorDict[newPos] = kvp.Value;
            }

            text = reversedText.ToString();
            colorDict = reversedColorDict;
        }

        // Second pass: Render the text with FIGfont
        charIndex = 0;
        for (var i = 0; i < text.Length; i++)
        {
            int codePoint;

            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                // This is a surrogate pair
                codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                i++; // Skip the low surrogate on the next iteration
            }
            else
            {
                // Regular BMP character
                codePoint = (int)text[i];
            }

            // Skip characters not in the font
            if (!useANSIColors && Font.Characters.ContainsKey(codePoint))
                plainText.Append(char.ConvertFromUtf32(codePoint));

            var charLines = Font.Characters[codePoint];
            var colorCode = string.Empty;
            _ = colorDict.TryGetValue(charIndex++, out colorCode);

            if (ol[0].Length == 0)
            {
                // First character, just append
                for (var lineIndex = 0; lineIndex < Font.Height; lineIndex++)
                {
                    // Add color code if needed
                    if (useANSIColors)
                        ol[lineIndex].Append(colorCode);
                    ol[lineIndex].Append(charLines[lineIndex]);
                }
            }
            else
            {
                // Calculate overlap with previous character
                var overlap = int.MaxValue;
                for (var lineIndex = 0; lineIndex < Font.Height; lineIndex++)
                    overlap = Math.Min(overlap, CalculateOverlap(ol[lineIndex].ToString(), charLines[lineIndex], mode));

                // Apply smushing rules
                for (var lineIndex = 0; lineIndex < Font.Height; lineIndex++)
                {
                    if (overlap == 0)
                    {
                        if (useANSIColors)
                            ol[lineIndex].Append(colorCode);
                        ol[lineIndex].Append(charLines[lineIndex]);
                    }
                    else
                    {
                        // Use enhanced Smush method with color support
                        Smush(ol[lineIndex], charLines[lineIndex], overlap, mode, colorCode ?? string.Empty);
                    }
                }
            }
        }

        return string.Join(lineSeparator, ol.Select(x => x.Replace(Font.HardBlank[0], ' ')
                                            .Append(ansiProcessor != null ? ANSIColorResetCode : string.Empty)
                                            .ToString()));
    }

    /// <summary>
    /// Smushes two lines together based on the specified layout mode.
    /// </summary>
    /// <param name="line">The first line.</param>
    /// <param name="character">The second line.</param>
    /// <param name="overlap">The number of characters to overlap.</param>
    /// <param name="mode">The layout mode to use for smushing.</param>
    /// <param name="colorCode">The current ANSI color state.</param>
    private void Smush(StringBuilder line, string character, int overlap, LayoutMode mode, string colorCode = "")
    {
        var lineEnd = line.ToString().Substring(line.Length - overlap);
        line.Length -= overlap;


        if (mode == LayoutMode.Kerning)
        {
            line.Append(colorCode);
            line.Append(character);
            return;
        }

        // Apply smushing rules character by character in the overlap area
        for (var i = 0; i < overlap; i++)
            line.Append(SmushCharacters(lineEnd[i], character[i], mode));

        // Append the remaining part of the character (after the overlap)
        line.Append(colorCode);
        line.Append(character.Substring(overlap));
    }

    /// <summary>
    /// Calculates the number of characters that can be overlapped between two lines based on the specified layout mode.
    /// </summary>
    /// <param name="line">The first line.</param>
    /// <param name="character">The second line.</param>
    /// <param name="mode">The layout mode to use for calculating overlap.</param>
    /// <returns>The number of characters that can be overlapped.</returns>
    private int CalculateOverlap(string line, string character, LayoutMode mode)
    {
        if (mode == LayoutMode.FullSize)
            return 0;

        var eol = line.Length < character.Length ? line : line.Substring(line.Length - character.Length);
        var m1 = LastNonWhitespaceRegex().Match(eol);
        var m2 = FirstNonWhitespaceRegex().Match(character);

        if (!m1.Success || !m2.Success)
            return character.Length;

        var canSmush = CanSmush(m1.Value[0], m2.Value[0], mode);
        var overlapLength = canSmush ? Math.Max(eol.Length - m1.Index, m2.Index) + 1 : 0;
        overlapLength = Math.Min(overlapLength, character.Length);
        // Special case when we have opposing slashes
        if ((canSmush && m1.Value[0] == '/' && m2.Value[0] == '\\') ||
            (canSmush && m1.Value[0] == '\\' && m2.Value[0] == '/'))
            overlapLength = Math.Max(overlapLength - 1, 0);

        return overlapLength;
    }

    /// <summary>
    /// Determines if two characters can be smushed together based on the specified layout mode.
    /// </summary>
    /// <param name="c1">The first character.</param>
    /// <param name="c2">The second character.</param>
    /// <param name="mode">The layout mode to use for smushing.</param>
    /// <returns>True if the characters can be smushed together; otherwise, false.</returns>
    private bool CanSmush(char c1, char c2, LayoutMode mode)
    {
        // Early return for kerning mode
        if (mode == LayoutMode.Kerning)
            return c1 == c2 && c1 == ' ';

        // Early return for full size
        if (mode == LayoutMode.FullSize)
            return false;

        // Handle hardblanks first
        if (c1 == Font.HardBlank[0] || c2 == Font.HardBlank[0])
            return Font.HasSmushingRule(SmushingRules.HardBlank);

        // Handle spaces
        if (c1 == ' ' && c2 == ' ') return true;
        if (c1 == ' ' || c2 == ' ') return true;

        // Rule 1: Equal Character Smushing
        if (Font.HasSmushingRule(SmushingRules.EqualCharacter) && c1 == c2)
            return true;

        // Rule 2: Underscore Smushing
        if (Font.HasSmushingRule(SmushingRules.Underscore))
            if ((c1 == '_' && HierarchyCharacters.Contains(c2)) || (c2 == '_' && HierarchyCharacters.Contains(c1)))
                return true;

        // Rule 3: Hierarchy Smushing
        if (Font.HasSmushingRule(SmushingRules.Hierarchy))
        {
            var hierarchy = HierarchyCharacters;
            var rank1 = hierarchy.IndexOf(c1);
            var rank2 = hierarchy.IndexOf(c2);

            if (rank1 >= 0 && rank2 >= 0)
                return true;
        }

        // Rule 4: Opposite Pair Smushing
        if (Font.HasSmushingRule(SmushingRules.OppositePair))
            if (oppositePairs.TryGetValue(c1, out var opposite) && opposite == c2)
                return true;

        // Rule 5: Big X Smushing
        if (Font.HasSmushingRule(SmushingRules.BigX))
            if (c1 == '>' && c2 == '<')
                return true;

        return false;
    }

    /// <summary>
    /// Smushes two characters together based on the specified layout mode.
    /// </summary>
    /// <param name="c1">The first character.</param>
    /// <param name="c2">The second character.</param>
    /// <param name="mode">The layout mode to use for smushing.</param>
    /// <returns>The resulting smushed character.</returns>
    private char SmushCharacters(char c1, char c2, LayoutMode mode)
    {
        // Rule 0: Universal smushing just picks the first character
        if (mode == LayoutMode.Kerning)
            return c1;

        // Handle spaces
        if (c1 == ' ' && c2 == ' ') return ' ';
        if (c1 == ' ') return c2;
        if (c2 == ' ') return c1;

        // Handle hardblanks first
        if (c1 == Font.HardBlank[0] || c2 == Font.HardBlank[0])
        {
            if (Font.HasSmushingRule(SmushingRules.HardBlank))
                return Font.HardBlank[0];
            return c1;
        }

        // Rule 1: Equal Character Smushing
        if (Font.HasSmushingRule(SmushingRules.EqualCharacter) && c1 == c2)
            return c1;

        // Rule 2: Underscore Smushing
        if (Font.HasSmushingRule(SmushingRules.Underscore))
        {
            if (c1 == '_' && HierarchyCharacters.Contains(c2)) return c2;
            if (c2 == '_' && HierarchyCharacters.Contains(c1)) return c1;
        }

        // Rule 3: Hierarchy Smushing
        if (Font.HasSmushingRule(SmushingRules.Hierarchy))
        {
            var hierarchy = HierarchyCharacters;
            var rank1 = hierarchy.IndexOf(c1);
            var rank2 = hierarchy.IndexOf(c2);

            if (rank1 >= 0 && rank2 >= 0)
                return hierarchy[Math.Max(rank1, rank2)];
        }

        // Rule 4: Opposite Pair Smushing
        if (Font.HasSmushingRule(SmushingRules.OppositePair))
            if (oppositePairs.TryGetValue(c1, out var opposite) && opposite == c2)
                return '|';

        // Rule 5: Big X Smushing
        if (Font.HasSmushingRule(SmushingRules.BigX))
        {
            if ((c1 == '/' && c2 == '\\') || (c1 == '\\' && c2 == '/'))
                return '|';
            if (c1 == '>' && c2 == '<')
                return 'X';
        }

        // Rule 6: Hardblank Smushing
        if (Font.HasSmushingRule(SmushingRules.HardBlank))
            if (c1 == Font.HardBlank[0] && c2 == Font.HardBlank[0])
                return Font.HardBlank[0];

        // If no smushing rules apply or are enabled, return the first character
        return c1;
    }

    /// <summary>
    /// A regex to match the last non-whitespace character in a string.
    /// </summary>
    /// <returns>A regex pattern to match the last non-whitespace character.</returns>
    private static Regex LastNonWhitespaceRegex() => new(@"\S(?=\s*$)");

    /// <summary>
    /// A regex to match the first non-whitespace character in a string.
    /// </summary>
    /// <returns>A regex pattern to match the first non-whitespace character.</returns>
    private static Regex FirstNonWhitespaceRegex() => new(@"(?<=^|\s*)\S");

    /// <summary>
    /// Detects and processes ANSI escape sequences during rendering.
    /// </summary>
    private class ANSIProcessor
    {
        // Track if we're currently inside an escape sequence
        private bool _inEscapeSequence = false;

        // Buffer to store the current escape sequence
        private readonly StringBuilder _escapeBuffer = new();

        // List of ANSI sequence terminators that are NOT color-related
        private static readonly HashSet<char> NonColorTerminators =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'f',    // Cursor movement
            'J', 'K',                                       // Screen clearing
            'S', 'T',                                       // Scrolling
            's', 'u',                                       // Cursor position
            'n', 'h', 'l', 'i', 'r', 't', '@',              // Various controls
            'P', 'X', 'L', 'M'                              // More controls
        ];

        // Current active color sequence to maintain color state
        public string CurrentColorSequence { get; private set; } = string.Empty;

        /// <summary>
        /// Processes a character, detecting and handling ANSI escape sequences.
        /// </summary>
        /// <param name="c">Character to process.</param>
        /// <returns>True if the character was part of an escape sequence, false otherwise.</returns>
        public bool ProcessCharacter(char c)
        {
            // Start of escape sequence
            if (c == '\u001b') // ESC character (27)
            {
                _inEscapeSequence = true;
                _escapeBuffer.Clear();
                _escapeBuffer.Append(c);
                return true;
            }

            // Continue building escape sequence
            if (_inEscapeSequence)
            {
                _escapeBuffer.Append(c);

                // Check for CSI sequences (Control Sequence Introducer)
                if (_escapeBuffer.Length == 2 && c != '[')
                {
                    // Not a CSI sequence, reset
                    _inEscapeSequence = false;
                    return true;
                }

                // Complete sequence detection
                if (_escapeBuffer.Length >= 3 &&
                    ((c >= 0x40 && c <= 0x7E) || NonColorTerminators.Contains(c) || c == 'm'))
                {
                    // Sequence is complete
                    _inEscapeSequence = false;
                    var sequence = _escapeBuffer.ToString();

                    // If color sequence (ends with 'm'), keep it
                    if (c == 'm')
                        CurrentColorSequence = sequence;

                    return true;
                }

                return true; // Part of escape sequence, but not complete
            }

            return false; // Not part of escape sequence
        }

        /// <summary>
        /// Gets the current ANSI color state.
        /// </summary>
        /// <returns>The current color sequence or empty string if no color is set.</returns>
        public string GetColorState()
        {
            return CurrentColorSequence;
        }

        /// <summary>
        /// Resets the color state.
        /// </summary>
        public void ResetColorState()
        {
            CurrentColorSequence = string.Empty;
        }
    }
}