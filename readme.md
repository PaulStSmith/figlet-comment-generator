# FIGLet Comment Generator

A Visual Studio and VS Code extension that allows developers to generate ASCII art comments using FIGLet fonts. Add beautiful, eye-catching header comments to your code with just a few keystrokes!

## Planned Features

- 🎨 Generates ASCII art comments in any programming language
- 🔤 Supports for multiple FIGLet fonts
- ⚙️ Automatically uses the correct comment syntax for different file types
- 📐 Multiple layout modes (Full Size, Kerning, Smushing)
- 🎯 Intelligent character compression with customizable smushing rules
- 💡 Context-aware comment insertion
- ⌨️ Configurable keyboard shortcuts

## Installation

### Visual Studio

1. Open Visual Studio
2. Go to Extensions > Manage Extensions
3. Search for "FIGLet Comment Generator"
4. Click Download and restart Visual Studio

### VS Code

1. Open VS Code
2. Press `Ctrl+P` (Windows/Linux) or `Cmd+P` (macOS)
3. Type `ext install FIGLet-comment-generator`
4. Press Enter and reload VS Code

## Usage

1. Place your cursor where you want to insert the ASCII art comment
2. Press `Ctrl+Shift+F` (Windows/Linux) or `Cmd+Shift+F` (macOS)
3. Type your text in the input box
4. Select a font from the dropdown (optional)
5. Press Enter to generate and insert the comment

### Example

Input:
```
Hello, World!
```

Output (using "small" font):

* C#
```csharp
/*
 *   _  _     _ _          __      __       _    _ _
 *  | || |___| | |___      \ \    / /__ _ _| |__| | |
 *  | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
 *  |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
 *                   |/
 */
```

* Visual Basic
```vb
'   _  _     _ _          __      __       _    _ _
'  | || |___| | |___      \ \    / /__ _ _| |__| | |
'  | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
'  |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
'                   |/
```

* Python
```python
#   _  _     _ _          __      __       _    _ _
#  | || |___| | |___      \ \    / /__ _ _| |__| | |
#  | __ / -_) | / _ \_     \ \/\/ / _ \ '_| / _` |_|
#  |_||_\___|_|_\___( )     \_/\_/\___/_| |_\__,_(_)
#                   |/
```

## Configuration

### Visual Studio

Go to Tools > Options > FIGLet Comment Generator to configure:
- Default font
- Layout mode
- Comment style
- Keyboard shortcuts

### VS Code

1. Open Settings (`Ctrl+,` or `Cmd+,`)
2. Search for "FIGLet"
3. Adjust settings as needed

## Supported Languages

The extension automatically detects the file type and uses the appropriate comment syntax:

- C-style languages (C, C++, C#, Java, JavaScript): `//` or `/* */`
- Python: `#`
- HTML/XML: `<!-- -->`
- SQL: `--`
- PowerShell: `#`
- And many more!

## Technical Details

The extension is built on a robust FIGLet implementation that includes:

- Full support for the FIGLet font format (.flf)
- Multiple layout modes (FullSize, Kerning, Smushing)
- Comprehensive smushing rules:
  - Equal character smushing
  - Underscore smushing
  - Hierarchy smushing
  - Opposite pair smushing
  - Big X smushing
  - Hardblank smushing
- Efficient string manipulation using StringBuilder
- Thread-safe design

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- FIGLet (http://www.org/) for the original FIGLet specification
- The FIGLet font designers for their creative contributions
- The Visual Studio and VS Code extension ecosystem

## Support

If you encounter any issues or have feature requests, please:
1. Check the [FAQ](docs/FAQ.md)
2. Search existing [issues](https://github.com/PaulStSmith/FIGLet-comment-generator/issues)
3. Create a new issue if needed

---

Made with ❤️ by Paulo Santos
