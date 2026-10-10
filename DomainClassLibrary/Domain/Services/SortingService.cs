using Lab1.Domain.Services.SortingServices;
using Lab1.Infrastructure;
using System.Diagnostics;
using System.Linq;

namespace Lab1.Domain.Services
{
    public class SortingService
    {
        /// <summary>
        /// Сортировка массива по неубыванию с выбором алгоритма
        /// </summary>
        public static (int[] Result, double Milliseconds) Sort(int[] input, string algorithmName)
        {
            Guard.Requires(input != null && input.Length > 0,
                "Массив не должен быть null или пустым");

            // Получаем нужный алгоритм
            var algorithm = SortAlgorithmFactory.GetAlgorithm(algorithmName);

            // Замеряем время выполнения
            var (milliseconds, result) = TimeMeasurement.Measure(() => algorithm.Sort(input));

            // Post: упорядочен
            for (int i = 1; i < result.Length; i++)
                Debug.Assert(result[i - 1] <= result[i],
                    "Нарушено постусловие: массив не упорядочен");

            // Post: мультимножество сохранено
            Debug.Assert(result.SequenceEqual(input.OrderBy(x => x)),
                "Нарушено постусловие: мультимножество элементов изменено");

            return (result, milliseconds);
        }
    }
}
