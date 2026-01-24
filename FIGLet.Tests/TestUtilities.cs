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

    public static string CreateMinimalValidFontContent(int height = 5, char hardBlank = '$')
    {
        var sb = new StringBuilder();
        
        // Header: signature + hardblank, height, baseline, maxlength, oldlayout, commentlines
        sb.AppendLine($"flf2a{hardBlank} {height} 4 10 15 0");
        
        // Required ASCII characters 32-126 (95 total characters)
        for (int charCode = 32; charCode <= 126; charCode++)
        {
            for (int line = 0; line < height; line++)
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
        
        for (int i = 0; i < expectedLines.Length; i++)
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
            $"{operationName} took {stopwatch.Elapsed:F3}, but should complete within {maxExpectedDuration:F3}");
    }

    public static string GenerateLargeText(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
        var random = new Random(42); // Fixed seed for reproducible tests
        var result = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        
        return result.ToString();
    }

    public static byte[] CreateZipWithFontFile(string fontContent, string fileName = "test.flf")
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(fileName);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            writer.Write(fontContent);
        }
        return memoryStream.ToArray();
    }

    public static readonly string[] TestTexts = 
    [
        "Hello",
        "World",
        "Test",
        "123",
        "ABC",
        "!@#$%^&*()",
        "The quick brown fox",
        "",
        " ",
        "  ",
        "a",
        "Hello\nWorld",
        "Line1\nLine2\nLine3",
        "Mixed 123 Text!",
        "\t\n\r",
        "Unicode: éñ中文",
        "Symbols: ←↑→↓",
        "Math: ∑∏∆∇",
        "🚀🎉✨", // Emoji (surrogate pairs)
    ];

    public static readonly string[] ANSIColoredTexts = 
    [
        "\x1b[31mRed\x1b[0m",
        "\x1b[32mGreen\x1b[33mYellow\x1b[0m",
        "\x1b[1;4;31mBold Underline Red\x1b[0m",
        "Normal\x1b[96mCyan\x1b[0mBack",
        "\x1b[38;5;196mRGB Red\x1b[0m",
        "\x1b[48;2;255;0;0mTrue Color BG\x1b[0m",
    ];
}