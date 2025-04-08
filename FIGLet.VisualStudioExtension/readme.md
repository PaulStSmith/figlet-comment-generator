# FIGLet Comment Generator for Visual Studio

```
 ___ ___ ___ _        _        ___                         _        ___                       _           
| __|_ _/ __| |   ___| |_     / __|___ _ __  _ __  ___ _ _| |_     / __|___ _ _  ___ _ _ __ _| |_ ___ _ _ 
| _| | | (_ | |__/ -_)  _|   | (__/ _ \ '  \| '  \/ -_) ' \  _|   | (_ / -_) ' \/ -_) '_/ _` |  _/ _ \ '_|
|_| |___\___|____\___|\__|    \___\___/_|_|_|_|_|_\___|_||_\__|    \___\___|_||_\___|_| \__,_|\__\___/_|  

```

Add stylish ASCII art text banners to your code comments! This Visual Studio extension allows you to generate FIGLet-based ASCII art text headers that make your code more organized and visually appealing.

## Features

* Generate ASCII art text headers for classes, methods, or any custom text
* Integrates directly into Visual Studio's menu and context menu
* Automatically detects current code elements (classes, methods) for quick banner creation
* Supports a wide variety of programming languages with appropriate comment styles
* Visual preview of generated banners before insertion
* Multiple FIGLet fonts and layout options
* Theme-aware UI that matches your Visual Studio color scheme

## Installation

Install directly from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ByteForge.FIGLetCommentGenerator).

## Usage

### From the Main Menu

1. Click on **Edit → FIGLet Comment Generator** in the main menu
2. Enter your text, select a font and layout mode
3. Preview how it will look in your code
4. Click "OK" to insert the banner at the current cursor position

### From the Context Menu

Right-click in your code editor and select one of:
* **Insert FIGLet Banner** - Creates a banner with custom text
* **Insert FIGLet Class Banner** - Automatically creates a banner for the current class
* **Insert FIGLet Method Banner** - Automatically creates a banner for the current method

### Configuration

Access the extension settings via **Tools → Options → FIGLet Comment Generator**:

* **Font Directory**: Specify a directory containing additional FIGLet font files (.flf)
* **Layout Mode**: Choose the default layout mode for banners
* **Preview**: Test how your banners will look with different fonts

## Supported Languages

The extension automatically detects the appropriate comment style for various programming languages, including:

* C#, C/C++, Java, JavaScript, TypeScript
* Python, Ruby, Perl, R
* HTML, XML, XAML, SVG
* SQL variants (T-SQL, MySQL, PostgreSQL, etc.)
* PowerShell, Bash, and many more

## Custom Fonts

The extension comes with the default FIGLet font, but you can add more by:

1. Downloading .flf font files from [FIGLet Font Database](http://www.figlet.org/fontdb.cgi)
2. Placing them in your configured font directory
3. Selecting them from the dropdown in the generator dialog

## Examples

### Class Banner
```csharp
/*
 *   ___         _     ___ _                   _   ___      _          _           
 *  / __|___  __| |___| __| |___ _ __  ___ _ _| |_|   \ ___| |_ ___ __| |_ ___ _ _ 
 * | (__/ _ \/ _` / -_) _|| / -_) '  \/ -_) ' \  _| |) / -_)  _/ -_) _|  _/ _ \ '_|
 *  \___\___/\__,_\___|___|_\___|_|_|_\___|_||_\__|___/\___|\__\___\__|\__\___/_|  
 *                                                                                 
 */
internal partial class CodeElementDetector
{
    // Class implementation
}
```

### Method Banner
```csharp
/*
 * __   ___ _        _ __  __     _   _            _ ___                       
 * \ \ / (_) |_ __ _| |  \/  |___| |_| |_  ___  __| | _ \_ _ ___  __ ___ ______
 *  \ V /| |  _/ _` | | |\/| / -_)  _| ' \/ _ \/ _` |  _/ '_/ _ \/ _/ -_)_-<_-<
 *   \_/ |_|\__\__,_|_|_|  |_\___|\__|_||_\___/\__,_|_| |_| \___/\__\___/__/__/
 *                                                                             
 */
public void VitalMethodProcess()
{
    // Method implementation
}
```

## Credits

* Based on the [FIGLet](http://www.figlet.org/) ASCII art text technology
* Developed by ByteForge

## License

[MIT License](LICENSE)