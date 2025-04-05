using FIGLet; // Your FIGlet Core Library
using System.CommandLine;

namespace FIGPrint
{
    class Program
    {
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

            // Add text argument
            var textArgument = new Argument<string[]>(
                "text",
                getDefaultValue: () => new string[0],
                description: "The text to render")
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            // Add options and arguments to the root command
            rootCommand.AddOption(fontOption);
            rootCommand.AddOption(layoutOption);
            rootCommand.AddOption(showListOption);
            rootCommand.AddArgument(textArgument);

            // Set the handler
            rootCommand.SetHandler((string font, LayoutMode layout, bool showList, string[] text) =>
            {
                if (showList)
                {
                    ShowAvailableFonts();
                    return Task.FromResult(0);
                }

                if (text.Length == 0)
                {
                    Console.WriteLine("No text provided to render. Use --help for usage information.");
                    return Task.FromResult(1);
                }

                return RenderTextWithFigletAsync(font, layout, string.Join(" ", text));
            }, fontOption, layoutOption, showListOption, textArgument);

            // Parse the command line arguments
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> RenderTextWithFigletAsync(string fontName, LayoutMode layout, string textToRender)
        {
            try
            {
                // Find the font file
                string fontPath = FindFontFile(fontName);

                if (string.IsNullOrEmpty(fontPath))
                {
                    Console.WriteLine($"Error: Font '{fontName}' not found in the fonts directory.");
                    ShowAvailableFonts();
                    return 1;
                }

                // Load the FIGlet font
                var font = FIGFont.FromFile(fontPath);

                // Create a renderer
                var renderer = new FIGLetRenderer(font);

                // Render the text using the specified layout mode
                string renderedText = renderer.Render(textToRender, layout);

                // Output the rendered text
                Console.WriteLine(renderedText);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private static string FindFontFile(string fontName)
        {
            // Determine the fonts directory relative to the executable
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string fontsDir = Path.Combine(exePath, "fonts");

            // Check if directory exists
            if (!Directory.Exists(fontsDir))
            {
                Console.WriteLine($"Warning: Fonts directory not found at {fontsDir}");
                return null;
            }

            // Look for the font file with various possible extensions
            string[] possibleExtensions = { ".flf", ".FLF", "" };

            foreach (var ext in possibleExtensions)
            {
                string fullFontName = fontName + ext;
                string fontPath = Path.Combine(fontsDir, fullFontName);

                if (File.Exists(fontPath))
                {
                    return fontPath;
                }
            }

            return null;
        }

        private static void ShowAvailableFonts()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string fontsDir = Path.Combine(exePath, "fonts");

            if (!Directory.Exists(fontsDir))
            {
                Console.WriteLine("No fonts directory found.");
                return;
            }

            string[] fontFiles = Directory.GetFiles(fontsDir, "*.flf")
                .Concat(Directory.GetFiles(fontsDir, "*.flc"))
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
                string fontName = Path.GetFileNameWithoutExtension(fontFile);
                Console.WriteLine($"  - {fontName}");
            }
        }
    }
}