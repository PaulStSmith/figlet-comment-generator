# 🌐 **BYTEFORGE FIGLET SUITE — FIGPRINT CLI**

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

> **FIGPrint**
> *A command-line tool for rendering text as FIGLet ASCII art.*

## 📘 Overview

`FIGPrint` is the CLI entry point of the ByteForge FIGLet Suite. It wraps the `ByteForge.FIGLet` library in a simple command-line interface, letting you render FIGLet banners directly from a terminal, shell script, or build pipeline.

Text can be supplied as command-line arguments or piped through standard input, making it easy to integrate into any workflow.

## ✨ Features

- 🔤 Render FIGLet ASCII art from the command line
- 📂 Load any `.flf` font from a `fonts/` directory next to the executable
- ⚙️ Choose between Full Size, Kerning, and Smushing layout modes
- 🎨 Optional ANSI color preservation for colorful terminal output
- 📋 List all available fonts with `--showList`
- ⏩ Read text from arguments or from standard input (pipe support)
- 📦 Distributed as a self-contained single-file executable — no .NET runtime required

## 🛠 Installation

### Via winget

```bash
winget install ByteForge.FIGPrint
```

### Manual download

Download the latest release for your platform from the
[GitHub Releases](https://github.com/PaulStSmith/figlet-comment-generator/releases) page
and place `FIGPrint` (or `FIGPrint.exe` on Windows) somewhere on your `PATH`.

## 🚀 Usage

```
FIGPrint [<text>...] [options]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `<text>` | One or more words to render. Multiple arguments are joined with a space. Omit to read from stdin. |

### Options

| Option | Default | Description |
|--------|---------|-------------|
| `--font <name>` | `small` | Name of the FIGlet font to use (without the `.flf` extension) |
| `--layout <mode>` | `Smushing` | Layout mode: `FullSize`, `Kerning`, or `Smushing` |
| `--ansi-colors` | off | Preserve ANSI color escape codes through the rendering |
| `--showList` | off | Print a list of all available fonts and exit |
| `--help` | | Show help and exit |
| `--version` | | Show version and exit |

---

## 🧱 Examples

### Render text with the default font

```bash
FIGPrint Hello, World!
```

```
  _  _     _ _          __      __       _    _ _
 | || |___| | |___      \ \    / /__ _ _| |__| | |
 | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
                  |/
```

### Use a different font

```bash
FIGPrint --font bmp Hello
```

### Change layout mode

```bash
FIGPrint --layout FullSize Hello World
FIGPrint --layout Kerning  Hello World
FIGPrint --layout Smushing Hello World
```

### Pipe text from stdin

```bash
echo "Hello World" | FIGPrint
git branch --show-current | FIGPrint --font bmp-condensed
```

### Render with ANSI colors

```bash
printf "\033[32mGreen\033[0m" | FIGPrint --ansi-colors
```

### List available fonts

```bash
FIGPrint --showList
```

```
Available fonts:
  - bmp
  - bmp-condensed
  - bmp-inverted
  - bmp-inverted-condensed
  - small
```

---

## 📁 Font Support

FIGPrint looks for fonts in a `fonts/` directory located next to the executable. Place any standard `.flf` font file there and refer to it by name (without extension) via `--font`.

The `small` font is embedded in the executable and is always available as the default, even if no `fonts/` directory is present. When a requested font is not found, FIGPrint prints the list of available fonts and exits with code `1`.

Fonts bundled with the release:

| Font name | Description |
|-----------|-------------|
| `small` | Compact font, the default |
| `bmp` | Bitmap-style font |
| `bmp-condensed` | Condensed bitmap variant |
| `bmp-inverted` | Inverted bitmap font |
| `bmp-inverted-condensed` | Inverted condensed bitmap variant |

## ↩ Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | No text provided, font not found, or rendering error |

---

## 🔗 Related Packages

- **[ByteForge.FIGLet](https://www.nuget.org/packages/FIGLet)** — The underlying C# / .NET library (NuGet)
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

This tool is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

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
