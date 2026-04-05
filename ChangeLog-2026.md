# ChangeLog 2026

[← Back to master index](ChangeLog.md) | [2025](ChangeLog-2025.md) | [2024](ChangeLog-2024.md)

---

## April 2026

### fix(py): remove redundant force-include causing duplicate ZIP entries
2026-04-04 : Paulo Santos
● hatchling's `packages = ["byteforge_figlet"]` already includes the entire package directory tree (fonts/ and all)
● The `force-include` block was re-adding `byteforge_figlet/fonts` a second time, producing a wheel with duplicate filenames in its local ZIP headers
● PyPI rejects such wheels with a 400 Bad Request — removing the redundant block fixes the upload

### fix(py): fix PyPI upload — correct repo URLs and add metadata validation
2026-04-04 : Paulo Santos
● Fix `Homepage`, `Repository`, and `Issues` URLs in pyproject.toml (were pointing at the old `FIGLetAddIn` repo name instead of `figlet-comment-generator`)
● Add `twine check dist/*` step before upload to catch metadata problems early and surface actionable errors
● Add `--verbose` to `twine upload` so a 400/4xx response shows the full PyPI error message instead of just "Bad Request"

### fix: address PR #27 Copilot review comments
2026-04-04 : Paulo Santos
● FIGFont.cs: separate the fused doc comments — DetectEndmark now has its own correct `<summary>` and `<param>` tags; ParseCharacterLine gets its own doc block with the added endmark param documented
● FIGFont.cs: add inline comment explaining why `Length - 1` indexing is used instead of `[^1]` (multi-target back to .NET Framework 4.7.2)
● FIGLet.csproj: add inline XML comment explaining the NU1510 suppression is intentional (System.IO.Compression explicit reference needed for net472 targets)

### chore(build): update build, permissions, and compat
2026-04-04 : Paulo Santos
● Remove permissions config from settings.local.json
● Ignore .claude/settings.local.json in .gitignore
● Add LICENSE copy step to VS extension build for compliance
● Refactor FIGFont endmark detection for C# compatibility
● Suppress NU1510 warning in FIGLet.csproj build

### fix(winget): bump ManifestVersion from 1.6.0 to 1.12.0
2026-04-04 : Paulo Santos
● 1.6.0 is deprecated in the winget-pkgs repo
● Future FIGPrint releases will now generate manifests targeting the currently recommended schema version (1.12.0)
● Updated all three manifest types: version, defaultLocale, installer

### chore(ci): fix Node.js 20 deprecation and suppress test nullable warnings
2026-04-04 : Paulo Santos
● Upgrade actions/setup-python from v5 to v6 to use the Node.js 24 runtime
● Add `<NoWarn>CS8618;CS8625;CS8602</NoWarn>` in FIGLet.Tests.csproj; these nullable warnings are meaningless in test code (fields set via TestInitialize, intentional null inputs, and assertion-only dereferences)

### feat(font): detect endmark dynamically across all three implementations
2026-04-04 : Paulo Santos
● C#, TypeScript, and VS Code extension FIGFont implementations now detect the glyph endmark character from the last character of the first raw line of each glyph block, rather than hardcoding `@`
● Brings all three into parity with the Python library fix
● Correctly handles fonts that use a non-standard endmark character

### fix(py): address all Copilot review comments on PR #26
2026-04-04 : Paulo Santos
● Fix invalid JSON in .claude/settings.local.json (missing closing paren, trailing comma)
● Revert pyproject.toml license to SPDX text expression to avoid missing-file build errors
● Fix header parsing to use `split()` instead of `split(" ")` for robustness with multiple spaces
● Detect FIGlet endmark dynamically from the first glyph line instead of hardcoding `@`; use `rstrip(endmark)` to correctly handle glyphs whose character equals the endmark; preserve the `#` workaround that mirrors the C# implementation
● Revert FIGLet.csproj to PackageLicenseExpression and add Exists() guard on LICENSE include
● Add Exists() guard to VS extension LICENSE content include

### feat(font): add class-level property descriptor
2026-04-04 : Paulo Santos
● Introduce `_ClassProperty` descriptor to replicate `@classmethod @property` behavior removed in Python 3.13
● Replace deprecated `@classmethod @property` on `FIGFont.default` with `@_ClassProperty`
● Ensures compatibility with Python 3.11+ while maintaining lazy loading and caching of the default FIGfont

### ci(workflow): copy LICENSE before dependency install
2026-04-04 : Paulo Santos
● Update CI to copy LICENSE from repo root into FIGLet.Python before installing dependencies
● Ensures LICENSE file is present when pip runs the build (hatchling validates it at install time)

### ci: improve version checks and Python/WinGet workflows
2026-04-04 : Paulo Santos
● Add Python version check to CI — ensures C#, TypeScript, and Python library versions stay in sync
● Add `test-python` job: sets up Python 3.13, installs pytest, and runs the full test suite
● Refactor Python publish workflow: use python3/sed for version bumping instead of fragile heredoc
● Rewrite WinGet YAML generation in PowerShell using string arrays to avoid here-string indentation issues in YAML

### feat(py): add byteforge-figlet Python library & CI
2026-04-04 : Paulo Santos
● Introduce `byteforge-figlet`: fast, spec-compliant FIGLet engine for Python with zero dependencies
● Add core modules: FIGFont, FIGLetRenderer, LayoutMode, SmushingRules, and bundled "small" font
● Implement CLI entry point (`__main__.py`) and `figprint` command
● Provide `pyproject.toml` for packaging and metadata (hatchling)
● Include full pytest-based test suite (141 tests) mirroring the C# and TypeScript implementations
● Add `publish-figlet-py.yml` GitHub Actions workflow for PyPI and GitHub Releases automation
● Update `sync-library-versions.yml` to sync Python version with C# and TypeScript

### ci(workflows): include LICENSE in packages, improve WinGet
2026-04-04 : Paulo Santos
● Copy root LICENSE into each package/project directory in all CI publish workflows
● Ensures LICENSE is present in all distributed artifacts (NuGet, npm, PyPI, VSIX)
● Replace interactive `wingetcreate` with manual PowerShell YAML manifest generation
● Compute SHA256 for installers and write all three WinGet manifest files directly

### fix(ci): open winget PR directly instead of relying on wingetcreate --submit
2026-04-04 : Paulo Santos
● `wingetcreate` only generates manifest YAML files; it does not open the PR in `microsoft/winget-pkgs`
● Fork `microsoft/winget-pkgs` under the token owner if not already forked
● Create a per-version branch (`figprint-vX.Y.Z`) on the fork
● Upload the three YAML files to `manifests/b/ByteForge/FIGPrint/<version>/`
● Open the PR against `microsoft/winget-pkgs` with the required title format

### chore(settings): update Bash command permissions
2026-04-04 : Paulo Santos
● Broaden allowed Bash commands with generic patterns
● Add support for GitHub CLI (`gh`) and `git mv` commands
● Remove duplicates and overly specific Bash command entries

### docs(claude): fix gh command for replying to top-level PR review comments
2026-04-04 : Paulo Santos
● The `/pulls/comments/ID/replies` endpoint only works for existing reply threads
● Top-level review comments require `POST /pulls/PR/comments` with `in_reply_to`
● Update CLAUDE.md with the correct form and a note explaining the distinction

### chore(config): add MIT license and update allowed cmds
2026-04-04 : Paulo Santos
● Add MIT License file for 2024–2026, Paulo Santos
● Update settings.local.json to allow git push, npm test, dotnet build, and additional Bash commands

### fix(ci): cap patch version at 65534 in NuGet and FIGPrint workflows
2026-04-04 : Paulo Santos
● System.Version components are limited to 0–65535; cap the patch derived from `github.run_number` at 65534
● Matches the same guard already in place in publish-vs-extension.yml

### docs(readme): modernize for ByteForge FIGLet Suite
2026-04-03 : Paulo Santos
● Rewrite readme to reflect ByteForge FIGLet Suite branding
● Replace extension-specific intro with cross-platform overview
● Add ASCII art banner and suite description at the top
● Introduce ecosystem components table with links
● Add ASCII architecture diagram of suite components
● Update install/getting started for all environments
● Simplify and clarify contributing and license sections
● Expand credits to include original authors/maintainers

### style(docs): add FIGlet ASCII banners and doc comments
2026-04-03 : Paulo Santos
● Add large FIGlet-style ASCII-art banners as block comments to the top of most TypeScript source files and major sections
● Add or improve JSDoc-style comments for class properties and methods, especially in FigletPanel.ts and FigletSettingsPanel.ts
● Remove quickpick.ts (dynamic font preview QuickPick) as it is no longer needed
● Make no functional or logic changes except for quickpick.ts removal

### feat(api): add cached VS Code webview API accessor
2026-04-03 : Paulo Santos
● Declare acquireVsCodeApi with postMessage, getState, setState methods
● Add singleton _api to cache the acquired API instance
● Export getVsCodeApi to safely acquire and reuse the API
● Document usage and rationale with detailed JSDoc comments

### feat(renderer): improve ANSI color & font handling
2026-04-03 : Paulo Santos
● Refactor FIGLetRenderer to support ANSI color preservation and mapping
● Add ANSIProcessor for robust escape sequence handling
● Make renderer configurable: layoutMode, lineSeparator, useANSIColors, paragraphMode
● Split rendering logic for paragraph and single-line modes
● Strengthen FIGFont parsing with stricter error checks
● Refactor smushing logic to be font-agnostic and C#-compatible
● Add decorative ASCII art headers and improve documentation throughout

### style(core): add ASCII art banners and header validation
2026-04-03 : Paulo Santos
● Add themed ASCII art banners as decorative comments to FIGFont.ts, FIGLetRenderer.ts, LayoutMode.ts, SmushingRules.ts, index.ts, and types.d.ts
● In FIGFont.ts, add validation for header parameters and file length, throwing errors for invalid fonts

### refactor(font): improve FIGFont header parsing
2026-04-03 : Paulo Santos
● Wrap header parsing in try-catch for better error messages
● Throw descriptive FormatException on parse errors
● Add check for sufficient lines for all required characters

### build(deps): dependency bumps via Dependabot
2026-04-03 : dependabot[bot]
● Bump glob from 10.4.5 to 10.5.0 in FIGLet.VSCodeExtension
● Bump flatted from 3.3.1 to 3.4.2 in FIGLet.VSCodeExtension
● Bump minimatch from 3.1.2 to 3.1.5 in FIGLet.VSCodeExtension
● Bump picomatch from 2.3.1 to 2.3.2 in FIGLet.VSCodeExtension

### Fix missing setup-node step in sync-extension-versions workflow
2026-04-02 : Paulo Santos
● Add actions/setup-node@v6 pinned to 22.x before the first node invocation
● Matches the pattern in sync-library-versions.yml
● Without this the job relied on whatever Node version happened to be pre-installed on ubuntu-latest

### Add icon, sync version, and extension version sync workflow
2026-04-02 : Paulo Santos
● Copy icon-128.png from VS extension to media/icon.png and register it in package.json so it appears on the VS Code Marketplace
● Bump VS Code extension version from 0.0.1 to 1.3.0 to match the VS extension (both at the same feature level)
● Add sync-extension-versions.yml: mirrors the library version sync workflow but watches the VS and VS Code extension manifests, picks the higher semver, and pushes a chore commit to align both

### Upgrade GitHub Actions to Node 24-compatible versions
2026-04-02 : Paulo Santos
● actions/checkout@v4 → v6; actions/setup-node@v4 → v6; actions/setup-dotnet@v4 → v5 across all 7 workflow files
● Eliminates the Node.js 20 deprecation warning
● Node.js 20 support is removed from GitHub runners on September 16, 2026

### Add README to VS Code extension for Marketplace description
2026-04-02 : Paulo Santos
● Add README.md mirroring the Visual Studio extension README in style, covering features, usage, keyboard shortcuts, context menu, settings, layout modes, supported languages, custom fonts, and examples
● Fix .vscodeignore: *.md was excluding README.md which the Marketplace requires
● Add "readme": "README.md" to package.json for explicit vsce reference

### Rename extension to figlet-comment-generator to avoid marketplace conflict
2026-04-02 : Paulo Santos
● The name 'figlet' is already taken on the VS Code Marketplace
● Rename to 'figlet-comment-generator' so the marketplace ID becomes PaulStSmith.figlet-comment-generator

### docs(project): remove battle-plan.md project plan
2026-04-02 : Paulo Santos
● Delete battle-plan.md, removing the entire project plan document
● Clean up repository by deleting outdated planning documentation

### Address PR#14 review comments
2026-04-02 : Paulo Santos
● Add "license": "MIT" to package.json so VS Code Marketplace metadata reflects the LICENSE file
● Align FIGLet.VSCodeExtension/LICENSE copyright holder to "Paulo Santos"
● Fix winget API catch block to only treat HTTP 404 as "package not found"; other errors now fail the job instead of silently falling through

### Fix FIGPrint winget submission for first-time packages
2026-04-02 : Paulo Santos
● wingetcreate update requires the package to already exist in winget-pkgs
● Detect whether the package exists via the GitHub API and use wingetcreate new for the initial submission, falling back to update for subsequent version bumps

### Fix vsce packaging errors in VS Code extension
2026-04-02 : Paulo Santos
● package.json: remove 'files' field — vsce does not support both 'files' and .vscodeignore simultaneously
● package.json: add 'repository' field pointing to the GitHub repo
● .vscodeignore: expand exclusions to cover everything previously handled by 'files'
● LICENSE: add MIT licence file (vsce warning: LICENSE not found)
● vsce package now produces a clean 30-file, 141 KB VSIX with no errors or warnings

### Fix VS Code extension packaging and add VSIX to GitHub Releases
2026-04-02 : Paulo Santos
● Add missing 'publisher' field ('PaulStSmith') to package.json — vsce publish requires it
● Add 'Package extension' step: vsce package produces a deterministically named VSIX
● Add 'Create GitHub Release' step that attaches the .vsix so users can install manually
● Attach the VS extension VSIX to its GitHub Release as well
● Add GitHub Releases to NuGet and npm publish workflows, attaching build artifacts

### Add workflow_dispatch to all publish workflows
2026-04-02 : Paulo Santos
● Each publishing workflow now supports manual triggering from the GitHub Actions UI
● The job-level if condition now handles both workflow_dispatch and pull_request merged events

### Add Getting Started page shown on first install
2026-04-02 : Paulo Santos
● WelcomePanel.ts: static webview with themed HTML explaining how to open the banner panel, keyboard shortcuts, layout modes, font directory config, and how to reopen the page
● extension.ts: registers figlet.showWelcome command; shows the panel exactly once on first install, not on subsequent updates
● package.json: adds figlet.showWelcome command (icon: $(info)) and wires it into the figlet.submenu

### Settings panel: close and toast on save
2026-04-02 : Paulo Santos
● FigletSettingsPanel: dispose() then showInformationMessage() after config.update() calls
● SettingsApp: remove inline 'Saved.' indicator (panel closes immediately, indicator never visible)

### Auto-select editor language in banner UI
2026-04-02 : Paulo Santos
● FigletPanel already forwarded editor.document.languageId in the init message
● Webview now normalises it before setting state so VS Code aliases (shellscript→sh, javascriptreact→javascript, etc.) resolve to the correct dropdown entry
● Falls back to 'csharp' when no match can be found

### feat(vscode-ext): add context/title menus, fix keybinding, improve preview
2026-04-02 : Paulo Santos
● Add 'FIGlet Comments' submenu to the editor right-click context menu with 'Generate FIGlet Banner' and 'FIGlet Settings' entries
● Add 'Generate FIGlet Banner' icon button to the editor title bar
● Fix keybinding: Ctrl+Shift+F → Ctrl+Alt+B (was conflicting with Find in Files)
● Show 'Hello, World!' as preview placeholder when no text is typed

### Sync FIGLet.VSCodeExtension engine with FIGLet.TS bug fixes
2026-04-02 : Paulo Santos
● LayoutMode: Default = Smushing (was -2, a broken sentinel value)
● FIGFont: String.fromCharCode → String.fromCodePoint (correct for code points > 0xFFFF)
● FIGLetRenderer: BigX rule '\' + '/' now returns 'Y' (was '|')
● FIGLetRenderer: escapeRegex() helper guards hardBlank regex construction

### feat(webview): bundle React/ReactDOM, update launch
2026-04-02 : Paulo Santos
● Bundle full, minified React 18.3.1 and ReactDOM 18.3.1 directly into webview.js for a self-contained webview UI
● Add explicit MIT license notices for React, ReactDOM, and Scheduler to webview.js.LICENSE.txt
● Update VS Code launch configuration: rename extension launch, add preLaunchTask, add Node.js app launch config

### feat(webview): add FIGlet comment generator UI
2026-04-02 : Paulo Santos
● Add complete webview UI for generating ASCII art comments using FIGlet fonts in VS Code
● Implement FIGlet font renderer and logic for wrapping ASCII art in language-appropriate comments
● Build React-based interface for selecting fonts, layouts, and languages, previewing output, and sending results to the extension
● Support dynamic font loading and mapping of VS Code language IDs to comment styles

### feat(vscode-ext): wire webview panels into extension and update build config
2026-04-01 : Paulo Santos
● extension.ts: replace inline render logic with FigletPanel.createOrShow; register new figlet.openSettings command backed by FigletSettingsPanel
● BannerUtils: make languageId optional (falls back to editor.document.languageId)
● package.json: declare figlet.openSettings command, add enumDescriptions for layoutMode, add build:webview / watch:webview scripts
● webpack.config.js: refactor to multi-entry config (webview + settings-webview)

### feat(vscode-ext): add webview panel UI for banner insertion and settings
2026-04-01 : Paulo Santos
● Introduce FigletPanel (banner preview/insert) and FigletSettingsPanel (font directory, default font, layout mode) as VS Code WebviewPanel wrappers
● Add React webview sources: App.tsx, SettingsApp.tsx, entry points, vscodeApi.ts

### feat(renderer): improve output formatting and tests
2026-04-01 : Paulo Santos
● Replace hard blanks with spaces and append ANSI reset code in FIGLetRenderer output
● Add integration tests for "TODO" and "FIX ME" banners to verify rendering correctness

### Add missing tests for RTL+ANSI, ZIP compression paths, and consecutive paragraphs
2026-04-01 : Paulo Santos
● TestUtilities: CreateMinimalValidFontContent gains optional printDirection param; CreateZipWithFontFile gains optional compressionLevel param
● New test: Render_WithRTLFontAndANSIColors_ShouldPreserveBothReverseAndColors
● New test: Render_WithParagraphMode_ConsecutiveParagraphs_ShouldConcatenateWithoutBlankSeparation
● New integration tests: EndToEnd_ZipFont_DeflateCompression and EndToEnd_ZipFont_StoreCompression

### Add ANSI color preservation to FIGLetRenderer (TypeScript)
2026-04-01 : Paulo Santos
● Add ANSIProcessor class (port of C# inner class): detects CSI escape sequences, accumulates color codes
● renderLine() first pass: when useANSIColors=true, strip ANSI sequences from input and build a colorDict mapping code-point position → color sequence
● Each output line is suffixed with ESC[0m reset when ANSI is enabled

### Add ZIP font loading to FIGFont.fromFile (TypeScript)
2026-04-01 : Paulo Santos
● fromFile() now reads the file as a Buffer and checks for the PK magic bytes
● Auto-detects ZIP vs plain .flf; supports Store (method 0) and Deflate (method 8) using Node's built-in zlib
● Matches the auto-detection behaviour of C#'s FIGFontStream wrapper

### Add instance properties, paragraph mode, and RTL support to FIGLetRenderer (TypeScript)
2026-04-01 : Paulo Santos
● Add layoutMode, lineSeparator, useANSIColors, paragraphMode instance properties
● Paragraph mode: splits on \r\n/\n, blank segments emit font.height empty lines
● RTL support: reverses text when font.printDirection === 1
● Add missing HardBlank rule 6 in smushCharacters()

### Make CanSmush font-agnostic in both C# and TypeScript
2026-04-01 : Paulo Santos
● Refactor CanSmush / canSmush to take hardBlank and rules as explicit parameters instead of reading them from the font instance
● Matching the pattern established by the SmushCharacters refactor

### feat(renderer, font): refactor smushing and hard blank logic (C#)
2026-04-01 : Paulo Santos
● Change FIGFont.HardBlank from string to char for clarity and type safety
● Make SmushCharacters internal and decouple from FIGFont property
● Add SmushingRulesExtensions with HasRule() for flag checks
● Improve accuracy of Big X and hard blank smushing
● Add net10.0 target and InternalsVisibleTo for FIGLet.Tests in csproj

### Add TypeScript test suite (81 tests, all green)
2026-04-01 : Paulo Santos
● Set up Vitest as the test runner for FIGLet.TS, mirroring the C# MSTest suite
● 27 FIGFont tests, 37 FIGLetRenderer tests, 17 integration tests
● TestUtilities: createMinimalValidFontContent, loadTestFont, createZipWithFontFile (CRC-32 + zlib), stripANSIColors, assertSmushingRule
● Test fonts copied to tests/fonts/ (mini-fixed.flf, smushing-test.flf)

### test(figlet): improve test coverage and reliability (C#)
2026-04-01 : Paulo Santos
● Treat HardBlank as char in all tests for type consistency
● Modernize test data: use dictionaries with expected outputs
● Assert exact rendered output in tests for robustness
● Refactor smushing rule tests to use SmushCharacters directly
● Add new tests for BigX smushing edge cases

### ci(versioning): sync C# and TS library versions
2026-04-01 : Paulo Santos
● Add CI job to check C# and TS versions match in PRs
● Introduce workflow to auto-sync versions to the greater one
● Update FIGLet.csproj and package.json as needed and push changes

### feat(tests): expand and document TestUtilities helpers (C#)
2026-04-01 : Paulo Santos
● Add methods for retrieving embedded resources as stream/text
● Add LoadTestFont for loading FIGFont from resources
● Add helpers to generate valid/invalid test font content
● Add StripANSIColors, AssertSmushingRule, AssertPerformance, GenerateLargeText, CreateZipWithFontFile

### ci(publish): fix all three publish workflows
2026-04-01 : Paulo Santos
● NuGet: add permissions.contents:write; bump dotnet-version to 10.0.x to match net10.0 TargetFramework
● npm: add registry-url back to setup-node so OIDC token exchange works
● VS Extension: add permissions.contents:write; remove broken 'Download VsixPublisher' step

### Fix VS extension publish: auto-versioning with run_number
2026-04-01 : Paulo Santos
● Derive full version at build time: major.minor from the manifest + Actions run_number as patch (e.g. 1.3.47)
● Cap patch at 65534 to stay within the 0–65535 limit of System.Version/VSIX version components
● Replace bare git tag push with gh release create, anchored to the merged commit SHA

### chore(package): bump @byte-forge/figlet to v2.0.0
2026-04-01 : Paulo Santos
● Update package version from 1.0.0 to 2.0.0 in package.json
● Bump driven by HardBlank string→char breaking change in C# library

### refactor(project): rename TS.FIGLet to FIGLet.TS
2026-04-01 : Paulo Santos
● Rename project in all docs, configs, and code
● Fix FIGletRenderer escape sequence bug: clear buffer correctly, concatenate color sequences
● Refactor C# tests to use CreateMinimalValidFontContent utility

### chore(gitignore): exclude Claude Code worktrees directory
2026-04-01 : Paulo Santos

---

## January 2026

### Automate GitHub/winget release in build-release.ps1
2026-01-24 : Paulo Santos
● Add -Publish switch to automate GitHub release and winget PR
● On -Publish: check for GitHub CLI, create GitHub release with assets, generate winget manifests with correct URLs and hashes, fork/clone winget-pkgs, push branch, and submit PR
● Retain manual next steps when -Publish is not used

### Date-based versioning for publish scripts & automation
2026-01-24 : Paulo Santos
● Introduce date-based versioning (Major.Minor.YY.MMDD[-suffix]) for both VS extension and CLI publish scripts
● Add new root publish.ps1 for VSIX: updates manifest, builds, and optionally publishes to Marketplace with PAT
● Refactor NuGet/CLI publish.ps1 to use date-based versioning
● Update FIGPrint release script to use date-based versioning and correct GitHub repo reference

### Add FIGLet test suite, release workflow, and docs
2026-01-16 : Paulo Santos
● Introduced comprehensive MSTest suite for FIGLet library with font, renderer, smushing, integration, and performance tests
● Embedded test fonts and expected outputs for regression testing
● Added GitHub Actions for NuGet, CLI, VS Code, and VS extension publishing
● Added CLAUDE.md for AI assistant guidance and project architecture
● Modernized VS Code extension webview to React/JSX, added webpack config and dependencies
● Removed legacy previewPanel.ts implementation

### Improve VSIX deploy: skip if same or higher version exists
2026-01-16 : Paulo Santos
● Add CheckExtensionVersion.ps1 to globally check installed VSIX versions
● Override FindExistingDeploymentPath to support skip logic
● Skip deploy if version is not newer than what is installed
