# FIGLet.Tests

Unit and integration test suite for the FIGLet library (`ByteForge.FIGLet`).

## Overview

This test project provides comprehensive coverage for the FIGLet rendering engine, including font parsing, text rendering, smushing rules, and edge cases.

## Requirements

- .NET 9.0 SDK
- MSTest 3.6.4

## Running Tests

```bash
# Run all tests
dotnet test FIGLet.Tests/FIGLet.Tests.csproj

# Run with verbose output
dotnet test FIGLet.Tests/FIGLet.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~FIGFontTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~DefaultFont_ShouldLoad_Successfully"

# Run tests by category
dotnet test --filter "TestCategory=Performance"
```

## Test Structure

| File                     | Description                                                        |
|--------------------------|--------------------------------------------------------------------|
| `FIGFontTests.cs`        | Tests for `FIGFont` class - font loading, parsing, and validation  |
| `FIGLetRendererTests.cs` | Tests for `FIGLetRenderer` class - text rendering and layout modes |
| `SmushingRulesTests.cs`  | Tests for all 6 smushing rules and their interactions              |
| `IntegrationTests.cs`    | End-to-end tests combining font loading and rendering              |
| `PerformanceTests.cs`    | Benchmarks and performance regression tests                        |
| `TestUtilities.cs`       | Helper methods for creating test fonts and assertions              |

## Test Resources

### TestFonts/

Embedded font files used for testing:

- `mini.flf` - Minimal font for basic tests
- `mini-fixed.flf` - Fixed-width minimal font
- `smushing-test.flf` - Font with all smushing rules enabled (63)

### ExpectedOutputs/

Reference text files containing expected rendering outputs for regression testing.

## Test Categories

### Font Loading Tests
- Default font loading and caching
- Loading from file, stream, reader, and string array
- Null and empty input handling
- Invalid format detection
- ZIP archive extraction

### Font Parsing Tests
- Header parsing (signature, dimensions, layout parameters)
- Comment extraction
- Character data parsing (ASCII 32-126)
- Extended Unicode characters
- Print direction (LTR/RTL)

### Layout Mode Tests
- `FullSize` - No character overlap
- `Kerning` - Minimal spacing
- `Smushing` - Full character merging

### Smushing Rule Tests
- `EqualCharacter` - Identical characters merge
- `Underscore` - Underscore merges with hierarchy chars
- `Hierarchy` - Character priority-based merging
- `OppositePair` - Bracket pairs merge (e.g., `[]` → `|`)
- `BigX` - Slash combinations form X
- `HardBlank` - Hard blank character merging

### Performance Tests
- Large text rendering benchmarks
- Font caching verification
- Memory allocation patterns

## TestUtilities

The `TestUtilities` class provides helper methods:

```csharp
// Load embedded test font
var font = TestUtilities.LoadTestFont("mini-fixed");

// Create minimal valid font content programmatically
var content = TestUtilities.CreateMinimalValidFontContent(height: 5, hardBlank: '$');

// Assert smushing rules
TestUtilities.AssertSmushingRule(font, SmushingRules.EqualCharacter, shouldHave: true);

// Performance assertions
TestUtilities.AssertPerformance(() => renderer.Render(text),
    maxExpectedDuration: TimeSpan.FromMilliseconds(100));

// Create ZIP archive with font for testing
var zipBytes = TestUtilities.CreateZipWithFontFile(fontContent, "test.flf");
```

## Adding New Tests

1. Add test methods to the appropriate test class
2. Use `[TestMethod]` attribute for standard tests
3. Use `[DataTestMethod]` with `[DataRow]` for parameterized tests
4. Add new test fonts to `TestFonts/` as embedded resources
5. Add expected outputs to `ExpectedOutputs/` for regression tests

Example:

```csharp
[TestMethod]
public void MyFeature_WithSpecificInput_ShouldProduceExpectedOutput()
{
    // Arrange
    var font = TestUtilities.LoadTestFont("mini-fixed");
    var renderer = new FIGLetRenderer(font);

    // Act
    var result = renderer.Render("Test");

    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Contains("expected content"));
}
```

## License

MIT License - See the root LICENSE file for details.
