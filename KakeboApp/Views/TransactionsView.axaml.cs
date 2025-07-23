// Views/TransactionsView.axaml.cs

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using KakeboApp.ViewModels;
using KakeboApp.Core.Models;

namespace KakeboApp.Views;

public partial class TransactionsView : UserControl
{
    public TransactionsView()
    {
        InitializeComponent();
    }

    private void DataGrid_DoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is DataGrid grid &&
            grid.SelectedItem is Transaction transaction &&
            DataContext is TransactionsViewModel viewModel)
        {
            viewModel.EditTransactionCommand.Execute(transaction);
        }
    }

    private async void DeleteTransaction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is Transaction transaction &&
            DataContext is TransactionsViewModel viewModel)
        {
            var result = await ShowConfirmationDialog(
                "Confirmar eliminación",
                $"¿Está seguro de que desea eliminar la transacción '{transaction.Description}'?");

            if (result)
            {
                viewModel.DeleteTransactionCommand.Execute(transaction);
            }
        }
    }

    private async Task<bool> ShowConfirmationDialog(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var deleteButton = new Button
        {
            Content = "Eliminar",
            Classes = { "accent" }
        };

        var cancelButton = new Button
        {
            Content = "Cancelar"
        };

        bool result = false;

        deleteButton.Click += (s, e) =>
        {
            result = true;
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            result = false;
            dialog.Close();
        };

        buttonPanel.Children.Add(deleteButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;

        var mainWindow = TopLevel.GetTopLevel(this) as Window;
        if (mainWindow != null)
        {
            await dialog.ShowDialog(mainWindow);
        }

        return result;
    }
}
