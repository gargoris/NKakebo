// ViewModel para agregar/editar transacciones

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;
using ReactiveUI;


namespace KakeboApp.ViewModels;

public class AddEditTransactionViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private Transaction? _originalTransaction;

    private readonly Subject<System.Reactive.Unit> _transactionSaved = new();
    private readonly Subject<System.Reactive.Unit> _cancelled = new();

    public AddEditTransactionViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;

        SuggestedSubcategories = new ObservableCollection<string>();

        // Comandos con validación
        var canSave = this.WhenAnyValue(
            x => x.Description,
            x => x.Amount,
            x => x.IsBusy,
            (desc, amount, busy) => !string.IsNullOrWhiteSpace(desc) && amount > 0 && !busy);

        SaveCommand = ReactiveCommand.CreateFromTask(SaveTransaction, canSave);
        CancelCommand = ReactiveCommand.Create(Cancel);

        // Comandos de subcategoría
        SelectSubcategoryCommand = ReactiveCommand.Create<string>(SelectSubcategory);
        ClearSubcategoryCommand = ReactiveCommand.Create(ClearSubcategory);

        // Actualizar sugerencias cuando cambia la categoría
        this.WhenAnyValue(x => x.Category)
            .Subscribe(UpdateSuggestedSubcategories);

        // Actualizar categorías válidas cuando cambia el tipo
        this.WhenAnyValue(x => x.Type)
            .Subscribe(OnTypeChanged);

        // Inicializar sugerencias
        UpdateSuggestedSubcategories(Category.Food);
    }

    public ObservableCollection<string> SuggestedSubcategories { get; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    public TransactionType Type { get; set; } = TransactionType.Expense;

    public Category Category { get; set; } = Category.Food;

    public string? Subcategory { get; set; }

    public string? Notes { get; set; }

    public bool IsEditing { get; set; }

    // Categorías válidas según el tipo
    public IEnumerable<Category> ValidCategories => Type switch
    {
        TransactionType.Income => CategoryUtils.GetIncomeCategories(),
        TransactionType.Expense => CategoryUtils.GetExpenseCategories(),
        _ => Enum.GetValues<Category>()
    };

    // Comandos
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SaveCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CancelCommand { get; }
    public ReactiveCommand<string, System.Reactive.Unit> SelectSubcategoryCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ClearSubcategoryCommand { get; }

    // Observables
    public IObservable<System.Reactive.Unit> TransactionSaved => _transactionSaved.AsObservable();
    public IObservable<System.Reactive.Unit> Cancelled => _cancelled.AsObservable();

    public void StartAdd()
    {
        _originalTransaction = null;
        IsEditing = false;
        ResetForm();
    }

    public void StartEdit(Transaction transaction)
    {
        _originalTransaction = transaction;
        IsEditing = true;
        LoadTransaction(transaction);
    }

    private void ResetForm()
    {
        Description = string.Empty;
        Amount = 0;
        Date = DateTime.Today;
        Type = TransactionType.Expense;
        Category = CategoryUtils.GetDefaultCategoryForType(Type);
        Subcategory = null;
        Notes = null;
    }

    private void LoadTransaction(Transaction transaction)
    {
        Description = transaction.Description;
        Amount = transaction.Amount;
        Date = transaction.Date;
        Type = transaction.Type;
        Category = transaction.Category;
        Subcategory = transaction.Subcategory;
        Notes = transaction.Notes;
    }

    private async Task SaveTransaction()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var transaction = new Transaction
            {
                Id = _originalTransaction?.Id,
                Description = Description,
                Amount = Amount,
                Date = Date,
                Type = Type,
                Category = Category,
                Subcategory = string.IsNullOrWhiteSpace(Subcategory) ? null : Subcategory,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes
            };

            if (IsEditing)
            {
                await _transactionService.UpdateTransactionAsync(transaction);
            }
            else
            {
                await _transactionService.AddTransactionAsync(transaction);
            }

            // Notificar que la transacción se guardó correctamente
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                _transactionSaved.OnNext(System.Reactive.Unit.Default);
            }
            else
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    _transactionSaved.OnNext(System.Reactive.Unit.Default));
            }
        }, "SaveTransaction");
    }

    private void Cancel()
    {
        _cancelled.OnNext(System.Reactive.Unit.Default);
    }

    private void SelectSubcategory(string subcategory)
    {
        Subcategory = subcategory;
    }

    private void ClearSubcategory()
    {
        Subcategory = null;
    }

    private void UpdateSuggestedSubcategories(Category category)
    {
        SuggestedSubcategories.Clear();
        var suggestions = CategoryUtils.GetCommonSubcategories(category);
        foreach (var suggestion in suggestions)
        {
            SuggestedSubcategories.Add(suggestion);
        }
    }

    private void OnTypeChanged(TransactionType type)
    {
        // Cambiar a una categoría válida si la actual no es válida
        if (!CategoryUtils.IsValidCategoryForType(Category, type))
        {
            Category = CategoryUtils.GetDefaultCategoryForType(type);
        }

        // Forzar notificación de ValidCategories ya que es una computed property
        this.RaisePropertyChanged(nameof(ValidCategories));
    }
}
