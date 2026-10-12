using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DomainClassLibrary;
using Lab1.Infrastructure;

namespace SortingAggregator.ViewModels
{
    // Класс для пошагового трейса, который требует XAML твоего коллеги (через Run Text)
    public class WpStepItem
    {
        public string Operator { get; set; }
        public string Condition { get; set; }
    }

    public class FragmentConstructorViewModel : OperationViewModelBase
    {
        private readonly WpEngine _wpEngine;

        // Поля, названные строго по XAML твоего коллеги
        private string _postText;
        private string _postHumanText;
        private string _codeText;
        private string _finalWp;
        private string _finalWpHuman;
        private string _errorText;
        private string _triadText;

        public override string DisplayName => "Конструктор фрагмента";

        // Свойства привязки XAML (TextBox и TextBlock)
        public string PostText
        {
            get => _postText;
            set => SetProperty(ref _postText, value);
        }

        public string PostHumanText
        {
            get => _postHumanText;
            set => SetProperty(ref _postHumanText, value);
        }

        public string CodeText
        {
            get => _codeText;
            set => SetProperty(ref _codeText, value);
        }

        public string FinalWp
        {
            get => _finalWp;
            set => SetProperty(ref _finalWp, value);
        }

        public string FinalWpHuman
        {
            get => _finalWpHuman;
            set => SetProperty(ref _finalWpHuman, value);
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        public string TriadText
        {
            get => _triadText;
            set => SetProperty(ref _triadText, value);
        }

        // Коллекции для списков XAML (Steps и DefinednessNotes)
        public ObservableCollection<WpStepItem> Steps { get; set; }
        public ObservableCollection<string> DefinednessNotes { get; set; }

        // Команды, названные строго по XAML кнопок
        public ICommand LoadPresetCommand { get; }
        public ICommand CalculateCommand { get; }
        public ICommand ShowTriadCommand { get; }

        // Заглушки для базового класса ЛР-1
        public override string ContractPre => "Авто-wp";
        public override string ContractPost => "Цель из XAML";
        public override string ContractEffects => "Анализ Дейкстры";
        public override string ExampleValid => "Используйте пресеты";
        public override string ExampleInvalid => "Статический анализ";

        public FragmentConstructorViewModel()
        {
            _wpEngine = new WpEngine();
            Steps = new ObservableCollection<WpStepItem>();
            DefinednessNotes = new ObservableCollection<string>();

            // Связываем кнопки XAML с логикой
            LoadPresetCommand = new RelayCommand(param => ExecuteLoadPreset(param?.ToString()));
            CalculateCommand = new RelayCommand(_ => ExecuteCalculateWp());
            ShowTriadCommand = new RelayCommand(_ => ExecuteShowHoareTriple());
        }

        private void ExecuteLoadPreset(string presetType)
        {
            if (string.IsNullOrEmpty(presetType)) return;

            var preset = WpPresets.GetPreset(presetType);
            CodeText = preset.Code;
            PostText = preset.PostFormula;
            PostHumanText = preset.PostHuman;

            // Очищаем предыдущие результаты
            Steps.Clear();
            DefinednessNotes.Clear();
            FinalWp = string.Empty;
            FinalWpHuman = string.Empty;
            ErrorText = string.Empty;
            TriadText = string.Empty;
        }

        private void ExecuteCalculateWp()
        {
            if (string.IsNullOrWhiteSpace(CodeText) || string.IsNullOrWhiteSpace(PostText))
            {
                ErrorText = "Ошибка: Заполните поле кода и краткую формулу цели (Post)!";
                return;
            }

            Steps.Clear();
            DefinednessNotes.Clear();
            ErrorText = string.Empty;
            TriadText = string.Empty;

            try
            {
                var result = _wpEngine.CalculateWp(CodeText, PostText);

                // Превращаем обычные строки в красивые объекты для Run Text в XAML
                Steps.Add(new WpStepItem { Operator = "START", Condition = PostText });

                foreach (var step in result.Steps.Skip(1)) // Пропускаем стартовое, так как добавили его красиво
                {
                    // Вырезаем название оператора для красивого разделения в интерфейсе
                    string opName = step.Contains("[") ? step.Split('[', ']')[1] : "Команда";
                    string cond = step.Contains("]:") ? step.Split(new[] { "]:" }, StringSplitOptions.None)[1].Trim() : step;

                    Steps.Add(new WpStepItem { Operator = opName, Condition = cond });
                }

                FinalWp = result.FinalFormula;

                // Если в коде есть деление, выводим красивую плашку предупреждения
                if (CodeText.Contains("/"))
                {
                    DefinednessNotes.Add("Внимание: Обнаружена операция деления. Движок автоматически добавил в контракт проверку определенности (знаменатель ≠ 0) для предотвращения падения программы.");
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"Ошибка анализа движком: {ex.Message}";
            }
        }

        private void ExecuteShowHoareTriple()
        {
            if (string.IsNullOrWhiteSpace(FinalWp))
            {
                MessageBox.Show("Сначала выполните расчет (Рассчитать wp)!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Выводим текст триады прямо на экран (в самый низ формы, куда фронтендщик привязал TriadText)
            TriadText = $"Академическая Триада Хоара для отчета:\n" +
                        $"{{{FinalWp}}}\n" +
                        $"{CodeText}\n" +
                        $"{{{PostText} ({PostHumanText})}}";
        }

        protected override bool CheckPre(out string failReason) { failReason = ""; return true; }
        protected override void RunOperation(int[] input, out string result) { result = ""; }
        protected override bool CheckPost(int[] input, string result) { return true; }
    }
}
