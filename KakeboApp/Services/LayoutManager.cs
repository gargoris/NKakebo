// Services/LayoutManager.cs - Gestor de diseño responsivo

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;

namespace KakeboApp.Services;

public interface ILayoutManager
{
    bool IsNarrowLayout { get; }
    bool IsWideLayout { get; }
    bool IsMobileSize { get; }
    double WindowWidth { get; }
    double WindowHeight { get; }
    
    event Action? LayoutChanged;
}

public class LayoutManager : ReactiveObject, ILayoutManager
{
    private double _windowWidth;
    private double _windowHeight;
    private bool _isNarrowLayout;
    private bool _isWideLayout;
    private bool _isMobileSize;

    public LayoutManager()
    {
        // Inicializar con valores por defecto
        UpdateLayout(1200, 800);
    }

    public bool IsNarrowLayout
    {
        get => _isNarrowLayout;
        private set => this.RaiseAndSetIfChanged(ref _isNarrowLayout, value);
    }

    public bool IsWideLayout
    {
        get => _isWideLayout;
        private set => this.RaiseAndSetIfChanged(ref _isWideLayout, value);
    }

    public bool IsMobileSize
    {
        get => _isMobileSize;
        private set => this.RaiseAndSetIfChanged(ref _isMobileSize, value);
    }

    public double WindowWidth
    {
        get => _windowWidth;
        private set => this.RaiseAndSetIfChanged(ref _windowWidth, value);
    }

    public double WindowHeight
    {
        get => _windowHeight;
        private set => this.RaiseAndSetIfChanged(ref _windowHeight, value);
    }

    public event Action? LayoutChanged;

    public void UpdateLayout(double width, double height)
    {
        WindowWidth = width;
        WindowHeight = height;

        // Definir breakpoints
        var wasMobile = IsMobileSize;
        var wasNarrow = IsNarrowLayout;
        var wasWide = IsWideLayout;

        IsMobileSize = width < 768;
        IsNarrowLayout = width < 1024;
        IsWideLayout = width >= 1400;

        // Disparar evento si cambió el layout
        if (wasMobile != IsMobileSize || wasNarrow != IsNarrowLayout || wasWide != IsWideLayout)
        {
            LayoutChanged?.Invoke();
        }
    }

    public Orientation GetOptimalOrientation() => IsNarrowLayout ? Orientation.Vertical : Orientation.Horizontal;
    
    public GridLength GetSidebarWidth() => IsMobileSize ? new GridLength(0) : new GridLength(200);
    
    public int GetOptimalColumnCount()
    {
        if (IsMobileSize) return 1;
        if (IsNarrowLayout) return 2;
        if (IsWideLayout) return 4;
        return 3;
    }
}
