using ReactiveUI;
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
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    
    /// <summary>
    /// Indica si el ViewModel está ocupado procesando una operación
    /// </summary>
    public bool IsBusy 
    { 
        get => _isBusy; 
        set => this.RaiseAndSetIfChanged(ref _isBusy, value); 
    }
    
    /// <summary>
    /// Mensaje de error actual
    /// </summary>
    public string ErrorMessage 
    { 
        get => _errorMessage; 
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value); 
    }
    
    /// <summary>
    /// Método para manejar excepciones de manera consistente
    /// </summary>
    protected void HandleException(Exception ex, string operation)
    {
        if (ex == null) return;
        
        Log.Error(ex, "Error in {Operation}: {Message}", operation, ex.Message);
        
        UIThreadHelper.InvokeOnUIThread(() => {
            ErrorMessage = $"Error: {ex.Message}";
        });
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
        UIThreadHelper.InvokeOnUIThread(() => {
            ErrorMessage = string.Empty;
            IsBusy = true;
        });
        
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
            UIThreadHelper.InvokeOnUIThread(() => IsBusy = false);
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
        UIThreadHelper.InvokeOnUIThread(() => {
            ErrorMessage = string.Empty;
            IsBusy = true;
        });
        
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
            UIThreadHelper.InvokeOnUIThread(() => IsBusy = false);
        }
    }
}

