using System;
using System.IO;

namespace FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Represents information about a FIGlet font.
/// </summary>
public class FontInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FontInfo"/> class with the specified file path.
    /// </summary>
    /// <param name="filePath">The file path of the font.</param>
    public FontInfo(string filePath) => FilePath = filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontInfo"/> class with the specified FIGFont object.
    /// </summary>
    /// <param name="font">The FIGFont object.</param>
    public FontInfo(FIGFont font)
    {
        _filePath = null;
        _name = string.Empty;
        Font = font ?? throw new ArgumentNullException(nameof(font));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontInfo"/> class with the specified FIGFont object and name.
    /// </summary>
    /// <param name="font">The FIGFont object.</param>
    /// <param name="name">The name of the font.</param>
    public FontInfo(FIGFont font, string name) : this(font) => _name = name;

    /// <summary>
    /// Gets the name of the font.
    /// </summary>
    public string Name => _name ??= Path.GetFileNameWithoutExtension(_filePath);
    private string _name;

    /// <summary>
    /// Gets the height of the font.
    /// </summary>
    public int Height => Font.Height;

    /// <summary>
    /// Gets the baseline of the font.
    /// </summary>
    public int Baseline => Font.Baseline;

    /// <summary>
    /// Gets the maximum length of the font.
    /// </summary>
    public int MaxLength => Font.MaxLength;

    /// <summary>
    /// Gets the smushing rules of the font.
    /// </summary>
    public SmushingRules SmushingRules => Font.SmushingRules;

    /// <summary>
    /// Gets the file path of the font.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

            if (_filePath != value)
            {
                _filePath = value;
                _name = Path.GetFileNameWithoutExtension(value);
                Font = FIGFont.FromFile(value);
            }
        }
    }
    private string _filePath;

    /// <summary>
    /// Gets the FIGFont object.
    /// </summary>
    public FIGFont Font { get; private set; }
}