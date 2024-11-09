namespace FIGlet
{
    /// <summary>
    /// Represents a FIGfont used for rendering text in FIGlet style.
    /// </summary>
    public class FIGFont
    {
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
        /// Gets the number of comment lines in the FIGfont.
        /// </summary>
        public int CommentLines { get; private set; }

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

        /// <summary>
        /// Loads a FIGfont from a file.
        /// </summary>
        /// <param name="path">The path to the FIGfont file.</param>
        /// <returns>A <see cref="FIGFont"/> object.</returns>
        /// <exception cref="FormatException">Thrown when the file format is invalid.</exception>
        public static FIGFont LoadFromFile(string path)
        {
            var font = new FIGFont();
            var lines = File.ReadAllLines(path);

            // Parse header
            var header = lines[0];
            if (!header.StartsWith("flf2a"))
                throw new FormatException("Invalid FIGfont format");

            var headerParts = header.Split(' ');
            font.Signature = headerParts[0];
            font.HardBlank = headerParts[0].Substring(5, 1);
            font.Height = int.Parse(headerParts[1]);
            font.Baseline = int.Parse(headerParts[2]);
            font.MaxLength = int.Parse(headerParts[3]);
            font.OldLayout = int.Parse(headerParts[4]);
            font.CommentLines = int.Parse(headerParts[5]);
            font.PrintDirection = headerParts.Length > 6 ? int.Parse(headerParts[6]) : 0;
            font.FullLayout = headerParts.Length > 7 ? int.Parse(headerParts[7]) : 0;

            // Skip header and comments
            var currentLine = 1 + font.CommentLines;

            // Load required characters (ASCII 32-126)
            for (var charCode = 32; charCode <= 126; charCode++)
            {
                var charLines = new string[font.Height];
                for (var i = 0; i < font.Height; i++)
                {
                    charLines[i] = lines[currentLine + i]
                        .TrimEnd(['@', '\n', '\r']);
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

                var codePoint = int.Parse(codeLine.Split([' '], 2)[0]);
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

        public bool HasSmushingRule(SmushingRules rule)
        {
            return (SmushingRules & rule) == rule;
        }
    }
}