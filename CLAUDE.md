# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FIGLet Comment Generator creates ASCII art comments using FIGLet fonts for Visual Studio and VS Code. The project has parallel implementations in C# and TypeScript with consistent APIs.

**Components:**
- **FIGLet**: Core .NET library (NuGet: `ByteForge.FIGLet`)
- **TS.FIGLet**: TypeScript library for browser/Node.js
- **FIGLet.VSCodeExtension**: VS Code extension with embedded TypeScript engine
- **FIGLet.VisualStudioExtension**: Visual Studio 2022+ extension (WPF)
- **FIGPrint**: .NET CLI tool
- **FontGenerator**: WPF tool for creating fonts from bitmap images
- **FIGLet.Tests**: MSTest suite for the .NET library

## Build Commands

```bash
# .NET solution (all projects)
dotnet build FIGLetAddIn.sln
dotnet test FIGLetAddIn.sln

# Run specific tests
dotnet test FIGLet.Tests/FIGLet.Tests.csproj --filter "FullyQualifiedName~FIGFontTests"
dotnet test FIGLet.Tests/FIGLet.Tests.csproj --filter "TestCategory=Performance"

# Create NuGet package
dotnet pack FIGLet/FIGLet.csproj -c Release

# Run CLI tool
dotnet run --project FIGPrint -- "Hello World" --font small

# VS Code extension
cd FIGLet.VSCodeExtension
npm install
npm run compile    # Build
npm run watch      # Watch mode
npm run lint       # ESLint
npm run test       # Tests

# TypeScript library
cd TS.FIGLet
npm install
npm run build      # Build to dist/
npm run dev        # Watch mode
```

## Architecture

### Dual Implementation Strategy

The FIGLet engine exists in two languages with identical APIs:

| C# (`FIGLet/`) | TypeScript (`TS.FIGLet/src/FIGLet/`) |
|----------------|--------------------------------------|
| `FIGFont.cs` | `FIGFont.ts` |
| `FIGLetRenderer.cs` | `FIGLetRenderer.ts` |
| `LayoutMode.cs` | `LayoutMode.ts` |
| `SmushingRules.cs` | `SmushingRules.ts` |

The VS Code extension embeds a **copy** of the TypeScript implementation at `FIGLet.VSCodeExtension/src/FIGLet/`. Changes to rendering logic must be synchronized across both implementations.

### Key Classes

**FIGFont**: Parses .flf font files. Factory methods: `FromFile()`, `FromStream()`, `FromReader()`, `FromArray()`. Default font loaded from embedded resource `fonts/small.flf`.

**FIGLetRenderer**: Renders text with three layout modes:
- `FullSize` (-1): No character overlap
- `Kerning` (0): Minimal spacing
- `Smushing` (1): Character merging with 6 smushing rules (default)

**SmushingRules** (flags): `EqualCharacter`, `Underscore`, `Hierarchy`, `OppositePair`, `BigX`, `HardBlank`

### Language Comment System

`LanguageCommentStyles` (in both C# and TypeScript) maps 40+ languages to their comment formats. Each extension uses this to wrap rendered ASCII art in appropriate comment syntax based on the active file type.

### Font Management

- Default font: `small.flf` (embedded resource)
- Custom fonts: Configured via extension settings pointing to a directory of .flf files
- Encoding: .flf files use Latin-1 encoding
- ZIP support: `FIGFontStream` class handles .flf files inside ZIP archives

## Project Structure

```
FIGLet/                    # Core C# library (multi-target .NET 4.7.2 through .NET 9.0)
FIGLet.Tests/              # MSTest suite with embedded TestFonts/ and ExpectedOutputs/
FIGLet.VisualStudioExtension/  # VS2022+ extension (WPF, VSSDK)
FIGLet.VSCodeExtension/    # VS Code extension
  ├── src/extension.ts     # Entry point, command handlers
  ├── src/FIGLet/          # Embedded TypeScript engine (copy of TS.FIGLet)
  ├── src/BannerUtils.ts   # Comment insertion logic
  └── webpack.config.js    # Bundles webview React components
TS.FIGLet/                 # Standalone TypeScript library
FIGPrint/                  # CLI tool (System.CommandLine)
FontGenerator/             # Bitmap-to-FIGLet font converter
```

## Development Notes

### When Modifying Rendering Logic

1. Update the C# version in `FIGLet/`
2. Port changes to TypeScript in `TS.FIGLet/src/FIGLet/`
3. Copy updated TypeScript files to `FIGLet.VSCodeExtension/src/FIGLet/`
4. Run tests: `dotnet test` and `npm run test` in VSCodeExtension

### Adding Language Support

Add language mapping in both:
- `FIGLet.VisualStudioExtension/LanguageCommentStyles.cs`
- `FIGLet.VSCodeExtension/src/LanguageCommentStyles.ts`

### Extension Publishing

**VS Code:**
```bash
cd FIGLet.VSCodeExtension
npm run vscode:prepublish
vsce package
vsce publish
```

**Visual Studio:**
Build VSIX via Visual Studio or MSBuild, then upload to marketplace.

### Testing Strategy

The test suite uses embedded resources for fonts and expected outputs:
- `TestFonts/*.flf`: Various font files for testing
- `ExpectedOutputs/*.txt`: Reference renderings for regression testing

Test categories: Unit tests per component, smushing rule tests, integration tests, performance benchmarks.
