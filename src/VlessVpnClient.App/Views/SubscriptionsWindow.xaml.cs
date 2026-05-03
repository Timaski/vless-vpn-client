using System.Windows;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.App.Views;

public partial class SubscriptionsWindow : Window
{
    public SubscriptionsWindow()
    {
        InitializeComponent();
        SubsGrid.ItemsSource = App.MainVm.Subscriptions;
    }

    private void Remove_Click(object? sender, RoutedEventArgs e)
    {
        if (SubsGrid.SelectedItem is not Subscription sub) return;
        var result = MessageBox.Show(
            $"Удалить подписку '{sub.Name}'? Это также удалит все сервера, добавленные через эту подписку.",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            App.MainVm.RemoveSubscription(sub);
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
