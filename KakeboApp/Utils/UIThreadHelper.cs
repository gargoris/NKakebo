// Utils/UIThreadHelper.cs - Helper para operaciones de UI thread

using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace KakeboApp.Utils;

public static class UIThreadHelper
{
    /// <summary>
    /// Ejecuta una acción en el UI thread de forma segura
    /// </summary>
    public static void InvokeOnUIThread(Action action)
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
    
    /// <summary>
    /// Ejecuta una acción en el UI thread de forma segura de manera síncrona
    /// </summary>
    public static void InvokeOnUIThreadSync(Action action)
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
    
    /// <summary>
    /// Ejecuta una tarea asíncrona en el UI thread de forma segura
    /// </summary>
    public static async Task InvokeOnUIThreadAsync(Func<Task> taskFactory)
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
}
