using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;

namespace ByteForge.FIGLet.VisualStudioExtension;

/*
 *  ___ ___ ___ _        _   ___        _   __  __                             
 * | __|_ _/ __| |   ___| |_| __|__ _ _| |_|  \/  |__ _ _ _  __ _ __ _ ___ _ _ 
 * | _| | | (_ | |__/ -_)  _| _/ _ \ ' \  _| |\/| / _` | ' \/ _` / _` / -_) '_|
 * |_| |___\___|____\___|\__|_|\___/_||_\__|_|  |_\__,_|_||_\__,_\__, \___|_|  
 *                                                               |___/         
 */
/// <summary>
/// Manages FIGLet fonts by loading them from a directory and monitoring for changes.
/// </summary>
public class FIGLetFontManager : IDisposable
{
    private FileSystemWatcher _fileSystemWatcher;
    private string _fontDirectory;
    private bool _disposed = false;
    private readonly Lazy<List<FIGFontInfo>> _availableFonts = new(static () => [FIGFontInfo.Default]);

    /// <summary>
    /// Gets the list of available fonts.
    /// </summary>
    public IReadOnlyList<FIGFontInfo> AvailableFonts { get => _availableFonts.Value; }

    /// <summary>
    /// Event that is raised when the font collection changes.
    /// </summary>
    public event EventHandler FontsChanged;

    /// <summary>
    /// Initializes a new instance of the FIGLetFontManager class.
    /// </summary>
    public FIGLetFontManager()
    {
        // Create file system watcher but don't enable it until directory is set
        _fileSystemWatcher = new FileSystemWatcher
        {
            Filter = "*.flf",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                                             NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };

        // Attach event handlers
        _fileSystemWatcher.Error += OnFileSystemWatcherError;
        _fileSystemWatcher.Created += OnFontFileChanged;
        _fileSystemWatcher.Deleted += OnFontFileChanged;
        _fileSystemWatcher.Renamed += OnFontFileChanged;
        _fileSystemWatcher.Changed += OnFontFileChanged;
    }

    /// <summary>
    /// Loads FIGLet fonts from the specified directory.
    /// </summary>
    /// <param name="directory">The directory to load fonts from.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
    private void LoadFontsFromDirectory(string directory)
    {
        if (string.IsNullOrEmpty(directory))
            return;

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Font directory not found: {directory}");

        var fontList = new List<FIGFontInfo>
        {
            FIGFontInfo.Default
        };

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

        _availableFonts?.Value.Clear();
        _availableFonts?.Value.AddRange(fontList);

        // Notify listeners that fonts have changed
        OnFontsChanged();
    }

    /// <summary>
    /// Gets or sets the directory from which to load FIGLet fonts and monitors for changes.
    /// </summary>
    public string FontDirectory
    {
        get => _fontDirectory;
        set
        {
            if (_fontDirectory != value)
            {
                // Stop watching the old directory
                if (_fileSystemWatcher.EnableRaisingEvents)
                {
                    _fileSystemWatcher.EnableRaisingEvents = false;
                }

                _fontDirectory = value;

                if (!string.IsNullOrEmpty(_fontDirectory) && Directory.Exists(_fontDirectory))
                {
                    // Configure file system watcher for the new directory
                    _fileSystemWatcher.Path = _fontDirectory;
                    _fileSystemWatcher.EnableRaisingEvents = true;

                    // Load fonts from the new directory
                    LoadFontsFromDirectory(_fontDirectory);
                }
                else
                {
                    // Reset to just the default font if directory is invalid
                    _availableFonts?.Value.Clear();
                    _availableFonts?.Value.Add(FIGFontInfo.Default);
                    OnFontsChanged();
                }
            }
        }
    }

    /// <summary>
    /// Event handler for file system change events
    /// </summary>
    private void OnFontFileChanged(object sender, FileSystemEventArgs e)
    {
        // Reload fonts when any changes are detected
        LoadFontsFromDirectory(_fontDirectory);
    }

    /// <summary>
    /// Event handler for file system watcher errors
    /// </summary>
    private void OnFileSystemWatcherError(object sender, ErrorEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"FileSystemWatcher error: {e.GetException().Message}");

        // Try to recover by restarting the watcher
        try
        {
            if (_fileSystemWatcher != null && !string.IsNullOrEmpty(_fontDirectory) && Directory.Exists(_fontDirectory))
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Path = _fontDirectory;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to restart FileSystemWatcher: {ex.Message}");
        }
    }

    /// <summary>
    /// Raises the FontsChanged event
    /// </summary>
    private void OnFontsChanged()
    {
        FontsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Releases all resources used by the FIGLetFontManager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the FIGLetFontManager and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_fileSystemWatcher != null)
                {
                    _fileSystemWatcher.EnableRaisingEvents = false;
                    _fileSystemWatcher.Created -= OnFontFileChanged;
                    _fileSystemWatcher.Deleted -= OnFontFileChanged;
                    _fileSystemWatcher.Renamed -= OnFontFileChanged;
                    _fileSystemWatcher.Changed -= OnFontFileChanged;
                    _fileSystemWatcher.Error -= OnFileSystemWatcherError;
                    _fileSystemWatcher.Dispose();
                    _fileSystemWatcher = null;
                }
            }

            _disposed = true;
        }
    }
}