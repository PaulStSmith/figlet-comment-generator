# 🌐 **BYTEFORGE FIGLET SUITE — @byte‑forge/figlet (TypeScript Library)**

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

> **@byte‑forge/figlet**
> *A fast, spec‑compliant FIGLet engine for Node.js and TypeScript.*

## 📘 Overview

`@byte-forge/figlet` is a TypeScript implementation of the FIGLet rendering engine used across the ByteForge FIGLet Suite.

It provides a robust and efficient implementation of the FIGLet specification, allowing you to create ASCII art from text using FIGLet fonts. It supports all standard FIGLet features including various smushing rules, layout modes, ANSI color preservation, and Unicode.

This library powers the **VS Code FIGLet Comments extension**, and is ideal for CLI tools, web apps, build scripts, and developer utilities.

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
- 🚀 Zero dependencies, fast and lightweight
- 🌐 Works in Node.js, Deno, Bun, and browsers
- 📦 ESM‑first with CommonJS fallback
- 🔷 Full TypeScript typings included

### Sample Output

```
  _  _     _ _          _        _       _    _ _
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
                  |/
```

## 🛠 Installation

Install via npm:

```bash
npm install @byte-forge/figlet
```

Or via Yarn:

```bash
yarn add @byte-forge/figlet
```

Or via pnpm:

```bash
pnpm add @byte-forge/figlet
```

## 🚀 Quick Start

### Basic Usage

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

// Load a font from a file (returns null if the file is not found)
const font = await FIGFont.fromFile('standard.flf');
if (!font) throw new Error('Font not found');

// Create a renderer
const renderer = new FIGLetRenderer(font);

// Render text
const asciiArt = renderer.render('Hello World!');
console.log(asciiArt);
```

### Using the Default Font

The library ships with a built-in default font — no file required:

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const renderer = new FIGLetRenderer(font);

console.log(renderer.render('Hello World!'));
```

### Static Rendering

For one-shot rendering without creating a renderer instance:

```typescript
import { FIGFont, FIGLetRenderer, LayoutMode } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const asciiArt = FIGLetRenderer.render('Hello World!', font, LayoutMode.Smushing);
console.log(asciiArt);
```

## ⚙️ Layout Modes

The library supports three layout modes:

| Mode | Value | Description |
|------|-------|-------------|
| `LayoutMode.FullSize` | `-1` | No character compression — each character is rendered at full width |
| `LayoutMode.Kerning` | `0` | Characters are moved together until they touch but do not overlap |
| `LayoutMode.Smushing` | `1` | Characters are merged according to the font's smushing rules (default) |

```typescript
import { FIGFont, FIGLetRenderer, LayoutMode } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();

const fullSize = FIGLetRenderer.render('Text', font, LayoutMode.FullSize);
const kerning  = FIGLetRenderer.render('Text', font, LayoutMode.Kerning);
const smushing = FIGLetRenderer.render('Text', font, LayoutMode.Smushing);
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

```typescript
import { FIGFont, SmushingRules } from '@byte-forge/figlet';

const font = await FIGFont.fromFile('standard.flf');
if (!font) throw new Error('Font not found');

const hasEqualCharRule  = font.hasSmushingRule(SmushingRules.EqualCharacter);
const hasUnderscoreRule = font.hasSmushingRule(SmushingRules.Underscore);
```

## 📁 Font Support

- Standard `.flf` font files
- Compressed `.flf` files inside `.zip` archives (auto-detected by PK magic bytes)

## 🎨 ANSI Color Support

The library preserves ANSI color codes through the rendering process, allowing you to create colorful FIGLet text in terminals:

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();

// Enable ANSI color support
const renderer = new FIGLetRenderer(font, undefined, '\n', /* useANSIColors */ true);

const colorfulText = '\x1b[31mRed\x1b[0m \x1b[32mGreen\x1b[0m \x1b[34mBlue\x1b[0m';
console.log(renderer.render(colorfulText));
```

## 📝 Paragraph Mode

When enabled (the default), blank lines in the input produce separate FIGLet renderings spaced by the font's character height:

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const renderer = new FIGLetRenderer(font);

const paragraphs = 'Paragraph 1\n\nParagraph 2';
console.log(renderer.render(paragraphs));
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

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const renderer = new FIGLetRenderer(font);

console.log(renderer.render('Hello 😊 World!'));
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
| `FIGFont.getDefault()` | Returns the built-in default font (cached after first load) |
| `FIGFont.fromFile(path)` | Loads a font from a `.flf` or `.zip` file |
| `FIGFont.fromText(text)` | Parses a font from a string |
| `FIGFont.fromLines(lines)` | Parses a font from an array of lines |
| `.height` | Character height in rows |
| `.hardBlank` | The hard-blank character |
| `.characters` | `Map<string, string[]>` — character string → glyph rows |
| `.smushingRules` | Active smushing rules flags |
| `.printDirection` | `0` = left-to-right, `1` = right-to-left |
| `.hasSmushingRule(rule)` | Tests whether a specific rule is active |

### `FIGLetRenderer`

Core rendering engine.

| Member | Description |
|--------|-------------|
| `new FIGLetRenderer(font, mode?, separator?, ansi?, paragraph?)` | Create an instance |
| `FIGLetRenderer.render(text, font, mode?, separator?, ansi?, paragraph?)` | Static one-shot render |
| `.render(text)` | Render text using instance settings |
| `.layoutMode` | Active `LayoutMode` |
| `.lineSeparator` | Line separator string (default `'\n'`) |
| `.useANSIColors` | Whether to preserve ANSI color codes through rendering |
| `.paragraphMode` | Whether blank lines produce separate FIGLet renders |

### `LayoutMode`

```typescript
enum LayoutMode {
    FullSize = -1,   // No character compression
    Kerning  =  0,   // Minimal spacing
    Smushing =  1,   // Full character overlap (default)
    Default  =  1,
}
```

### `SmushingRules`

```typescript
enum SmushingRules {
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

1. **FIGFont** — Parses `.flf` font files; supports loading from files, strings, or arrays; handles ZIP-compressed font files; manages smushing rule configuration.

2. **FIGLetRenderer** — Converts input text to FIGLet output; implements character smushing logic; handles different layout modes; processes ANSI color codes in a single pre-pass; supports paragraph formatting and RTL fonts.

3. **LayoutMode** — Enumeration controlling how characters are combined during rendering.

4. **SmushingRules** — Flags enumeration defining which character-combining rules are active.

## ⚡ Performance Considerations

- Default font is cached after the first load and reused across all calls
- ANSI color codes are handled in a single pre-pass, not during rendering
- Surrogate pairs are supported without performance degradation
- Efficient string building throughout the rendering pipeline

## 🔧 Used By

This library powers:

- **VS Code FIGLet Comments Extension**
- **@byte-forge/figlet** CLI utilities
- Build scripts and code generators

## 🔗 Related Packages

- **[ByteForge.FIGLet](https://www.nuget.org/packages/ByteForge.FIGLet)** — The equivalent C# / .NET library (NuGet)
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
