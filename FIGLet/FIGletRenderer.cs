using System.Text;
using System.Text.RegularExpressions;

namespace FIGLet;

/// <summary>
/// Class for rendering text using FIGLet fonts.
/// </summary>
public partial class FIGLetRenderer
{
    /// <summary>
    /// Characters used for hierarchy smushing.
    /// </summary>
    private const string HierarchyCharacters = "|/\\[]{}()<>";

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
    /// Gets the FIGFont used for rendering text.
    /// </summary>
    public FIGFont Font { get; }

    /// <summary>
    /// Renders the specified text using the given FIGFont and layout mode.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="font">The FIGFont to use for rendering the text.</param>
    /// <param name="mode">The layout mode to use for rendering. Default is LayoutMode.Smushing.</param>
    /// <param name="lineSeparator">The line separator to use. Default is "\r\n".</param>
    /// <returns>The rendered text as a string.</returns>
    public static string Render(string text, FIGFont font, LayoutMode mode = LayoutMode.Default, string lineSeparator = "\r\n")
    {
        var renderer = new FIGLetRenderer(font);
        return renderer.Render(text, mode, lineSeparator);
    }

    /// <summary>
    /// Renders the specified text using the FIGFont and layout mode.
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="mode">The layout mode to use for rendering. Default is LayoutMode.Smushing.</param>
    /// <param name="lineSeparator">The line separator to use. Default is "\r\n".</param>
    /// <returns>The rendered text as a string.</returns>
    public string Render(string text, LayoutMode mode = LayoutMode.Default, string lineSeparator = "\r\n")
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var ol = new StringBuilder[Font.Height];
        for (var i = 0; i < Font.Height; i++)
            ol[i] = new StringBuilder();

        mode = mode == LayoutMode.Default ? LayoutMode.Smushing : mode;

        foreach (var c in text)
        {
            // Debug.WriteLine($"Character being rendered: '{c}'");
            if (!Font.Characters.ContainsKey(c))
                continue;

            var charLines = Font.Characters[c];
            if (ol[0].Length == 0)
            {
                // First character, just append
                // Debug.WriteLine("First character, just append");
                for (var i = 0; i < Font.Height; i++)
                    ol[i].Append(charLines[i]);
                continue;
            }

            // Calculate overlap with previous character
            var overlap = int.MaxValue;
            for (var i = 0; i < Font.Height; i++)
                overlap = Math.Min(overlap, CalculateOverlap(ol[i].ToString(), charLines[i], mode));

            // Apply smushing rules
            for (var i = 0; i < Font.Height; i++)
            {
                if (overlap == 0)
                    ol[i].Append(charLines[i]);
                else
                    Smush(ol[i], charLines[i], overlap, mode);
            }
        }

        return string.Join(lineSeparator, ol.Select(x => x.Replace(Font.HardBlank[0], ' ').ToString()));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetRenderer"/> class with the specified FIGfont.
    /// </summary>
    /// <param name="font">The FIGfont to use for rendering text.</param>
    public FIGLetRenderer(FIGFont? font) => Font = font ?? FIGFont.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetRenderer"/> class using the default FIGfont.
    /// </summary>
    public FIGLetRenderer() : this(null) { }

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
        var m1 = (LastNonWhitespaceRegex()).Match(eol);
        var m2 = (FirstNonWhitespaceRegex()).Match(character);

        // Debug.WriteLine($"Debug - Line end: '{eol}', Char start: '{character}'");
        // Debug.WriteLine($"Debug - m1 success: {m1.Success}, index: {(m1.Success ? m1.Index : -1)}, value: '{(m1.Success ? m1.Value : "")}'");
        // Debug.WriteLine($"Debug - m2 success: {m2.Success}, index: {(m2.Success ? m2.Index : -1)}, value: '{(m2.Success ? m2.Value : "")}'");

        if (!m1.Success || !m2.Success)
        {
            // Debug.WriteLine($"Debug - overlap: {character.Length}");
            return character.Length;
        }

        var canSmush = CanSmush(m1.Value[0], m2.Value[0], mode);
        // Debug.WriteLine($"Debug - Can smush: {canSmush}");
        var overlapLength = canSmush ? Math.Max(eol.Length - m1.Index, m2.Index) + 1 : 0;
        overlapLength = Math.Min(overlapLength, character.Length);
        // Special case when we have oposing slashes
        if ((canSmush && m1.Value[0] == '/' && m2.Value[0] == '\\') || 
            (canSmush && m1.Value[0] == '\\' && m2.Value[0] == '/'))
            overlapLength = Math.Max(overlapLength - 1, 0);
        // Debug.WriteLine($"Debug - overlap: {overlapLength}");

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
        // Debug.WriteLine($"CanSmush called with c1='{c1}', c2='{c2}', mode={mode}");

        // Early return for kerning mode
        if (mode == LayoutMode.Kerning)
        {
            var result = c1 == c2 && c1 == ' ';
            // Debug.WriteLine($"Kerning check: {result}");
            return result;
        }

        // Early return for full size
        if (mode == LayoutMode.FullSize)
        {
            // Debug.WriteLine("FullSize mode - returning false");
            return false;
        }

        // Handle hardblanks first
        if (c1 == Font.HardBlank[0] || c2 == Font.HardBlank[0])
        {
            var result = Font.HasSmushingRule(SmushingRules.HardBlank);
            // Debug.WriteLine($"Hardblank check: {result}");
            return result;
        }

        // Handle spaces
        if (c1 == ' ' && c2 == ' ')
        {
            // Debug.WriteLine("Both spaces - returning true");
            return true;
        }
        if (c1 == ' ' || c2 == ' ')
        {
            // Debug.WriteLine("One space - returning true");
            return true;
        }

        // Rule 1: Equal Character Smushing
        if (Font.HasSmushingRule(SmushingRules.EqualCharacter) && c1 == c2)
        {
            // Debug.WriteLine("Equal character rule matched");
            return true;
        }

        // Rule 2: Underscore Smushing
        if (Font.HasSmushingRule(SmushingRules.Underscore))
        {
            if ((c1 == '_' && HierarchyCharacters.Contains(c2)) ||
                (c2 == '_' && HierarchyCharacters.Contains(c1)))
            {
                // Debug.WriteLine("Underscore rule matched");
                return true;
            }
        }

        // Rule 3: Hierarchy Smushing
        if (Font.HasSmushingRule(SmushingRules.Hierarchy))
        {
            var hierarchy = HierarchyCharacters;
            var rank1 = hierarchy.IndexOf(c1);
            var rank2 = hierarchy.IndexOf(c2);

            // Debug.WriteLine($"Hierarchy check - rank1: {rank1}, rank2: {rank2}");
            if (rank1 >= 0 && rank2 >= 0)
            {
                // Debug.WriteLine("Hierarchy rule matched");
                return true;
            }
        }

        // Rule 4: Opposite Pair Smushing
        if (Font.HasSmushingRule(SmushingRules.OppositePair))
        {
            if (oppositePairs.TryGetValue(c1, out var opposite) && opposite == c2)
            {
                // Debug.WriteLine("Opposite pair rule matched");
                return true;
            }
        }

        // Rule 5: Big X Smushing
        if (Font.HasSmushingRule(SmushingRules.BigX))
        {
            if (c1 == '>' && c2 == '<')
            {
                // Debug.WriteLine("Big X rule matched");
                return true;
            }
        }

        // Debug.WriteLine("No rules matched - returning false");
        return false;
    }

    /// <summary>
    /// Smushes the given character into the line with the specified overlap and layout mode.
    /// </summary>
    /// <param name="line">The line to smush the character into.</param>
    /// <param name="character">The character to smush into the line.</param>
    /// <param name="overlap">The number of characters to overlap.</param>
    /// <param name="mode">The layout mode to use for smushing.</param>
    private void Smush(StringBuilder line, string character, int overlap, LayoutMode mode)
    {
        var lineEnd = line.ToString().Substring(line.Length - overlap);
        line.Length -= overlap;
        if (mode == LayoutMode.Kerning)
        {
            line.Append(character);
            return;
        }
        for (var i = 0; i < overlap; i++)
            line.Append(SmushCharacters(lineEnd[i], character[i], mode));

        line.Append(character.Substring(overlap));
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
        {
            if (oppositePairs.TryGetValue(c1, out var opposite) && opposite == c2)
                return '|';
        }

        // Rule 5: Big X Smushing
        if (Font.HasSmushingRule(SmushingRules.BigX))
        {
            if ((c1 == '/' && c2 == '\\') || (c1 == '\\' && c2 == '/'))
                return '|';
            if ((c1 == '>' && c2 == '<'))
                return 'X';
        }

        // Rule 6: Hardblank Smushing
        if (Font.HasSmushingRule(SmushingRules.HardBlank))
        {
            if (c1 == Font.HardBlank[0] && c2 == Font.HardBlank[0])
                return Font.HardBlank[0];
        }

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
}