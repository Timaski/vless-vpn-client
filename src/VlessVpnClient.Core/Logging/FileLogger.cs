using System.Globalization;

namespace VlessVpnClient.Core.Logging;

/// <summary>
/// Thread-safe file logger with an in-memory ring buffer used by the UI log viewer.
/// </summary>
public sealed class FileLogger : IAppLogger
{
    private readonly object _lock = new();
    private readonly string _path;
    private readonly LinkedList<string> _buffer = new();
    private readonly int _maxBufferEntries;

    public event EventHandler<string>? EntryAdded;

    public FileLogger(string path, int maxBufferEntries = 1000)
    {
        _path = path;
        _maxBufferEntries = maxBufferEntries;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public IReadOnlyList<string> Snapshot()
    {
        lock (_lock)
        {
            return _buffer.ToArray();
        }
    }

    public void LogInfo(string message) => Write("INFO", message);
    public void LogWarning(string message) => Write("WARN", message);
    public void LogError(string message) => Write("ERROR", message);
    public void LogDebug(string message) => Write("DEBUG", message);

    private void Write(string level, string message)
    {
        var entry = string.Format(CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}",
            DateTime.Now, level, message);

        lock (_lock)
        {
            _buffer.AddLast(entry);
            while (_buffer.Count > _maxBufferEntries)
            {
                _buffer.RemoveFirst();
            }

            try
            {
                File.AppendAllText(_path, entry + Environment.NewLine);
            }
            catch
            {
                // best-effort; never throw from logger
            }
        }

        EntryAdded?.Invoke(this, entry);
    }
}
