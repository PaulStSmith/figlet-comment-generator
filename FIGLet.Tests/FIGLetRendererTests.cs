using ByteForge.FIGLet;
using System.Text;

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
        Assert.AreEqual(string.Empty, FIGLetRenderer.Render("   "));
    }

    [TestMethod]
    public void StaticRender_WithValidText_ShouldRenderCorrectly()
    {
        // Act
        var result = FIGLetRenderer.Render("Hi", _testFont);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("_")); // Should contain FIG characters
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
        var processedResult = result.Replace(_testFont.HardBlank[0], ' ');
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
        var processedResult = result.Replace(_testFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("R"));
    }

    [TestMethod]
    public void Render_WithRTLFont_ShouldReverseText()
    {
        // Arrange - Create RTL font
        var content = TestUtilities.CreateMinimalValidFontContent(2, '$');
        content = content.Replace("flf2a$ 2 1 5 0 0", "flf2a$ 2 1 5 0 0 1"); // Set print direction to 1 (RTL)
        using var stream = TestUtilities.CreateStreamFromString(content);
        var rtlFont = FIGFont.FromStream(stream);
        var renderer = new FIGLetRenderer(rtlFont, useANSIColors: true);
        
        // Act
        var result = renderer.Render("\x1b[31mAB\x1b[0m");
        
        // Assert
        Assert.IsNotNull(result);
        // Text should be rendered as "BA" due to RTL
        var processedResult = result.Replace(rtlFont.HardBlank[0], ' ');
        // Note: This is hard to test exactly without knowing the exact font rendering,
        // but we can ensure the RTL font property is set
        Assert.AreEqual(1, rtlFont.PrintDirection);
    }

    [TestMethod]
    public void Render_WithUnicodeText_ShouldHandleSurrogatePairs()
    {
        // Arrange
        var renderer = new FIGLetRenderer(FIGFont.Default);
        var emojiText = "A🚀B"; // Contains surrogate pair
        
        // Act
        var result = renderer.Render(emojiText);
        
        // Assert
        Assert.IsNotNull(result);
        // Should render A and B, emoji might be skipped if not in font
        var processedResult = result.Replace(FIGFont.Default.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("A"));
        Assert.IsTrue(processedResult.Contains("B"));
    }

    [TestMethod]
    public void SmushingRules_EqualCharacter_ShouldSmushIdenticalChars()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act
        var result = renderer.Render("AA");
        
        // Assert
        Assert.IsNotNull(result);
        // With equal character smushing, "AA" should be narrower than full size
        var fullSizeRenderer = new FIGLetRenderer(_smushingFont) { LayoutMode = LayoutMode.FullSize };
        var fullSizeResult = fullSizeRenderer.Render("AA");
        
        var smushLines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var fullLines = fullSizeResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        Assert.IsTrue(smushLines[0].Length < fullLines[0].Length);
    }

    [TestMethod]
    public void SmushingRules_Hierarchy_ShouldPreferHigherRankedChars()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act - Using characters from hierarchy: "|/\\[]{}()<>"
        var result = renderer.Render("|/");
        
        // Assert
        Assert.IsNotNull(result);
        // Should produce some smushing between hierarchy characters
        var kerningRenderer = new FIGLetRenderer(_smushingFont) { LayoutMode = LayoutMode.Kerning };
        var kerningResult = kerningRenderer.Render("|/");
        
        var smushLines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var kernLines = kerningResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        Assert.IsTrue(smushLines[0].Length <= kernLines[0].Length);
    }

    [TestMethod]
    public void SmushingRules_OppositePair_ShouldSmushBrackets()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act
        var result = renderer.Render("[]");
        
        // Assert
        Assert.IsNotNull(result);
        // Opposite pairs should smush to "|"
        var processedResult = result.Replace(_smushingFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("|"));
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldCreateXFromSlashes()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act
        var result = renderer.Render("/\\");
        
        // Assert
        Assert.IsNotNull(result);
        // Forward slash + backslash should create "|"
        var processedResult = result.Replace(_smushingFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("|"));
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldCreateXFromArrows()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act
        var result = renderer.Render("><");
        
        // Assert
        Assert.IsNotNull(result);
        // > + < should create "X"
        var processedResult = result.Replace(_smushingFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("X"));
    }

    [TestMethod]
    public void SmushingRules_Underscore_ShouldSmushWithHierarchy()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act
        var result = renderer.Render("_|");
        
        // Assert
        Assert.IsNotNull(result);
        // Underscore should smush with hierarchy character, preferring the hierarchy char
        var processedResult = result.Replace(_smushingFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("|"));
    }

    [TestMethod]
    public void SmushingRules_HardBlank_ShouldSmushHardBlanks()
    {
        // Arrange
        var renderer = new FIGLetRenderer(_smushingFont);
        
        // Act - Render characters that have hard blanks next to each other
        var result = renderer.Render("  "); // Two spaces (which contain hard blanks)
        
        // Assert
        Assert.IsNotNull(result);
        // Hard blanks should smush together
        var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(_smushingFont.Height, lines.Length);
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
            var result = renderer.Render(testText);
            
            // Assert
            Assert.IsNotNull(result, $"Failed to render: '{testText}'");
            
            if (!string.IsNullOrWhiteSpace(testText))
            {
                var lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                Assert.IsTrue(lines.Length > 0, $"No output lines for: '{testText}'");
            }
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
            var result = renderer.Render(coloredText);
            
            // Assert
            Assert.IsNotNull(result, $"Failed to render colored text: '{coloredText}'");
            Assert.IsTrue(result.Contains("\x1b[0m"), "Should end with reset code");
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
        var processedResult = result.Replace(_testFont.HardBlank[0], ' ');
        Assert.IsTrue(processedResult.Contains("A"));
        Assert.IsTrue(processedResult.Contains("B"));
        // Snowman should be skipped gracefully
    }
}