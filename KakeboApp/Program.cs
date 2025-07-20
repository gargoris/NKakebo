using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Services;
using KakeboApp.Core.Data;
using KakeboApp.Services;
using KakeboApp.ViewModels;
using KakeboApp.Views;

namespace KakeboApp;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .WithInterFont();
}

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        _host = CreateHostBuilder().Build();
        _host.Start();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = _host!.Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = _host!.Services.GetRequiredService<MainView>();
            singleViewPlatform.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Platform services
                services.AddSingleton<IPlatformService>(sp =>
                {
#if ANDROID
                    return new AndroidPlatformService();
#else
                    return new DesktopPlatformService();
#endif
                });

                // Core services
                services.AddSingleton<IKakeboDatabase, LiteDbKakeboDatabase>();
                services.AddScoped<IDatabaseService, DatabaseService>();
                services.AddScoped<ITransactionService, TransactionService>();
                services.AddScoped<IBudgetService, BudgetService>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<DatabaseConnectionViewModel>();
                services.AddTransient<TransactionsViewModel>();
                services.AddTransient<AddEditTransactionViewModel>();
                services.AddTransient<BudgetViewModel>();
                services.AddTransient<ReportsViewModel>();

                // Views
                services.AddTransient<MainWindow>();
                services.AddTransient<MainView>();
            });
}
