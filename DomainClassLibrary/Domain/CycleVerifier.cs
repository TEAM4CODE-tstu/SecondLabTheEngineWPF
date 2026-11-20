using System;
using System.Diagnostics;
using System.Linq;

namespace DomainClassLibrary
{
    public class CycleStepResult
    {
        public int Index { get; set; }
        public long ResultValue { get; set; }
        public bool IsInvariantTrueBefore { get; set; }
        public bool IsInvariantTrueAfter { get; set; }
        public int VariantValue { get; set; }
        public bool IsVariantDecreasing { get; set; }
        public string LogMessage { get; set; }
        public int CurrentJ { get; set; }
    }

    public class CycleVerifier
    {
        private int[] _array;
        private int _threshold;
        private CycleModeInfo _mode;
        private int _n;
        private int _j;
        private long _res;
        private int _prevVariant;
        private readonly WpEngine _wpEngine;
        private string _wpCondition;

        public CycleVerifier(int[] array, int threshold, CycleModeInfo mode)
        {
            _array = array;
            _threshold = threshold;
            _mode = mode;
            _n = array.Length;
            _wpEngine = new WpEngine();

            _j = 0;
            _res = mode.Name == "PrefixMax" ? long.MinValue : 0L;
            _prevVariant = _n - _j;

            CalculateWpForBody();
        }

        private void CalculateWpForBody()
        {
            try
            {
                var result = _wpEngine.CalculateWp(_mode.BodyCode, _mode.InvariantFormula);
                _wpCondition = result.FinalFormula;
            }
            catch
            {
                _wpCondition = "Ошибка вычисления wp движком";
            }
        }

        public CycleStepResult ExecuteStep()
        {
            if (_j >= _n)
                return new CycleStepResult { LogMessage = "Цикл завершен", CurrentJ = _j };

            bool invBefore = CheckInvariant(_j, _res);
            Debug.Assert(invBefore, $"Инвариант нарушен ПЕРЕД шагом j={_j}");

            long newRes;
            if (_mode.Name == "Count > T")
                newRes = _array[_j] > _threshold ? _res + 1 : _res;
            else
                newRes = _mode.Logic(_array, _j, _res);

            int nextJ = _j + 1;

            bool invAfter = CheckInvariant(nextJ, newRes);
            Debug.Assert(invAfter, $"Инвариант нарушен ПОСЛЕ шага j={nextJ}");

            int currentVariant = _n - _j;
            int nextVariant = _n - nextJ;
            bool isDecreasing = nextVariant < currentVariant;
            Debug.Assert(isDecreasing, $"Вариант-функция не убывает: {currentVariant} -> {nextVariant}");

            _res = newRes;
            _j = nextJ;
            _prevVariant = nextVariant;

            return new CycleStepResult
            {
                Index = _j,
                ResultValue = _res,
                IsInvariantTrueBefore = invBefore,
                IsInvariantTrueAfter = invAfter,
                VariantValue = currentVariant,
                IsVariantDecreasing = isDecreasing,
                CurrentJ = _j - 1,
                LogMessage = $"Шаг j={_j - 1}: res={_res}, t={currentVariant}->{nextVariant}"
            };
        }

        private bool CheckInvariant(int j, long res)
        {
            if (j == 0) return true;

            switch (_mode.Name)
            {
                case "PrefixSum":
                    long expectedSum = 0;
                    for (int i = 0; i < j; i++) expectedSum += _array[i];
                    return res == expectedSum;

                case "Count > T":
                    int expectedCount = 0;
                    for (int i = 0; i < j; i++) if (_array[i] > _threshold) expectedCount++;
                    return res == expectedCount;

                case "PrefixMax":
                    long expectedMax = long.MinValue;
                    for (int i = 0; i < j; i++) expectedMax = Math.Max(expectedMax, _array[i]);
                    return res == expectedMax;

                default: return false;
            }
        }

        public bool IsFinished() => _j >= _n;
        public long GetFinalResult() => _res;
        public bool CheckPostCondition() => _j == _n && CheckInvariant(_n, _res);

        public string GetWpExplanation() =>
            $"Режим: {_mode.Name}\n" +
            $"Тело цикла S: {_mode.BodyCode}\n" +
            $"Инвариант R: {_mode.InvariantFormula}\n" +
            $"Порог T: {_threshold}\n" +
            $"wp(S, R) = {_wpCondition}\n" +
            $"Условие сохранения: Inv ∧ B ⇒ wp(S, Inv')\n" +
            $"Определенность: {_mode.DefCondition}";
    }
}