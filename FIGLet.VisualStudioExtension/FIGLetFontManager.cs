using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FIGLet.VisualStudioExtension
{
    // Font management helper class
    public class FIGLetFontManager
    {
        private FIGLetFontManager() { }

        public static FIGLetFontManager Instance { get; } = _instance ??= new FIGLetFontManager();
        private static FIGLetFontManager _instance;

        private readonly Dictionary<string, string> _fontPaths = [];

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

        public bool FontExists(string fontName)
        {
            return _fontPaths.ContainsKey(fontName) ||
                   File.Exists(Path.Combine(GetDefaultFontPath(), fontName));
        }

        private string GetDefaultFontPath()
        {
            // Return path to built-in fonts directory
            return Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "Fonts");
        }
    }
}