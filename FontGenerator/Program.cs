using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace ByteForge.BitmapToFigFont;

class Program
{
    // Hardcoded filenames
    private const string INPUT_BITMAP = "charmap-inverted.png";
    private const string OUTPUT_FIGFONT = "bmp-inverted.flf";
    private const string OUTPUT_CONDENSED_FIGFONT = "bmp-inverted-condensed.flf";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting bitmap to figfont conversion...");

            // Process the bitmap and generate both figfont files
            ProcessBitmap();

            Console.WriteLine("Conversion complete!");
            Console.WriteLine($"Regular figfont file created: {OUTPUT_FIGFONT}");
            Console.WriteLine($"Condensed figfont file created: {OUTPUT_CONDENSED_FIGFONT}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes the bitmap to generate both standard and condensed figfont files.
    /// </summary>
    static void ProcessBitmap()
    {
        // Load the bitmap
        using var bitmap = new Bitmap(INPUT_BITMAP);

        // Verify dimensions
        if (bitmap.Width != 128 || bitmap.Height != 128)
        {
            throw new ArgumentException($"Expected 128x128 bitmap, got {bitmap.Width}x{bitmap.Height}");
        }

        // Extract character data from bitmap
        var pixels = ExtractPixels(bitmap);

        // Extract character data for all 256 characters
        var standardCharData = new string[256][];
        var condensedCharData = new string[256][];

        // Process each character (16x16 grid of 8x8 characters)
        for (var charY = 0; charY < 16; charY++)
        {
            for (var charX = 0; charX < 16; charX++)
            {
                var charIndex = charY * 16 + charX;
                var baseX = charX * 8;
                var baseY = charY * 8;

                // Process standard character data (2-pixel representation)
                standardCharData[charIndex] = ExtractStandardCharacter(pixels, baseX, baseY);

                // Process condensed character data (4-pixel representation)
                condensedCharData[charIndex] = ExtractCondensedCharacter(pixels, baseX, baseY);
            }
        }

        // Generate standard figfont (2-pixel representation)
        GenerateFigFont(standardCharData, OUTPUT_FIGFONT, "Each character represents 2 vertical pixels", 4, 8);

        // Generate condensed figfont (4-pixel representation)
        GenerateFigFont(condensedCharData, OUTPUT_CONDENSED_FIGFONT, "Each character represents 4 pixels (2x2 grid)", 4, 4);
    }

    /// <summary>
    /// Extracts pixel data from the bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap to extract pixel data from.</param>
    /// <returns>A 2D array of boolean values representing the pixel data.</returns>
    static bool[,] ExtractPixels(Bitmap bitmap)
    {
        var pixels = new bool[128, 128]; // true for black, false for white

        for (var y = 0; y < 128; y++)
        {
            for (var x = 0; x < 128; x++)
            {
                // In most bitmaps, black is represented by Color.Black (0,0,0)
                // and white by Color.White (255,255,255)
                var pixelColor = bitmap.GetPixel(x, y);
                pixels[x, y] = IsPixelBlack(pixelColor);
            }
        }

        return pixels;
    }

    /// <summary>
    /// Determines if a pixel is black based on its color.
    /// </summary>
    /// <param name="color">The color of the pixel.</param>
    /// <returns>True if the pixel is black, otherwise false.</returns>
    static bool IsPixelBlack(Color color)
    {
        // Simple threshold to determine if a pixel is black
        // This might need adjustment based on your bitmap
        var threshold = 128;
        var brightness = (color.R + color.G + color.B) / 3;
        return brightness < threshold;
    }

    /// <summary>
    /// Extracts a character using the standard 2-pixel representation.
    /// </summary>
    /// <param name="pixels">The pixel data from the bitmap.</param>
    /// <param name="baseX">The base X coordinate of the character in the bitmap.</param>
    /// <param name="baseY">The base Y coordinate of the character in the bitmap.</param>
    /// <returns>An array of strings representing the character's figfont lines.</returns>
    static string[] ExtractStandardCharacter(bool[,] pixels, int baseX, int baseY)
    {
        var lines = new string[4];

        for (var line = 0; line < 4; line++)
        {
            var sb = new StringBuilder();

            for (var x = 0; x < 8; x++)
            {
                var topPixel = pixels[baseX + x, baseY + line * 2];
                var bottomPixel = pixels[baseX + x, baseY + line * 2 + 1];

                var blockChar = GetBlockChar(topPixel, bottomPixel);
                sb.Append(blockChar);
            }

            // Add the end-of-line marker
            lines[line] = sb.ToString() + "@";
        }

        return lines;
    }

    /// <summary>
    /// Maps a 2-pixel pattern to a block character.
    /// </summary>
    /// <param name="topPixel">The top pixel value.</param>
    /// <param name="bottomPixel">The bottom pixel value.</param>
    /// <returns>The corresponding block character.</returns>
    static char GetBlockChar(bool topPixel, bool bottomPixel)
    {
        // Map the 2-pixel pattern to a block character
        if (topPixel && bottomPixel) return '█'; // Full block
        if (topPixel && !bottomPixel) return '▀'; // Upper half block
        if (!topPixel && bottomPixel) return '▄'; // Lower half block
        return ' '; // Space (both white)
    }

    /// <summary>
    /// Extracts a character using the condensed 4-pixel representation.
    /// </summary>
    /// <param name="pixels">The pixel data from the bitmap.</param>
    /// <param name="baseX">The base X coordinate of the character in the bitmap.</param>
    /// <param name="baseY">The base Y coordinate of the character in the bitmap.</param>
    /// <returns>An array of strings representing the character's figfont lines.</returns>
    static string[] ExtractCondensedCharacter(bool[,] pixels, int baseX, int baseY)
    {
        var lines = new string[4];

        for (var line = 0; line < 4; line++)
        {
            var sb = new StringBuilder();

            for (var col = 0; col < 4; col++)
            {
                var x = col * 2;
                var y = line * 2;

                var topLeft = pixels[baseX + x, baseY + y];
                var topRight = pixels[baseX + x + 1, baseY + y];
                var bottomLeft = pixels[baseX + x, baseY + y + 1];
                var bottomRight = pixels[baseX + x + 1, baseY + y + 1];

                var quadChar = GetQuadrantChar(topLeft, topRight, bottomLeft, bottomRight);
                sb.Append(quadChar);
            }

            // Add the end-of-line marker
            lines[line] = sb.ToString() + "@";
        }

        return lines;
    }

    /// <summary>
    /// Maps a 2x2 pixel pattern to a block character.
    /// </summary>
    /// <param name="topLeft">The top-left pixel value.</param>
    /// <param name="topRight">The top-right pixel value.</param>
    /// <param name="bottomLeft">The bottom-left pixel value.</param>
    /// <param name="bottomRight">The bottom-right pixel value.</param>
    /// <returns>The corresponding block character.</returns>
    static char GetQuadrantChar(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        // Map the 2x2 pixel pattern to a block character
        var pattern = (topLeft ? 8 : 0) | (topRight ? 4 : 0) | (bottomLeft ? 2 : 0) | (bottomRight ? 1 : 0);

        return pattern switch
        {
            0 => ' ',      // All white
            1 => '▗',      // Bottom right only
            2 => '▖',      // Bottom left only
            3 => '▄',      // Bottom half
            4 => '▝',      // Top right only
            5 => '▐',      // Right half
            6 => '▞',      // Diagonal (top-right and bottom-left)
            7 => '▟',      // All except top-left
            8 => '▘',      // Top left only
            9 => '▚',      // Diagonal (top-left and bottom-right)
            10 => '▌',     // Left half
            11 => '▙',     // All except top-right
            12 => '▀',     // Top half
            13 => '▜',     // All except bottom-left
            14 => '▛',     // All except bottom-right
            15 => '█',     // All black
            _ => '?',      // Should never reach here
        };
    }

    /// <summary>
    /// Generates a figfont file from character data.
    /// </summary>
    /// <param name="charData">The character data to write to the figfont file.</param>
    /// <param name="outputFile">The name of the output figfont file.</param>
    /// <param name="description">A description of the font to include in the header.</param>
    /// <param name="height">The height of the font in lines.</param>
    /// <param name="maxWidth">The maximum width of a character in the font.</param>
    static void GenerateFigFont(string[][] charData, string outputFile, string description, int height, int maxWidth)
    {
        using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);

        // Write the figfont header
        // flf2a$ <height> <baseline> <max_width> <old_layout> <comment_lines>
        writer.WriteLine($"flf2a$ {height} {height} {maxWidth} -1 4");
        writer.WriteLine("Generated by Bitmap to FIGfont Converter");
        writer.WriteLine(description);
        writer.WriteLine("Characters are 8x8 pixels from a 128x128 bitmap");
        writer.WriteLine("Characters are arranged in a 16x16 grid in the bitmap");

        // First output the required ASCII characters (32-126) without character codes
        for (var asciiCode = 32; asciiCode <= 126; asciiCode++)
        {
            foreach (var line in charData[asciiCode])
            {
                writer.WriteLine(line);
            }
        }

        // Now output any additional characters (0-31, 127-255) with their character codes
        for (var charIndex = 0; charIndex < 256; charIndex++)
        {
            // Skip the standard ASCII characters we've already output
            if (charIndex >= 32 && charIndex <= 126)
                continue;

            // Write the character code before the character
            writer.WriteLine(charIndex.ToString());

            // Output the character representation
            foreach (var line in charData[charIndex])
            {
                writer.WriteLine(line);
            }
        }
    }
}