using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Storage;

public sealed class SubscriptionStore
{
    private readonly string _path;
    private readonly object _lock = new();
    private List<Subscription> _items;

    public SubscriptionStore(string path)
    {
        _path = path;
        _items = JsonStorage.Load<List<Subscription>>(path) ?? new List<Subscription>();
    }

    public IReadOnlyList<Subscription> All
    {
        get { lock (_lock) return _items.ToArray(); }
    }

    public Subscription? GetById(string id)
    {
        lock (_lock) return _items.FirstOrDefault(s => s.Id == id);
    }

    public void Add(Subscription sub)
    {
        lock (_lock)
        {
            _items.Add(sub);
            JsonStorage.Save(_path, _items);
        }
    }

    public void Remove(string id)
    {
        lock (_lock)
        {
            _items.RemoveAll(s => s.Id == id);
            JsonStorage.Save(_path, _items);
        }
    }

    public void Save()
    {
        lock (_lock) JsonStorage.Save(_path, _items);
    }
}
