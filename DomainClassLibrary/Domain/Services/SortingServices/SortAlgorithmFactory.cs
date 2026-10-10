namespace Lab1.Domain.Services.SortingServices
{
    public static class SortAlgorithmFactory
    {
        public static ISortAlgorithm GetAlgorithm(string algorithmName)
        {
            return algorithmName switch
            {
                "Подсчётом" => new Sortings.CountingSortService(),
                "Быстрая" => new Sortings.QuickSortingService(),
                "Слиянием" => new Sortings.MergeSortService(),
                _ => throw new ArgumentException($"Неизвестный алгоритм: {algorithmName}")
            };
        }
    }
}