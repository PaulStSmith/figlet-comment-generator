# ChangeLog

Master index for the FIGLet Comment Generator / ByteForge FIGLet Suite changelog.

## Years

- [2026](ChangeLog-2026.md) — VS Code extension v1 launch, full CI/CD automation, renderer overhaul, dual-library sync, ByteForge branding
- [2025](ChangeLog-2025.md) — FIGPrint CLI, multi-framework NuGet, FontGenerator, RTL/paragraph rendering, ANSI colors
- [2024](ChangeLog-2024.md) — Initial Visual Studio extension, TypeScript port of FIGLet engine

## Highlights

### 2026
- Shipped **VS Code Extension v1** to the marketplace (`PaulStSmith.figlet-comment-generator`)
- Automated publish pipelines for NuGet, npm, VS Marketplace, VS Code Marketplace, winget, and GitHub Releases
- Overhauled TypeScript renderer: ANSI color preservation, paragraph mode, RTL, configurable layout
- Full test suite: 81 TypeScript (Vitest) + comprehensive C# MSTest suite
- Synced versions between C# library and TypeScript library via CI

### 2025
- Added **FIGPrint** CLI tool (System.CommandLine)
- Added **FontGenerator** (bitmap-to-FIGLet converter)
- Multi-target NuGet package (.NET 4.7.2 → .NET 9.0)
- Added `FIGFontStream` for ZIP-compressed font loading
- RTL text support, paragraph mode, ANSI color support in C# renderer

### 2024
- Initial Visual Studio 2022 extension release (v1.2.x)
- TypeScript port of FIGLet engine (`FIGLet.TS`)
- Language comment style mapping for 40+ languages
