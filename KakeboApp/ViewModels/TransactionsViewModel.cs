using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;
using Unit = System.Reactive.Unit;
using Serilog;

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
        _transactionService = transactionService;

        Transactions = new ObservableCollection<Transaction>();
        FilteredTransactions = new ObservableCollection<Transaction>();

        // Comandos
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadTransactions);
        AddTransactionCommand = ReactiveCommand.Create(AddTransaction);
        EditTransactionCommand = ReactiveCommand.Create<Transaction>(EditTransaction);
        DeleteTransactionCommand = ReactiveCommand.CreateFromTask<Transaction>(DeleteTransaction);
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
        // _ = Task.Run(LoadData);
    }

    public ObservableCollection<Transaction> Transactions { get; }
    public ObservableCollection<Transaction> FilteredTransactions { get; }
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
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTransactionCommand { get; }
    public ReactiveCommand<Transaction, Unit> EditTransactionCommand { get; }
    public ReactiveCommand<Transaction, Unit> DeleteTransactionCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseEditPanelCommand { get; }

    private async Task LoadTransactions()
    {
        IsBusy = true;
        try
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            Transactions.Clear();
            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading transactions");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddTransaction()
    {
        AddEditViewModel.StartAdd();
        IsEditPanelVisible = true;
    }

    private void EditTransaction(Transaction transaction)
    {
        AddEditViewModel.StartEdit(transaction);
        IsEditPanelVisible = true;
        SelectedTransaction = transaction;
    }

    private async Task DeleteTransaction(Transaction transaction)
    {
        try
        {
            if (transaction.Id.HasValue)
            {
                await _transactionService.DeleteTransactionAsync(transaction.Id.Value);
                Transactions.Remove(transaction);
                ApplyFilters();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction");
        }
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
        FilteredTransactions.Clear();

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

        foreach (var transaction in filtered.OrderByDescending(t => t.Date))
        {
            FilteredTransactions.Add(transaction);
        }
    }
}

