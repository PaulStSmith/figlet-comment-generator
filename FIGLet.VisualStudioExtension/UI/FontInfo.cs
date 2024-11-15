using System;
using System.IO;

namespace FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Represents information about a FIGlet font.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FontInfo"/> class with the specified file path.
/// </remarks>
/// <param name="filePath">The file path of the FIGlet font.</param>
public class FontInfo(string filePath)
{

    /// <summary>
    /// Gets the name of the font.
    /// </summary>
    public string Name { get; private set; } = Path.GetFileNameWithoutExtension(filePath);

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
    /// Gets or sets the file path of the font.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    public string FilePath
    {
        get => filePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

            if (filePath != value)
            {
                filePath = value;
                Font = FIGFont.FromFile(value);
                Name = Path.GetFileNameWithoutExtension(value);
            }
        }
    }

    /// <summary>
    /// Gets the FIGFont object.
    /// </summary>
    public FIGFont Font { get; private set; } = FIGFont.FromFile(filePath);
}