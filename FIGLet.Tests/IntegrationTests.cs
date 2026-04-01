using ByteForge.FIGLet;
using System.Text;

namespace FIGLet.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public void EndToEnd_DefaultFontAndRenderer_ShouldWorkCorrectly()
    {
        // Act
        var result = FIGLetRenderer.Render("Hello");
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines.Length > 0);
        Assert.IsTrue(lines.All(line => line.Length > 0));
    }

    [TestMethod]
    public void EndToEnd_CustomFontFromFile_ShouldWorkCorrectly()
    {
        // Arrange - Create a temporary font file
        var fontContent = TestUtilities.CreateMinimalValidFontContent(4, '#');
        var tempFile = Path.GetTempFileName() + ".flf";
        
        try
        {
            File.WriteAllText(tempFile, fontContent);
            
            // Act
            var font = FIGFont.FromFile(tempFile);
            var result = FIGLetRenderer.Render("Test", font);
            
            // Assert
            Assert.IsNotNull(font);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, font.Height);
            var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(4, lines.Length);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void EndToEnd_ZippedFont_ShouldWorkCorrectly()
    {
        // Arrange
        var fontContent = TestUtilities.CreateMinimalValidFontContent(3, '@');
        var zipBytes = TestUtilities.CreateZipWithFontFile(fontContent, "test.flf");
        var tempZipFile = Path.GetTempFileName() + ".zip";
        
        try
        {
            File.WriteAllBytes(tempZipFile, zipBytes);
            
            // Act
            var font = FIGFont.FromFile(tempZipFile);
            var result = FIGLetRenderer.Render("Zip", font);
            
            // Assert
            Assert.IsNotNull(font);
            Assert.IsNotNull(result);
            Assert.AreEqual("@", font.HardBlank);
            Assert.AreEqual(3, font.Height);
            var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, lines.Length);
        }
        finally
        {
            if (File.Exists(tempZipFile))
                File.Delete(tempZipFile);
        }
    }

    [TestMethod]
    public void EndToEnd_ComplexTextWithAllFeatures_ShouldWorkCorrectly()
    {
        // Arrange
        var font = TestUtilities.LoadTestFont("smushing-test");
        var renderer = new FIGLetRenderer(font, LayoutMode.Smushing, "\n", true, true);
        var complexText = "\x1b[31mHello\x1b[0m\n\x1b[32mWorld\x1b[0m!";
        
        // Act
        var result = renderer.Render(complexText);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("\x1b[31m")); // Red color preserved
        Assert.IsTrue(result.Contains("\x1b[32m")); // Green color preserved
        Assert.IsTrue(result.Contains("\x1b[0m"));  // Reset codes preserved
        Assert.IsTrue(result.Contains("\n"));       // Line breaks from paragraph mode
        
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines.Length >= font.Height * 2); // At least two paragraphs
    }

    [TestMethod]
    public void EndToEnd_AllLayoutModes_ShouldProduceDifferentOutputWidths()
    {
        // Arrange
        var font = TestUtilities.LoadTestFont("mini-fixed");
        var text = "ABCD";
        
        // Act
        var fullSizeResult = FIGLetRenderer.Render(text, font, LayoutMode.FullSize);
        var kerningResult = FIGLetRenderer.Render(text, font, LayoutMode.Kerning);
        var smushingResult = FIGLetRenderer.Render(text, font, LayoutMode.Smushing);
        
        // Assert
        var fullSizeLines = fullSizeResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var kerningLines = kerningResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var smushingLines = smushingResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        // All should have same height
        Assert.AreEqual(font.Height, fullSizeLines.Length);
        Assert.AreEqual(font.Height, kerningLines.Length);
        Assert.AreEqual(font.Height, smushingLines.Length);
        
        // Widths should generally decrease: FullSize >= Kerning >= Smushing
        Assert.IsTrue(fullSizeLines[0].Length >= kerningLines[0].Length);
        Assert.IsTrue(kerningLines[0].Length >= smushingLines[0].Length);
    }

    [TestMethod]
    public void EndToEnd_RealWorldCodeCommentGeneration_ShouldWorkCorrectly()
    {
        // Arrange - Simulate generating code comments
        var renderer = new FIGLetRenderer(FIGFont.Default, lineSeparator: "\n");
        var functionName = "ProcessData";
        
        // Act
        var banner = renderer.Render(functionName);
        var commentedBanner = ConvertToCodeComment(banner, "//");
        
        // Assert
        Assert.IsNotNull(commentedBanner);
        var lines = commentedBanner.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines.All(line => line.StartsWith("//")));
        Assert.IsTrue(commentedBanner.Contains("P")); // Should contain parts of "ProcessData"
        Assert.IsTrue(commentedBanner.Contains("D"));
    }

    [TestMethod]
    public void EndToEnd_MultipleLanguageComments_ShouldFormatCorrectly()
    {
        // Arrange
        var renderer = new FIGLetRenderer(FIGFont.Default, lineSeparator: "\n");
        var text = "API";
        var banner = renderer.Render(text);
        
        var commentStyles = new Dictionary<string, string>
        {
            { "C#", "//" },
            { "Python", "#" },
            { "SQL", "--" },
            { "HTML", "<!-- " }
        };
        
        // Act & Assert
        foreach (var (language, commentPrefix) in commentStyles)
        {
            var commented = ConvertToCodeComment(banner, commentPrefix);
            Assert.IsNotNull(commented, $"Failed for {language}");
            
            var lines = commented.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(lines.All(line => line.StartsWith(commentPrefix)), 
                $"Not all lines start with {commentPrefix} for {language}");
        }
    }

    [TestMethod]
    public void EndToEnd_ErrorRecovery_ShouldHandleGracefully()
    {
        // Test error recovery scenarios
        
        // 1. Renderer with missing characters should skip gracefully
        var font = TestUtilities.LoadTestFont("mini-fixed");
        var renderer = new FIGLetRenderer(font);
        var textWithUnicode = "A\u2603B"; // A + snowman + B
        
        var result = renderer.Render(textWithUnicode);
        Assert.IsNotNull(result);
        var processedResult = result.Replace(font.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("A"));
        Assert.IsTrue(processedResult.Contains("B"));
        
        // 2. Empty or whitespace input should return gracefully
        Assert.AreEqual(string.Empty, renderer.Render(""));
        Assert.AreEqual(string.Empty, renderer.Render(null));
        
        // 3. Very long text should still work
        var longText = new string('A', 1000);
        var longResult = renderer.Render(longText);
        Assert.IsNotNull(longResult);
        Assert.IsTrue(longResult.Length > 0);
    }

    [TestMethod]
    public void EndToEnd_MemoryUsage_ShouldBeReasonable()
    {
        // Arrange
        var renderer = new FIGLetRenderer(FIGFont.Default);
        var longText = TestUtilities.GenerateLargeText(500);
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act
        var result = renderer.Render(longText);
        var afterMemory = GC.GetTotalMemory(false);
        
        // Assert
        Assert.IsNotNull(result);
        
        // Memory usage shouldn't be excessive (arbitrary limit: 50MB for this test)
        var memoryUsed = afterMemory - initialMemory;
        Assert.IsTrue(memoryUsed < 50 * 1024 * 1024, 
            $"Memory usage too high: {memoryUsed:N0} bytes");
    }

    [TestMethod]
    public void EndToEnd_ConcurrentRendering_ShouldBeThreadSafe()
    {
        // Arrange
        var font = FIGFont.Default;
        var texts = new[] { "Test1", "Test2", "Test3", "Test4", "Test5" };
        var results = new string[texts.Length];
        var exceptions = new Exception[texts.Length];
        
        // Act - Render concurrently
        Parallel.For(0, texts.Length, i =>
        {
            try
            {
                results[i] = FIGLetRenderer.Render(texts[i], font);
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        });
        
        // Assert
        for (int i = 0; i < texts.Length; i++)
        {
            Assert.IsNull(exceptions[i], $"Exception in thread {i}: {exceptions[i]?.Message}");
            Assert.IsNotNull(results[i], $"Null result in thread {i}");
            Assert.IsTrue(results[i].Length > 0, $"Empty result in thread {i}");
        }
    }

    [TestMethod]
    public void EndToEnd_DifferentLineSeparators_ShouldWorkCorrectly()
    {
        // Arrange
        var font = TestUtilities.LoadTestFont("mini-fixed");
        var text = "Test";
        var separators = new[] { "\n", "\r\n", "|", "|||" };
        
        // Act & Assert
        foreach (var separator in separators)
        {
            var renderer = new FIGLetRenderer(font, lineSeparator: separator);
            var result = renderer.Render(text);
            
            Assert.IsNotNull(result, $"Failed with separator: '{separator}'");
            Assert.IsTrue(result.Contains(separator), $"Result doesn't contain separator: '{separator}'");
            
            var parts = result.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(font.Height, parts.Length, $"Wrong number of parts with separator: '{separator}'");
        }
    }

    [TestMethod]
    public void EndToEnd_ComplexANSISequences_ShouldPreserveCorrectly()
    {
        // Arrange
        var renderer = new FIGLetRenderer(FIGFont.Default, useANSIColors: true);
        var complexANSI = string.Join("", new[]
        {
            "\x1b[38;5;196m",      // 256-color red
            "\x1b[48;2;0;255;0m",  // True color green background
            "\x1b[1m",             // Bold
            "\x1b[3m",             // Italic
            "\x1b[4m",             // Underline
            "Complex",
            "\x1b[0m"              // Reset
        });
        
        // Act
        var result = renderer.Render(complexANSI);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("\x1b[38;5;196m"));   // 256-color preserved
        Assert.IsTrue(result.Contains("\x1b[48;2;0;255;0m")); // True color preserved
        Assert.IsTrue(result.Contains("\x1b[1m"));          // Bold preserved
        Assert.IsTrue(result.Contains("\x1b[3m"));          // Italic preserved
        Assert.IsTrue(result.Contains("\x1b[4m"));          // Underline preserved
        Assert.IsTrue(result.Contains("\x1b[0m"));          // Reset at end
        
        var processedResult = TestUtilities.StripANSIColors(result);
        Assert.IsTrue(processedResult.Contains("C")); // Text should be rendered
    }

    [TestMethod]
    public void EndToEnd_FontWithAllSmushingRules_ShouldApplyCorrectly()
    {
        // Arrange
        var font = TestUtilities.LoadTestFont("smushing-test");
        var renderer = new FIGLetRenderer(font);
        
        // Test different smushing scenarios
        var testCases = new[]
        {
            ("AA", "Equal character smushing"),
            ("_|", "Underscore smushing"),
            ("||", "Hierarchy smushing"),
            ("[]", "Opposite pair smushing"),
            ("/\\", "Big X smushing (slashes)"),
            ("><", "Big X smushing (arrows)")
        };
        
        // Act & Assert
        foreach (var (text, description) in testCases)
        {
            var result = renderer.Render(text);
            Assert.IsNotNull(result, $"Failed to render: {description}");
            
            var fullSizeResult = FIGLetRenderer.Render(text, font, LayoutMode.FullSize);
            var resultLines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var fullSizeLines = fullSizeResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            
            // Smushing should generally produce narrower output than full size
            Assert.IsTrue(resultLines[0].Length <= fullSizeLines[0].Length, 
                $"Smushing didn't reduce width for: {description}");
        }
    }

    [TestMethod]
    public void EndToEnd_RealWorldBannerGeneration_ShouldMatchExpectations()
    {
        // Arrange - Simulate real-world usage scenarios
        var scenarios = new[]
        {
            new { Text = "ERROR", Context = "Error message banner" },
            new { Text = "SUCCESS", Context = "Success message banner" },
            new { Text = "WARNING", Context = "Warning message banner" },
            new { Text = "DEBUG", Context = "Debug section banner" },
            new { Text = "TODO", Context = "TODO comment banner" },
            new { Text = "FIXME", Context = "FIXME comment banner" }
        };
        
        // Act & Assert
        foreach (var scenario in scenarios)
        {
            var result = FIGLetRenderer.Render(scenario.Text);
            
            Assert.IsNotNull(result, $"Failed to render: {scenario.Context}");
            Assert.IsTrue(result.Length > scenario.Text.Length, 
                $"Banner should be larger than input for: {scenario.Context}");
            
            var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(lines.Length > 1, $"Banner should be multi-line for: {scenario.Context}");
            Assert.IsTrue(lines.All(line => line.Length > 0), 
                $"All lines should have content for: {scenario.Context}");
            
            // Replace hard blanks and check that original text characters are present
            var processedResult = result.Replace(FIGFont.Default.HardBlank[0], ' ');
            foreach (char c in scenario.Text)
            {
                if (c != ' ')
                {
                    Assert.IsTrue(processedResult.Contains(c), 
                        $"Character '{c}' not found in banner for: {scenario.Context}");
                }
            }
        }
    }

    [TestMethod]
    public void EndToEnd_ZipFont_DeflateCompression_ShouldLoadCorrectly()
    {
        // Arrange — ZIP with Deflate (Optimal) compression, the default
        var fontContent = TestUtilities.CreateMinimalValidFontContent(3, '@');
        var zipBytes    = TestUtilities.CreateZipWithFontFile(fontContent, "test.flf",
                              System.IO.Compression.CompressionLevel.Optimal);
        var tempFile = Path.GetTempFileName() + ".zip";

        try
        {
            File.WriteAllBytes(tempFile, zipBytes);

            // Act
            var font   = FIGFont.FromFile(tempFile);
            var result = FIGLetRenderer.Render("Hi", font);

            // Assert
            Assert.IsNotNull(font,   "Font should load from Deflate-compressed ZIP");
            Assert.AreEqual("@", font.HardBlank);
            Assert.AreEqual(3, font.Height);
            Assert.IsNotNull(result);
            var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, lines.Length);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void EndToEnd_ZipFont_StoreCompression_ShouldLoadCorrectly()
    {
        // Arrange — ZIP with Store (NoCompression / method 0) — no deflate applied
        var fontContent = TestUtilities.CreateMinimalValidFontContent(4, '%');
        var zipBytes    = TestUtilities.CreateZipWithFontFile(fontContent, "test.flf",
                              System.IO.Compression.CompressionLevel.NoCompression);
        var tempFile = Path.GetTempFileName() + ".zip";

        try
        {
            File.WriteAllBytes(tempFile, zipBytes);

            // Act
            var font   = FIGFont.FromFile(tempFile);
            var result = FIGLetRenderer.Render("Hi", font);

            // Assert
            Assert.IsNotNull(font,   "Font should load from Store-compressed (uncompressed) ZIP");
            Assert.AreEqual("%", font.HardBlank);
            Assert.AreEqual(4, font.Height);
            Assert.IsNotNull(result);
            var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(4, lines.Length);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    private static string ConvertToCodeComment(string banner, string commentPrefix)
    {
        var lines = banner.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return string.Join('\n', lines.Select(line => $"{commentPrefix} {line}"));
    }
}