using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FIGLet.VisualStudioExtension;

/// <summary>
/// Manages FIGLet fonts by loading them from a directory and checking their existence.
/// </summary>
public class FIGLetFontManager
{
    private FIGLetFontManager() { }

    /// <summary>
    /// Gets the singleton instance of the <see cref="FIGLetFontManager"/> class.
    /// </summary>
    public static FIGLetFontManager Instance { get; } = _instance ??= new FIGLetFontManager();
    private static FIGLetFontManager _instance;

    private readonly Dictionary<string, string> _fontPaths = new();

    /// <summary>
    /// Loads FIGLet fonts from the specified directory.
    /// </summary>
    /// <param name="directory">The directory to load fonts from.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
    public void LoadFontsFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Font directory not found: {directory}");

        _fontPaths.Clear();
        foreach (var file in Directory.GetFiles(directory, "*.flf"))
        {
            var fontName = Path.GetFileNameWithoutExtension(file);
            _fontPaths[fontName] = file;
        }
    }

    /// <summary>
    /// Checks if a font with the specified name exists.
    /// </summary>
    /// <param name="fontName">The name of the font to check.</param>
    /// <returns><c>true</c> if the font exists; otherwise, <c>false</c>.</returns>
    public bool FontExists(string fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            return false;

        if (_fontPaths.TryGetValue(fontName, out var fontPath))
            return File.Exists(fontPath);

        return false;
    }

    /// <summary>
    /// Gets the default path to the built-in fonts directory.
    /// </summary>
    /// <returns>The default fonts directory path.</returns>
    private string GetDefaultFontPath()
    {
        // Return path to built-in fonts directory
        return Path.Combine(Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location), "Fonts");
    }
}
