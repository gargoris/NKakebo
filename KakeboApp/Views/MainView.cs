using Avalonia.Controls;
using KakeboApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;

namespace KakeboApp.Views;

public class MainView : UserControl
{
    public MainView()
    {
        // Obtener el host desde la aplicación para resolver dependencias correctamente
        if (Application.Current is App app && app.Host != null)
        {
            var mainWindowViewModel = app.Host.Services.GetRequiredService<MainWindowViewModel>();
            Content = new MainWindow(mainWindowViewModel);
        }
        else
        {
            // Fallback para desarrollo - crear una vista básica
            Content = new TextBlock { Text = "Error: No se pudo inicializar la aplicación" };
        }
    }
}