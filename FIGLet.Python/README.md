# 🐍 **BYTEFORGE FIGLET SUITE — byteforge-figlet (Python Library)**

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

> **byteforge-figlet**
> *A fast, spec-compliant FIGLet engine for Python — zero dependencies.*

## 📘 Overview

`byteforge-figlet` is a Python implementation of the FIGLet rendering engine used across the ByteForge FIGLet Suite.

It provides a robust and efficient implementation of the FIGLet specification, allowing you to create ASCII art from text using FIGLet fonts. It supports all standard FIGLet features including various smushing rules, layout modes, ANSI color preservation, and Unicode.

The library ships with the built-in **small** font so it works out of the box with no additional downloads.

## ✨ Features

- 🔤 Render FIGLet text using any `.flf` font
- 📄 Full FIGLet font (`.flf`) file parsing and loading
- 🗜️ Automatic handling of compressed/zipped font files
- 🎨 ANSI color support for terminal output
- 🌏 Unicode support
- 📝 Paragraph formatting support
- ⚙️ Supports Full Size, Kerning, and Smushing layout modes
- 🧠 Implements all official smushing rules
- 📦 Default embedded font included — works out of the box
- 🚀 Zero dependencies — stdlib only
- 🐍 Python 3.9+
- 💻 Includes a `figprint` CLI command

### Sample Output

```
  _  _     _ _          _        _       _    _ _
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
                  |/
```

## 🛠 Installation

Install via pip:

```bash
pip install byteforge-figlet
```

## 🚀 Quick Start

### Basic Usage

```python
from byteforge_figlet import FIGLetRenderer

print(FIGLetRenderer.render("Hello World!"))
```

### Using a Custom Font

```python
from byteforge_figlet import FIGFont, FIGLetRenderer

font = FIGFont.from_file("/path/to/myfont.flf")
print(FIGLetRenderer.render("Hello!", font=font))
```

### Choosing a Layout Mode

```python
from byteforge_figlet import FIGLetRenderer, LayoutMode

# Full Size — no character overlap
print(FIGLetRenderer.render("Hi", mode=LayoutMode.FullSize))

# Kerning — characters touch but don't overlap
print(FIGLetRenderer.render("Hi", mode=LayoutMode.Kerning))

# Smushing — characters may merge (default)
print(FIGLetRenderer.render("Hi", mode=LayoutMode.Smushing))
```

### Renderer Instance

```python
from byteforge_figlet import FIGLetRenderer, LayoutMode

renderer = FIGLetRenderer(mode=LayoutMode.Kerning, line_separator="\n")
print(renderer.render_text("Hello\nWorld"))
```

## 💻 CLI Usage

The `figprint` command is installed automatically with the package:

```bash
figprint "Hello World"
figprint "Hello World" --font /path/to/font.flf
figprint "Hello World" --mode kerning
figprint "Hello World" --mode full
figprint --help
```

Or run as a module:

```bash
python -m byteforge_figlet "Hello World"
```

## 📐 API Reference

### `FIGFont`

| Method / Property             | Description                                        |
| ----------------------------- | -------------------------------------------------- |
| `FIGFont.default`             | The built-in `small` font (class property, cached) |
| `FIGFont.from_file(path)`     | Load a font from a `.flf` file path                |
| `FIGFont.from_stream(stream)` | Load a font from a binary stream                   |
| `FIGFont.from_text(text)`     | Load a font from a string                          |
| `FIGFont.from_lines(lines)`   | Load a font from a list of strings                 |
| `.height`                     | Character height in rows                           |
| `.hard_blank`                 | Hard blank character                               |
| `.characters`                 | `dict` mapping character code point → glyph rows   |
| `.smushing_rules`             | `SmushingRules` flags for this font                |
| `.print_direction`            | `0` = left-to-right, `1` = right-to-left           |
| `.has_smushing_rule(rule)`    | Check if a specific rule is set                    |

### `FIGLetRenderer`

| Member                             | Description                                            |
| ---------------------------------- | ------------------------------------------------------ |
| `FIGLetRenderer.render(text, ...)` | Static method — render and return ASCII art string     |
| `renderer.render_text(text)`       | Instance method — render using configured settings     |
| `.layout_mode`                     | Active `LayoutMode`                                    |
| `.line_separator`                  | Line separator string (default `os.linesep`)           |
| `.use_ansi_colors`                 | Whether to preserve ANSI color codes through rendering |
| `.paragraph_mode`                  | Whether blank lines produce separate FIGLet renders    |

**Constructor parameters:**

| Parameter         | Type                | Default      | Description                    |
| ----------------- | ------------------- | ------------ | ------------------------------ |
| `font`            | `FIGFont &#124; None` | `built-in small` | Font to use                    |
| `mode`            | `LayoutMode`        | `Smushing`   | Layout mode                    |
| `line_separator`  | `str &#124; None`   | `os.linesep` | Line separator                 |
| `use_ansi_colors` | `bool`              | `False`      | Preserve ANSI color codes      |
| `paragraph_mode`  | `bool`              | `True`       | Treat `\n` as paragraph breaks |

### `LayoutMode`

| Value                        | Description                    |
| ---------------------------- | ------------------------------ |
| `LayoutMode.FullSize` (`-1`) | No character overlap           |
| `LayoutMode.Kerning` (`0`)   | Minimal spacing, no merge      |
| `LayoutMode.Smushing` (`1`)  | Characters may merge (default) |

### `SmushingRules`

| Flag                           | Value | Description                            |
| ------------------------------ | ----- | -------------------------------------- |
| `SmushingRules.None`           | 0     | No smushing rules active               |
| `SmushingRules.EqualCharacter` | 1     | Two identical characters → one         |
| `SmushingRules.Underscore`     | 2     | Underscore replaced by hierarchy char  |
| `SmushingRules.Hierarchy`      | 4     | Higher-ranked char wins                |
| `SmushingRules.OppositePair`   | 8     | Opposing brackets → `\|`               |
| `SmushingRules.BigX`           | 16    | `/+\` → `\|`, `\+/` → `Y`, `>+<` → `X` |
| `SmushingRules.HardBlank`      | 32    | Two hardblanks → one hardblank         |

## 📁 Font Support

- Standard `.flf` font files
- Compressed `.flf` files inside `.zip` archives (auto-detected by PK magic bytes)

## 🎨 ANSI Color Support

The library preserves ANSI color codes through the rendering process, allowing you to create colorful FIGLet text in terminals:

```python
from byteforge_figlet import FIGLetRenderer

renderer = FIGLetRenderer(use_ansi_colors=True)

colorful_text = '\x1b[31mRed\x1b[0m \x1b[32mGreen\x1b[0m \x1b[34mBlue\x1b[0m'
print(renderer.render_text(colorful_text))
```

## 📝 Paragraph Mode

When enabled (the default), blank lines in the input produce separate FIGLet renderings spaced by the font's character height:

```python
from byteforge_figlet import FIGLetRenderer

renderer = FIGLetRenderer()

paragraphs = 'Paragraph 1\n\nParagraph 2'
print(renderer.render_text(paragraphs))
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

The library fully supports Unicode characters. Characters not present in the font are skipped gracefully:

```python
from byteforge_figlet import FIGLetRenderer

renderer = FIGLetRenderer()
print(renderer.render_text('Hello 😊 World!'))
```
Output:
```
  _  _     _ _          __      __       _    _ _
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \      \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___/       \_/\_/\___/_| |_\__,_(_)

```

## 🏗 Implementation Details

1. **FIGFont** — Parses `.flf` font files; supports loading from files, streams, strings, or line lists; handles ZIP-compressed font files; manages smushing rule configuration.

2. **FIGLetRenderer** — Converts input text to FIGLet output; implements character smushing logic; handles different layout modes; processes ANSI color codes in a single pre-pass; supports paragraph formatting.

3. **LayoutMode** — Enumeration controlling how characters are combined during rendering.

4. **SmushingRules** — Flags enumeration defining which character-combining rules are active.

## ⚡ Performance Considerations

- Default font is cached after the first load and reused across all calls
- ANSI color codes are handled in a single pre-pass, not during rendering
- Efficient string building throughout the rendering pipeline

## 🤝 Contributing

Contributions are welcome!
To contribute:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Open a Pull Request

## 🔗 ByteForge FIGLet Suite

| Component                                                                                                         | Description                   |
| ----------------------------------------------------------------------------------------------------------------- | ----------------------------- |
| [**FIGLet** (.NET)](https://www.nuget.org/packages/ByteForge.FIGLet)                                              | Core C# library on NuGet      |
| [**@byte-forge/figlet** (TypeScript)](https://www.npmjs.com/package/@byte-forge/figlet)                           | TypeScript library on npm     |
| [**byteforge-figlet** (Python)](https://pypi.org/project/byteforge-figlet)                                        | Python library on PyPI        |
| [**FIGPrint**](https://github.com/PaulStSmith/FIGLetAddIn/releases)                                               | .NET CLI tool                 |
| [**VS Extension**](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.FIGLetCommentGenerator)        | Visual Studio 2022+ extension |
| [**VS Code Extension**](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.figlet-comment-generator) | VS Code extension             |

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
