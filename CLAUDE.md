# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FIGLet Comment Generator creates ASCII art comments using FIGLet fonts for Visual Studio and VS Code. The project has parallel implementations in C# and TypeScript with consistent APIs.

**Components:**
- **FIGLet**: Core .NET library (NuGet: `ByteForge.FIGLet`)
- **FIGLet.TS**: TypeScript library for browser/Node.js
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
cd FIGLet.TS
npm install
npm run build      # Build to dist/
npm run dev        # Watch mode
```

## Architecture

### Dual Implementation Strategy

The FIGLet engine exists in two languages with identical APIs:

| C# (`FIGLet/`) | TypeScript (`FIGLet.TS/src/`) |
|----------------|-------------------------------|
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

**FIGLetFontManager** (VS Code extension): Singleton that loads and caches `.flf` files from the configured font directory. Exposes `availableFonts` and `setFontDirectory()`.

### VS Code Webview Architecture

The VS Code extension uses **React (TSX)** components for its UI, bundled by webpack:
- `src/webview/App.tsx` — main banner composer panel
- `src/webview/SettingsApp.tsx` — settings panel
- `src/webview/index.tsx` / `settings-index.tsx` — React entry points

The extension host and webview communicate via `postMessage`. Message types (defined in `FigletPanel.ts`):
- `ready` — webview signals it is ready to receive data
- `ok` — user confirmed; carries `{ text, font, layoutMode, language }`
- `cancel` — user dismissed the panel
- `requestFont` — webview requests raw `.flf` content by font name
- `openExternal` — webview requests the host open a URL in the system browser

When editing webview UI, run `npm run watch` (runs webpack in watch mode) and press **F5** in VS Code to launch the Extension Development Host.

### Language Comment System

`LanguageCommentStyles` (in both C# and TypeScript) maps 40+ languages to their comment formats. Each extension uses this to wrap rendered ASCII art in appropriate comment syntax based on the active file type.

### VS Code Extension Settings

Configuration key prefix: `figlet`
- `fontDirectory` — path to directory of `.flf` font files
- `defaultFont` — name of the default font (default: `small`)
- `layoutMode` — `full` | `kerning` | `smush` (default: `smush`)
- `defaultWidth` — target render width in characters (default: `80`)

Publisher ID: `PaulStSmith.figlet-comment-generator`

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
  ├── src/FIGLet/          # Embedded TypeScript engine (copy of FIGLet.TS)
  ├── src/BannerUtils.ts   # Comment insertion logic
  └── webpack.config.js    # Bundles webview React components
FIGLet.TS/                 # Standalone TypeScript library
FIGPrint/                  # CLI tool (System.CommandLine)
FontGenerator/             # Bitmap-to-FIGLet font converter
```

## Development Notes

### When Modifying Rendering Logic

1. Update the C# version in `FIGLet/`
2. Port changes to TypeScript in `FIGLet.TS/src/`
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

## Changelog Update Process

**CRITICAL: ChangeLog is organized by year with separate files**

The ChangeLog system uses a master index file with year-specific detail files:

### File Structure:
- **`ChangeLog.md`** - Master index with links to year-specific files and quick summaries
- **`ChangeLog-YYYY.md`** - Detailed entries for each year (e.g., `ChangeLog-2024.md`, `ChangeLog-2025.md`)

### Update Process:
1. **Determine Year**: Check commit date to identify which year file to update
2. **Get Full Commit Messages**: Use `git log` to retrieve complete commit messages including full body text:
   - Use `git log --format="%h|%ad|%an|%s%n%b" --date=short` to get full commit details
   - Each commit includes hash, date, author, subject, and complete message body
3. **Target File**: Update the appropriate year-specific file:
   - **2024 commits** → Update `ChangeLog-2024.md`
   - **2025 commits** → Update `ChangeLog-2025.md`
   - **Future years** → Create new `ChangeLog-YYYY.md` file and update master `ChangeLog.md`
4. **Format**: Each entry uses the FULL text from the git commit message, following this pattern:
   ```
   ### Commit Subject Line
   YYYY-MM-DD : Author Name
   ● Bullet point from commit body (use full text from git log)
   ● Additional bullet points from commit body
   ● All details from the complete commit message
   ```
5. **Order**: Entries MUST be in reverse chronological order (newest commits at the TOP of the file)
6. **Master File**: Update `ChangeLog.md` summary highlights when significant milestones are reached

### Important Notes:
- **DO NOT create custom changelog entries** - always base them on actual git commit messages
- **DO NOT update the wrong file** - check commit dates carefully
- **Always maintain both the master index and year-specific files**
- **For new years**: Create new year file and add navigation link in master `ChangeLog.md`

## Pull Request Review Comments

When addressing review comments on a PR (from Copilot, team members, or any reviewer):

1. **Fix the code** as described in the comment
2. **Reply to the comment** immediately after fixing it — do not leave comments unacknowledged

To reply to an inline PR comment via `gh`:
```bash
gh api repos/OWNER/REPO/pulls/PR_NUMBER/comments \
  -X POST \
  -f body="Your reply here" \
  -f commit_id="$(gh api repos/OWNER/REPO/pulls/PR_NUMBER --jq '.head.sha')" \
  -f path="path/to/file.ext" \
  -F position=LINE_POSITION \
  -f in_reply_to="COMMENT_ID"
```
> **Note:** The `/pulls/comments/COMMENT_ID/replies` endpoint only works for comments that are themselves replies. For top-level review comments, use the form above with `in_reply_to`.

- Keep replies concise: describe what was changed and why it resolves the concern
- Reply to each comment individually as it is addressed, not in a batch at the end
