using ByteForge.FIGLet; // Your FIGlet Core Library
using System.CommandLine;

namespace ByteForge.FIGPrint;

/// <summary>
/// The main program class for the FIGPrint application.
/// </summary>
class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An integer representing the exit code.</returns>
    static async Task<int> Main(string[] args)
    {
        // Create command line root command
        var rootCommand = new RootCommand
        {
            Description = "A command-line utility for rendering text using FIGlet fonts"
        };

        // Add options
        var fontOption = new Option<string>(
            "--font",
            getDefaultValue: () => "small",
            description: "The FIGlet font to use for rendering");

        var layoutOption = new Option<LayoutMode>(
            "--layout",
            getDefaultValue: () => LayoutMode.Smushing,
            description: "The layout mode to use: FullSize, Kerning, or Smushing");

        var showListOption = new Option<bool>(
            "--showList",
            getDefaultValue: () => false,
            description: "Display a list of available fonts");

        var useANSIColorsOption = new Option<bool>(
            "--ansi-colors",
            getDefaultValue: () => false,
            description: "Enable the use of ANSI colors");

        // Add text argument
        var textArgument = new Argument<string[]>(
            "text",
            getDefaultValue: () => [],
            description: "The text to render")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        // Add options and arguments to the root command
        rootCommand.AddOption(fontOption);
        rootCommand.AddOption(layoutOption);
        rootCommand.AddOption(showListOption);
        rootCommand.AddOption(useANSIColorsOption);
        rootCommand.AddArgument(textArgument);

        // Set the handler
        rootCommand.SetHandler((string font, LayoutMode layout, bool showList, bool useAnsi, string[] text) =>
        {
            if (showList)
            {
                ShowAvailableFonts();
                return Task.FromResult(0);
            }

            // Ensure UTF-8 encoding for console output
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Check if we have text from arguments
            if (text.Length > 0)
                return RenderTextWithFigletAsync(font, layout, useAnsi, string.Join(" ", text));

            // No text args provided, try to read from stdin
            // Check if we have stdin data (i.e., if something was piped to the application)
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("No text provided to render. Use --help for usage information.");
                return Task.FromResult(1);
            }

            // Read from stdin
            return ProcessStdinAsync(font, layout, useAnsi);
        }, fontOption, layoutOption, showListOption, useANSIColorsOption, textArgument);

        // Parse the command line arguments
        return await rootCommand.InvokeAsync(args);
    }

    /// <summary>
    /// Processes input from standard input (stdin).
    /// </summary>
    /// <param name="font">The FIGlet font to use.</param>
    /// <param name="layout">The layout mode to use.</param>
    /// <param name="useAnsi">Whether to use ANSI colors.</param>
    /// <returns>An integer representing the exit code.</returns>
    private static async Task<int> ProcessStdinAsync(string font, LayoutMode layout, bool useAnsi)
    {
        try
        {
            // Read all text from stdin
            var input = await Console.In.ReadToEndAsync();

            // Process the input
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No input received from stdin.");
                return 1;
            }

            return await RenderTextWithFigletAsync(font, layout, useAnsi, input);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing stdin: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Renders text using the specified FIGlet font and layout mode.
    /// </summary>
    /// <param name="fontName">The name of the FIGlet font to use.</param>
    /// <param name="layout">The layout mode to use.</param>
    /// <param name="useAnsi">Whether to use ANSI colors.</param>
    /// <param name="textToRender">The text to render.</param>
    /// <returns>An integer representing the exit code.</returns>
    private static async Task<int> RenderTextWithFigletAsync(string fontName, LayoutMode layout, bool useAnsi, string textToRender)
    {
        try
        {
            // Find the font file
            var fontPath = FindFontFile(fontName);

            if (string.IsNullOrEmpty(fontPath))
            {
                Console.WriteLine($"Error: Font '{fontName}' not found in the fonts directory.");
                ShowAvailableFonts();
                return 1;
            }

            // Load the FIGlet font
            var font = FIGFont.FromFile(fontPath);

            // Create a renderer
            var renderer = new FIGLetRenderer(font: font, mode: layout, useANSIColors: useAnsi);

            // Render the text using the specified layout mode
            var renderedText = await Task.Run(() => renderer.Render(textToRender));

            // Output the rendered text
            Console.Out.WriteLine(renderedText);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Finds the file path of the specified FIGlet font.
    /// </summary>
    /// <param name="fontName">The name of the FIGlet font to find.</param>
    /// <returns>The file path of the font, or null if not found.</returns>
    private static string? FindFontFile(string fontName)
    {
        // Determine the fonts directory relative to the executable
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        var fontsDir = Path.Combine(exePath, "fonts");

        // Check if directory exists
        if (!Directory.Exists(fontsDir))
        {
            Console.WriteLine($"Warning: Fonts directory not found at {fontsDir}");
            return null;
        }

        // Look for the font file with various possible extensions
        string[] possibleExtensions = [".flf", ".FLF", ""];

        foreach (var ext in possibleExtensions)
        {
            var fullFontName = fontName + ext;
            var fontPath = Path.Combine(fontsDir, fullFontName);

            if (File.Exists(fontPath))
            {
                return fontPath;
            }
        }

        return null;
    }

    /// <summary>
    /// Displays a list of available FIGlet fonts.
    /// </summary>
    private static void ShowAvailableFonts()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        var fontsDir = Path.Combine(exePath, "fonts");

        if (!Directory.Exists(fontsDir))
        {
            Console.WriteLine("No fonts directory found.");
            return;
        }

        var fontFiles = Directory.GetFiles(fontsDir, "*.flf")
            .Concat(Directory.GetFiles(fontsDir, "*.tlf"))
            .ToArray();

        if (fontFiles.Length == 0)
        {
            Console.WriteLine("No font files found in the fonts directory.");
            return;
        }

        Console.WriteLine("Available fonts:");
        foreach (var fontFile in fontFiles.OrderBy(f => Path.GetFileNameWithoutExtension(f)))
        {
            var fontName = Path.GetFileNameWithoutExtension(fontFile);
            Console.WriteLine($"  - {fontName}");
        }
    }
}