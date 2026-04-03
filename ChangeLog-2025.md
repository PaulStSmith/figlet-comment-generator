# ChangeLog 2025

[← Back to master index](ChangeLog.md) | [2026](ChangeLog-2026.md) | [2024](ChangeLog-2024.md)

---

## May 2025

### Update versioning, add publish script, and improve docs
2025-05-01 : Paulo Santos
● Updated FIGLet Comment Generator version to 1.2.3
● Modified target frameworks in FIGLet.csproj to include net9.0 and updated package version to 1.1.0
● Added publish.ps1 script for automating NuGet package updates and publishing
● Refactored FIGLetRenderer class for improved constructor initialization
● Enhanced README with new features, usage examples, and documentation updates

### Update package name and success message in publish script
2025-05-01 : Paulo Santos
● Removed "ByteForge" prefix from package name in synopsis and description
● Updated success message to reflect the new package name
● Ensured naming consistency throughout the script

---

## April 2025

### Refactor FIGLetFontManager and update UI components
2025-04-11 : Paulo Santos
● Refactored FIGLetFontManager to implement IDisposable and added FileSystemWatcher for monitoring font directory changes
● Updated font loading method to be an instance method and improved directory handling
● Removed ObjectDataProvider in FIGLetOptionsControl.xaml and populated LayoutModeComboBox with LayoutModeItem objects in code-behind
● Changed PreviewTextBox font to "Cascadia Mono" in FIGLetOptionsControl.xaml

### Refactor FIGLetRenderer and update project files
2025-04-09 : Paulo Santos
● Updated paragraph splitting method in FIGLetRenderer.cs for improved syntax
● Simplified AssemblyName in FIGPrint.csproj by removing the ByteForge. prefix
● Removed Ivrit.flf font resource from the project
● Rearranged comments and changed output method in Program.cs for clarity

### Refactor FIGFont and FIGLetRenderer classes
2025-04-08 : Paulo Santos
● Updated Characters property in FIGFont to use Dictionary<int, string[]> for broader character support
● Added Comments property to FIGFont for storing font-related comments
● Refactored character parsing into a new method ParseCharacterLine for improved readability
● Introduced paragraphMode parameter in FIGLetRenderer.Render for paragraph rendering options
● Updated rendering logic in FIGLetRenderer to support right-to-left (RTL) fonts and surrogate pairs
● Added new font file Ivrit.flf containing Hebrew characters

### Update version to 1.2.2.1 and release notes
2025-04-08 : Paulo Santos
● Updated ReleaseNotes.html to include version 1.2.2.1 and a bug fixes section
● Updated version number in source.extension.vsixmanifest from 1.2.2 to 1.2.2.1

### Update extension publisher in installation instructions
2025-04-08 : Paulo Santos
● Changed the extension name from "ByteForge.FIGLetCommentGenerator" to "PaulStSmith.FIGLetCommentGenerator" to reflect the correct publisher

### Enhance FIGLet Comment Generator functionality
2025-04-08 : Paulo Santos
● Improved GenerateIndentation method to handle virtual space
● Revised copyright year in LICENSE.txt and AssemblyInfo.cs
● Added OriginalOverview.md for detailed extension description
● Enhanced text input handling in FIGLetOptionsControl
● Improved UI and input handling in FigletInputDialogView
● Introduced FIGFontStream to allow loading of compressed font files
● Refactored FIGletRenderer for better rendering logic
● Updated command-line handling in Program.cs
● Added readme.md for user guidance and installation

### Update documentation and enhance FigPrint features
2025-04-06 : Paulo Santos
● Clarified licensing information and contribution notes in readme.md
● Added ASCII art, detailed synopsis, and extensive usage descriptions in figlet.man.md
● Specified that FigPrint looks for fonts in a fonts directory relative to the executable
● Updated future enhancements to include ANSI color support and custom font directory options

### Update project to support multiple target frameworks
2025-04-06 : Paulo Santos
● Added support for .NET Framework (4.7.2, 4.8, 4.8.1), .NET Standard (2.0, 2.1), and .NET Core (3.1, 5.0, 6.0, 7.0, 8.0)
● Introduced new package metadata properties: PackageId, Version, Authors, Description, PackageTags, PackageLicenseExpression, PackageReadmeFile, PackageProjectUrl, and RepositoryUrl
● Included readme.md in the package

### Refactor project namespace to ByteForge
2025-04-06 : Paulo Santos
● Changed namespace from FIGLet.VisualStudioExtension to ByteForge.FIGLet.VisualStudioExtension across multiple files
● Updated using directives to reflect the new namespace
● Modified project files (.csproj) to update RootNamespace and AssemblyName
● Restructured Program.cs with improved error handling
● Updated BitmapToFigFont project to align with the new namespace

### Update README with sample output and layout modes
2025-04-06 : Paulo Santos

### Add ANSI color support
2025-04-06 : Paulo Santos
● Add ANSI escape sequence support to FIGLetRenderer for colored terminal output

### Enhance FIGFont handling and add FontGenerator project
2025-04-05 : Paulo Santos
● Enhanced FIGFont parsing and handling
● Added FontGenerator project for converting bitmap images to FIGLet fonts

### Add FIGPrint project to solution
2025-04-05 : Paulo Santos
● Added FIGPrint CLI tool to the solution
● Wired up System.CommandLine for argument parsing

### Create FIGPrint console application with dependencies
2025-04-05 : Paulo Santos
● Created FIGPrint.csproj as a new .NET console application
● Added dependencies on FIGLet core library and System.CommandLine
