using DomainClassLibrary;
using Lab1.Domain.Services;
using Lab1.Domain.Services.SortingServices;
using Lab1.Infrastructure;
using System.Security.Cryptography;
using static Lab1.Domain.Services.MinMaxService;
using static Lab1.Domain.Services.SumService;
namespace Contracts.Tests
{
    // тесты Сони Залкиной для первой лабораторной работы

    public class FirstLabTest
    {
        private ISortAlgorithm countingSortAlgorithm = SortAlgorithmFactory.GetAlgorithm("Подсчётом");
        private ISortAlgorithm mergeSortAlgorithm = SortAlgorithmFactory.GetAlgorithm("Слиянием");
        private ISortAlgorithm quickSortingAlgorithm = SortAlgorithmFactory.GetAlgorithm("Быстрая");

        [Fact]
        public void Sort_TypicalArray_ReturnsSortedCopy()
        {
            int[] input = { 5, 3, 8, 1 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            for (int i = 1; i < countingSortResult.Length; i++)
                Assert.True(countingSortResult[i - 1] <= countingSortResult[i]);

            for (int i = 1; i < mergeSortResult.Length; i++)
                Assert.True(mergeSortResult[i - 1] <= mergeSortResult[i]);

            for (int i = 1; i < quickSortingResult.Length; i++)
                Assert.True(quickSortingResult[i - 1] <= quickSortingResult[i]);
        }

        [Fact]
        public void Sort_TypicalArray_PreservesMultiset()
        {
            int[] input = { 5, 3, 8, 1 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.True(countingSortResult.OrderBy(x => x).SequenceEqual(input.OrderBy(x => x)));
            Assert.True(mergeSortResult.OrderBy(x => x).SequenceEqual(input.OrderBy(x => x)));
            Assert.True(quickSortingResult.OrderBy(x => x).SequenceEqual(input.OrderBy(x => x)));
        }

        [Fact]
        public void Sort_SingleElement_ReturnsSameElement()
        {
            int[] input = { 42 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.Single(countingSortResult);
            Assert.Equal(42, countingSortResult[0]);

            Assert.Single(mergeSortResult);
            Assert.Equal(42, mergeSortResult[0]);

            Assert.Single(quickSortingResult);
            Assert.Equal(42, quickSortingResult[0]);
        }

        [Fact]
        public void Sort_AlreadySorted_ReturnsSameOrder()
        {
            int[] input = { 1, 2, 3, 4, 5 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.Equal(input, countingSortResult);
            Assert.Equal(input, mergeSortResult);
            Assert.Equal(input, quickSortingResult);
        }

        [Fact]
        public void Sort_ReverseSorted_ReturnsAscending()
        {
            int[] input = { 9, 7, 5, 3, 1 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.Equal(new[] { 1, 3, 5, 7, 9 }, countingSortResult);
            Assert.Equal(new[] { 1, 3, 5, 7, 9 }, mergeSortResult);
            Assert.Equal(new[] { 1, 3, 5, 7, 9 }, quickSortingResult);
        }

        [Fact]
        public void Sort_DuplicateElements_PreservesAll()
        {
            int[] input = { 3, 1, 3, 1, 2 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.Equal(new[] { 1, 1, 2, 3, 3 }, countingSortResult);
            Assert.Equal(new[] { 1, 1, 2, 3, 3 }, mergeSortResult);
            Assert.Equal(new[] { 1, 1, 2, 3, 3 }, quickSortingResult);
        }

        [Fact]
        public void Sort_NegativeNumbers_SortsCorrectly()
        {
            int[] input = { -5, -1, -3, 0, 2 };

            int[] countingSortResult = countingSortAlgorithm.Sort(input);
            int[] mergeSortResult = mergeSortAlgorithm.Sort(input);
            int[] quickSortingResult = quickSortingAlgorithm.Sort(input);

            Assert.Equal(new[] { -5, -3, -1, 0, 2 }, countingSortResult);
            Assert.Equal(new[] { -5, -3, -1, 0, 2 }, mergeSortResult);
            Assert.Equal(new[] { -5, -3, -1, 0, 2 }, quickSortingResult);
        }

        [Fact]
        public void Sort_NullInput_ThrowsException()
        {
            ;
            Assert.ThrowsAny<Exception>(() => countingSortAlgorithm.Sort(null!));
            Assert.ThrowsAny<Exception>(() => mergeSortAlgorithm.Sort(null!));
            Assert.ThrowsAny<Exception>(() => quickSortingAlgorithm.Sort(null!));
        }


        [Fact]
        public void FindMinMax_TypicalArray_ReturnsCorrectMinMax()
        {
            int[] input = { 5, 3, 8, 1 };

            var (min, max) = FindMinMax(input);

            Assert.Equal(1, min);
            Assert.Equal(8, max);
        }

        [Fact]
        public void FindMinMax_MinLessOrEqualMax()
        {
            int[] input = { 10, -4, 7, 0, 3 };

            var (min, max) = FindMinMax(input);

            Assert.True(min <= max);
        }

        [Fact]
        public void FindMinMax_MinAndMaxExistInArray()
        {
            int[] input = { 5, 3, 8, 1 };

            var (min, max) = FindMinMax(input);

            Assert.Contains(min, input);
            Assert.Contains(max, input);
        }

        [Fact]
        public void FindMinMax_SingleElement_MinEqualsMax()
        {
            int[] input = { 7 };

            var (min, max) = FindMinMax(input);

            Assert.Equal(7, min);
            Assert.Equal(7, max);
        }

        [Fact]
        public void FindMinMax_AllSameElements_MinEqualsMax()
        {
            int[] input = { 4, 4, 4, 4 };

            var (min, max) = FindMinMax(input);

            Assert.Equal(4, min);
            Assert.Equal(4, max);
        }

        [Fact]
        public void FindMinMax_NegativeNumbers_ReturnsCorrect()
        {
            int[] input = { -10, -3, -7, -1 };

            var (min, max) = FindMinMax(input);

            Assert.Equal(-10, min);
            Assert.Equal(-1, max);
        }

        [Fact]
        public void FindMinMax_NullInput_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => FindMinMax(null!));
        }

        [Fact]
        public void FindMinMax_EmptyArray_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => FindMinMax(Array.Empty<int>()));
        }


        [Fact]
        public void Sum_TypicalArray_ReturnsCorrectSum()
        {
            int[] input = { 5, 3, 8, 1 };

            long result = Sum(input);

            Assert.Equal(17, result);
        }

        [Fact]
        public void Sum_SingleElement_ReturnsThatElement()
        {
            int[] input = { 42 };

            long result = Sum(input);

            Assert.Equal(42, result);
        }

        [Fact]
        public void Sum_NegativeAndPositive_ReturnsCorrectSum()
        {
            int[] input = { -5, 3, -8, 10 };

            long result = Sum(input);

            Assert.Equal(0, result);
        }

        [Fact]
        public void Sum_AllNegative_ReturnsNegativeSum()
        {
            int[] input = { -1, -2, -3 };

            long result = Sum(input);

            Assert.Equal(-6, result);
        }

        [Fact]
        public void Sum_ResultWithinBounds()
        {
            int[] input = { 5, 3, 8, 1 };

            long sum = Sum(input);
            int min = input.Min();
            int max = input.Max();

            Assert.True(sum >= (long)min * input.Length);
            Assert.True(sum <= (long)max * input.Length);
        }

        [Fact]
        public void Sum_NullInput_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => Sum(null!));
        }

        [Fact]
        public void Sum_EmptyArray_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => Sum(Array.Empty<int>()));
        }
    }
}
