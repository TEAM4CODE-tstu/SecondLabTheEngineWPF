using System.Diagnostics;
using static Lab1.Domain.Services.MinMaxService;
namespace Lab1.Domain.Services
{

    public static class SumService
    {
        /// <summary>
        /// Сумма элементов массива
        /// Pre:  массив не null и не пустой
        /// Post: результат = сумма всех элементов; результат ≥ min*len и результат ≤ max*len
        /// </summary>
        public static long Sum(int[] input)
        {
            Guard.Requires(input != null && input.Length > 0,
                "Массив не должен быть null или пустым");

            long sum = input.Sum();

            var (min, max) = FindMinMax(input);

            // Post: сумма в ожидаемых границах
            Debug.Assert(sum >= (long)min * input.Length,
                "Нарушено постусловие: сумма меньше возможного минимума");
            Debug.Assert(sum <= (long)max * input.Length,
                "Нарушено постусловие: сумма больше возможного максимума");

            return sum;
        }
    }
}
