using System.Diagnostics;
using VlessVpnClient.Core.Logging;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Xray;

public sealed class XrayProcessManager : IDisposable
{
    private readonly object _lock = new();
    private readonly IAppLogger _logger;
    private Process? _process;
    private string? _configPath;

    public bool IsRunning
    {
        get
        {
            lock (_lock)
            {
                return _process is { HasExited: false };
            }
        }
    }

    public event EventHandler<string>? StdoutReceived;
    public event EventHandler<string>? StderrReceived;
    public event EventHandler<int>? Exited;

    public XrayProcessManager(IAppLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Writes the generated config.json next to the binary and starts xray.
    /// </summary>
    public void Start(string xrayBinaryPath, ServerProfile profile, AppSettings settings, string workingDirectory)
    {
        if (string.IsNullOrEmpty(xrayBinaryPath) || !File.Exists(xrayBinaryPath))
        {
            throw new FileNotFoundException("xray binary not found", xrayBinaryPath);
        }

        Directory.CreateDirectory(workingDirectory);
        var configJson = XrayConfigBuilder.Build(profile, settings);
        _configPath = Path.Combine(workingDirectory, "config.json");
        File.WriteAllText(_configPath, configJson);

        var psi = new ProcessStartInfo
        {
            FileName = xrayBinaryPath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        psi.ArgumentList.Add("run");
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add(_configPath);

        lock (_lock)
        {
            Stop();

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                _logger.LogInfo($"xray: {e.Data}");
                StdoutReceived?.Invoke(this, e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                _logger.LogWarning($"xray-stderr: {e.Data}");
                StderrReceived?.Invoke(this, e.Data);
            };
            process.Exited += (_, _) =>
            {
                _logger.LogInfo($"xray exited with code {process.ExitCode}");
                Exited?.Invoke(this, process.ExitCode);
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start xray process");
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _process = process;
            _logger.LogInfo($"xray started with PID {process.Id}");
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_process is null) return;
            try
            {
                if (!_process.HasExited)
                {
                    _logger.LogInfo($"Stopping xray PID {_process.Id}");
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping xray: {ex.Message}");
            }
            finally
            {
                _process.Dispose();
                _process = null;
            }
        }
    }

    public void Dispose() => Stop();
}
