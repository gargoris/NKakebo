using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KakeboApp.Views;

public partial class AddEditTransactionView : UserControl
{
    public AddEditTransactionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}