using VlessVpnClient.App.Helpers;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.App.ViewModels;

public sealed class ServerProfileViewModel : ObservableObject
{
    public ServerProfile Profile { get; }

    public ServerProfileViewModel(ServerProfile profile)
    {
        Profile = profile;
    }

    public string ProfileId => Profile.ProfileId;
    public string DisplayName => Profile.DisplayName;
    public string Address => $"{Profile.Config.Address}:{Profile.Config.Port}";
    public string Network => Profile.Config.Network;
    public string Security => Profile.Config.Security;

    public int LatencyValue => Profile.LatencyMs ?? -1;

    public string LatencyText
    {
        get
        {
            if (Profile.LatencyMs.HasValue)
            {
                return $"{Profile.LatencyMs.Value} ms";
            }
            if (!string.IsNullOrEmpty(Profile.LatencyError))
            {
                return "—";
            }
            return "—";
        }
    }

    public string Tooltip
    {
        get
        {
            var lines = new List<string>
            {
                $"Address: {Profile.Config.Address}:{Profile.Config.Port}",
                $"Network: {Profile.Config.Network}",
                $"Security: {Profile.Config.Security}"
            };
            if (!string.IsNullOrEmpty(Profile.Config.Sni)) lines.Add($"SNI: {Profile.Config.Sni}");
            if (!string.IsNullOrEmpty(Profile.Config.Flow)) lines.Add($"Flow: {Profile.Config.Flow}");
            if (!string.IsNullOrEmpty(Profile.Config.Fingerprint)) lines.Add($"Fingerprint: {Profile.Config.Fingerprint}");
            if (!string.IsNullOrEmpty(Profile.LatencyError)) lines.Add($"Last error: {Profile.LatencyError}");
            return string.Join('\n', lines);
        }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetField(ref _isActive, value);
    }

    private bool _isPinging;
    public bool IsPinging
    {
        get => _isPinging;
        set => SetField(ref _isPinging, value);
    }

    public void RefreshLatency()
    {
        RaisePropertyChanged(nameof(LatencyValue));
        RaisePropertyChanged(nameof(LatencyText));
        RaisePropertyChanged(nameof(Tooltip));
    }

    public void RefreshDisplay()
    {
        RaisePropertyChanged(nameof(DisplayName));
        RaisePropertyChanged(nameof(Address));
        RaisePropertyChanged(nameof(Network));
        RaisePropertyChanged(nameof(Security));
        RaisePropertyChanged(nameof(Tooltip));
        RefreshLatency();
    }
}
