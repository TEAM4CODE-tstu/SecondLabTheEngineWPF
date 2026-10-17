using Lab1.Infrastructure;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SortingAggregator.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // Коллекция только для ЛР-1 (отображается в левом боковом меню)
        public ObservableCollection<OperationViewModelBase> Operations { get; } = new();

        private OperationViewModelBase? _selectedOperation;
        private string _helpText = "Выберите вкладку, чтобы увидеть инструкцию по ней.";
        public string HelpText
        {
            get => _helpText;
            set
            {
                _helpText = value;
                OnPropertyChanged(); // Важно! Это заставит HelpWindow обновиться
            }
        }
        // Метод для установки текста при переключении вкладок
        public void UpdateHelpText(string tabName)
        {
            switch (tabName)
            {
                case "Контракты в интерфейсе":
                    HelpText = @"Инструкция: Контракты в интерфейсе
                    
1. Выберите операцию из списка слева (например, 'Сортировка подсчётом').
2. Введите веса грузов в поле (разделители: пробел, запятая, точка с запятой).
3. Нажмите 'Выполнить'.
4. Зелёные индикаторы означают, что предусловие и постусловие выполнены. Красный — ошибка логики или ввода.
5. Кнопка 'Показать контракт' выведет спецификацию (Pre/Post условия) для выбранной операции.";
                    break;

                case "Конструктор фрагмента":
                    HelpText = @"Инструкция: Конструктор фрагмента (WP-анализ)

1. Используйте кнопки-пресеты слева для загрузки примеров кода.
2. Либо вручную отредактируйте поле 'Цель (Post)'.
3. Нажмите 'Рассчитать wp', чтобы получить слабейшее предусловие.
4. Нажмите 'Показать шаги', чтобы увидеть пошаговый вывод алгоритма Дейкстры в отдельном окне.
5. Нажмите 'Показать триаду', чтобы увидеть полную спецификацию Хоара {Pre} Code {Post}.";
                    break;

                default:
                    HelpText = "Выберите вкладку для получения инструкции.";
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public OperationViewModelBase? SelectedOperation
        {
            get => _selectedOperation;
            set => SetProperty(ref _selectedOperation, value);
        }

        // Выделенное свойство специально для ЛР-2 (для верхней вкладки)
        public FragmentConstructorViewModel FragmentConstructor { get; } = new();

        public MainViewModel()
        {
            // Здесь остаются ТОЛЬКО операции первой лабораторной
            Operations.Add(new MinMaxViewModel());
            Operations.Add(new SortViewModel());
            Operations.Add(new SumViewModel());

            if (Operations.Count > 0)
            {
                SelectedOperation = Operations[0];
            }
        }
    }
}
