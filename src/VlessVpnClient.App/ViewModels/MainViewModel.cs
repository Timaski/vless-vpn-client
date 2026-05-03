using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using VlessVpnClient.App.Helpers;
using VlessVpnClient.App.Services;
using VlessVpnClient.Core.Logging;
using VlessVpnClient.Core.Models;
using VlessVpnClient.Core.Network;
using VlessVpnClient.Core.Parsing;
using VlessVpnClient.Core.Storage;
using VlessVpnClient.Core.Subscriptions;

namespace VlessVpnClient.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ProfileStore _profileStore;
    private readonly SubscriptionStore _subscriptionStore;
    private readonly SettingsStore _settingsStore;
    private readonly ConnectionManager _connectionManager;
    private readonly IAppLogger _logger;
    private readonly SubscriptionUpdater _subscriptionUpdater;

    public ObservableCollection<ServerProfileViewModel> Profiles { get; } = new();
    public ObservableCollection<Subscription> Subscriptions { get; } = new();

    public AppSettings Settings => _settingsStore.Current;
    public ConnectionManager Connection => _connectionManager;

    private ServerProfileViewModel? _selectedProfile;
    public ServerProfileViewModel? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetField(ref _selectedProfile, value))
            {
                ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteProfileCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public RelayCommand ConnectCommand { get; }
    public RelayCommand DisconnectCommand { get; }
    public RelayCommand DeleteProfileCommand { get; }
    public AsyncRelayCommand PingAllCommand { get; }
    public AsyncRelayCommand RefreshSubscriptionsCommand { get; }

    public MainViewModel(
        ProfileStore profileStore,
        SubscriptionStore subscriptionStore,
        SettingsStore settingsStore,
        ConnectionManager connectionManager,
        IAppLogger logger)
    {
        _profileStore = profileStore;
        _subscriptionStore = subscriptionStore;
        _settingsStore = settingsStore;
        _connectionManager = connectionManager;
        _logger = logger;
        _subscriptionUpdater = new SubscriptionUpdater(logger, settingsStore.Current.DefaultSubscriptionUserAgent);

        foreach (var p in _profileStore.All)
        {
            Profiles.Add(new ServerProfileViewModel(p));
        }
        foreach (var s in _subscriptionStore.All)
        {
            Subscriptions.Add(s);
        }

        var activeId = _settingsStore.Current.ActiveProfileId;
        if (activeId is not null)
        {
            var active = Profiles.FirstOrDefault(p => p.ProfileId == activeId);
            if (active is not null)
            {
                SelectedProfile = active;
            }
        }

        ConnectCommand = new RelayCommand(_ => _ = ConnectAsync(), _ => SelectedProfile is not null);
        DisconnectCommand = new RelayCommand(_ => _ = _connectionManager.DisconnectAsync());
        DeleteProfileCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedProfile is not null);
        PingAllCommand = new AsyncRelayCommand(PingAllAsync);
        RefreshSubscriptionsCommand = new AsyncRelayCommand(RefreshSubscriptionsAsync);

        _connectionManager.PropertyChanged += (_, _) => StatusMessage = _connectionManager.StatusMessage;
    }

    private async Task ConnectAsync()
    {
        if (SelectedProfile is null) return;
        foreach (var p in Profiles)
        {
            p.IsActive = false;
        }
        SelectedProfile.IsActive = true;
        await _connectionManager.ConnectAsync(SelectedProfile.Profile);
    }

    private void DeleteSelected()
    {
        if (SelectedProfile is null) return;
        var id = SelectedProfile.ProfileId;
        Profiles.Remove(SelectedProfile);
        _profileStore.Remove(id);
    }

    public int ImportFromVlessUrl(string url)
    {
        if (!VlessUrlParser.TryParse(url, out var config, out var error) || config is null)
        {
            StatusMessage = $"Не удалось распарсить vless URL: {error}";
            return 0;
        }
        var profile = new ServerProfile
        {
            Config = config,
            OriginalUrl = url
        };
        var added = _profileStore.AddRange(new[] { profile });
        if (added > 0)
        {
            Profiles.Add(new ServerProfileViewModel(profile));
            StatusMessage = $"Импортирован сервер: {profile.DisplayName}";
        }
        else
        {
            StatusMessage = "Сервер уже добавлен";
        }
        return added;
    }

    public int ImportFromSubscriptionContent(string content, string? subscriptionId = null)
    {
        var configs = SubscriptionParser.Parse(content);
        var profiles = configs.Select(c => new ServerProfile
        {
            Config = c,
            OriginalUrl = $"vless://{c.Id}@{c.Address}:{c.Port}",
            SubscriptionId = subscriptionId
        }).ToList();
        var added = _profileStore.AddRange(profiles);
        foreach (var p in profiles.Take(added))
        {
            Profiles.Add(new ServerProfileViewModel(p));
        }
        StatusMessage = $"Добавлено серверов: {added} из {profiles.Count}";
        return added;
    }

    public async Task<int> ImportFromSubscriptionUrlAsync(string name, string url, CancellationToken ct = default)
    {
        var sub = new Subscription { Name = name, Url = url };
        _subscriptionStore.Add(sub);
        Subscriptions.Add(sub);

        var result = await _subscriptionUpdater.FetchAsync(sub, ct);
        if (!result.Success)
        {
            StatusMessage = $"Подписка '{name}' не загрузилась: {result.Error}";
            return 0;
        }

        var profiles = result.Configs.Select(c => new ServerProfile
        {
            Config = c,
            OriginalUrl = $"vless://{c.Id}@{c.Address}:{c.Port}",
            SubscriptionId = sub.Id
        }).ToList();

        var added = _profileStore.AddRange(profiles);
        foreach (var p in profiles.Take(added))
        {
            Profiles.Add(new ServerProfileViewModel(p));
        }
        _subscriptionStore.Save();
        StatusMessage = $"Подписка '{name}': добавлено {added} серверов";
        return added;
    }

    public async Task RefreshSubscriptionsAsync()
    {
        if (Subscriptions.Count == 0)
        {
            StatusMessage = "Нет подписок для обновления";
            return;
        }

        var totalAdded = 0;
        foreach (var sub in Subscriptions.ToArray())
        {
            var result = await _subscriptionUpdater.FetchAsync(sub);
            if (!result.Success) continue;

            var profiles = result.Configs.Select(c => new ServerProfile
            {
                Config = c,
                OriginalUrl = $"vless://{c.Id}@{c.Address}:{c.Port}",
                SubscriptionId = sub.Id
            }).ToList();
            var added = _profileStore.AddRange(profiles);
            foreach (var p in profiles.Take(added))
            {
                Profiles.Add(new ServerProfileViewModel(p));
            }
            totalAdded += added;
        }
        _subscriptionStore.Save();
        StatusMessage = $"Подписки обновлены: добавлено {totalAdded} новых серверов";
    }

    public async Task PingAllAsync()
    {
        StatusMessage = "Тестирование пинга…";
        foreach (var p in Profiles) p.IsPinging = true;

        var settings = _settingsStore.Current;
        await PingTester.PingAllTcpAsync(
            Profiles.Select(p => p.Profile),
            settings.PingTimeoutMs,
            concurrency: 16);

        foreach (var p in Profiles)
        {
            p.IsPinging = false;
            p.RefreshLatency();
        }
        _profileStore.Save();
        StatusMessage = "Пинг обновлён";
    }

    public void RemoveSubscription(Subscription sub)
    {
        _subscriptionStore.Remove(sub.Id);
        Subscriptions.Remove(sub);
        _profileStore.RemoveBySubscription(sub.Id);
        var toRemove = Profiles.Where(p => p.Profile.SubscriptionId == sub.Id).ToList();
        foreach (var p in toRemove) Profiles.Remove(p);
    }

    public void SortByLatency()
    {
        var sorted = Profiles
            .OrderBy(p => p.LatencyValue < 0 ? int.MaxValue : p.LatencyValue)
            .ToList();
        Profiles.Clear();
        foreach (var p in sorted) Profiles.Add(p);
    }
}
