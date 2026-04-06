# 🌐 **BYTEFORGE FIGLET SUITE — FIGLET .NET LIBRARY**

```
██████╗ ██╗   ██╗████████╗███████╗███████╗ ██████╗ ██████╗  ██████╗ ███████╗
██╔══██╗╚██╗ ██╔╝╚══██╔══╝██╔════╝██╔════╝██╔═══██╗██╔══██╗██╔════╝ ██╔════╝
██████╔╝ ╚████╔╝    ██║   █████╗  █████╗  ██║   ██║██████╔╝██║  ███╗█████╗
██╔══██╗  ╚██╔╝     ██║   ██╔══╝  ██╔══╝  ██║   ██║██╔══██╗██║   ██║██╔══╝
██████╔╝   ██║      ██║   ███████╗██║     ╚██████╔╝██║  ██║╚██████╔╝███████╗
╚═════╝    ╚═╝      ╚═╝   ╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝
                 ███████╗██╗ ██████╗ ██╗     ███████╗████████╗    ███████╗██╗   ██╗██╗████████╗███████╗
                 ██╔════╝██║██╔════╝ ██║     ██╔════╝╚══██╔══╝    ██╔════╝██║   ██║██║╚══██╔══╝██╔════╝
                 █████╗  ██║██║  ███╗██║     █████╗     ██║       ███████╗██║   ██║██║   ██║   █████╗
                 ██╔══╝  ██║██║   ██║██║     ██╔══╝     ██║       ╚════██║██║   ██║██║   ██║   ██╔══╝
                 ██║     ██║╚██████╔╝███████╗███████╗   ██║       ███████║╚██████╔╝██║   ██║   ███████╗
                 ╚═╝     ╚═╝ ╚═════╝ ╚══════╝╚══════╝   ╚═╝       ╚══════╝ ╚═════╝ ╚═╝   ╚═╝   ╚══════╝
```

> **ByteForge.FIGLet**
> *A fast, spec‑compliant FIGLet engine for .NET.*

## 📘 Overview

`ByteForge.FIGLet` is the C# / .NET implementation of the FIGLet rendering engine at the heart of the ByteForge FIGLet Suite.

It provides a robust and efficient implementation of the FIGLet specification, allowing you to create ASCII art from text using FIGLet fonts. It supports all standard FIGLet features including various smushing rules, layout modes, ANSI color preservation, and Unicode.

This library powers:

- The **Visual Studio FIGLet Comment Generator** extension
- The **FIGPrint CLI**
- Any .NET application that needs FIGLet rendering

## ✨ Features

- 🔤 Render FIGLet text using any `.flf` font
- 📄 Full FIGLet font (`.flf`) file parsing and loading
- 🗜️ Automatic handling of compressed/zipped font files
- 🎨 ANSI color support for terminal output
- 🌏 Unicode support including surrogate pairs
- 📝 Paragraph formatting support
- ⚙️ Supports Full Size, Kerning, and Smushing layout modes
- 🧠 Implements all official smushing rules
- 📦 Default embedded font included — works out of the box
- 🚀 No external dependencies, fast and lightweight
- 🧱 Multi-target: .NET Framework 4.7.2 through .NET 10.0

### Sample Output

```
  _  _     _ _          _        _       _    _ _
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
                  |/
```

## 🛠 Installation

Install via **NuGet**:

```bash
dotnet add package FIGLet
```

Or via the NuGet Package Manager UI — search for `FIGLet`.

## 🚀 Quick Start

### Basic Usage

```csharp
using ByteForge.FIGLet;

// Load a font from a file (returns null if the file is not found)
var font = FIGFont.FromFile("standard.flf") ?? throw new FileNotFoundException("Font not found");

// Create a renderer
var renderer = new FIGLetRenderer(font);

// Render text
string asciiArt = renderer.Render("Hello, World!");
Console.WriteLine(asciiArt);
```

### Using the Default Font

The library ships with a built-in default font — no file required:

```csharp
using ByteForge.FIGLet;

var renderer = new FIGLetRenderer(FIGFont.Default);
Console.WriteLine(renderer.Render("Hello, World!"));
```

### Static Rendering

For one-shot rendering without creating a renderer instance:

```csharp
using ByteForge.FIGLet;

string asciiArt = FIGLetRenderer.Render("Hello, World!");
Console.WriteLine(asciiArt);
```

## ⚙️ Layout Modes

The library supports three layout modes:

| Mode | Value | Description |
|------|-------|-------------|
| `LayoutMode.FullSize` | `-1` | No character compression — each character is rendered at full width |
| `LayoutMode.Kerning` | `0` | Characters are moved together until they touch but do not overlap |
| `LayoutMode.Smushing` | `1` | Characters are merged according to the font's smushing rules (default) |

```csharp
using ByteForge.FIGLet;

var font = FIGFont.Default;

string fullSize = FIGLetRenderer.Render("Text", font, LayoutMode.FullSize);
string kerning  = FIGLetRenderer.Render("Text", font, LayoutMode.Kerning);
string smushing = FIGLetRenderer.Render("Text", font, LayoutMode.Smushing);
```

## 🧩 Smushing Rules

The library implements all official FIGLet smushing rules as defined in the FIGLet specification. The font file determines which rules are active.

| Rule | Flag | Description |
|------|------|-------------|
| Equal Character | `1` | Two identical characters smush into one |
| Underscore | `2` | Underscore is replaced by a character from the hierarchy |
| Hierarchy | `4` | Characters from "higher" classes replace those from "lower" ones |
| Opposite Pair | `8` | Matching bracket/parenthesis pairs smush into a vertical bar |
| Big X | `16` | `/+\` → `\|`, `\+/` → `Y`, `>+<` → `X` |
| Hardblank | `32` | Two hardblanks smush into one hardblank |

You can inspect a font's active rules at runtime:

```csharp
using ByteForge.FIGLet;

// FromFile returns null if the file is not found
var font = FIGFont.FromFile("standard.flf") ?? throw new FileNotFoundException("Font not found");

bool hasEqualCharRule  = font.HasSmushingRule(SmushingRules.EqualCharacter);
bool hasUnderscoreRule = font.HasSmushingRule(SmushingRules.Underscore);
```

## 📁 Font Support

- Standard `.flf` font files
- Compressed `.flf` files inside `.zip` archives (auto-detected via `FIGFontStream`)

## 🎨 ANSI Color Support

The library preserves ANSI color codes through the rendering process, allowing you to create colorful FIGLet text in terminals:

```csharp
using ByteForge.FIGLet;

var renderer = new FIGLetRenderer(
    FIGFont.Default,
    useANSIColors: true
);

string colorfulText = "\x1b[31mRed\x1b[0m \x1b[32mGreen\x1b[0m \x1b[34mBlue\x1b[0m";
Console.WriteLine(renderer.Render(colorfulText));
```

## 📝 Paragraph Mode

When enabled (the default), blank lines in the input produce separate FIGLet renderings spaced by the font's character height:

```csharp
using ByteForge.FIGLet;

var renderer = new FIGLetRenderer(FIGFont.Default);

string paragraphs = "Paragraph 1\n\nParagraph 2";
Console.WriteLine(renderer.Render(paragraphs));
```
Output:
```text
  ___                                  _        _ 
 | _ \__ _ _ _ __ _ __ _ _ _ __ _ _ __| |_     / |
 |  _/ _` | '_/ _` / _` | '_/ _` | '_ \ ' \    | |
 |_| \__,_|_| \__,_\__, |_| \__,_| .__/_||_|   |_|
                   |___/         |_|              





  ___                                  _        ___ 
 | _ \__ _ _ _ __ _ __ _ _ _ __ _ _ __| |_     |_  )
 |  _/ _` | '_/ _` / _` | '_/ _` | '_ \ ' \     / / 
 |_| \__,_|_| \__,_\__, |_| \__,_| .__/_||_|   /___|
                   |___/         |_|                

```

## 🌏 Unicode Support

The library fully supports Unicode characters including surrogate pairs. Characters not present in the font are skipped gracefully:

```csharp
using ByteForge.FIGLet;

var renderer = new FIGLetRenderer(FIGFont.Default);
Console.WriteLine(renderer.Render("Hello 😊 World!"));
```
Output:
```
  _  _     _ _          __      __       _    _ _ 
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \      \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___/       \_/\_/\___/_| |_\__,_(_)

```
## 📖 API Reference

### `FIGFont`

Handles font loading and storage.

| Member | Description |
|--------|-------------|
| `FIGFont.Default` | Returns the built-in default font (lazy-loaded, cached) |
| `FIGFont.FromFile(path)` | Loads a font from a `.flf` or `.zip` file |
| `FIGFont.FromStream(stream)` | Loads a font from a `Stream` |
| `FIGFont.FromReader(reader)` | Loads a font from a `TextReader` |
| `FIGFont.FromArray(lines)` | Parses a font from a `string[]` |
| `.Height` | Character height in rows |
| `.HardBlank` | The hard-blank character |
| `.Characters` | Dictionary mapping character code point → glyph rows |
| `.SmushingRules` | Active smushing rules flags |
| `.PrintDirection` | `0` = left-to-right, `1` = right-to-left |
| `.HasSmushingRule(rule)` | Tests whether a specific rule is active |

### `FIGLetRenderer`

Core rendering engine.

| Member | Description |
|--------|-------------|
| `new FIGLetRenderer(font, mode?, separator?, useANSIColors?, paragraphMode?)` | Create an instance |
| `FIGLetRenderer.Render(text, font?, mode?, separator?, useANSIColors?, paragraphMode?)` | Static one-shot render |
| `.Render(text)` | Render text using instance settings |
| `.LayoutMode` | Active `LayoutMode` |
| `.LineSeparator` | Line separator string (default: platform line separator) |
| `.UseANSIColors` | Whether to preserve ANSI color codes through rendering |
| `.ParagraphMode` | Whether blank lines produce separate FIGLet renders |

### `LayoutMode`

```csharp
enum LayoutMode
{
    FullSize = -1,   // No character compression
    Kerning  =  0,   // Minimal spacing
    Smushing =  1,   // Full character overlap (default)
    Default  =  1,
}
```

### `SmushingRules`

```csharp
[Flags]
enum SmushingRules
{
    None           =  0,
    EqualCharacter =  1,
    Underscore     =  2,
    Hierarchy      =  4,
    OppositePair   =  8,
    BigX           = 16,
    HardBlank      = 32,
}
```

## 🏗 Implementation Details

1. **FIGFont** — Parses `.flf` font files; supports loading from files, streams, readers, or string arrays; handles ZIP-compressed font files via `FIGFontStream`; manages smushing rule configuration.

2. **FIGLetRenderer** — Converts input text to FIGLet output; implements character smushing logic; handles different layout modes; processes ANSI color codes in a single pre-pass; supports paragraph formatting and RTL fonts.

3. **LayoutMode** — Enumeration controlling how characters are combined during rendering.

4. **SmushingRules** — Flags enumeration defining which character-combining rules are active.

## ⚡ Performance Considerations

- Default font is lazy-loaded once and cached for the application lifetime
- ANSI color codes are handled in a single pre-pass, not during rendering
- Surrogate pairs are supported without performance degradation
- Efficient string building throughout the rendering pipeline

## 🔧 Used By

This library powers:

- **Visual Studio FIGLet Comment Generator** extension
- **FIGPrint** CLI tool
- Any .NET application that needs ASCII art banners

## 🔗 Related Packages

- **[@byte-forge/figlet](https://www.npmjs.com/package/@byte-forge/figlet)** — The equivalent TypeScript / Node.js library (npm)
- **[FIGLet Comment Generator (VS Code)](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.figlet-comment-generator)** — VS Code extension
- **[FIGLet Comment Generator (Visual Studio)](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.FIGLetCommentGenerator)** — Visual Studio 2022+ extension

## 🤝 Contributing

Contributions are welcome!
To contribute:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Open a Pull Request

## 📜 License

This library is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

## 💡 Credits

- Original FIGLet concept by **Frank, Ian & Glenn**
- Implementations by **Paulo Santos (ByteForge)**
- FIGLet specification: [figlet.org](http://www.figlet.org/)

## Support

If you encounter any issues or have feature requests, please:
1. Search existing [issues](https://github.com/PaulStSmith/figlet-comment-generator/issues)
2. Create a new issue if needed

---

Made with ❤️ by Paulo Santos
