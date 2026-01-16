# FIGLet Library Test Battle Plan

## Project Overview
This document outlines a comprehensive testing strategy for the **FIGLet** library (`ByteForge.FIGLet` namespace), which provides ASCII art text rendering capabilities using FIGFont files. The library consists of four main components that require thorough testing.

## Core Components Analysis

### 1. FIGFont Class (`FIGFont.cs`)
**Purpose**: Parses and represents FIGFont (.flf) files with character definitions and smushing rules.

**Key Functionality**:
- Static factory methods for loading fonts from files, streams, readers, and string arrays
- Default font management with embedded resource loading
- FIGFont header parsing (signature, dimensions, layout parameters)
- Character data parsing for ASCII 32-126 and extended Unicode characters
- Layout parameter parsing to determine smushing rules
- Support for both old layout and full layout modes
- Number parsing utilities (decimal, hex, binary, octal)

### 2. FIGLetRenderer Class (`FIGletRenderer.cs`)
**Purpose**: Renders text using FIGFont with various layout modes and advanced features.

**Key Functionality**:
- Text rendering with multiple layout modes (FullSize, Kerning, Smushing)
- Character smushing with 6 different rules
- ANSI color code processing and preservation
- Paragraph mode with line break handling
- Right-to-left text support for RTL fonts
- Unicode surrogate pair handling
- Complex overlap calculation and character merging algorithms

### 3. LayoutMode Enum (`LayoutMode.cs`)
**Purpose**: Defines text rendering layout modes.

**Values**:
- `FullSize = -1`: No character overlap
- `Kerning = 0`: Minimal character spacing
- `Smushing = 1`: Character merging with rules
- `Default = Smushing`: Default behavior

### 4. SmushingRules Enum (`SmushingRules.cs`)
**Purpose**: Flags enum defining character smushing behavior.

**Rules**:
- `None = 0`: No smushing
- `EqualCharacter = 1`: Merge identical characters
- `Underscore = 2`: Underscore merges with hierarchy chars
- `Hierarchy = 4`: Character hierarchy-based merging
- `OppositePair = 8`: Opposite bracket pairs merge
- `BigX = 16`: Slash combinations form X
- `HardBlank = 32`: Hard blank character merging

### 5. FIGFontStream Class (`FIGFontStream.cs`)
**Purpose**: Stream wrapper supporting both regular streams and ZIP archive first entries.

**Key Functionality**:
- ZIP file detection via PK signature
- Automatic extraction of first ZIP entry
- Fallback to regular stream processing
- Buffer management for signature bytes

## Test Strategy Categories

### 1. Unit Tests for FIGFont Class

#### Font Loading Tests
- **Valid Font Loading**
  - Load default embedded font
  - Load font from valid .flf file
  - Load font from stream
  - Load font from TextReader
  - Load font from string array

- **Invalid Input Handling**
  - Null file path handling
  - Non-existent file handling
  - Null stream handling
  - Null reader handling
  - Empty/null string array handling
  - Invalid font format (missing signature)
  - Corrupted font header
  - Malformed character data

- **Font Header Parsing**
  - Signature validation ("flf2a")
  - Hard blank character extraction
  - Height, baseline, max length parsing
  - Old layout parameter parsing
  - Full layout parameter parsing
  - Comment line counting and extraction
  - Print direction parameter

#### Character Data Tests
- **Required Character Range**
  - All ASCII characters 32-126 present
  - Correct character height consistency
  - Proper line termination handling
  - Hard blank character variations

- **Extended Characters**
  - Unicode character support
  - Code point line parsing
  - Character data validation
  - Surrogate pair handling

#### Layout Parameter Tests
- **Old Layout Mode**
  - Value -1 (no smushing)
  - Value 0 (kerning)
  - Positive values (smushing rules)
  - Rule extraction and mapping

- **Full Layout Mode**
  - Horizontal smushing bit (bit 0)
  - Smushing rules extraction (bits 1-6)
  - Rule combination validation
  - Priority over old layout

#### Number Parsing Tests
- **ParseInt Method**
  - Decimal numbers
  - Hexadecimal (0x prefix)
  - Binary (0b prefix)
  - Octal (0 prefix)
  - Negative numbers
  - Invalid formats
  - Edge cases (empty, whitespace)

### 2. Unit Tests for FIGLetRenderer Class

#### Basic Rendering Tests
- **Simple Text Rendering**
  - Single character rendering
  - Multiple character strings
  - Empty string handling
  - Whitespace-only strings
  - Special characters

- **Layout Mode Testing**
  - FullSize mode (no overlap)
  - Kerning mode (minimal spacing)
  - Smushing mode (rule-based merging)
  - Mode switching behavior

#### Smushing Rules Tests
- **Individual Rule Testing**
  - Equal character smushing
  - Underscore smushing with hierarchy chars
  - Hierarchy smushing priority
  - Opposite pair smushing (brackets)
  - Big X smushing (slashes to X, arrows to X)
  - Hard blank smushing

- **Rule Combination Testing**
  - Multiple rules active
  - Rule priority conflicts
  - Rule interaction edge cases

#### Advanced Feature Tests
- **ANSI Color Support**
  - Color code detection and preservation
  - Color state management
  - Non-color ANSI sequence filtering
  - Color reset handling
  - Complex color sequences

- **Paragraph Mode**
  - Line break processing
  - Empty line handling
  - Multi-paragraph text
  - Paragraph mode vs single line mode

- **RTL Support**
  - Right-to-left text rendering
  - Character order reversal
  - Color code position adjustment
  - Unicode text element handling

#### Overlap Calculation Tests
- **Character Overlap Logic**
  - Whitespace overlap calculation
  - Non-whitespace character detection
  - Maximum overlap constraints
  - Special case handling (opposing slashes)

#### Unicode and Encoding Tests
- **Surrogate Pair Handling**
  - High/low surrogate detection
  - UTF-32 conversion
  - Proper character indexing
  - Font character availability

### 3. Integration Tests

#### Font and Renderer Integration
- **Different Font Compatibility**
  - Various .flf files
  - Different character heights
  - Different smushing rule sets
  - Character availability variations

- **Rendering Pipeline**
  - Font loading to rendering workflow
  - Error propagation
  - Performance with large texts
  - Memory usage patterns

#### Real-world Scenarios
- **Typical Use Cases**
  - Code comment generation
  - Banner text creation
  - Multi-line text rendering
  - Colored output generation

### 4. Edge Case and Error Handling Tests

#### Boundary Conditions
- **Input Limits**
  - Very long text strings
  - Maximum character heights
  - Unicode boundary characters
  - Memory constraints

#### Error Recovery
- **Graceful Degradation**
  - Missing characters in font
  - Corrupt font data recovery
  - Invalid smushing configurations
  - Stream reading errors

### 5. Performance Tests

#### Rendering Performance
- **Benchmarking**
  - Small vs large text rendering
  - Different layout modes performance
  - Color processing overhead
  - Memory allocation patterns

#### Font Loading Performance
- **Loading Optimization**
  - Default font caching
  - File vs stream loading
  - ZIP archive extraction
  - Character parsing efficiency

### 6. Compatibility Tests

#### File Format Support
- **FIGFont Variations**
  - Different FIGfont versions
  - Various character encodings
  - Compressed (.flf.zip) files
  - Font with extended character sets

#### Platform Compatibility
- **Cross-platform Behavior**
  - Line separator handling
  - File path compatibility
  - Encoding differences

## Test Data Requirements

### Sample Fonts
- **Default Font**: `small.flf` (included)
- **Test Fonts**: Various heights, styles, and rule sets
- **Edge Case Fonts**: Minimal, maximal, and unusual configurations
- **Broken Fonts**: For error handling tests

### Test Strings
- **Basic**: "Hello", "Test", "123"
- **Special**: Unicode characters, RTL text, mixed content
- **Edge**: Empty strings, very long strings, special characters
- **ANSI**: Colored text samples with various escape sequences

### Expected Outputs
- **Reference Renderings**: Known good outputs for regression testing
- **Layout Comparisons**: Same text in different modes
- **Color Preservation**: ANSI sequence handling verification

## Test Framework Recommendations

### MSTest Structure
- **Test Classes**: One per main component (FIGFontTests, FIGLetRendererTests)
- **Test Categories**: Unit, Integration, Performance, Edge Cases
- **Data-driven Tests**: Use `[DataRow]` for multiple input scenarios
- **Resource Management**: Embedded test fonts and expected outputs

### Test Organization
```
FIGLet.Tests/
├── FIGFontTests.cs              // FIGFont class tests
├── FIGLetRendererTests.cs       // FIGLetRenderer class tests
├── SmushingRulesTests.cs        // Smushing behavior tests
├── IntegrationTests.cs          // Cross-component tests
├── PerformanceTests.cs          // Benchmarking tests
├── TestFonts/                   // Sample .flf files
├── ExpectedOutputs/             // Reference renderings
└── TestUtilities.cs             // Helper methods and utilities
```

### Key Testing Principles
1. **Comprehensive Coverage**: Test all public methods and edge cases
2. **Isolation**: Unit tests should not depend on external files when possible
3. **Repeatability**: Tests should produce consistent results
4. **Documentation**: Clear test names and descriptions
5. **Performance Awareness**: Include performance regression detection
6. **Error Validation**: Verify proper exception handling and error messages

This battle plan provides a roadmap for creating a robust test suite that ensures the FIGLet library functions correctly across all supported scenarios and edge cases.
