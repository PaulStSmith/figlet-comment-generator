using FIGLet.VisualStudioExtension.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace FIGLet.VisualStudioExtension;

/// <summary>
/// Manages FIGLet fonts by loading them from a directory and checking their existence.
/// </summary>
public static class FIGLetFontManager
{
    /// <summary>
    /// Gets the list of available fonts.
    /// </summary>
    public static IReadOnlyList<FIGFontInfo> AvaliableFonts
    {
        get
        {
            return _availableFonts;
        }
    }
    private static IReadOnlyList<FIGFontInfo> _availableFonts;

    /// <summary>
    /// Loads FIGLet fonts from the specified directory.
    /// </summary>
    /// <param name="directory">The directory to load fonts from.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
    private static void LoadFontsFromDirectory(string directory)
    {
        var fontList = new List<FIGFontInfo>
        {
            FIGFontInfo.Default
        };

        try
        {
            if (string.IsNullOrEmpty(directory))
                return;

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Font directory not found: {directory}");

            foreach (var file in Directory.GetFiles(directory, "*.flf"))
            {
                try
                {
                    fontList.Add(new FIGFontInfo(file));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading font {file}: {ex.Message}");
                }
            }
        }
        finally
        {
            _availableFonts = new ReadOnlyCollection<FIGFontInfo>(fontList);
        }

    }

    /// <summary>
    /// Sets the directory from which to load FIGLet fonts.
    /// </summary>
    /// <param name="directory">The directory to load fonts from.</param>
    public static void SetFontDirectory(string directory)
    {
        if (_fontDirectory != directory)
        {
            _fontDirectory = directory;
            LoadFontsFromDirectory(directory);
        }
    }

    private static string _fontDirectory;
}
