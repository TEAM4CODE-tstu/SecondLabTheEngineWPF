using System;
using System.Collections.Generic;

namespace DomainClassLibrary
{
    public class CycleModeInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string InvariantFormula { get; set; }
        public string InvariantText { get; set; }
        public string VariantFunction { get; set; }
        public string PostCondition { get; set; }
        public string PreCondition { get; set; }
        public string GuardCondition { get; set; }
        public string BodyCode { get; set; }
        public string DefCondition { get; set; }
        public Func<int[], int, long, long> Logic { get; set; }
    }

    public static class CyclePresets
    {
        public static List<CycleModeInfo> GetModes()
        {
            return new List<CycleModeInfo>
            {
                new CycleModeInfo
                {
                    Name = "PrefixSum",
                    Description = "Сумма префикса массива",
                    PreCondition = "N >= 0",
                    InvariantFormula = "res == sum(a[0]..a[j-1])",
                    InvariantText = "Переменная res равна сумме элементов массива от 0 до j-1",
                    GuardCondition = "j < N",
                    BodyCode = "res := res + a[j]; j := j + 1;",
                    VariantFunction = "N - j",
                    PostCondition = "res == sum(a[0]..a[N-1]) AND j == N",
                    DefCondition = "Нет операций деления или корня",
                    Logic = (arr, j, res) => res + arr[j]
                },
                new CycleModeInfo
                {
                    Name = "Count > T",
                    Description = "Количество элементов > порога T",
                    PreCondition = "N >= 0 AND T определено",
                    InvariantFormula = "res == count(a[0]..a[j-1] > T)",
                    InvariantText = "Переменная res равна количеству элементов в префиксе, больших T",
                    GuardCondition = "j < N",
                    BodyCode = "if (a[j] > T) then res := res + 1 else res := res; j := j + 1;",
                    VariantFunction = "N - j",
                    PostCondition = "res == count(a[0]..a[N-1] > T) AND j == N",
                    DefCondition = "Нет операций деления или корня",
                    Logic = null
                },
                new CycleModeInfo
                {
                    Name = "PrefixMax",
                    Description = "Максимум префикса",
                    PreCondition = "N >= 1",
                    InvariantFormula = "res == max(a[0]..a[j-1])",
                    InvariantText = "Переменная res равна максимуму среди элементов префикса",
                    GuardCondition = "j < N",
                    BodyCode = "res := max(res, a[j]); j := j + 1;",
                    VariantFunction = "N - j",
                    PostCondition = "res == max(a[0]..a[N-1]) AND j == N",
                    DefCondition = "Нет операций деления или корня",
                    Logic = (arr, j, res) => Math.Max(res, arr[j])
                }
            };
        }
    }
}