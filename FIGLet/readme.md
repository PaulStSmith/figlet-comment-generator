# FIGLet Core Library

A C# implementation of FIGLet (Frank, Ian & Glenn's letters) - a program for making large letters out of ordinary text.

## Overview

This library provides a robust and efficient implementation of the FIGLet specification, allowing you to create ASCII art from text using FIGLet fonts. It supports all standard FIGLet features including various smushing rules and layout modes.

## Features

- Full FIGLet font (.flf) file parsing and loading
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
- Thread-safe design
- Efficient string manipulation
- Comprehensive XML documentation

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

The library now supports ANSI color codes for terminal output, allowing you to create colorful FIGLet text:

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

### Smushing Rules

The library implements all standard FIGLet smushing rules as defined in the FIGLet specification:

```csharp
// Check if a specific smushing rule is enabled
bool hasEqualCharRule = font.HasSmushingRule(SmushingRules.EqualCharacter);
bool hasUnderscoreRule = font.HasSmushingRule(SmushingRules.Underscore);
```

## Implementation Details

The library consists of four main components:

1. `FIGFont`: Handles font loading and storage
   - Parses .flf font files
   - Stores character data and font metadata
   - Manages smushing rules configuration

2. `FIGLetRenderer`: Core rendering engine
   - Converts input text to FIGLet output
   - Implements character smushing logic
   - Handles different layout modes
   - Processes ANSI color codes

3. `LayoutMode`: Enumeration defining rendering modes
   - Controls how characters are combined

4. `SmushingRules`: Flags enumeration for smushing rules
   - Defines available character combining rules

## Performance Considerations

- Uses `StringBuilder` for efficient string manipulation
- Lazy initialization where appropriate
- Immutable design for thread safety
- Efficient regular expressions for whitespace handling
- Optimized character smushing calculations
- Intelligent ANSI color code handling

## Requirements

- .NET 8.0 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- Original FIGLet concept by Frank, Ian & Glenn
- FIGLet specifications: http://www.org/
- Implementation by Paulo Santos