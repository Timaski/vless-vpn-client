using VlessVpnClient.App.Helpers;

namespace VlessVpnClient.App.ViewModels;

public enum ImportSourceType
{
    VlessUrl,
    SubscriptionUrl,
    SubscriptionContent
}

public sealed class ImportViewModel : ObservableObject
{
    private string _input = string.Empty;
    public string Input
    {
        get => _input;
        set => SetField(ref _input, value);
    }

    private ImportSourceType _sourceType = ImportSourceType.VlessUrl;
    public ImportSourceType SourceType
    {
        get => _sourceType;
        set => SetField(ref _sourceType, value);
    }

    private string _subscriptionName = string.Empty;
    public string SubscriptionName
    {
        get => _subscriptionName;
        set => SetField(ref _subscriptionName, value);
    }

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }
}
