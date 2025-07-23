using System;
using System.Globalization;
using System.Threading;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Services;
using KakeboApp.Core.Data;
using KakeboApp.Core.Utils;
using KakeboApp.Services;
using KakeboApp.ViewModels;
using KakeboApp.Views;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ReactiveUI;

namespace KakeboApp;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Configurar cultura española para mostrar euros
        var culture = new CultureInfo("es-ES");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        try
        {
            Log.Debug("Iniciando aplicación KakeboApp");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error iniciando aplicación");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect() // Detección automática de plataforma
            .WithInterFont()
            .LogToTrace()
            .UseSkia(); // Renderer multiplataforma
}

public partial class App : Application
{
    private IHost? _host;
    
    public IHost? Host => _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Configurar el dispatcher de threading para el Core con manejo de errores
        ThreadingHelper.UIThreadDispatcher = action =>
        {
            try
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    action();
                }
                else
                {
                    // Usar Invoke en lugar de Post para garantizar que la acción se ejecuta
                    // antes de continuar, evitando problemas de sincronización
                    Dispatcher.UIThread.Invoke(action);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing action on UI thread");
                throw;
            }
        };

        _host = CreateHostBuilder().Build();
        _host.Start();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = _host!.Services.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow(mainWindowViewModel);
            
            desktop.MainWindow = mainWindow;
            
            // Manejar cierre de aplicación
            desktop.ShutdownRequested += OnShutdownRequested;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Para plataformas móviles (futuro soporte)
            var mainView = new MainView();
            singleViewPlatform.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        _host?.StopAsync();
        _host?.Dispose();
    }

    private static IHostBuilder CreateHostBuilder() =>
        Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseSerilog(Log.Logger, dispose: true) // Integrar Serilog como logger global
            .ConfigureServices((context, services) =>
            {
                // Configurar servicio de plataforma según OS
                services.AddSingleton<IPlatformService, CrossPlatformService>();

                // Nuevos servicios de mejores prácticas
                services.AddSingleton<Navigation.INavigationService, Navigation.NavigationService>();
                services.AddSingleton<Services.ILayoutManager, Services.LayoutManager>();

                // Core services - completamente multiplataforma
                services.AddSingleton<IKakeboDatabase, LiteDbKakeboDatabase>();
                services.AddScoped<IDatabaseService, DatabaseService>();
                services.AddScoped<ITransactionService, TransactionService>();
                services.AddScoped<IBudgetService, BudgetService>();

                // ViewModels - sin dependencias de plataforma
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<DatabaseConnectionViewModel>();
                services.AddTransient<TransactionsViewModel>();
                services.AddTransient<AddEditTransactionViewModel>();
                services.AddTransient<BudgetViewModel>();
                services.AddTransient<ReportsViewModel>();
            });
}
