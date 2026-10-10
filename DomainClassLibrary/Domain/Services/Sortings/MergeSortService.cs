using Lab1.Domain.Services.SortingServices;

namespace Lab1.Domain.Services.Sortings
{
    public class MergeSortService : ISortAlgorithm
    {
        public int[] Sort(int[] input)
        {
            int[] tmp = new int[input.Length];
            MergeSortRecursive(input, tmp, 0, input.Length - 1);
            return input; 
        }

        private static void MergeSortRecursive(int[] arr, int[] temp, int left, int right)
        {
            if (left >= right)
                return;

            int mid = left + (right - left) / 2;
            MergeSortRecursive(arr, temp, left, mid);
            MergeSortRecursive(arr, temp, mid + 1, right);

            Merge(arr, temp, left, mid, right);
        }

        private static void Merge(int[] arr, int[] temp, int left, int mid, int right)
        {
            int i = left;      
            int j = mid + 1;   
            int k = left;    

            while (i <= mid && j <= right)
            {
                if (arr[i] <= arr[j])
                {
                    temp[k++] = arr[i++];
                }
                else
                {
                    temp[k++] = arr[j++];
                }
            }

            while (i <= mid)
                temp[k++] = arr[i++];

            while (j <= right)
                temp[k++] = arr[j++];

            for (int idx = left; idx <= right; idx++)
                arr[idx] = temp[idx];
        }
    }
}
