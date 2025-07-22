using System;
using System.Threading;
using System.Threading.Tasks;

namespace KakeboApp.Core.Utils;

/// <summary>
/// Helper para manejo de threading que no depende de Avalonia
/// </summary>
public static class ThreadingHelper
{
    /// <summary>
    /// Acción a ejecutar para despachar al UI thread
    /// Será configurada por la aplicación Avalonia
    /// </summary>
    public static Action<Action>? UIThreadDispatcher { get; set; }

    /// <summary>
    /// Ejecuta una acción en el UI thread si el dispatcher está configurado,
    /// de lo contrario ejecuta directamente
    /// </summary>
    public static void InvokeOnUIThread(Action action)
    {
        if (UIThreadDispatcher != null)
        {
            UIThreadDispatcher(action);
        }
        else
        {
            // Si no hay dispatcher configurado, ejecutar directamente
            // TODO: Log para debugging
            Console.WriteLine("WARNING: UIThreadDispatcher is null, executing directly");
            action();
        }
    }

    /// <summary>
    /// Ejecuta una acción de manera asíncrona
    /// </summary>
    public static Task InvokeAsync(Action action)
    {
        return Task.Run(action);
    }
}
