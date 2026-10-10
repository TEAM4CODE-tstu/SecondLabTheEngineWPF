using Lab1.Domain.Services.SortingServices;

namespace Lab1.Domain.Services.Sortings
{
    public class QuickSortingService : ISortAlgorithm
    {
        private static readonly Random _random = new Random();

        public int[] Sort(int[] input)
        {
            var arr = (int[])input.Clone();
            if (arr.Length > 1)
            {
                QuickSortRecursive(arr, 0, arr.Length - 1);
            }

            return arr;
        }

        private static void QuickSortRecursive(int[] arr, int low, int high)
        {
            if (low >= high)
                return;

            int pivotIndex = Partition(arr, low, high);
            QuickSortRecursive(arr, low, pivotIndex - 1);
            QuickSortRecursive(arr, pivotIndex + 1, high);
        }

        private static int Partition(int[] arr, int low, int high)
        {
            int randomIndex = _random.Next(low, high + 1);
            Swap(arr, randomIndex, high);

            int pivot = arr[high];
            int i = low;

            for (int j = low; j < high; j++)
            {
                if (arr[j] <= pivot)
                {
                    Swap(arr, i, j);
                    i++;
                }
            }

            Swap(arr, i, high);
            return i;
        }

        private static void Swap(int[] arr, int i, int j)
        {
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
}
