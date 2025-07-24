using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

    public TransactionsViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        // Usar ThreadSafeObservableCollection en lugar de ObservableCollection
        Transactions = new ThreadSafeObservableCollection<Transaction>();
        FilteredTransactions = new ThreadSafeObservableCollection<Transaction>();

        // Comandos con manejo de errores
        RefreshDataCommand = new AsyncCommand(LoadTransactions, null, ex => HandleException(ex, "Error al cargar transacciones"));
        AddTransactionCommand = ReactiveCommand.Create(AddTransaction, outputScheduler: RxApp.MainThreadScheduler);
        EditTransactionCommand = ReactiveCommand.Create<Transaction>(EditTransaction, outputScheduler: RxApp.MainThreadScheduler);
        DeleteTransactionCommand = new AsyncCommand<Transaction>(DeleteTransaction, null, ex => HandleException(ex, "Error al eliminar transacción"));
        CloseEditPanelCommand = ReactiveCommand.Create(CloseEditPanel, outputScheduler: RxApp.MainThreadScheduler);

        // ViewModel de edición
        AddEditViewModel = new AddEditTransactionViewModel(_transactionService);
        AddEditViewModel.TransactionSaved
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => OnTransactionSaved());
        AddEditViewModel.Cancelled
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => CloseEditPanel());

        // Filtros reactivos
        this.WhenAnyValue(x => x.SearchText, x => x.FilterCategory, x => x.FilterType)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFilters());

        // Cargar datos iniciales de forma asíncrona
        _ = Initialize();
    }

    // Propiedades con notificación de cambios
    public ThreadSafeObservableCollection<Transaction> Transactions { get; }
    public ThreadSafeObservableCollection<Transaction> FilteredTransactions { get; }
    public AddEditTransactionViewModel AddEditViewModel { get; }

    [Reactive] public Transaction? SelectedTransaction { get; set; }
    [Reactive] public bool IsEditPanelVisible { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public Category? FilterCategory { get; set; }
    [Reactive] public TransactionType? FilterType { get; set; }

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
            AddEditViewModel.StartAdd();
            IsEditPanelVisible = true;
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
            AddEditViewModel.StartEdit(transaction);
            IsEditPanelVisible = true;
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
        IsEditPanelVisible = false;
        SelectedTransaction = null;
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

