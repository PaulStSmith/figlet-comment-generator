# FIGLet Comment Generator for VS Code

```
 ___ ___ ___ _        _        ___                         _        ___                       _
| __|_ _/ __| |   ___| |_     / __|___ _ __  _ __  ___ _ _| |_     / __|___ _ _  ___ _ _ __ _| |_ ___ _ _
| _| | | (_ | |__/ -_)  _|   | (__/ _ \ '  \| '  \/ -_) ' \  _|   | (_ / -_) ' \/ -_) '_/ _` |  _/ _ \ '_|
|_| |___\___|____\___|\__|    \___\___/_|_|_|_|_|_\___|_||_\__|    \___\___|_||_\___|_| \__,_|\__\___/_|
```

Add stylish ASCII art text banners to your code comments! This VS Code extension lets you generate
FIGLet-based ASCII art text headers that make your code more organised and visually appealing.

## Features

* Generate ASCII art text headers for classes, methods, or any custom text
* Integrates into the editor context menu and the Command Palette
* Automatically wraps banners in the correct comment style for the active language
* Supports 40+ programming languages with appropriate comment syntax
* Live preview of the generated banner before insertion
* Multiple FIGLet fonts and layout modes (Full Size, Kerning, Smushing)
* Dedicated settings panel with font preview

## Installation

Install directly from the
[VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.figlet-comment-generator).

## Usage

### Keyboard Shortcut

Press **`Ctrl+Alt+B`** (Windows / Linux) or **`Cmd+Alt+B`** (macOS) while the editor is focused to
open the banner generator.

### Command Palette

Open the Command Palette (**`Ctrl+Shift+P`** / **`Cmd+Shift+P`**) and search for:

* **FIGlet Comments: Generate FIGlet Banner** – open the banner generator

### Context Menu

Right-click anywhere in the editor and choose **FIGlet Comments → Generate FIGlet Banner**.

### Settings

Open the settings panel via:

* Context menu **FIGlet Comments → FIGlet Settings**, or
* Command Palette **FIGlet Comments: FIGlet Settings**

Available settings:

| Setting | Description |
|---|---|
| `figlet.fontDirectory` | Path to a directory containing additional `.flf` font files |
| `figlet.defaultFont` | Font selected by default when the panel opens |
| `figlet.layoutMode` | Default layout: `full`, `kerning`, or `smush` |

You can also configure these values in **File → Preferences → Settings** under the
**FIGlet Comments** section.

## Layout Modes

| Mode | Description |
|---|---|
| **Full Size** | Characters placed side-by-side with no overlap |
| **Kerning** | Characters moved together until they touch |
| **Smushing** | Characters merged using the font's smushing rules *(default)* |

## Supported Languages

The extension automatically detects the appropriate comment style for the active file, including:

* C#, C/C++, Java, JavaScript, TypeScript
* Python, Ruby, Perl, R
* HTML, XML, XAML, SVG
* SQL variants (T-SQL, MySQL, PostgreSQL, SQLite, …)
* PowerShell, Bash, and many more

## Custom Fonts

The extension ships with the built-in `small` FIGLet font. To use additional fonts:

1. Download `.flf` font files from the [FIGLet Font Database](http://www.figlet.org/fontdb.cgi)
2. Place them in a directory on your machine
3. Set that path in **`figlet.fontDirectory`** (via Settings or the settings panel)
4. The new fonts will appear in the font dropdown immediately

## Examples

### JavaScript

```javascript
//  _  _     _ _      __      __       _    _
// | || |___| | |___  \ \    / /__ _ _| |__| |
// | __ / -_) | / _ \  \ \/\/ / _ \ '_| / _` |
// |_||_\___|_|_\___/   \_/\_/\___/_| |_\__,_|
//
function HelloWorld() {
    // ...
}
```

### Python

```python
#  ___      _   _
# | _ \_  _| |_| |_  ___ _ _
# |  _/ || |  _| ' \/ _ \ ' \
# |_|  \_, |\__|_||_\___/_||_|
#       |__/
def python():
    pass
```

### C#

```csharp
/*
 *   ___         _     ___ _                   _   ___      _          _
 *  / __|___  __| |___| __| |___ _ __  ___ _ _| |_|   \ ___| |_ ___ __| |_ ___ _ _
 * | (__/ _ \/ _` / -_) _|| / -_) '  \/ -_) ' \  _| |) / -_)  _/ -_) _|  _/ _ \ '_|
 *  \___\___/\__,_\___|___|_\___|_|_|_\___|_||_\__|___/\___|\__\___\__|\__\___/_|
 */
internal partial class CodeElementDetector
{
    // ...
}
```

## Getting Started Page

After installation, a **Getting Started** page opens automatically. You can reopen it at any time
via **FIGlet Comments: Getting Started** in the Command Palette.

## Credits

* Based on the [FIGLet](http://www.figlet.org/) ASCII art text technology
* Developed by [ByteForge](https://github.com/PaulStSmith)

## License

[MIT License](LICENSE)
