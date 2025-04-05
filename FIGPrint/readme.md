# FigPrint

A command-line utility for rendering text using FIGlet fonts in .NET.

## Overview

FigPrint is a simple yet powerful command-line application that leverages the FIGLet Core Library to render text in various ASCII art fonts. It provides an easy way to create eye-catching text banners for console applications, scripts, or just for fun.

## Features

- Render text using any FIGlet font (.flf files)
- Support for all standard FIGlet layout modes:
  - Full Size (no smushing)
  - Kerning (minimal smushing)
  - Smushing (full character overlap)
- Easily list all available fonts
- Simple, intuitive command-line interface

## Installation

1. Download the latest release from the releases page
2. Extract the files to a directory of your choice
3. Add the directory to your PATH (optional, for global access)

## Usage

```
Usage:
  figprint [options] [text...]

Arguments:
  [text...]  The text to render

Options:
  --font <fontname>     The FIGlet font to use for rendering [default: standard]
  --layout <mode>       The layout mode to use: FullSize, Kerning, or Smushing [default: Smushing]
  --showList            Display a list of available fonts
  --help                Display help and usage information
```

### Examples

```bash
# Basic usage
figprint Hello World

# Specify a font
figprint --font big Hello World

# Change layout mode
figprint --layout Kerning Hello World

# Show available fonts
figprint --showList
```

## Font Management

FigPrint looks for fonts in a `fonts` directory relative to the executable. All `.flf`, `.flc`, and `.tlf` files in this directory will be available for use.

## Requirements

- .NET 8.0 or higher

## Building from Source

1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Build the project: `dotnet build`
4. Run the application: `dotnet run`

## Future Enhancements

The following features are planned for future releases:

1. **Color support** - Add options to colorize the output using console colors
2. **Custom font directory** - Allow specifying a custom font directory with a `--fontDir` parameter
3. **Preview mode** - Show a sample of each font when listing available fonts
4. **Horizontal alignment** - Add options for left, center, or right alignment

## Dependencies

- [FIGLet Core Library](https://github.com/PaulStSmith/figlet-comment-generator/tree/master/FIGLet) - A C# implementation of FIGLet
- [System.CommandLine](https://github.com/dotnet/command-line-api) - A powerful command-line parsing library

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Original FIGLet concept by Frank, Ian & Glenn
- Implementation using the FIGLet Core Library by Paulo Santos