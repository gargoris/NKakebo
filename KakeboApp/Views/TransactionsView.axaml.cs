// Views/TransactionsView.axaml.cs
using Avalonia.Controls;
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
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Eliminar",
            SecondaryButtonText = "Cancelar"
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}