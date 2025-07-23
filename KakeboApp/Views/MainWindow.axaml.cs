using Avalonia.Controls;
using KakeboApp.ViewModels;

namespace KakeboApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public MainWindow()
    {
        InitializeComponent();
    }
}