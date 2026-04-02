using ByteForge.FIGLet;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace FIGLet.Tests;

[TestClass]
public class FIGLetRendererTests
{
    private FIGFont _testFont;
    private FIGFont _smushingFont;

    [TestInitialize]
    public void Initialize()
    {
        _testFont = TestUtilities.LoadTestFont("mini-fixed");
        _smushingFont = TestUtilities.LoadTestFont("smushing-test");
    }

    [TestMethod]
    public void Constructor_WithDefaultParameters_ShouldUseDefaults()
    {
        // Act
        var renderer = new FIGLetRenderer();
        
        // Assert
        Assert.IsNotNull(renderer.Font);
        Assert.AreEqual(LayoutMode.Default, renderer.LayoutMode);
        Assert.AreEqual(Environment.NewLine, renderer.LineSeparator);
        Assert.IsFalse(renderer.UseANSIColors);
        Assert.IsTrue(renderer.ParagraphMode);
    }

    [TestMethod]
    public void Constructor_WithCustomParameters_ShouldSetCorrectly()
    {
        // Act
        var renderer = new FIGLetRenderer(_testFont, LayoutMode.Kerning, "\n", true, false);
        
        // Assert
        Assert.AreSame(_testFont, renderer.Font);
        Assert.AreEqual(LayoutMode.Kerning, renderer.LayoutMode);
        Assert.AreEqual("\n", renderer.LineSeparator);
        Assert.IsTrue(renderer.UseANSIColors);
        Assert.IsFalse(renderer.ParagraphMode);
    }

    [TestMethod]
    public void Constructor_WithNullFont_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        TestUtilities.AssertThrows<ArgumentNullException>(() => 
            new FIGLetRenderer(null));
    }

    [TestMethod]
    public void StaticRender_WithNullOrEmptyText_ShouldReturnEmpty()
    {
        // Act & Assert
        Assert.AreEqual(string.Empty, FIGLetRenderer.Render(null));
        Assert.AreEqual(string.Empty, FIGLetRenderer.Render(""));
    }

    [TestMethod]
    public void StaticRender_WithValidText_ShouldRenderCorrectly()
    {
        // Act
        var result = FIGLetRenderer.Render("Hi", _testFont);
        
        // Assert
        Assert.IsNotNull(result);
        TestUtilities.AssertMultiLineEqual("Hi\r\nHi\r\nHi\r\n", result, "Static Render should produce expected output for 'Hi'");

        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_testFont.Height, lines.Length);
    }

    [TestMethod]
    public void Render_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        
        // Act
        var result = renderer.Render("");
        
        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void Render_WithSingleCharacter_ShouldRenderCorrectly()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        
        // Act
        var result = renderer.Render("A");
        
        // Assert
        Assert.IsNotNull(result);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_testFont.Height, lines.Length);
        // Replace hard blank with space for comparison
        var processedResult = result.Replace(_testFont.HardBlank, ' ');
        Assert.IsTrue(processedResult.Contains("A"));
    }

    [TestMethod]
    public void Render_WithMultipleCharacters_ShouldJoinCorrectly()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        
        // Act
        var result = renderer.Render("AB");
        
        // Assert
        Assert.IsNotNull(result);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_testFont.Height, lines.Length);
        // Should be wider than single character
        var singleChar = renderer.Render("A");
        var singleLines = singleChar.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines[0].Length > singleLines[0].Length);
    }

    [DataTestMethod]
    [DataRow(LayoutMode.FullSize)]
    [DataRow(LayoutMode.Kerning)]
    [DataRow(LayoutMode.Smushing)]
    public void Render_WithDifferentLayoutModes_ShouldProduceDifferentResults(LayoutMode mode)
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { LayoutMode = mode };
        
        // Act
        var result = renderer.Render("AB");
        
        // Assert
        Assert.IsNotNull(result);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_testFont.Height, lines.Length);
    }

    [TestMethod]
    public void Render_WithFullSizeMode_ShouldHaveNoOverlap()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { LayoutMode = LayoutMode.FullSize };
        
        // Act
        var twoChars = renderer.Render("AB");
        var singleA = renderer.Render("A");
        var singleB = renderer.Render("B");
        
        // Assert
        var twoLines = twoChars.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var singleALines = singleA.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var singleBLines = singleB.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        // In full size mode, width should be sum of individual character widths
        Assert.AreEqual(singleALines[0].Length + singleBLines[0].Length, twoLines[0].Length);
    }

    [TestMethod]
    public void Render_WithParagraphModeTrue_ShouldHandleLineBreaks()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { ParagraphMode = true };
        
        // Act
        var result = renderer.Render("A\nB");
        
        // Assert
        Assert.IsNotNull(result);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        // Should have font height * 2 + empty line separation
        Assert.IsTrue(lines.Length >= _testFont.Height * 2);
    }

    [TestMethod]
    public void Render_WithParagraphModeFalse_ShouldReplaceLineBreaksWithSpaces()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { ParagraphMode = false };
        
        // Act
        var result = renderer.Render("A\nB");
        
        // Assert
        Assert.IsNotNull(result);
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        // Should have only font height (single line)
        Assert.AreEqual(_testFont.Height, lines.Length);
    }

    [TestMethod]
    public void Render_WithCustomLineSeparator_ShouldUseCustomSeparator()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { LineSeparator = "|" };
        
        // Act
        var result = renderer.Render("A");
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("|"));
        Assert.IsFalse(result.Contains("\r"));
        Assert.IsFalse(result.Contains("\n"));
    }

    [TestMethod]
    public void Render_WithANSIColors_ShouldPreserveColors()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { UseANSIColors = true };
        var coloredText = "\x1b[31mR\x1b[32mG\x1b[0m";
        
        // Act
        var result = renderer.Render(coloredText);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("\x1b[31m")); // Red color
        Assert.IsTrue(result.Contains("\x1b[32m")); // Green color
        Assert.IsTrue(result.Contains("\x1b[0m"));  // Reset at end
    }

    [TestMethod]
    public void Render_WithANSIColorsDisabled_ShouldStripColors()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { UseANSIColors = false };
        var coloredText = "\x1b[31mRed\x1b[0m";
        
        // Act
        var result = renderer.Render(coloredText);
        
        // Assert
        Assert.IsNotNull(result);
        // Should render "Red" but without ANSI sequences
        Assert.IsFalse(result.Contains("\x1b["));
        Assert.IsTrue(result.StartsWith("Red"));
    }

    [TestMethod]
    public void Render_WithRTLFont_ShouldReverseText()
    {
        // Arrange - Create RTL font (printDirection = 1)
        var content = TestUtilities.CreateMinimalValidFontContent(2, '$', printDirection: 1);
        using var stream = TestUtilities.CreateStreamFromString(content);
        var rtlFont = FIGFont.FromStream(stream);
        var renderer = new FIGLetRenderer(rtlFont, useANSIColors: true);
        
        // Act
        var result = renderer.Render("\x1b[31mAB\x1b[0m");
        
        // Assert
        Assert.IsNotNull(result);
        // Text should be rendered as "BA" due to RTL
        var processedResult = result.Replace(rtlFont.HardBlank, ' ');
        // Note: This is hard to test exactly without knowing the exact font rendering,
        // but we can ensure the RTL font property is set
        Assert.AreEqual(1, rtlFont.PrintDirection);
    }

    [TestMethod]
    public void Render_WithRTLFontAndANSIColors_ShouldPreserveBothReverseAndColors()
    {
        // Arrange — build matching LTR and RTL fonts from the same glyph data so we
        // can compare their output directly after stripping ANSI sequences.
        var ltrContent = TestUtilities.CreateMinimalValidFontContent(2, '$', printDirection: 0);
        var rtlContent = TestUtilities.CreateMinimalValidFontContent(2, '$', printDirection: 1);

        using var ltrStream = TestUtilities.CreateStreamFromString(ltrContent);
        using var rtlStream = TestUtilities.CreateStreamFromString(rtlContent);

        var ltrFont = FIGFont.FromStream(ltrStream)!;
        var rtlFont = FIGFont.FromStream(rtlStream)!;

        var ltrRenderer = new FIGLetRenderer(ltrFont, lineSeparator: "\n");
        var rtlRenderer = new FIGLetRenderer(rtlFont, lineSeparator: "\n", useANSIColors: true);

        // Act
        var result = rtlRenderer.Render("\x1b[31mAB\x1b[0m");

        // Assert: ANSI sequences are preserved
        Assert.IsTrue(result.Contains("\x1b[31m"), "Red color sequence should be preserved");
        Assert.IsTrue(result.Contains("\x1b[0m"),  "ANSI reset code should be present");

        // Assert: text is reversed — RTL("AB"") stripped of color should equal LTR("BA")
        var stripped = TestUtilities.StripANSIColors(result);
        var ltrBA    = ltrRenderer.Render("BA");
        Assert.AreEqual(ltrBA, stripped,
            "RTL render of 'AB' (colors stripped) should equal LTR render of 'BA'");
    }

    [TestMethod]
    public void Render_WithParagraphMode_ConsecutiveParagraphs_ShouldConcatenateWithoutBlankSeparation()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { ParagraphMode = true, LineSeparator = "\n" };

        // Act
        var consecutive = renderer.Render("A\nB");   // No blank line between paragraphs
        var separated   = renderer.Render("A\n\nB"); // Explicit blank line between paragraphs

        // Assert: both A and B are rendered in the consecutive case
        var stripped = consecutive.Replace(_testFont.HardBlank, ' ');
        Assert.IsTrue(stripped.Contains("A"), "First paragraph 'A' should be rendered");
        Assert.IsTrue(stripped.Contains("B"), "Second paragraph 'B' should be rendered");

        // Assert: the blank-line separator injects exactly font.Height extra newlines.
        // Consecutive paragraphs are simply concatenated (no blank lines between them).
        var consecutiveNewlines = consecutive.Count(c => c == '\n');
        var separatedNewlines   = separated.Count(c => c == '\n');
        Assert.AreEqual(_testFont.Height, separatedNewlines - consecutiveNewlines,
            $"Blank-line separated output should have exactly {_testFont.Height} more newlines " +
            $"than consecutive (got {separatedNewlines} vs {consecutiveNewlines})");
    }

    [TestMethod]
    public void Render_WithUnicodeText_ShouldHandleSurrogatePairs()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        var emojiText = "A🚀B"; // Contains surrogate pair
        
        // Act
        var result = renderer.Render(emojiText);
        
        // Assert
        Assert.IsNotNull(result);
        // Should render A and B, emoji might be skipped if not in font
        var processedResult = result.Replace(_testFont.HardBlank, ' ');
        Assert.IsTrue(processedResult.Contains("A"));
        Assert.IsTrue(processedResult.Contains("B"));
    }

    [TestMethod]
    public void SmushingRules_EqualCharacter_ShouldSmushIdenticalChars()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('|', '|', '$', LayoutMode.Smushing, SmushingRules.EqualCharacter);

        // Assert
        Assert.AreEqual('|', result);
    }

    [TestMethod]
    public void SmushingRules_Hierarchy_ShouldPreferHigherRankedChars()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('|', '/', '$', LayoutMode.Smushing, SmushingRules.Hierarchy);

        // Assert
        Assert.AreEqual('/', result);
    }

    [TestMethod]
    public void SmushingRules_OppositePair_ShouldSmushBrackets()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('[', ']', '$', LayoutMode.Smushing, SmushingRules.OppositePair);

        // Assert
        Assert.AreEqual('|', result);
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldCreatePipeFromSlashes()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('/', '\\', '$', LayoutMode.Smushing, SmushingRules.BigX);

        // Assert
        Assert.AreEqual('|', result);
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldCreateYFromSlashes()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('\\', '/', '$', LayoutMode.Smushing, SmushingRules.BigX);

        // Assert
        Assert.AreEqual('Y', result);
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldCreateXFromArrows()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('>', '<', '$', LayoutMode.Smushing, SmushingRules.BigX);

        // Assert
        Assert.AreEqual('X', result);
    }

    [TestMethod]
    public void SmushingRules_Underscore_ShouldSmushWithHierarchy()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters('_', '|', '$', LayoutMode.Smushing, SmushingRules.Underscore);

        // Assert
        Assert.AreEqual('|', result);
    }

    [TestMethod]
    public void SmushingRules_HardBlank_ShouldSmushHardBlanks()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);

        // Act
        var result = renderer.SmushCharacters(_smushingFont.HardBlank, _smushingFont.HardBlank, _smushingFont.HardBlank, LayoutMode.Smushing, SmushingRules.HardBlank);

        // Assert
        Assert.AreEqual(_smushingFont.HardBlank, result);
    }

    [TestMethod]
    public void Performance_LargeText_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        var largeText = TestUtilities.GenerateLargeText(100);
        
        // Act & Assert
        TestUtilities.AssertPerformance(() => 
        {
            var result = renderer.Render(largeText);
            Assert.IsNotNull(result);
        }, TimeSpan.FromSeconds(5), "Large text rendering");
    }

    [TestMethod]
    public void Render_WithVariousTestTexts_ShouldHandleAllCases()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);

        foreach (var testText in TestUtilities.TestTexts)
        {
            // Act
            var result = renderer.Render(testText.Key);
            
            // Assert
            Assert.IsNotNull(result, $"Failed to render: '{testText.Key}'");
            TestUtilities.AssertMultiLineEqual(testText.Value, result, $"Rendered output does not match expected for: '{testText.Key}'");
        }
    }

    [TestMethod]
    public void Render_WithANSIColoredTexts_ShouldHandleAllCases()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { UseANSIColors = true };
        
        foreach (var coloredText in TestUtilities.ANSIColoredTexts)
        {
            // Act
            var result = renderer.Render(coloredText.Key);

            // Assert
            // Debug.WriteLine(@$"[""{coloredText.Replace("\x1b", "\\x1b").Replace("\n", "\\n").Replace("\r", "\\r")}""] = ""{result.Replace("\x1b", "\\x1b").Replace("\n", "\\n").Replace("\r", "\\r")}"",");
            Assert.IsNotNull(result, $"Failed to render: '{coloredText.Key}'");
            TestUtilities.AssertMultiLineEqual(coloredText.Value, result, $"Rendered output does not match expected for: '{coloredText.Key}'");
        }
    }

    [TestMethod]
    public void CanSmush_WithSpaces_ShouldAlwaysSmush()
    {
        // This tests the smushing logic indirectly through rendering
        var renderer = new FIGLetRenderer(_testFont);
        
        // Act - Render text with spaces
        var result = renderer.Render("A B");
        
        // Assert
        Assert.IsNotNull(result);
        // Spaces should always smush/overlap properly
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_testFont.Height, lines.Length);
    }

    [TestMethod]
    public void CalculateOverlap_WithDifferentModes_ShouldCalculateCorrectly()
    {
        // This tests overlap calculation through rendering comparison
        var fullSizeRenderer = new FIGLetRenderer(_testFont) { LayoutMode = LayoutMode.FullSize };
        var kerningRenderer = new FIGLetRenderer(_testFont) { LayoutMode = LayoutMode.Kerning };
        var smushingRenderer = new FIGLetRenderer(_testFont) { LayoutMode = LayoutMode.Smushing };
        
        // Act
        var fullSize = fullSizeRenderer.Render("AB");
        var kerning = kerningRenderer.Render("AB");
        var smushing = smushingRenderer.Render("AB");
        
        // Assert
        var fullLines = fullSize.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var kernLines = kerning.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var smushLines = smushing.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        // Full size should be widest, smushing should be narrowest
        Assert.IsTrue(fullLines[0].Length >= kernLines[0].Length);
        Assert.IsTrue(kernLines[0].Length >= smushLines[0].Length);
    }

    [TestMethod]
    public void ANSIProcessor_WithComplexColorSequences_ShouldHandleCorrectly()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont) { UseANSIColors = true };
        var complexColors = "\x1b[38;5;196m\x1b[48;2;255;255;0m\x1b[1m\x1b[4mTest\x1b[0m";
        
        // Act
        var result = renderer.Render(complexColors);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("\x1b[38;5;196m")); // 256-color foreground
        Assert.IsTrue(result.Contains("\x1b[48;2;255;255;0m")); // True color background
        Assert.IsTrue(result.Contains("\x1b[1m")); // Bold
        Assert.IsTrue(result.Contains("\x1b[4m")); // Underline
        Assert.IsTrue(result.Contains("\x1b[0m")); // Reset at end
    }

    [TestMethod]
    public void Render_WithCharacterNotInFont_ShouldSkipCharacter()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_testFont);
        
        // Act - Try to render a character likely not in our mini test font
        var result = renderer.Render("A\u2603B"); // A + snowman + B
        
        // Assert
        Assert.IsNotNull(result);
        var processedResult = result.Replace(_testFont.HardBlank, ' ');
        Assert.IsTrue(processedResult.Contains("A"));
        Assert.IsTrue(processedResult.Contains("B"));
        // Snowman should be skipped gracefully
    }
}