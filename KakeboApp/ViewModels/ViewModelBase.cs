using ReactiveUI;
using System.Reactive;
using System.Reactive.Subjects;
using System.ComponentModel;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;

namespace KakeboApp.ViewModels;

// Base ViewModel con INotifyPropertyChanged
public abstract class ViewModelBase : ReactiveObject
{
    private bool _isBusy;
    
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
}

