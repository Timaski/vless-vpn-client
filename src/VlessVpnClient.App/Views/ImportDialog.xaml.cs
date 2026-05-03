using System.Windows;

namespace VlessVpnClient.App.Views;

public partial class ImportDialog : Window
{
    public ImportDialog()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    public async Task ProcessImportAsync()
    {
        var input = InputBox.Text?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        if (ModeVless.IsChecked == true)
        {
            App.MainVm.ImportFromVlessUrl(input);
        }
        else if (ModeSubText.IsChecked == true)
        {
            App.MainVm.ImportFromSubscriptionContent(input);
        }
        else if (ModeSubUrl.IsChecked == true)
        {
            var name = string.IsNullOrWhiteSpace(SubscriptionName.Text)
                ? new Uri(input).Host
                : SubscriptionName.Text.Trim();
            await App.MainVm.ImportFromSubscriptionUrlAsync(name, input);
        }
    }
}
