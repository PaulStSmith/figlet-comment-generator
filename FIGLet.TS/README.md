# @byte-forge/figlet

A TypeScript implementation of FIGLet (Frank, Ian & Glenn's letters) — a library for making large letters out of ordinary text.

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
- Paragraph formatting support
- Default embedded font included
- Full TypeScript typings included
- ESM module (Node.js ≥ 18)

## Installation

```bash
npm install @byte-forge/figlet
```

## Usage

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

// Load a font from a file
const font = await FIGFont.fromFile('standard.flf');

// Create a renderer
const renderer = new FIGLetRenderer(font);

// Render text
const asciiArt = renderer.render('Hello World!');
console.log(asciiArt);
```

### Using the Default Font

The library comes with a built-in default font:

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

```typescript
import { FIGFont, FIGLetRenderer, LayoutMode } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const renderer = new FIGLetRenderer(font);

const fullSize = FIGLetRenderer.render('Text', font, LayoutMode.FullSize);
const kerning  = FIGLetRenderer.render('Text', font, LayoutMode.Kerning);
const smushing = FIGLetRenderer.render('Text', font, LayoutMode.Smushing);
```

### ANSI Color Support

The library supports ANSI color codes for terminal output, allowing you to create colorful FIGLet text:

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();

// Enable ANSI color support
const renderer = new FIGLetRenderer(font, undefined, '\n', /* useANSIColors */ true);

const colorfulText = '\x1b[31mRed\x1b[0m \x1b[32mGreen\x1b[0m \x1b[34mBlue\x1b[0m';
console.log(renderer.render(colorfulText));
```

### Paragraph Mode

The library can automatically handle paragraphs in input text:

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();

// Paragraph mode is enabled by default
const renderer = new FIGLetRenderer(font);

const paragraphs = 'Paragraph 1\n\nParagraph 2';
console.log(renderer.render(paragraphs));
```

### Smushing Rules

The library implements all standard FIGLet smushing rules as defined in the FIGLet specification:

```typescript
import { FIGFont, SmushingRules } from '@byte-forge/figlet';

const font = await FIGFont.fromFile('standard.flf');

const hasEqualCharRule  = font.hasSmushingRule(SmushingRules.EqualCharacter);
const hasUnderscoreRule = font.hasSmushingRule(SmushingRules.Underscore);
```

### Unicode Support

The library fully supports Unicode characters, including surrogate pairs (unknown characters are skipped gracefully):

```typescript
import { FIGFont, FIGLetRenderer } from '@byte-forge/figlet';

const font = await FIGFont.getDefault();
const renderer = new FIGLetRenderer(font);

console.log(renderer.render('Hello 😊 World!'));
```

## API Reference

### `FIGFont`

Handles font loading and storage.

| Member | Description |
|--------|-------------|
| `FIGFont.getDefault()` | Returns the built-in default font (cached) |
| `FIGFont.fromFile(path)` | Loads a font from a `.flf` or `.zip` file |
| `FIGFont.fromText(text)` | Parses a font from a string |
| `FIGFont.fromLines(lines)` | Parses a font from an array of lines |
| `.height` | Character height in rows |
| `.hardBlank` | The hard-blank character |
| `.characters` | Map of character data |
| `.smushingRules` | Active smushing rules flags |
| `.hasSmushingRule(rule)` | Tests whether a specific rule is active |

### `FIGLetRenderer`

Core rendering engine.

| Member | Description |
|--------|-------------|
| `new FIGLetRenderer(font, mode?, separator?, ansi?, paragraph?)` | Create an instance |
| `FIGLetRenderer.render(text, font, mode?, separator?)` | Static one-shot render |
| `.render(text)` | Render text using instance settings |
| `.layoutMode` | Active `LayoutMode` |
| `.lineSeparator` | Line separator string (default `'\n'`) |
| `.useANSIColors` | Whether to preserve ANSI color codes |
| `.paragraphMode` | Whether blank lines produce separate renders |

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

## Implementation Details

1. **FIGFont** — Parses `.flf` font files; supports loading from files, strings, or arrays; handles ZIP-compressed font files; manages smushing rule configuration.

2. **FIGLetRenderer** — Converts input text to FIGLet output; implements character smushing logic; handles different layout modes; processes ANSI color codes; supports paragraph formatting.

3. **LayoutMode** — Enumeration controlling how characters are combined.

4. **SmushingRules** — Flags enumeration defining available character-combining rules.

## Performance Considerations

- Cached default font (loaded once, reused)
- Efficient string building
- Optimized character smushing calculations
- ANSI color codes handled in a single pre-pass
- Surrogate pairs supported without performance degradation

## Related Packages

- **[ByteForge.FIGLet](https://www.nuget.org/packages/ByteForge.FIGLet)** — The equivalent C# / .NET library (NuGet)
- **[FIGLet Comment Generator](https://marketplace.visualstudio.com/items?itemName=Paulo-Santos---Paul.St.Smith.FIGLet-Comment-Generator)** — VS Code extension
- **[FIGLet Comment Generator (Visual Studio)](https://marketplace.visualstudio.com/items?itemName=Paulo-Santos---Paul.St.Smith.FIGLet-Comment-Generator-VS)** — Visual Studio 2022+ extension

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## Credits

- Original FIGLet concept by Frank, Ian & Glenn
- FIGLet specification: http://www.figlet.org/
- Implementation by Paulo Santos
