using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KakeboApp.Collections;
using KakeboApp.Commands;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;
using KakeboApp.Utils;
using Unit = System.Reactive.Unit;
using Serilog;
using Avalonia.Threading;

namespace KakeboApp.ViewModels;

// ViewModel para la lista de transacciones
public class TransactionsViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private Transaction? _selectedTransaction;
    private bool _isEditPanelVisible;
    private string _searchText = string.Empty;
    private Category? _filterCategory;
    private TransactionType? _filterType;

    public TransactionsViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        // Usar ThreadSafeObservableCollection en lugar de ObservableCollection
        Transactions = new ThreadSafeObservableCollection<Transaction>();
        FilteredTransactions = new ThreadSafeObservableCollection<Transaction>();

        // Comandos con manejo de errores
        RefreshDataCommand = new AsyncCommand(LoadTransactions, null, ex => HandleException(ex, "Error al cargar transacciones"));
        AddTransactionCommand = ReactiveCommand.Create(AddTransaction);
        EditTransactionCommand = ReactiveCommand.Create<Transaction>(EditTransaction);
        DeleteTransactionCommand = new AsyncCommand<Transaction>(DeleteTransaction, null, ex => HandleException(ex, "Error al eliminar transacción"));
        CloseEditPanelCommand = ReactiveCommand.Create(CloseEditPanel);

        // ViewModel de edición
        AddEditViewModel = new AddEditTransactionViewModel(_transactionService);
        AddEditViewModel.TransactionSaved.Subscribe(_ => OnTransactionSaved());
        AddEditViewModel.Cancelled.Subscribe(_ => CloseEditPanel());

        // Filtros reactivos
        this.WhenAnyValue(x => x.SearchText, x => x.FilterCategory, x => x.FilterType)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(_ => ApplyFilters());

        // Cargar datos iniciales de forma asíncrona
        _ = Initialize();
    }

    // Propiedades con notificación de cambios
    public ThreadSafeObservableCollection<Transaction> Transactions { get; }
    public ThreadSafeObservableCollection<Transaction> FilteredTransactions { get; }
    public AddEditTransactionViewModel AddEditViewModel { get; }

    public Transaction? SelectedTransaction
    {
        get => _selectedTransaction;
        set => this.RaiseAndSetIfChanged(ref _selectedTransaction, value);
    }

    public bool IsEditPanelVisible
    {
        get => _isEditPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isEditPanelVisible, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public Category? FilterCategory
    {
        get => _filterCategory;
        set => this.RaiseAndSetIfChanged(ref _filterCategory, value);
    }

    public TransactionType? FilterType
    {
        get => _filterType;
        set => this.RaiseAndSetIfChanged(ref _filterType, value);
    }

    // Propiedades para UI
    public IEnumerable<Category> AllCategories => Enum.GetValues<Category>();
    public IEnumerable<TransactionType> AllTypes => Enum.GetValues<TransactionType>();

    // Comandos
    public ICommand RefreshDataCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTransactionCommand { get; }
    public ReactiveCommand<Transaction, Unit> EditTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseEditPanelCommand { get; }

    // Método de inicialización
    public async Task Initialize()
    {
        await LoadTransactions();
    }

    private async Task LoadTransactions()
    {
        // Usar el método ExecuteSafelyAsync del ViewModelBase
        await ExecuteSafelyAsync(async () =>
        {
            // Ejecutar la consulta en background thread
            var transactions = await Task.Run(async () => 
                await _transactionService.GetAllTransactionsAsync());

            // Actualizar colecciones de forma segura
            Transactions.ReplaceAll(transactions);
            ApplyFilters();
        }, "LoadTransactions");
    }

    private void AddTransaction()
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                AddEditViewModel.StartAdd();
                IsEditPanelVisible = true;
            }
            else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    AddEditViewModel.StartAdd();
                    IsEditPanelVisible = true;
                });
            }
        }
        catch (Exception ex)
        {
            HandleException(ex, "AddTransaction");
        }
    }

    private void EditTransaction(Transaction transaction)
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                AddEditViewModel.StartEdit(transaction);
                IsEditPanelVisible = true;
            }
            else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    AddEditViewModel.StartEdit(transaction);
                    IsEditPanelVisible = true;
                });
            }
        }
        catch (Exception ex)
        {
            HandleException(ex, "EditTransaction");
        }
    }

    private async Task DeleteTransaction(Transaction? transaction)
    {
        if (transaction?.Id == null) return;

        await ExecuteSafelyAsync(async () =>
        {
            await _transactionService.DeleteTransactionAsync(transaction.Id.Value);
            await LoadTransactions();
        }, "DeleteTransaction");
    }

    private void CloseEditPanel()
    {
        UIThreadHelper.InvokeOnUIThread(() =>
        {
            IsEditPanelVisible = false;
            SelectedTransaction = null;
        });
    }

    private async void OnTransactionSaved()
    {
        await LoadTransactions();
        CloseEditPanel();
    }

    private void ApplyFilters()
    {
        UIThreadHelper.InvokeOnUIThread(() =>
        {
            try
            {
                // Crear una lista temporal para evitar múltiples notificaciones
                var filteredList = new List<Transaction>();
                var filtered = Transactions.AsEnumerable();

                // Filtro de texto
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(t =>
                        t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (t.Subcategory?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Notes?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // Filtro de categoría
                if (FilterCategory.HasValue)
                {
                    filtered = filtered.Where(t => t.Category == FilterCategory.Value);
                }

                // Filtro de tipo
                if (FilterType.HasValue)
                {
                    filtered = filtered.Where(t => t.Type == FilterType.Value);
                }

                // Ordenar y añadir a la lista temporal
                filteredList.AddRange(filtered.OrderByDescending(t => t.Date));

                // Actualizar la colección filtrada de una sola vez
                FilteredTransactions.ReplaceAll(filteredList);
            }
            catch (Exception ex)
            {
                HandleException(ex, "ApplyFilters");
            }
        });
    }
}

