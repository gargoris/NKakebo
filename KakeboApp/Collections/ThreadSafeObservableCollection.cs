// Collections/ThreadSafeObservableCollection.cs - Colección observable thread-safe mejorada

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using KakeboApp.Utils;

namespace KakeboApp.Collections;

/// <summary>
/// Colección observable thread-safe mejorada que garantiza que todas las operaciones
/// se realizan en el hilo de UI para evitar excepciones de acceso entre hilos
/// </summary>
public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
{
    private readonly object _lock = new object();

    public ThreadSafeObservableCollection()
    {
    }

    public ThreadSafeObservableCollection(IEnumerable<T> collection) : base()
    {
        // No llamamos al constructor base con la colección para evitar problemas de hilo
        // En su lugar, añadimos los elementos de forma segura
        AddRange(collection);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            base.OnCollectionChanged(e);
        }
        else
        {
            // Usamos Invoke en lugar de Post para garantizar que la notificación se procesa
            // antes de continuar, evitando problemas de sincronización
            Dispatcher.UIThread.Invoke(() => base.OnCollectionChanged(e));
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
            Dispatcher.UIThread.Invoke(() => base.OnPropertyChanged(e));
        }
    }

    public new void Add(T item)
    {
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            base.Add(item);
        });
    }

    public new bool Remove(T item)
    {
        bool result = false;
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            result = base.Remove(item);
        });
        return result;
    }

    public new void Clear()
    {
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            base.Clear();
        });
    }

    public new void Insert(int index, T item)
    {
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            base.Insert(index, item);
        });
    }

    public new T this[int index]
    {
        get
        {
            T result = default!;
            UIThreadHelper.InvokeOnUIThreadSync(() => {
                result = base[index];
            });
            return result;
        }
        set
        {
            UIThreadHelper.InvokeOnUIThreadSync(() => {
                base[index] = value;
            });
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        // Hacemos una copia de los elementos para evitar problemas si la colección
        // original cambia durante la operación
        var itemsList = items.ToList();

        UIThreadHelper.InvokeOnUIThreadSync(() => {
            foreach (var item in itemsList)
            {
                base.Add(item);
            }
        });
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        // Hacemos una copia de los elementos para evitar problemas si la colección
        // original cambia durante la operación
        var itemsList = items.ToList();

        UIThreadHelper.InvokeOnUIThreadSync(() => {
            base.Clear();
            foreach (var item in itemsList)
            {
                base.Add(item);
            }
        });
    }

    /// <summary>
    /// Actualiza un elemento existente en la colección
    /// </summary>
    public void Update(T item, Func<T, bool> predicate)
    {
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(this[i]))
                {
                    base[i] = item;
                    break;
                }
            }
        });
    }

    /// <summary>
    /// Añade o actualiza un elemento en la colección
    /// </summary>
    public void AddOrUpdate(T item, Func<T, bool> predicate)
    {
        UIThreadHelper.InvokeOnUIThreadSync(() => {
            bool found = false;
            for (int i = 0; i < Count; i++)
            {
                if (predicate(this[i]))
                {
                    base[i] = item;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                base.Add(item);
            }
        });
    }
}
