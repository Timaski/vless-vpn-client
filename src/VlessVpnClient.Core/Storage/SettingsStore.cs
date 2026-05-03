using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Storage;

public sealed class SettingsStore
{
    private readonly string _path;
    private AppSettings _settings;

    public SettingsStore(string path)
    {
        _path = path;
        _settings = JsonStorage.Load<AppSettings>(path) ?? new AppSettings();
    }

    public AppSettings Current => _settings;

    public void Update(Action<AppSettings> mutate)
    {
        mutate(_settings);
        JsonStorage.Save(_path, _settings);
    }

    public void Replace(AppSettings settings)
    {
        _settings = settings;
        JsonStorage.Save(_path, _settings);
    }
}
