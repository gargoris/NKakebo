using Avalonia.Controls;
using KakeboApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using System;
using Serilog;
using Avalonia.Threading;

namespace KakeboApp.Views;

public class MainView : UserControl
{
    public MainView()
    {
        try
        {
            InitializeView();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing MainView");
            ShowErrorView(ex);
        }
    }

    private void InitializeView()
    {
        // Verificar que estamos en el UI thread
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(InitializeView);
            return;
        }

        // Obtener el host desde la aplicación para resolver dependencias correctamente
        if (Application.Current is App app && app.Host != null)
        {
            try
            {
                var mainWindowViewModel = app.Host.Services.GetRequiredService<MainWindowViewModel>();
                Content = new MainWindow(mainWindowViewModel);
                Log.Information("MainView initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting MainWindowViewModel from DI container");
                ShowErrorView(ex);
            }
        }
        else
        {
            var errorMessage = "Error: Application host not available";
            Log.Error(errorMessage);
            ShowErrorView(new InvalidOperationException(errorMessage));
        }
    }

    private void ShowErrorView(Exception ex)
    {
        Content = new StackPanel
        {
            Children =
            {
                new TextBlock 
                { 
                    Text = "Error al inicializar la aplicación",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = Avalonia.Media.Brushes.Red,
                    Margin = new Avalonia.Thickness(20)
                },
                new TextBlock 
                { 
                    Text = $"Detalles: {ex.Message}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(20, 10),
                    MaxWidth = 600
                },
                new Button
                {
                    Content = "Reintentar",
                    Margin = new Avalonia.Thickness(20),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Command = ReactiveUI.ReactiveCommand.Create(() => InitializeView())
                }
            }
        };
    }
}