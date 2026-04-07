# FIGlet Comment Generator — Release Notes

## 1.4.0 — April 2026

### New Features

- **Insert Class Banner** — right-click inside a class, interface, struct, enum, or module to open the banner panel pre-filled with the detected symbol name. Requires an active language server for the file type.
- **Insert Function/Method Banner** — same smart workflow for function and method declarations. Both commands are available in the right-click context menu and the Command Palette.
- Both commands fall back gracefully to cursor-position insertion when no language server is available or no matching symbol is found at the cursor.

### Enhancements

- Smart insertion point detection: banners are placed *above* any existing XML-doc, JSDoc, or block comments that immediately precede the declaration — not between the doc comment and the code.
- Pascal language: added PasDoc-style block comment recognition (`{ }` and `(* *)`) for correct insertion-point detection.
- Webview panel is now kept alive when hidden behind another editor tab (`retainContextWhenHidden`), avoiding a cold-start penalty on subsequent invocations.
- Pre-filled text in the banner panel is reliably selected on first open, allowing the user to press **Enter** immediately or retype to change it.

---

## 1.3.0 — April 2026

### New Features

- Retro-terminal aesthetic with a clean, modern UI for the banner composer panel.
- Context-aware insertion: the extension detects the cursor position and applies the correct comment syntax automatically.
- Customizable keyboard shortcuts — default shortcut `Ctrl+Alt+B` to invoke the banner generator.
- Live preview of banners before insertion.

### Enhancements

- Renderer refactored to support ANSI color codes, paragraph mode, and RTL fonts.
- Stricter FIGfont header validation for better error messages on malformed `.flf` files.
- Font manager now loads and caches `.flf` files from a configurable directory.
- Updated settings panel with improved UX: accessible via `Shift+Ctrl+P` → `FIGLet Settings`, right-click context menu, or `Ctrl+,`.
- Three layout modes fully supported: Full Size, Kerning, and Smushing (default).
- All six official smushing rules implemented: Equal, Underscore, Hierarchy, Opposite Pair, Big X, Hardblank.

### Language Support

Automatic detection of the active file type with correct comment-syntax wrapping:

- C-style languages (C, C++, C#, Java, JS, TS): `//` or `/* */`
- Python, PowerShell, Ruby, Shell: `#`
- HTML / XML: `<!-- -->`
- SQL: `--`
- 20+ additional languages supported
