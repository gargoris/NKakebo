using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using KakeboApp.Utils;
using Serilog;

namespace KakeboApp.Commands;

/// <summary>
/// Comando asíncrono mejorado con manejo de excepciones y sincronización de hilos
/// </summary>
public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;
    private readonly Action<Exception>? _onError;

    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onError = onError;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            // Aseguramos que la operación larga no bloquee la UI
            await Task.Run(async () => {
                try
                {
                    await _execute();
                }
                catch (Exception ex)
                {
                    // Loguear la excepción
                    Log.Error(ex, "Error executing command");
                    
                    // Notificar la excepción si hay un handler
                    if (_onError != null)
                    {
                        UIThreadHelper.InvokeOnUIThread(() => _onError(ex));
                    }
                }
            });
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        UIThreadHelper.InvokeOnUIThread(() => {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        });
    }
}

/// <summary>
/// Comando asíncrono genérico mejorado con manejo de excepciones y sincronización de hilos
/// </summary>
public class AsyncCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private bool _isExecuting;
    private readonly Action<Exception>? _onError;

    public AsyncCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null, Action<Exception>? onError = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onError = onError;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            // Aseguramos que la operación larga no bloquee la UI
            await Task.Run(async () => {
                try
                {
                    await _execute((T?)parameter);
                }
                catch (Exception ex)
                {
                    // Loguear la excepción
                    Log.Error(ex, "Error executing command with parameter");
                    
                    // Notificar la excepción si hay un handler
                    if (_onError != null)
                    {
                        UIThreadHelper.InvokeOnUIThread(() => _onError(ex));
                    }
                }
            });
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        UIThreadHelper.InvokeOnUIThread(() => {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        });
    }
}
