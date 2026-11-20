using DomainClassLibrary;
using Lab1.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SortingAggregator.ViewModels
{
    public class ArrayItemViewModel : ObservableObject
    {
        private int _value;
        private bool _isHighlighted;

        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        public override string ToString() => Value.ToString();
    }

    public class CycleViewModel : OperationViewModelBase
    {
        private ObservableCollection<ArrayItemViewModel> _arrayItems;
        private string _arrayInputText;
        private int _threshold;
        private int _length;
        private int _minVal;
        private int _maxVal;
        private CycleModeInfo _selectedMode;
        private ObservableCollection<CycleModeInfo> _modes;

        private CycleVerifier _verifier;

        public int Length { get => _length; set => SetProperty(ref _length, value); }
        public int MinVal { get => _minVal; set => SetProperty(ref _minVal, value); }
        public int MaxVal { get => _maxVal; set => SetProperty(ref _maxVal, value); }

        public ObservableCollection<ArrayItemViewModel> ArrayItems
        {
            get => _arrayItems;
            set => SetProperty(ref _arrayItems, value);
        }

        public string ArrayInputText
        {
            get => _arrayInputText;
            set
            {
                if (SetProperty(ref _arrayInputText, value)) ParseArray();
            }
        }

        public int Threshold
        {
            get => _threshold;
            set
            {
                if (SetProperty(ref _threshold, value))
                    ResetSimulation();
            }
        }

        public ObservableCollection<CycleModeInfo> Modes
        {
            get => _modes;
            set => SetProperty(ref _modes, value);
        }

        public CycleModeInfo SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (SetProperty(ref _selectedMode, value)) ResetSimulation();
            }
        }

        private bool _isInvTrueBefore;
        public bool IsInvTrueBefore
        {
            get => _isInvTrueBefore;
            set => SetProperty(ref _isInvTrueBefore, value);
        }

        private bool _isInvTrueAfter;
        public bool IsInvTrueAfter
        {
            get => _isInvTrueAfter;
            set => SetProperty(ref _isInvTrueAfter, value);
        }

        private string _invStatusText;
        public string InvStatusText
        {
            get => _invStatusText;
            set => SetProperty(ref _invStatusText, value);
        }

        private string _variantStatusText;
        public string VariantStatusText
        {
            get => _variantStatusText;
            set => SetProperty(ref _variantStatusText, value);
        }

        private double _variantProgress;
        public double VariantProgress
        {
            get => _variantProgress;
            set => SetProperty(ref _variantProgress, value);
        }

        private string _wpExplanation;
        public string WpExplanation
        {
            get => _wpExplanation;
            set => SetProperty(ref _wpExplanation, value);
        }

        private string _correctnessConditions;
        public string CorrectnessConditions
        {
            get => _correctnessConditions;
            set => SetProperty(ref _correctnessConditions, value);
        }

        private int _currentStepIndex;
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set => SetProperty(ref _currentStepIndex, value);
        }

        public ICommand StepCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand GenerateCommand { get; }

        public override string DisplayName => "Циклы: Инварианты";
        public override string ContractPre => "Массив не пуст, параметры генератора корректны";
        public override string ContractPost => "Инвариант сохранен, вариант-функция убывает, постусловие выполнено";
        public override string ContractEffects => "Пошаговая визуализация цикла с проверкой корректности";
        public override string ExampleValid => "Длина=5, Мин=1, Макс=10";
        public override string ExampleInvalid => "Длина=0 или Мин>Макс";

        public CycleViewModel()
        {
            Modes = new ObservableCollection<CycleModeInfo>(CyclePresets.GetModes());
            SelectedMode = Modes.FirstOrDefault();
            _arrayItems = new ObservableCollection<ArrayItemViewModel>();
            Length = 6; MinVal = 1; MaxVal = 99;

            StepCommand = new RelayCommand(_ => DoStep());
            RunCommand = new RelayCommand(_ => DoRun());
            ResetCommand = new RelayCommand(_ => ResetSimulation());
            GenerateCommand = new RelayCommand(_ => GenerateArray());

            GenerateArray();
            ResetSimulation();
        }

        private void GenerateArray()
        {
            var rnd = new Random();
            var items = new ObservableCollection<ArrayItemViewModel>();
            for (int i = 0; i < Length; i++)
            {
                items.Add(new ArrayItemViewModel
                {
                    Value = rnd.Next(MinVal, MaxVal + 1),
                    IsHighlighted = false
                });
            }
            ArrayItems = items;
            ArrayInputText = string.Join(", ", items.Select(x => x.Value));
            ResetSimulation();
        }

        private void ParseArray()
        {
            try
            {
                var parts = ArrayInputText.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var items = new ObservableCollection<ArrayItemViewModel>();
                foreach (var p in parts)
                {
                    if (int.TryParse(p, out int val))
                        items.Add(new ArrayItemViewModel { Value = val, IsHighlighted = false });
                }
                ArrayItems = items;
                Length = items.Count;
            }
            catch { }
        }

        private void UpdateHighlight(int j)
        {
            foreach (var item in ArrayItems) item.IsHighlighted = false;
            if (j >= 0 && j < ArrayItems.Count)
                ArrayItems[j].IsHighlighted = true;
        }

        private void ResetSimulation()
        {
            if (SelectedMode == null || ArrayItems == null || ArrayItems.Count == 0) return;

            _verifier = new CycleVerifier(ArrayItems.Select(x => x.Value).ToArray(), Threshold, SelectedMode);
            CurrentStepIndex = 0;
            UpdateHighlight(-1);

            IsInvTrueBefore = true;
            IsInvTrueAfter = true;
            InvStatusText = "Ожидание запуска...";
            VariantStatusText = $"t = {ArrayItems.Count}";
            VariantProgress = 100;

            WpExplanation = _verifier.GetWpExplanation();

            CorrectnessConditions =
                $"1. Инициализация: P ⇒ Inv[j:=0]\n" +
                $"   ({SelectedMode.PreCondition}) ⇒ ({SelectedMode.InvariantFormula}[j=0])\n\n" +
                $"2. Сохранение: Inv ∧ B ⇒ wp(S, Inv')\n" +
                $"   ({SelectedMode.InvariantFormula} ∧ {SelectedMode.GuardCondition}) ⇒ wp(S, Inv')\n\n" +
                $"3. Выход: Inv ∧ ¬B ⇒ Post\n" +
                $"   ({SelectedMode.InvariantFormula} ∧ j≥N) ⇒ ({SelectedMode.PostCondition})\n\n" +
                $"4. Завершение: t ≥ 0 ∧ t' < t\n" +
                $"   ({SelectedMode.VariantFunction} ≥ 0) ∧ ((N-(j+1)) < (N-j))";
        }

        private void DoStep()
        {
            if (_verifier == null || _verifier.IsFinished())
            {
                InvStatusText = "Цикл завершен.";
                return;
            }

            var result = _verifier.ExecuteStep();
            CurrentStepIndex = result.Index;
            UpdateHighlight(result.CurrentJ);

            IsInvTrueBefore = result.IsInvariantTrueBefore;
            IsInvTrueAfter = result.IsInvariantTrueAfter;

            if (result.IsInvariantTrueBefore && result.IsInvariantTrueAfter)
                InvStatusText = "Инвариант: СОХРАНЕН";
            else
                InvStatusText = "Инвариант: НАРУШЕН!";

            VariantStatusText = $"t = {result.VariantValue} → {result.VariantValue - 1}";
            if (result.IsVariantDecreasing)
                VariantStatusText += " (Убывает)";

            int initialN = ArrayItems.Count;
            double percent = ((double)(initialN - result.Index) / initialN) * 100;
            VariantProgress = percent;
        }

        private void DoRun()
        {
            ResetSimulation();
            while (!_verifier.IsFinished()) DoStep();

            bool postOk = _verifier.CheckPostCondition();
            InvStatusText = postOk ? "Постусловие: ВЫПОЛНЕНО" : "Постусловие: НЕ ВЫПОЛНЕНО";
        }

        protected override bool CheckPre(out string failReason)
        {
            if (ArrayItems == null || ArrayItems.Count == 0)
            {
                failReason = "Массив пуст";
                return false;
            }
            failReason = "";
            return true;
        }

        protected override void RunOperation(int[] input, out string result)
        {
            result = "Используйте кнопки Шаг/Пуск";
        }

        protected override bool CheckPost(int[] input, string result)
        {
            return true;
        }
    }
}