# FIGLet Core Library

A C# implementation of FIGLet (Frank, Ian & Glenn's letters) - a program for making large letters out of ordinary text.

## Overview

This library provides a robust and efficient implementation of the FIGLet specification, allowing you to create ASCII art from text using FIGLet fonts. It supports all standard FIGLet features including various smushing rules and layout modes.

## Features

- Full FIGLet font (.flf) file parsing and loading
- Automatic handling of compressed/zipped font files
- Support for all standard FIGLet smushing rules:
  - Equal Character
  - Underscore
  - Hierarchy
  - Opposite Pair
  - Big X
  - Hard Blank
- Multiple layout modes:
  - Full Size (no smushing)
  - Kerning (minimal smushing)
  - Smushing (full character overlap)
- ANSI color support for terminal output
- Unicode support including surrogate pairs
- Thread-safe design
- Efficient string manipulation with StringBuilder
- Automatic handling of paragraph formatting
- Default embedded font included
- Comprehensive XML documentation
- Nullable reference type support

## Usage

```csharp
// Load a FIGLet font
var font = FIGFont.FromFile("standard.flf");

// Create a renderer
var renderer = new FIGLetRenderer(font);

// Render text
string asciiArt = renderer.Render("Hello World!", LayoutMode.Smushing);
Console.WriteLine(asciiArt);
```

### Using the Default Font

The library comes with a built-in default font:

```csharp
// Use the default embedded font
var renderer = new FIGLetRenderer();

// Render text
string asciiArt = renderer.Render("Hello World!");
Console.WriteLine(asciiArt);
```

### Sample Output

Below is an example of text rendered using the default settings:

```
  _  _     _ _          _        _       _    _ _ 
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
                  |/                               
```

### Layout Modes

The library supports three layout modes:

1. `LayoutMode.FullSize`: No character compression
2. `LayoutMode.Kerning`: Basic spacing adjustment
3. `LayoutMode.Smushing`: Full character combining (default)

```csharp
// Example with different layout modes
string fullSize = renderer.Render("Text", LayoutMode.FullSize);
string kerning = renderer.Render("Text", LayoutMode.Kerning);
string smushing = renderer.Render("Text", LayoutMode.Smushing);
```

### ANSI Color Support

The library supports ANSI color codes for terminal output, allowing you to create colorful FIGLet text:

```csharp
// Create a renderer with ANSI color support enabled
var colorRenderer = new FIGLetRenderer(font, useANSIColors: true);

// Render text with ANSI colors
string colorfulText = "\u001b[31mRed\u001b[0m \u001b[32mGreen\u001b[0m \u001b[34mBlue\u001b[0m";
string colorfulAsciiArt = colorRenderer.Render(colorfulText);
Console.WriteLine(colorfulAsciiArt);
```

You can also enable color support when using the static rendering methods:

```csharp
string colorfulAsciiArt = FIGLetRenderer.Render(colorfulText, font, LayoutMode.Smushing, useANSIColors: true);
```

### Paragraph Mode

The library can automatically handle paragraphs in input text:

```csharp
// Multi-paragraph text
string paragraphs = "Paragraph 1\n\nParagraph 2";

// Enable paragraph mode (enabled by default)
var renderer = new FIGLetRenderer(font, paragraphMode: true);
string formattedText = renderer.Render(paragraphs);
```

### Smushing Rules

The library implements all standard FIGLet smushing rules as defined in the FIGLet specification:

```csharp
// Check if a specific smushing rule is enabled
bool hasEqualCharRule = font.HasSmushingRule(SmushingRules.EqualCharacter);
bool hasUnderscoreRule = font.HasSmushingRule(SmushingRules.Underscore);
```

### Unicode Support

The library fully supports Unicode characters, including surrogate pairs:

```csharp
// Render Unicode text
string unicodeArt = renderer.Render("Hello 😊 World! 你好，世界！");
```

## Implementation Details

The library consists of the following main components:

1. **FIGFont**: Handles font loading and storage
   - Parses .flf font files
   - Supports loading from files, streams, or embedded resources
   - Handles compressed/zipped font files
   - Stores character data and font metadata
   - Manages smushing rules configuration

2. **FIGLetRenderer**: Core rendering engine
   - Converts input text to FIGLet output
   - Implements character smushing logic
   - Handles different layout modes
   - Processes ANSI color codes
   - Supports paragraph formatting

3. **LayoutMode**: Enumeration defining rendering modes
   - Controls how characters are combined

4. **SmushingRules**: Flags enumeration for smushing rules
   - Defines available character combining rules

5. **FIGFontStream**: Utility class for font loading
   - Handles automatic detection and extraction of zipped fonts

## Performance Considerations

- Uses `StringBuilder` for efficient string manipulation
- Lazy initialization of the default font
- Immutable font design for thread safety
- Efficient regular expressions for whitespace handling
- Optimized character smushing calculations
- Intelligent ANSI color code handling
- Support for surrogate pairs without performance degradation

## Supported Frameworks

- .NET Framework 4.7.2, 4.8, 4.8.1
- .NET Standard 2.0, 2.1
- .NET Core 3.1
- .NET 5.0, 6.0, 7.0, 8.0, 9.0

## NuGet Installation

Install the package from NuGet:

```
Install-Package FIGLet
```

Or using the .NET CLI:

```
dotnet add package FIGLet
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- Original FIGLet concept by Frank, Ian & Glenn
- FIGLet specifications: http://www.figlet.org/
- Implementation by Paulo Santos
