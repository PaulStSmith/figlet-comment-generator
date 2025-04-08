using System.IO.Compression;

namespace ByteForge.FIGLet;

/// <summary>
/// A stream wrapper that supports reading from a regular stream or the first entry of a ZIP archive.
/// </summary>
internal class FIGFontStream : Stream
{
    private readonly Stream _innerStream;
    private Stream _currentStream;
    private bool _isInitialized = false;
    private readonly byte[] _buffer = new byte[2];
    private int _bufferPos = 0;
    private int _bufferLen = 0;
    private bool _isZipStream = false;
    private ZipArchive? _zipArchive = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGFontStream"/> class.
    /// </summary>
    /// <param name="innerStream">The underlying stream to wrap.</param>
    public FIGFontStream(Stream innerStream)
    {
        _innerStream = innerStream;
        _currentStream = innerStream;
    }

    /// <summary>
    /// Ensures the stream is initialized and determines if it is a ZIP archive.
    /// </summary>
    private void InitializeIfNeeded()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        // Read the first two bytes to check for ZIP signature
        _bufferLen = _innerStream.Read(_buffer, 0, 2);

        // Check if we got a complete signature and if it matches 'PK'
        _isZipStream = (_bufferLen == 2 && _buffer[0] == 'P' && _buffer[1] == 'K');

        if (_isZipStream)
        {
            // Create a composite stream with the signature bytes + rest of original stream
            var compositeStream = new MemoryStream();
            compositeStream.Write(_buffer, 0, _bufferLen);
            _innerStream.CopyTo(compositeStream);
            compositeStream.Position = 0;

            // Extract the first file from the ZIP
            try
            {
                _zipArchive = new ZipArchive(compositeStream, ZipArchiveMode.Read);
                if (_zipArchive.Entries.Count > 0)
                {
                    var firstEntry = _zipArchive.Entries[0];
                    _currentStream = firstEntry.Open();

                    // Clear our buffer since we're now using the zip entry stream
                    _bufferLen = 0;
                    return;
                }
            }
            catch
            {
                // If there's an error opening the ZIP, fall back to treating it as a regular stream
                _isZipStream = false;
            }
        }

        // If we reach here, we're not using a ZIP entry stream,
        // so we'll keep the first bytes in our buffer for reading
    }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the position within the stream.
    /// </summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">The zero-based byte offset in the buffer at which to begin storing data.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <returns>The total number of bytes read into the buffer.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        InitializeIfNeeded();

        var totalBytesRead = 0;

        // First, read any bytes from our internal buffer
        if (_bufferLen > 0 && !_isZipStream)
        {
            var bytesToCopy = Math.Min(_bufferLen - _bufferPos, count);
            Array.Copy(_buffer, _bufferPos, buffer, offset, bytesToCopy);
            _bufferPos += bytesToCopy;
            offset += bytesToCopy;
            count -= bytesToCopy;
            totalBytesRead += bytesToCopy;

            // If we've consumed all buffered bytes, clear the buffer
            if (_bufferPos >= _bufferLen)
            {
                _bufferLen = 0;
                _bufferPos = 0;
            }
        }

        // If we still need more bytes and haven't consumed the whole request from the buffer
        if (count > 0)
        {
            var bytesRead = _currentStream.Read(buffer, offset, count);
            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    /// <inheritdoc />
    public override void Flush() => _currentStream.Flush();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <summary>
    /// Releases the resources used by the <see cref="FIGFontStream"/>.
    /// </summary>
    /// <param name="disposing">A value indicating whether the method is called from Dispose.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_zipArchive != null)
            {
                _zipArchive.Dispose();
                _zipArchive = null;
            }

            if (_currentStream != _innerStream)
            {
                _currentStream.Dispose();
            }

            _innerStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
