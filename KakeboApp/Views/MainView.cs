using Avalonia.Controls;
using KakeboApp.ViewModels;

namespace KakeboApp.Views;

public class MainView : UserControl
{
    public MainView()
    {
        Content = new MainWindow(new MainWindowViewModel(
            // Inyección manual para Android - mejorar con DI container
            null!, null!, null!, null!, null!));
    }
}