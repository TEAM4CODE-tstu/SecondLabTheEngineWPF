using System.Windows.Input;
namespace SortingAggregator.ViewModels;
using Lab1.Infrastructure;

public abstract class OperationViewModelBase : ObservableObject
// ObservableObject — это базовый класс,
// который позволяет объектам поддерживать механизм уведомлений об изменениях свойств
{
    private string _inputText = "";
    private string _resultText = "";
    private bool _isPreMet;
    private bool _isPostMet;
    private bool _hasResult;
    private string _preStatus = "Предусловие: ожидание ввода";
    private string _postStatus = "Постусловие: ожидание выполнения";

    public abstract string DisplayName { get;}
    public abstract string ContractPre { get; }
    public abstract string ContractPost { get; }
    public abstract string ContractEffects { get; }
    public abstract string ExampleValid { get; }
    public abstract string ExampleInvalid { get; }

    public string InputText
    {
        get => _inputText;
        set
        {
            if (!SetProperty(ref _inputText, value))
                return;

            ResultText = "";
            HasResult = false;
            IsPostMet = false;
            PostStatus = "Постусловие: ожидание выполнения";

            EvaluatePre();
        }

    }
    public string ResultText
    {
        get => _resultText;
        set => SetProperty(ref _resultText, value);
    }

    public bool IsPreMet
    {
        get => _isPreMet;
        set => SetProperty(ref _isPreMet, value);
    }

    public bool IsPostMet
    {
        get => _isPostMet;
        set => SetProperty(ref _isPostMet, value);
    }
    public bool HasResult
    {
        get => _hasResult;
        set => SetProperty(ref _hasResult, value);
    }

    public string PreStatus
    {
        get => _preStatus;
        set => SetProperty(ref _preStatus, value);
    }

    public string PostStatus
    {
        get => _postStatus;
        set => SetProperty(ref _postStatus, value);
    }

    public ICommand ExecuteCommand { get; }
    public ICommand ShowContractCommand { get; }

    protected OperationViewModelBase()
    {
        ExecuteCommand = new Lab1.Infrastructure.RelayCommand(_ => Execute());
        ShowContractCommand = new Lab1.Infrastructure.RelayCommand(_ => ShowContract());
    }

    /// <summary>
    /// Проверка предусловия на текущем вводе
    /// </summary>
    protected abstract bool CheckPre(out string failReason);

    /// <summary>
    /// Выполнение операции. Выбрасывает исключение при нарушении Pre
    /// </summary>
    protected abstract void RunOperation(int[] input, out string result);

    /// <summary>
    /// Проверка постусловия по результату
    /// </summary>
    protected abstract bool CheckPost(int[] input, string result);

    private void EvaluatePre()
    {
        var (isValid, error, _) = ValidateInputText();

        IsPreMet = isValid;
        PreStatus = isValid
            ? "Предусловие: выполнено"
            : $"Предусловие: не выполнено ({error})";
    }

    private void Execute()
    {
        var (isValid, error, input) = ValidateInputText();

        if (!isValid)
        {
            IsPreMet = false;
            PreStatus = $"Предусловие: не выполнено ({error})";
            IsPostMet = false;
            PostStatus = "Постусловие: не проверялось";
            HasResult = false;
            return;
        }

        IsPreMet = true;
        PreStatus = "Предусловие: выполнено";

        try
        {
            RunOperation(input, out var result);
            ResultText = result;
            HasResult = true;

            var postOk = CheckPost(input, result);
            IsPostMet = postOk;
            PostStatus = postOk
                ? "Постусловие: выполнено"
                : "Постусловие: не выполнено";
        }
        catch (ArgumentException ex)
        {
            IsPostMet = false;
            PostStatus = $"Постусловие: операция не выполнена ({ex.Message})";
            HasResult = false;
        }
        catch (Exception ex)
        {
            IsPostMet = false;
            PostStatus = $"Постусловие: ошибка ({ex.Message})";
            HasResult = false;
        }
    }


    protected (bool isValid, string? error, int[] values) ValidateInputText()
    {
        var parts = InputText
            .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0 || parts.Length >= 100)
            return (false, "Количество грузов должно быть меньше 100 и больше 0", Array.Empty<int>());

        var result = new List<int>(parts.Length);
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var value))
                return (false, $"Неподходящий ввод: '{part}'. Вес округляется до целого числа кг.", Array.Empty<int>());
            if (value > 20)
            {
                return (false, $"Ошибка логистики: вес '{value} кг' превышает лимит стандартного отправления (макс. 20 кг).", Array.Empty<int>());
            }
            result.Add(value);
        }

        return (true, null, result.ToArray());
    }

    protected virtual void ShowContract()
    {
        var window = new GoodsLogistics.Views.ContractWindow(this);
        window.ShowDialog();
    }
}
