using Lab1.Domain.Services.SortingServices;
using Lab1.Infrastructure;
namespace SortingAggregator.ViewModels;

public class SortViewModel : OperationViewModelBase
{
    public override string DisplayName => "Оптимизация погрузки";
    public override string ContractPre =>
        "Массив не null и не пустой";
    public override string ContractPost =>
        "Массив упорядочен по неубыванию; мультимножество элементов сохранено";
    public override string ContractEffects =>
        "Возвращает новый отсортированный массив. Исходный массив не изменяется \n" +
        "Исключение: ArgumentException при пустом или null-входе";
    public override string ExampleValid =>
        "Вход: 5, 3, 8, 1 → результат: 1, 3, 5, 8";
    public override string ExampleInvalid =>
        "Вход: (пусто) → предусловие не выполнено, операция не запускается";

    private string _selectedSortType = "Подсчётом";

    private double _executionTime;
    public double ExecutionTime
    {
        get => _executionTime;
        set => SetProperty(ref _executionTime, value); 
    }
    public string SelectedSortType
    {
        get => _selectedSortType;
        set
        {
            if (_selectedSortType != value)
            {
                _selectedSortType = value;
                SetProperty(ref _selectedSortType, value);
            }
        }
    }

    protected override bool CheckPre(out string failReason)
    {
        (bool isValid, failReason, int[] values) = ValidateInputText();

        return isValid;
    }

    protected override void RunOperation(int[] input, out string result)
    {
        try
        {
            var algorithm = SortAlgorithmFactory.GetAlgorithm(_selectedSortType);

            var (milliseconds, sorted) = TimeMeasurement.Measure(() => algorithm.Sort(input));

            ExecutionTime = milliseconds;

            result = string.Join(", ", sorted);
        }
        catch (ArgumentException ex)
        {
            result = $"Ошибка: {ex.Message}";
            ExecutionTime = 0;
        }
    }

    protected override bool CheckPost(int[] input, string result)
    {
        var sorted = result.Split(new[] { ", " }, StringSplitOptions.None)
                           .Select(s => int.Parse(s))
                           .ToArray();

        for (int i = 1; i < sorted.Length; i++)
            if (sorted[i - 1] > sorted[i]) return false;

        return sorted.SequenceEqual(input.OrderBy(x => x));
    }
}
