using System.Text;

namespace ByteForge.FIGLet;

/*
 *  ___ ___ ___ ___        _   
 * | __|_ _/ __| __|__ _ _| |_ 
 * | _| | | (_ | _/ _ \ ' \  _|
 * |_| |___\___|_|\___/_||_\__|
 *                             
 */
/// <summary>
/// Represents a FIGfont used for rendering text in FIGLet style.
/// </summary>
public class FIGFont
{
    /// <summary>
    /// Gets the default FIGfont.
    /// </summary>
    /// <remarks>
    /// If the default font is not already loaded, it loads the default font from the embedded resource.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the default FIGfont is not found.</exception>
    public static FIGFont Default
    {
        get
        {
            _default ??= LoadDefaultFont();
            return _default ?? throw new InvalidOperationException("Default FIGfont not found");
        }
    }
    private static FIGFont? _default;

    /// <summary>
    /// Gets the signature of the FIGfont.
    /// </summary>
    public string Signature { get; private set; } = "flf2a";

    /// <summary>
    /// Gets the hard blank character used in the FIGfont.
    /// </summary>
    public string HardBlank { get; private set; } = "#";

    /// <summary>
    /// Gets the height of the FIGfont characters.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the baseline of the FIGfont characters.
    /// </summary>
    public int Baseline { get; private set; }

    /// <summary>
    /// Gets the maximum length of the FIGfont characters.
    /// </summary>
    public int MaxLength { get; private set; }

    /// <summary>
    /// Gets the old layout mode of the FIGfont.
    /// </summary>
    public int OldLayout { get; private set; }

    /// <summary>
    /// Gets the print direction of the FIGfont.
    /// </summary>
    public int PrintDirection { get; private set; }

    /// <summary>
    /// Gets the full layout mode of the FIGfont.
    /// </summary>
    public int FullLayout { get; private set; }

    /// <summary>
    /// Gets the dictionary of characters in the FIGfont.
    /// </summary>
    public Dictionary<char, string[]> Characters { get; private set; } = [];

    /// <summary>
    /// Gets the smushing rules for the FIGfont.
    /// </summary>
    public SmushingRules SmushingRules { get; private set; } = SmushingRules.None;

    public string Comments { get; private set; } = "";

    /// <summary>
    /// Creates a FIGFont from a file.
    /// </summary>
    /// <param name="path">The path to the FIGfont file.</param>
    /// <returns>The FIGFont if the file is found and valid; otherwise, null.</returns>
    public static FIGFont? FromFile(String? path)
    {
        if (String.IsNullOrEmpty(path))
            return null;

        using var stream = File.OpenRead(path);
        return FromStream(stream);
    }

    /// <summary>
    /// Creates a FIGFont from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the FIGfont data.</param>
    /// <returns>The FIGFont if the stream is valid; otherwise, null.</returns>
    public static FIGFont? FromStream(Stream? stream)
    {
        if (stream == null)
            return null;

        using var zipStream = new FIGFontStream(stream);
        using var reader = new StreamReader(zipStream, Encoding.UTF8, true);
        return FromReader(reader);
    }

    /// <summary>
    /// Creates a FIGFont from a text reader.
    /// </summary>
    /// <param name="reader">The text reader containing the FIGfont data.</param>
    /// <returns>The FIGFont if the reader is valid; otherwise, null.</returns>
    public static FIGFont? FromReader(TextReader? reader)
    {
        if (reader == null)
            return null;

        var lines = reader.ReadToEnd().Split('\n');
        return FromLines(lines);
    }

    /// <summary>
    /// Creates a FIGFont from an array of lines.
    /// </summary>
    /// <param name="lines">The lines containing the FIGfont data.</param>
    /// <returns>The FIGFont if the lines are valid; otherwise, null.</returns>
    public static FIGFont? FromLines(string[]? lines)
    {
        if (lines == null || lines.Length == 0)
            return null;

        var font = new FIGFont();

        // Parse header
        var header = lines[0].Trim();
        if (!header.StartsWith("flf2a"))
            throw new FormatException("Invalid FIGfont format");

        var headerParts = header.Split(' ');
        font.Signature = headerParts[0];
        font.HardBlank = headerParts[0].Substring(5, 1);
        font.Height = int.Parse(headerParts[1]);
        font.Baseline = int.Parse(headerParts[2]);
        font.MaxLength = int.Parse(headerParts[3]);
        font.OldLayout = int.Parse(headerParts[4]);
        var commentLines = int.Parse(headerParts[5]);
        if (headerParts.Length > 6 && int.TryParse(headerParts[6], out var printDirection))
            font.PrintDirection = printDirection;
        if (headerParts.Length > 7 && int.TryParse(headerParts[7], out var fullLayout))
            font.FullLayout = fullLayout;

        font.Comments = string.Join("\n", lines.Skip(1).Take(commentLines));

        // Skip header and comments
        var currentLine = 1 + commentLines;

        // Load required characters (ASCII 32-126)
        for (var charCode = 32; charCode <= 126; charCode++)
        {
            var charLines = new string[font.Height];
            for (var i = 0; i < font.Height; i++)
            {
                charLines[i] = lines[currentLine + i].TrimEnd(['@', '\n', '\r']);

                /*
                 * A special case when FIG font designer uses a different hard blank character
                 * than the one specified in the header. In this case, the "hard blank" character
                 * will not be honored. This is a workaround to handle such cases.
                 */
                if (charLines[i].EndsWith("#") && font.HardBlank != "#")
                    charLines[i] = charLines[i].TrimEnd('#');
            }
            font.Characters.Add((char)charCode, charLines);
            currentLine += font.Height;
        }

        // Continue reading additional characters if they exist
        while (currentLine + font.Height <= lines.Length)
        {
            // Try to parse the code point line
            var codeLine = lines[currentLine];
            if (string.IsNullOrWhiteSpace(codeLine) || !char.IsDigit(codeLine[0]))
                break;

            var codePoint = ParseInt(codeLine.Split([' '], 2)[0]);
            currentLine++; // Move past the code point line

            var charLines = new string[font.Height];
            for (var i = 0; i < font.Height; i++)
            {
                if (currentLine + i >= lines.Length) break;
                charLines[i] = lines[currentLine + i]
                    .TrimEnd(['@', '\n', '\r']);
            }
            font.Characters.Add((char)codePoint, charLines);
            currentLine += font.Height;
        }

        // Parse the layout parameters to determine smushing rules
        ParseLayoutParameters(font);

        return font;
    }

    /// <summary>
    /// Parses a string representation of a number into an integer.
    /// </summary>
    /// <param name="text">The string containing the number to parse. 
    /// It can be in decimal, hexadecimal (prefixed with "0x"), 
    /// binary (prefixed with "0b"), or octal (prefixed with "0").</param>
    /// <returns>The parsed integer value. Returns 0 if the input is null.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not in a valid format.</exception>
    /// <exception cref="OverflowException">Thrown when the input string represents a number less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.</exception>
    private static int ParseInt(string text)
    {
        if (text == null)
            return 0;

        if (text.StartsWith("0x"))
            return int.Parse(text.Substring(2), System.Globalization.NumberStyles.HexNumber);
        if (text.StartsWith("0b"))
            return Convert.ToInt32(text.Substring(2), 2);
        if (text.StartsWith("0"))
            return Convert.ToInt32(text, 8);
        return int.Parse(text);
    }

    /// <summary>
    /// Loads the default FIGfont from the embedded resource.
    /// </summary>
    /// <returns>The default FIGfont if found; otherwise, null.</returns>
    private static FIGFont? LoadDefaultFont()
    {
        var fontName = typeof(FIGFont).Assembly
            .GetManifestResourceNames()
            .First(name => name.EndsWith("small.flf", StringComparison.OrdinalIgnoreCase));
        using var stream = typeof(FIGFont).Assembly.GetManifestResourceStream(fontName);
        return FromStream(stream);
    }

    /// <summary>
    /// Parses the layout parameters to determine smushing rules for the FIGfont.
    /// </summary>
    /// <param name="font">The FIGfont object to parse layout parameters for.</param>
    private static void ParseLayoutParameters(FIGFont font)
    {
        // First, determine if we should use full_layout or old_layout
        int layoutMask;

        if (font.FullLayout > 0)
        {
            // Full layout is present
            layoutMask = font.FullLayout;

            // In full layout mode:
            // Bit 0: Horizontal smushing (smush vs. kern)
            // Bit 1-6: Specific smushing rules
            // Bit 7-15: Reserved for future use

            // Check if horizontal smushing is enabled
            var horizontalSmushingEnabled = (layoutMask & 1) == 1;
            if (!horizontalSmushingEnabled)
            {
                // If horizontal smushing is not enabled, no rules apply
                font.SmushingRules = SmushingRules.None;
                return;
            }

            // Extract just the rules part (bits 1-6)
            layoutMask = (layoutMask >> 1) & 0x3F;
        }
        else
        {
            // Use old layout
            layoutMask = font.OldLayout;

            // In old layout, -1 means no smushing
            if (layoutMask == -1)
            {
                font.SmushingRules = SmushingRules.None;
                return;
            }

            // In old layout, 0 means kerning
            if (layoutMask == 0)
            {
                font.SmushingRules = SmushingRules.None;
                return;
            }

            // For positive values, extract the rules
            if (layoutMask > 0)
            {
                // Convert old layout to new layout rules format
                layoutMask &= 0x3F;
            }
        }

        // Apply the final smushing rules
        font.SmushingRules = (SmushingRules)layoutMask;
    }

    /// <summary>
    /// Determines if the FIGfont has a specific smushing rule.
    /// </summary>
    /// <param name="rule">The smushing rule to check for.</param>
    /// <returns>True if the smushing rule is present; otherwise, false.</returns>
    public bool HasSmushingRule(SmushingRules rule) => (SmushingRules & rule) == rule;
}