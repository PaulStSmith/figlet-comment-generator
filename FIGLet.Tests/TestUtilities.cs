using System.Reflection;
using System.Text;
using ByteForge.FIGLet;

namespace FIGLet.Tests;

public static class TestUtilities
{
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

    public static string GetEmbeddedResourceText(string resourceName)
    {
        using var stream = GetEmbeddedResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

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

    public static void AssertStringContainsLines(string actual, params string[] expectedLines)
    {
        var actualLines = actual.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var expectedLine in expectedLines)
        {
            Assert.IsTrue(actualLines.Contains(expectedLine.Trim()), 
                $"Expected line '{expectedLine}' not found in output. Actual lines: {string.Join(", ", actualLines)}");
        }
    }

    public static void AssertMultiLineEqual(string expected, string actual, string message = "")
    {
        var expectedLines = expected.Split(['\r', '\n'], StringSplitOptions.None);
        var actualLines = actual.Split(['\r', '\n'], StringSplitOptions.None);
        
        Assert.AreEqual(expectedLines.Length, actualLines.Length, 
            $"{message}Line count mismatch. Expected: {expectedLines.Length}, Actual: {actualLines.Length}");
        
        for (var i = 0; i < expectedLines.Length; i++)
        {
            Assert.AreEqual(expectedLines[i], actualLines[i], 
                $"{message}Line {i + 1} mismatch. Expected: '{expectedLines[i]}', Actual: '{actualLines[i]}'");
        }
    }

    public static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    public static string StripANSIColors(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[0-?]*[ -/]*[@-~]", "");
    }

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

    public static MemoryStream CreateStreamFromString(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

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

    public static void AssertPerformance(Action action, TimeSpan maxExpectedDuration, string operationName = "Operation")
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        
        Assert.IsTrue(stopwatch.Elapsed <= maxExpectedDuration, 
            $"{operationName} took {stopwatch.Elapsed}, but should complete within {maxExpectedDuration}");
    }

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