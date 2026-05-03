using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Storage;

public sealed class ProfileStore
{
    private readonly string _path;
    private readonly object _lock = new();
    private List<ServerProfile> _profiles;

    public ProfileStore(string path)
    {
        _path = path;
        _profiles = JsonStorage.Load<List<ServerProfile>>(path) ?? new List<ServerProfile>();
    }

    public IReadOnlyList<ServerProfile> All
    {
        get
        {
            lock (_lock) return _profiles.ToArray();
        }
    }

    public ServerProfile? GetById(string profileId)
    {
        lock (_lock) return _profiles.FirstOrDefault(p => p.ProfileId == profileId);
    }

    public void Add(ServerProfile profile)
    {
        lock (_lock)
        {
            _profiles.Add(profile);
            Persist();
        }
    }

    public int AddRange(IEnumerable<ServerProfile> profiles)
    {
        lock (_lock)
        {
            var added = 0;
            foreach (var p in profiles)
            {
                if (_profiles.Any(existing => existing.OriginalUrl == p.OriginalUrl))
                {
                    continue;
                }
                _profiles.Add(p);
                added++;
            }
            Persist();
            return added;
        }
    }

    public void Remove(string profileId)
    {
        lock (_lock)
        {
            _profiles.RemoveAll(p => p.ProfileId == profileId);
            Persist();
        }
    }

    public void RemoveBySubscription(string subscriptionId)
    {
        lock (_lock)
        {
            _profiles.RemoveAll(p => p.SubscriptionId == subscriptionId);
            Persist();
        }
    }

    public void Replace(IEnumerable<ServerProfile> profiles)
    {
        lock (_lock)
        {
            _profiles = profiles.ToList();
            Persist();
        }
    }

    public void Save()
    {
        lock (_lock) Persist();
    }

    private void Persist()
    {
        JsonStorage.Save(_path, _profiles);
    }
}
