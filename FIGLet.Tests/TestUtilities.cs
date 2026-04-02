using System.Reflection;
using System.Text;
using ByteForge.FIGLet;

namespace FIGLet.Tests;

public static class TestUtilities
{
    /// <summary>
    /// Retrieves an embedded resource stream by name from the executing assembly.
    /// </summary>
    /// <param name="resourceName">The name of the resource to retrieve.</param>
    /// <returns>A <see cref="Stream"/> to the embedded resource.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the resource is not found.</exception>
    public static Stream GetEmbeddedResourceStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        
        if (fullResourceName == null)
        {
            var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
            throw new FileNotFoundException($"Resource '{resourceName}' not found. Available resources: {availableResources}");
        }
        
        return assembly.GetManifestResourceStream(fullResourceName) ?? 
               throw new FileNotFoundException($"Resource '{resourceName}' not found.");
    }

    /// <summary>
    /// Retrieves the text content of an embedded resource.
    /// </summary>
    /// <param name="resourceName">The name of the resource to retrieve.</param>
    /// <returns>The text content of the embedded resource.</returns>
    public static string GetEmbeddedResourceText(string resourceName)
    {
        using var stream = GetEmbeddedResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Loads a test FIGFont from an embedded resource.
    /// </summary>
    /// <param name="fontName">The name of the font file (without extension).</param>
    /// <returns>A <see cref="FIGFont"/> instance loaded from the resource.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the font cannot be loaded.</exception>
    public static FIGFont LoadTestFont(string fontName)
    {
        try
        {
            using var stream = GetEmbeddedResourceStream($"{fontName}.flf");
            return FIGFont.FromStream(stream) ?? throw new InvalidOperationException($"Failed to load test font: {fontName}");
        }
        catch (Exception ex)
        {
            // Fallback to mini-fixed if the requested font fails
            if (fontName != "mini-fixed")
            {
                using var stream = GetEmbeddedResourceStream("mini-fixed.flf");
                return FIGFont.FromStream(stream) ?? throw new InvalidOperationException($"Failed to load fallback test font: mini-fixed. Original error: {ex.Message}");
            }
            throw;
        }
    }

    /// <summary>
    /// Creates minimal valid font content for testing purposes.
    /// </summary>
    /// <param name="height">The height of the font.</param>
    /// <param name="hardBlank">The hard blank character.</param>
    /// <param name="printDirection">The print direction.</param>
    /// <returns>A string containing the font content.</returns>
    public static string CreateMinimalValidFontContent(int height = 5, char hardBlank = '$', int printDirection = 0)
    {
        var sb = new StringBuilder();

        // Header: signature + hardblank, height, baseline, maxlength, oldlayout, commentlines[, printDirection]
        var directionSuffix = printDirection != 0 ? $" {printDirection}" : "";
        sb.AppendLine($"flf2a{hardBlank} {height} 4 10 15 0{directionSuffix}");
        
        // Required ASCII characters 32-126 (95 total characters)
        for (var charCode = 32; charCode <= 126; charCode++)
        {
            for (var line = 0; line < height; line++)
            {
                if (charCode == 32) // Space character - use hard blank
                {
                    if (line == height - 1)
                        sb.AppendLine($"{hardBlank}@@");
                    else
                        sb.AppendLine($"{hardBlank}@");
                }
                else if (line == height - 1) // Last line gets @@ terminator
                {
                    sb.AppendLine($"{(char)charCode}@@");
                }
                else
                {
                    sb.AppendLine($"{(char)charCode}@");
                }
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Creates invalid font content for testing error handling.
    /// </summary>
    /// <param name="issue">The type of issue to simulate.</param>
    /// <returns>A string containing invalid font content.</returns>
    /// <exception cref="ArgumentException">Thrown if the issue type is unknown.</exception>
    public static string CreateInvalidFontContent(string issue)
    {
        return issue switch
        {
            "no_signature" => "invalid_signature 5 4 10 15 0",
            "missing_height" => "flf2a$ missing 4 10 15 0",
            "negative_height" => "flf2a$ -5 4 10 15 0",
            "missing_characters" => "flf2a$ 5 4 10 15 0\n", // No character data
            _ => throw new ArgumentException($"Unknown issue type: {issue}")
        };
    }

    /// <summary>
    /// Asserts that the actual string contains all specified expected lines.
    /// </summary>
    /// <param name="actual">The actual string to check.</param>
    /// <param name="expectedLines">The expected lines that should be present.</param>
    public static void AssertStringContainsLines(string actual, params string[] expectedLines)
    {
        var actualLines = actual.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var expectedLine in expectedLines)
        {
            Assert.IsTrue(actualLines.Contains(expectedLine.Trim()), 
                $"Expected line '{expectedLine}' not found in output. Actual lines: {string.Join(", ", actualLines)}");
        }
    }

    /// <summary>
    /// Asserts that two multi-line strings are equal, ignoring carriage return differences.
    /// </summary>
    /// <param name="expected">The expected string.</param>
    /// <param name="actual">The actual string.</param>
    /// <param name="message">Optional message to include in assertion failures.</param>
    public static void AssertMultiLineEqual(string expected, string actual, string message = "")
    {
        actual = actual.Replace("\r", "");
        expected = expected.Replace("\r", "");
        var expectedLines = expected.Split('\n', StringSplitOptions.None);
        var actualLines = actual.Split('\n', StringSplitOptions.None);
        
        Assert.AreEqual(expectedLines.Length, actualLines.Length, 
            $"{message}Line count mismatch. Expected: {expectedLines.Length}, Actual: {actualLines.Length}");
        
        for (var i = 0; i < expectedLines.Length; i++)
        {
            Assert.AreEqual(expectedLines[i], actualLines[i], 
                $"{message}Line {i + 1} mismatch. Expected: '{expectedLines[i]}', Actual: '{actualLines[i]}'");
        }
    }

    /// <summary>
    /// Strips ANSI color codes from the given text.
    /// </summary>
    /// <param name="text">The text to strip colors from.</param>
    /// <returns>The text without ANSI color codes.</returns>
    public static string StripANSIColors(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[0-?]*[ -/]*[@-~]", "");
    }

    /// <summary>
    /// Asserts whether a FIGFont has a specific smushing rule.
    /// </summary>
    /// <param name="font">The FIGFont to check.</param>
    /// <param name="expectedRule">The smushing rule to check for.</param>
    /// <param name="shouldHave">Whether the font should have the rule.</param>
    public static void AssertSmushingRule(FIGFont font, SmushingRules expectedRule, bool shouldHave = true)
    {
        var hasRule = font.HasSmushingRule(expectedRule);
        if (shouldHave)
        {
            Assert.IsTrue(hasRule, $"Font should have smushing rule: {expectedRule}");
        }
        else
        {
            Assert.IsFalse(hasRule, $"Font should not have smushing rule: {expectedRule}");
        }
    }

    /// <summary>
    /// Creates a MemoryStream from a string content.
    /// </summary>
    /// <param name="content">The string content.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the content.</returns>
    public static MemoryStream CreateStreamFromString(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Asserts that the specified action throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of exception expected.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="expectedMessage">Optional substring that should be in the exception message.</param>
    public static void AssertThrows<T>(Action action, string expectedMessage = "") where T : Exception
    {
        try
        {
            action();
            Assert.Fail($"Expected exception of type {typeof(T).Name} was not thrown.");
        }
        catch (T ex)
        {
            if (!string.IsNullOrEmpty(expectedMessage))
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), 
                    $"Exception message '{ex.Message}' does not contain expected text '{expectedMessage}'");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected exception of type {typeof(T).Name}, but got {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Asserts that an action completes within a specified time limit.
    /// </summary>
    /// <param name="action">The action to time.</param>
    /// <param name="maxExpectedDuration">The maximum allowed duration.</param>
    /// <param name="operationName">Name of the operation for error messages.</param>
    public static void AssertPerformance(Action action, TimeSpan maxExpectedDuration, string operationName = "Operation")
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        
        Assert.IsTrue(stopwatch.Elapsed <= maxExpectedDuration, 
            $"{operationName} took {stopwatch.Elapsed}, but should complete within {maxExpectedDuration}");
    }

    /// <summary>
    /// Generates a large random text string of specified length.
    /// </summary>
    /// <param name="length">The length of the text to generate.</param>
    /// <returns>A random text string.</returns>
    public static string GenerateLargeText(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
        var random = new Random(42); // Fixed seed for reproducible tests
        var result = new StringBuilder(length);
        
        for (var i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Creates a ZIP archive containing a font file.
    /// </summary>
    /// <param name="fontContent">The content of the font file.</param>
    /// <param name="fileName">The name of the file in the archive.</param>
    /// <param name="compressionLevel">The compression level to use.</param>
    /// <returns>A byte array containing the ZIP archive.</returns>
    public static byte[] CreateZipWithFontFile(
        string fontContent,
        string fileName = "test.flf",
        System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(fileName, compressionLevel);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            writer.Write(fontContent);
        }
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Dictionary containing test texts and their expected rendered outputs.
    /// </summary>
    public static readonly Dictionary<string, string> TestTexts = new() 
    {
        ["Hello"] = "Helo\r\nHelo\r\nHelo\r\n",
        ["World"] = "World\r\nWorld\r\nWorld\r\n",
        ["Test"] = "Test\r\nTest\r\nTest\r\n",
        ["123"] = "123\r\n123\r\n123\r\n",
        ["ABC"] = "ABC\r\nABC\r\nABC\r\n",
        ["!@#$%^&*()"] = "! %^&*)\r\n! %^&*)\r\n  %^&*)\r\n",
        ["The quick brown fox"] = "The quick brown fox\r\nThe quick brown fox\r\nThe quick brown fox\r\n",
        ["a"] = "a\r\na\r\na\r\n",
        ["Hello\nWorld"] = "Helo\r\nHelo\r\nHelo\r\nWorld\r\nWorld\r\nWorld\r\n",
        ["Line1\nLine2\nLine3"] = "Line1\r\nLine1\r\nLine1\r\nLine2\r\nLine2\r\nLine2\r\nLine3\r\nLine3\r\nLine3\r\n",
        ["Mixed 123 Text!"] = "Mixed 123 Text!\r\nMixed 123 Text!\r\nMixed 123 Text \r\n",
        ["Unicode: éñ中文"] = "Unicode: \r\nUnicode: \r\nUnicode: \r\n",
        ["Symbols: ←↑→↓"] = "Symbols: \r\nSymbols: \r\nSymbols: \r\n",
        ["Math: ∑∏∆∇"] = "Math: \r\nMath: \r\nMath: \r\n",
        ["🚀🎉✨"] = "\r\n\r\n\r\n"
    };

    /// <summary>
    /// Dictionary containing ANSI colored test texts and their expected outputs.
    /// </summary>
    public static readonly Dictionary<string, string> ANSIColoredTexts = new()
    {
        ["\x1b[31mRed\x1b[0m"] = "\x1b[31mRed\x1b[0m\r\n\x1b[31mRed\x1b[0m\r\n\x1b[31mRed\x1b[0m\r\n",
        ["\x1b[32mGreen\x1b[33mYellow\x1b[0m"] = "\x1b[32mGren\x1b[33mYelow\x1b[0m\r\n\x1b[32mGren\x1b[33mYelow\x1b[0m\r\n\x1b[32mGren\x1b[33mYelow\x1b[0m\r\n",
        ["\x1b[1;4;31mBold Underline Red\x1b[0m"] = "\x1b[1;4;31mBold Underline Red\x1b[0m\r\n\x1b[1;4;31mBold Underline Red\x1b[0m\r\n\x1b[1;4;31mBold Underline Red\x1b[0m\r\n",
        ["Normal\x1b[96mCyan\x1b[0mBack"] = "Normal\x1b[96mCyan\x1b[0mBack\x1b[0m\r\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\r\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\r\n",
        ["\x1b[38;5;196mRGB Red\x1b[0m"] = "\x1b[38;5;196mRGB Red\x1b[0m\r\n\x1b[38;5;196mRGB Red\x1b[0m\r\n\x1b[38;5;196mRGB Red\x1b[0m\r\n",
        ["\x1b[48;2;255;0;0mTrue Color BG\x1b[0m"] = "\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\r\n\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\r\n\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\r\n",
    };
}