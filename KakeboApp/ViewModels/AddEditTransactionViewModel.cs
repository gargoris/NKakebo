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
using KakeboApp.ViewModels;
using ReactiveUI;

public class AddEditTransactionViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private Transaction? _originalTransaction;
    private string _description = string.Empty;
    private decimal _amount;
    private DateTime _date = DateTime.Today;
    private TransactionType _type = TransactionType.Expense;
    private Category _category = Category.Food;
    private string? _subcategory;
    private string? _notes;
    private bool _isEditing;

    private readonly Subject<Unit> _transactionSaved = new();
    private readonly Subject<Unit> _cancelled = new();

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
        UpdateSuggestedSubcategories(_category);
    }

    public ObservableCollection<string> SuggestedSubcategories { get; }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => this.RaiseAndSetIfChanged(ref _amount, value);
    }

    public DateTime Date
    {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    public TransactionType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    public Category Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public string? Subcategory
    {
        get => _subcategory;
        set => this.RaiseAndSetIfChanged(ref _subcategory, value);
    }

    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    // Categorías válidas según el tipo
    public IEnumerable<Category> ValidCategories => Type switch
    {
        TransactionType.Income => CategoryUtils.GetIncomeCategories(),
        TransactionType.Expense => CategoryUtils.GetExpenseCategories(),
        _ => Enum.GetValues<Category>()
    };

    // Comandos
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<string, Unit> SelectSubcategoryCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearSubcategoryCommand { get; }

    // Observables
    public IObservable<Unit> TransactionSaved => _transactionSaved.AsObservable();
    public IObservable<Unit> Cancelled => _cancelled.AsObservable();

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
        IsBusy = true;
        try
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

            _transactionSaved.OnNext(Unit.Default);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving transaction: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        _cancelled.OnNext(Unit.Default);
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

        // Notificar cambio en categorías válidas
        this.RaisePropertyChanged(nameof(ValidCategories));
    }
}
