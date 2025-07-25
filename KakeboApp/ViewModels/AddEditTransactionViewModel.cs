// ViewModel para agregar/editar transacciones

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using KakeboApp.Commands;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;
using KakeboApp.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;


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

        // Comandos usando AsyncCommand para evitar problemas de threading
        SaveCommand = new AsyncCommand(SaveTransaction, () => CanSave(), ex => HandleException(ex, "Error al guardar transacción"));
        CancelCommand = new AsyncCommand(CancelAsync, null, ex => HandleException(ex, "Error al cancelar"));

        // Comandos de subcategoría
        SelectSubcategoryCommand = new AsyncCommand<string>(SelectSubcategoryAsync, null, ex => HandleException(ex, "Error al seleccionar subcategoría"));
        ClearSubcategoryCommand = new AsyncCommand(ClearSubcategoryAsync, null, ex => HandleException(ex, "Error al limpiar subcategoría"));

        // Actualizar sugerencias cuando cambia la categoría
        this.WhenAnyValue(x => x.Category)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateSuggestedSubcategories);

        // Actualizar categorías válidas cuando cambia el tipo
        this.WhenAnyValue(x => x.Type)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(OnTypeChanged);

        // Inicializar sugerencias de forma segura
        UpdateSuggestedSubcategories(Category.Food);
    }

    public ObservableCollection<string> SuggestedSubcategories { get; }

    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public decimal Amount { get; set; }
    [Reactive] public DateTime Date { get; set; } = DateTime.Today;
    [Reactive] public TransactionType Type { get; set; } = TransactionType.Expense;
    [Reactive] public Category Category { get; set; } = Category.Food;
    [Reactive] public string? Subcategory { get; set; }
    [Reactive] public string? Notes { get; set; }
    [Reactive] public bool IsEditing { get; set; }

    // Categorías válidas según el tipo
    public IEnumerable<Category> ValidCategories => Type switch
    {
        TransactionType.Income => CategoryUtils.GetIncomeCategories(),
        TransactionType.Expense => CategoryUtils.GetExpenseCategories(),
        _ => Enum.GetValues<Category>()
    };

    // Comandos
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectSubcategoryCommand { get; }
    public ICommand ClearSubcategoryCommand { get; }

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
        try
        {
            // Crear la transacción con los valores actuales
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

            // Realizar la operación de base de datos
            if (IsEditing)
            {
                await _transactionService.UpdateTransactionAsync(transaction);
            }
            else
            {
                await _transactionService.AddTransactionAsync(transaction);
            }

            // Notificar que la transacción se guardó correctamente
            _transactionSaved.OnNext(System.Reactive.Unit.Default);
        }
        catch (Exception ex)
        {
            HandleException(ex, "SaveTransaction");
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Description) && Amount > 0 && !IsBusy;
    }

    private async Task CancelAsync()
    {
        await Task.Run(() => _cancelled.OnNext(System.Reactive.Unit.Default));
    }

    private async Task SelectSubcategoryAsync(string? subcategory)
    {
        await Task.Run(() => 
        {
            UIThreadHelper.InvokeOnUIThread(() => Subcategory = subcategory);
        });
    }

    private async Task ClearSubcategoryAsync()
    {
        await Task.Run(() => 
        {
            UIThreadHelper.InvokeOnUIThread(() => Subcategory = null);
        });
    }

    private void UpdateSuggestedSubcategories(Category category)
    {
        UIThreadHelper.InvokeOnUIThread(() =>
        {
            SuggestedSubcategories.Clear();
            var suggestions = CategoryUtils.GetCommonSubcategories(category);
            foreach (var suggestion in suggestions)
            {
                SuggestedSubcategories.Add(suggestion);
            }
        });
    }

    private void OnTypeChanged(TransactionType type)
    {
        UIThreadHelper.InvokeOnUIThread(() =>
        {
            // Cambiar a una categoría válida si la actual no es válida
            if (!CategoryUtils.IsValidCategoryForType(Category, type))
            {
                Category = CategoryUtils.GetDefaultCategoryForType(type);
            }

            // Forzar notificación de ValidCategories ya que es una computed property
            this.RaisePropertyChanged(nameof(ValidCategories));
        });
    }
}
