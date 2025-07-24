using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Utils;
using Serilog;
using Avalonia.Threading;

namespace KakeboApp.ViewModels;

/// <summary>
/// Base ViewModel con soporte para notificación de cambios y manejo de errores
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// Override para asegurar que todas las notificaciones de propiedades se ejecuten en el UI thread
    /// </summary>
    protected override void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            base.RaisePropertyChanged(propertyName);
        }
        else
        {
            Dispatcher.UIThread.Post(() => base.RaisePropertyChanged(propertyName));
        }
    }
    /// <summary>
    /// Indica si el ViewModel está ocupado procesando una operación
    /// </summary>
    [Reactive] public bool IsBusy { get; set; }
    
    /// <summary>
    /// Mensaje de error actual
    /// </summary>
    [Reactive] public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Método para manejar excepciones de manera consistente
    /// </summary>
    protected void HandleException(Exception ex, string operation)
    {
        if (ex == null) return;
        
        Log.Error(ex, "Error in {Operation}: {Message}", operation, ex.Message);
        
        ErrorMessage = $"Error: {ex.Message}";
    }
    
    /// <summary>
    /// Método para ejecutar operaciones asíncronas de manera segura
    /// </summary>
    protected async Task ExecuteSafelyAsync(Func<Task> operation, string operationName)
    {
        if (operation == null)
        {
            Log.Warning("Attempted to execute null operation: {OperationName}", operationName);
            return;
        }
        
        // Limpiar error anterior y establecer estado ocupado
        ErrorMessage = string.Empty;
        IsBusy = true;
        
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            HandleException(ex, operationName);
        }
        finally
        {
            // Restablecer estado ocupado
            IsBusy = false;
        }
    }
    
    /// <summary>
    /// Método para ejecutar operaciones asíncronas de manera segura con resultado
    /// </summary>
    protected async Task<T?> ExecuteSafelyWithResultAsync<T>(Func<Task<T>> operation, string operationName)
    {
        if (operation == null)
        {
            Log.Warning("Attempted to execute null operation: {OperationName}", operationName);
            return default;
        }
        
        // Limpiar error anterior y establecer estado ocupado
        ErrorMessage = string.Empty;
        IsBusy = true;
        
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            HandleException(ex, operationName);
            return default;
        }
        finally
        {
            // Restablecer estado ocupado
            IsBusy = false;
        }
    }
}

