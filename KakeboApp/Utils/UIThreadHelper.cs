// Utils/UIThreadHelper.cs - Helper mejorado para operaciones de UI thread

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Serilog;

namespace KakeboApp.Utils;

/// <summary>
/// Helper para ejecutar código en el hilo de UI de manera segura
/// </summary>
public static class UIThreadHelper
{
    /// <summary>
    /// Ejecuta una acción en el UI thread de forma segura (asíncrona)
    /// </summary>
    public static void InvokeOnUIThread(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.UIThread.Post(action);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing action on UI thread");
            throw;
        }
    }
    
    /// <summary>
    /// Ejecuta una acción en el UI thread de forma segura de manera síncrona
    /// </summary>
    public static void InvokeOnUIThreadSync(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.UIThread.Invoke(action);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing action synchronously on UI thread");
            throw;
        }
    }
    
    /// <summary>
    /// Ejecuta una tarea asíncrona en el UI thread de forma segura
    /// </summary>
    public static async Task InvokeOnUIThreadAsync(Func<Task> taskFactory)
    {
        if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));
        
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                await taskFactory();
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(taskFactory);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing async task on UI thread");
            throw;
        }
    }
    
    /// <summary>
    /// Ejecuta una función con valor de retorno en el UI thread de forma segura
    /// </summary>
    public static T InvokeOnUIThreadWithResult<T>(Func<T> function)
    {
        if (function == null) throw new ArgumentNullException(nameof(function));
        
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return function();
            }
            else
            {
                return Dispatcher.UIThread.Invoke(function);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing function with result on UI thread");
            throw;
        }
    }
    
    /// <summary>
    /// Ejecuta una función asíncrona con valor de retorno en el UI thread de forma segura
    /// </summary>
    public static async Task<T> InvokeOnUIThreadWithResultAsync<T>(Func<Task<T>> taskFactory)
    {
        if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));
        
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return await taskFactory();
            }
            else
            {
                return await Dispatcher.UIThread.InvokeAsync(taskFactory);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing async function with result on UI thread");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si el código actual se está ejecutando en el hilo de UI
    /// </summary>
    public static bool IsOnUIThread()
    {
        return Dispatcher.UIThread.CheckAccess();
    }
    
    /// <summary>
    /// Asegura que una acción se ejecute en el hilo de UI, lanzando una excepción si no es así
    /// </summary>
    public static void EnsureOnUIThread(Action action, string errorMessage = "Esta operación debe ejecutarse en el hilo de UI")
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            throw new InvalidOperationException(errorMessage);
        }
    }
}
