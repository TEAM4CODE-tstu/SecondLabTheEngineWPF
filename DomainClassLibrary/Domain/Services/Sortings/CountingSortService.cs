using Lab1.Domain.Services.SortingServices;
using static Lab1.Domain.Services.MinMaxService;
namespace Lab1.Domain.Services.Sortings
{
    public class CountingSortService : ISortAlgorithm
    {
        public int[] Sort(int[] input)
        {
            (int min, int max) = FindMinMax(input);
            int len = input.Length;
            int k = max - min + 1;
            var count = new int[k];

            foreach (var value in input)
            {
                count[value - min]++;
            }

            for (int i = 1; i < k; i++)
            {
                count[i] += count[i - 1];
            }

            var output = new int[len];
            for (int i = len - 1; i >= 0; i--)
            {
                int value = input[i];
                int index = value - min;

                count[index]--;
                output[count[index]] = value;
            }
            return output;
        }
    }
}
