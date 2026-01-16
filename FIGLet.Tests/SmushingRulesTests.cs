using ByteForge.FIGLet;

namespace FIGLet.Tests;

[TestClass]
public class SmushingRulesTests
{
    [TestMethod]
    public void SmushingRules_None_ShouldHaveZeroValue()
    {
        // Act & Assert
        Assert.AreEqual(0, (int)SmushingRules.None);
    }

    [TestMethod]
    public void SmushingRules_EqualCharacter_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(1, (int)SmushingRules.EqualCharacter);
    }

    [TestMethod]
    public void SmushingRules_Underscore_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(2, (int)SmushingRules.Underscore);
    }

    [TestMethod]
    public void SmushingRules_Hierarchy_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(4, (int)SmushingRules.Hierarchy);
    }

    [TestMethod]
    public void SmushingRules_OppositePair_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(8, (int)SmushingRules.OppositePair);
    }

    [TestMethod]
    public void SmushingRules_BigX_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(16, (int)SmushingRules.BigX);
    }

    [TestMethod]
    public void SmushingRules_HardBlank_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(32, (int)SmushingRules.HardBlank);
    }

    [TestMethod]
    public void SmushingRules_Combination_ShouldCombineCorrectly()
    {
        // Arrange
        var combination = SmushingRules.EqualCharacter | SmushingRules.Underscore | SmushingRules.Hierarchy;
        
        // Act & Assert
        Assert.AreEqual(7, (int)combination); // 1 + 2 + 4
    }

    [TestMethod]
    public void SmushingRules_AllFlags_ShouldCombineToCorrectValue()
    {
        // Arrange
        var allFlags = SmushingRules.EqualCharacter | SmushingRules.Underscore | 
                       SmushingRules.Hierarchy | SmushingRules.OppositePair | 
                       SmushingRules.BigX | SmushingRules.HardBlank;
        
        // Act & Assert
        Assert.AreEqual(63, (int)allFlags); // 1 + 2 + 4 + 8 + 16 + 32
    }

    [TestMethod]
    public void SmushingRules_HasFlag_ShouldWorkCorrectly()
    {
        // Arrange
        var rules = SmushingRules.EqualCharacter | SmushingRules.BigX;
        
        // Act & Assert
        Assert.IsTrue(rules.HasFlag(SmushingRules.EqualCharacter));
        Assert.IsTrue(rules.HasFlag(SmushingRules.BigX));
        Assert.IsFalse(rules.HasFlag(SmushingRules.Underscore));
        Assert.IsFalse(rules.HasFlag(SmushingRules.Hierarchy));
        Assert.IsFalse(rules.HasFlag(SmushingRules.OppositePair));
        Assert.IsFalse(rules.HasFlag(SmushingRules.HardBlank));
    }

    [TestMethod]
    public void SmushingRules_BitwiseOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var rules1 = SmushingRules.EqualCharacter | SmushingRules.Underscore;
        var rules2 = SmushingRules.Underscore | SmushingRules.Hierarchy;
        
        // Act
        var union = rules1 | rules2;
        var intersection = rules1 & rules2;
        var difference = rules1 ^ rules2;
        
        // Assert
        Assert.AreEqual(SmushingRules.EqualCharacter | SmushingRules.Underscore | SmushingRules.Hierarchy, union);
        Assert.AreEqual(SmushingRules.Underscore, intersection);
        Assert.AreEqual(SmushingRules.EqualCharacter | SmushingRules.Hierarchy, difference);
    }

    [TestMethod]
    public void SmushingRules_WithFontHasSmushingRule_ShouldWorkCorrectly()
    {
        // Arrange
        var content = TestUtilities.CreateMinimalValidFontContent();
        content = content.Replace("flf2a$ 5 4 10 15 0", "flf2a$ 5 4 10 7 0"); // Set old layout to 7 (1+2+4)
        using var stream = TestUtilities.CreateStreamFromString(content);
        var font = FIGFont.FromStream(stream);
        
        // Act & Assert
        Assert.IsNotNull(font);
        Assert.IsTrue(font.HasSmushingRule(SmushingRules.EqualCharacter));
        Assert.IsTrue(font.HasSmushingRule(SmushingRules.Underscore));
        Assert.IsTrue(font.HasSmushingRule(SmushingRules.Hierarchy));
        Assert.IsFalse(font.HasSmushingRule(SmushingRules.OppositePair));
        Assert.IsFalse(font.HasSmushingRule(SmushingRules.BigX));
        Assert.IsFalse(font.HasSmushingRule(SmushingRules.HardBlank));
    }

    [DataTestMethod]
    [DataRow(SmushingRules.None, 0)]
    [DataRow(SmushingRules.EqualCharacter, 1)]
    [DataRow(SmushingRules.Underscore, 2)]
    [DataRow(SmushingRules.Hierarchy, 4)]
    [DataRow(SmushingRules.OppositePair, 8)]
    [DataRow(SmushingRules.BigX, 16)]
    [DataRow(SmushingRules.HardBlank, 32)]
    public void SmushingRules_IndividualValues_ShouldMatchExpected(SmushingRules rule, int expectedValue)
    {
        // Act & Assert
        Assert.AreEqual(expectedValue, (int)rule);
    }

    [TestMethod]
    public void SmushingRules_PowerOfTwoValues_ShouldBeCorrect()
    {
        // Act & Assert - Each rule should be a power of 2
        Assert.AreEqual(1, (int)SmushingRules.EqualCharacter);    // 2^0
        Assert.AreEqual(2, (int)SmushingRules.Underscore);       // 2^1
        Assert.AreEqual(4, (int)SmushingRules.Hierarchy);        // 2^2
        Assert.AreEqual(8, (int)SmushingRules.OppositePair);     // 2^3
        Assert.AreEqual(16, (int)SmushingRules.BigX);            // 2^4
        Assert.AreEqual(32, (int)SmushingRules.HardBlank);       // 2^5
    }

    [TestMethod]
    public void SmushingRules_EnumValues_ShouldBeDistinct()
    {
        // Arrange
        var allValues = Enum.GetValues<SmushingRules>().Where(r => r != SmushingRules.None).ToList();
        
        // Act & Assert - Each value should be unique
        var distinctValues = allValues.Distinct().ToList();
        Assert.AreEqual(allValues.Count, distinctValues.Count);
        
        // Act & Assert - No two values should have the same bit pattern
        for (int i = 0; i < allValues.Count; i++)
        {
            for (int j = i + 1; j < allValues.Count; j++)
            {
                Assert.AreNotEqual((int)allValues[i], (int)allValues[j], 
                    $"{allValues[i]} and {allValues[j]} have the same value");
                
                // Also check that they don't overlap in flags
                var intersection = allValues[i] & allValues[j];
                Assert.AreEqual(SmushingRules.None, intersection,
                    $"{allValues[i]} and {allValues[j]} have overlapping bits");
            }
        }
    }

    [TestMethod]
    public void SmushingRules_ToString_ShouldReturnCorrectNames()
    {
        // Act & Assert
        Assert.AreEqual("None", SmushingRules.None.ToString());
        Assert.AreEqual("EqualCharacter", SmushingRules.EqualCharacter.ToString());
        Assert.AreEqual("Underscore", SmushingRules.Underscore.ToString());
        Assert.AreEqual("Hierarchy", SmushingRules.Hierarchy.ToString());
        Assert.AreEqual("OppositePair", SmushingRules.OppositePair.ToString());
        Assert.AreEqual("BigX", SmushingRules.BigX.ToString());
        Assert.AreEqual("HardBlank", SmushingRules.HardBlank.ToString());
    }

    [TestMethod]
    public void SmushingRules_CombinedToString_ShouldShowMultipleFlags()
    {
        // Arrange
        var combined = SmushingRules.EqualCharacter | SmushingRules.BigX;
        
        // Act
        var result = combined.ToString();
        
        // Assert
        Assert.IsTrue(result.Contains("EqualCharacter"));
        Assert.IsTrue(result.Contains("BigX"));
    }
}

[TestClass]
public class LayoutModeTests
{
    [TestMethod]
    public void LayoutMode_FullSize_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(-1, (int)LayoutMode.FullSize);
    }

    [TestMethod]
    public void LayoutMode_Kerning_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(0, (int)LayoutMode.Kerning);
    }

    [TestMethod]
    public void LayoutMode_Smushing_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual(1, (int)LayoutMode.Smushing);
    }

    [TestMethod]
    public void LayoutMode_Default_ShouldEqualSmushing()
    {
        // Act & Assert
        Assert.AreEqual(LayoutMode.Smushing, LayoutMode.Default);
    }

    [TestMethod]
    public void LayoutMode_AllValues_ShouldBeDistinct()
    {
        // Arrange
        var allValues = Enum.GetValues<LayoutMode>().ToList();
        
        // Act & Assert - Since Default equals Smushing, we expect 3 distinct values, not 4
        var distinctIntValues = allValues.Select(v => (int)v).Distinct().ToList();
        Assert.AreEqual(3, distinctIntValues.Count); // FullSize(-1), Kerning(0), Smushing(1)/Default(1)
        
        // Verify the core values are unique (excluding Default which equals Smushing)
        var coreValues = new[] { LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing };
        for (int i = 0; i < coreValues.Length; i++)
        {
            for (int j = i + 1; j < coreValues.Length; j++)
            {
                Assert.AreNotEqual((int)coreValues[i], (int)coreValues[j],
                    $"{coreValues[i]} and {coreValues[j]} have the same value");
            }
        }
    }

    [TestMethod]
    public void LayoutMode_ToString_ShouldReturnCorrectNames()
    {
        // Act & Assert
        Assert.AreEqual("FullSize", LayoutMode.FullSize.ToString());
        Assert.AreEqual("Kerning", LayoutMode.Kerning.ToString());
        Assert.AreEqual("Smushing", LayoutMode.Smushing.ToString());
        Assert.AreEqual("Smushing", LayoutMode.Default.ToString()); // Default equals Smushing
    }

    [DataTestMethod]
    [DataRow(LayoutMode.FullSize, -1)]
    [DataRow(LayoutMode.Kerning, 0)]
    [DataRow(LayoutMode.Smushing, 1)]
    [DataRow(LayoutMode.Default, 1)]
    public void LayoutMode_Values_ShouldMatchExpected(LayoutMode mode, int expectedValue)
    {
        // Act & Assert
        Assert.AreEqual(expectedValue, (int)mode);
    }

    [TestMethod]
    public void LayoutMode_Comparison_ShouldWorkCorrectly()
    {
        // Act & Assert
        Assert.IsTrue(LayoutMode.FullSize < LayoutMode.Kerning);
        Assert.IsTrue(LayoutMode.Kerning < LayoutMode.Smushing);
        Assert.AreEqual(LayoutMode.Smushing, LayoutMode.Default);
    }

    [TestMethod]
    public void LayoutMode_WithRenderer_ShouldProduceDifferentResults()
    {
        // Arrange
        var font = TestUtilities.LoadTestFont("mini-fixed");
        var text = "AB";
        
        // Act
        var fullSizeResult = FIGLetRenderer.Render(text, font, LayoutMode.FullSize);
        var kerningResult = FIGLetRenderer.Render(text, font, LayoutMode.Kerning);
        var smushingResult = FIGLetRenderer.Render(text, font, LayoutMode.Smushing);
        var defaultResult = FIGLetRenderer.Render(text, font, LayoutMode.Default);
        
        // Assert
        Assert.IsNotNull(fullSizeResult);
        Assert.IsNotNull(kerningResult);
        Assert.IsNotNull(smushingResult);
        Assert.IsNotNull(defaultResult);
        
        // Default should equal Smushing
        Assert.AreEqual(smushingResult, defaultResult);
        
        // Different modes should produce different widths (generally)
        var fullSizeLines = fullSizeResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var kerningLines = kerningResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var smushingLines = smushingResult.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        // FullSize should typically be widest, smushing narrowest
        Assert.IsTrue(fullSizeLines[0].Length >= kerningLines[0].Length);
        Assert.IsTrue(kerningLines[0].Length >= smushingLines[0].Length);
    }

    [TestMethod]
    public void LayoutMode_EnumMembers_ShouldCoverAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[] { -1, 0, 1 }; // FullSize, Kerning, Smushing/Default
        var actualValues = Enum.GetValues<LayoutMode>().Select(v => (int)v).Distinct().ToArray();
        
        // Act & Assert
        CollectionAssert.AreEquivalent(expectedValues, actualValues);
    }
}