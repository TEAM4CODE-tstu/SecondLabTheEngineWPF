using System.Diagnostics;

namespace Lab1.Domain.Services
{
    public class MinMaxService
    {
        /// <summary>
        /// Поиск минимума и максимума.
        /// Pre:  массив не null и не пустой
        /// Post: (min, max) — элементы из массива; min ≤ max;
        ///       min — наименьший, max — наибольший
        /// </summary>
        public static (int Min, int Max) FindMinMax(int[] input)
        {
            Guard.Requires(input != null && input.Length > 0,
                "Массив не должен быть null или пустым");

            int min = input.Min();
            int max = input.Max();

            // Post: min ≤ max
            Debug.Assert(min <= max, "Нарушено постусловие: min > max");
            // Post: min и max присутствуют в массиве
            Debug.Assert(input.Contains(min) && input.Contains(max),
                "Нарушено постусловие: min/max не найдены в массиве");

            return (min, max);
        }
    }
}
