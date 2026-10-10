using static Lab1.Domain.Services.MinMaxService;
namespace SortingAggregator.ViewModels;

public class MinMaxViewModel : OperationViewModelBase
{
    public override string DisplayName => "Контроль перегруза (Мин/Макс)";
    public override string ContractPre =>
        "Массив не null и не пустой";
    public override string ContractPost =>
        "Возвращены min и max — элементы массива; min ≤ max";
    public override string ContractEffects =>
        "Возвращает кортеж (min, max). " +
        "Исключение: ArgumentException при пустом или null-входе";
    public override string ExampleValid =>
        "Вход: 5, 3, 8, 1 → результат: min=1, max=8";
    public override string ExampleInvalid =>
        "Вход: (пусто) → предусловие не выполнено";

    protected override bool CheckPre(out string failReason)
    {
        (bool isValid, failReason, int[] values) = ValidateInputText();

        return isValid;
    }

    protected override void RunOperation(int[] input, out string result)
    {
        var (min, max) = FindMinMax(input);
        result = $"min = {min}, max = {max}";
    }

    protected override bool CheckPost(int[] input, string result)
    {
        var parts = result.Split(new[] { ", " }, StringSplitOptions.None);
        int min = int.Parse(parts[0].Split('=')[1].Trim());
        int max = int.Parse(parts[1].Split('=')[1].Trim());

        return min <= max && input.Contains(min) && input.Contains(max)
            && min == input.Min() && max == input.Max();
    }
}
