// Views/DatabaseConnectionView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KakeboApp.ViewModels;

namespace KakeboApp.Views;

public partial class DatabaseConnectionView : UserControl
{
    public DatabaseConnectionView()
    {
        InitializeComponent();
    }

    private async void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DatabaseConnectionViewModel viewModel) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleccionar Base de Datos",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Base de Datos")
                {
                    Patterns = new[] { "*.db", "*.sqlite", "*.sqlite3" }
                },
                new FilePickerFileType("Todos los Archivos")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        });

        if (files.Count > 0)
        {
            viewModel.DatabasePath = files[0].Path.LocalPath;
        }
    }

    private async void CreateNew_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DatabaseConnectionViewModel viewModel) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Crear Nueva Base de Datos",
            DefaultExtension = "db",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Base de Datos")
                {
                    Patterns = new[] { "*.db" }
                }
            }
        });

        if (file != null)
        {
            viewModel.DatabasePath = file.Path.LocalPath;
        }
    }
}