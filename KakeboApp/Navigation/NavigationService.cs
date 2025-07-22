// Navigation/INavigationService.cs - Servicio de navegación avanzado

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KakeboApp.ViewModels;

namespace KakeboApp.Navigation;

public interface INavigationService
{
    ViewModelBase? CurrentPage { get; }
    bool CanGoBack { get; }
    
    event Action<ViewModelBase>? NavigatedTo;
    event Action<ViewModelBase>? NavigatingFrom;
    
    Task NavigateToAsync<T>() where T : ViewModelBase;
    Task NavigateToAsync(ViewModelBase viewModel);
    Task NavigateToAsync(Type viewModelType, object? parameter = null);
    
    Task<bool> GoBackAsync();
    void ClearHistory();
    
    // Para diálogos modales
    Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase dialogViewModel);
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private ViewModelBase? _currentPage;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage != value)
            {
                var oldPage = _currentPage;
                _currentPage = value;
                
                if (oldPage != null)
                    NavigatingFrom?.Invoke(oldPage);
                
                if (_currentPage != null)
                    NavigatedTo?.Invoke(_currentPage);
            }
        }
    }

    public bool CanGoBack => _navigationStack.Count > 1;

    public event Action<ViewModelBase>? NavigatedTo;
    public event Action<ViewModelBase>? NavigatingFrom;

    public async Task NavigateToAsync<T>() where T : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<T>();
        await NavigateToAsync(viewModel);
    }

    public async Task NavigateToAsync(ViewModelBase viewModel)
    {
        if (CurrentPage != null)
        {
            _navigationStack.Push(CurrentPage);
        }
        
        CurrentPage = viewModel;
        
        // Si el ViewModel implementa IAsyncInitialize, inicializarlo
        if (viewModel is IAsyncInitialize asyncInit)
        {
            await asyncInit.InitializeAsync();
        }
    }

    public async Task NavigateToAsync(Type viewModelType, object? parameter = null)
    {
        var viewModel = (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType);
        
        // Si el ViewModel acepta parámetros
        if (parameter != null && viewModel is IParameterReceiver paramReceiver)
        {
            paramReceiver.ReceiveParameter(parameter);
        }
        
        await NavigateToAsync(viewModel);
    }

    public async Task<bool> GoBackAsync()
    {
        if (!CanGoBack) return false;

        CurrentPage = _navigationStack.Pop();
        
        // Si el ViewModel implementa IAsyncInitialize, reinicializarlo
        if (CurrentPage is IAsyncInitialize asyncInit)
        {
            await asyncInit.InitializeAsync();
        }
        
        return true;
    }

    public void ClearHistory()
    {
        _navigationStack.Clear();
    }

    public async Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase dialogViewModel)
    {
        // Esta implementación dependería de cómo manejes los diálogos
        // Por ahora, es un placeholder
        await Task.CompletedTask;
        return default(TResult);
    }
}

// Interfaces auxiliares
public interface IAsyncInitialize
{
    Task InitializeAsync();
}

public interface IParameterReceiver
{
    void ReceiveParameter(object parameter);
}
