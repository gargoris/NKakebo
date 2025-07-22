// Collections/ThreadSafeObservableCollection.cs - Colección observable thread-safe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;

namespace KakeboApp.Collections;

public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
{
    private readonly object _lock = new object();
    private readonly SynchronizationContext? _synchronizationContext;

    public ThreadSafeObservableCollection()
    {
        _synchronizationContext = SynchronizationContext.Current;
    }

    public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection)
    {
        _synchronizationContext = SynchronizationContext.Current;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            base.OnCollectionChanged(e);
        }
        else
        {
            Dispatcher.UIThread.Post(() => base.OnCollectionChanged(e));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            base.OnPropertyChanged(e);
        }
        else
        {
            Dispatcher.UIThread.Post(() => base.OnPropertyChanged(e));
        }
    }

    public new void Add(T item)
    {
        lock (_lock)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                base.Add(item);
            }
            else
            {
                Dispatcher.UIThread.Post(() => base.Add(item));
            }
        }
    }

    public new void Remove(T item)
    {
        lock (_lock)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                base.Remove(item);
            }
            else
            {
                Dispatcher.UIThread.Post(() => base.Remove(item));
            }
        }
    }

    public new void Clear()
    {
        lock (_lock)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                base.Clear();
            }
            else
            {
                Dispatcher.UIThread.Post(() => base.Clear());
            }
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                foreach (var item in items)
                {
                    base.Add(item);
                }
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var item in items)
                    {
                        base.Add(item);
                    }
                });
            }
        }
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        lock (_lock)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                base.Clear();
                foreach (var item in items)
                {
                    base.Add(item);
                }
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    base.Clear();
                    foreach (var item in items)
                    {
                        base.Add(item);
                    }
                });
            }
        }
    }
}
