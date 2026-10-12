using System.Collections.ObjectModel;
using Lab1.Infrastructure;

namespace SortingAggregator.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // Коллекция только для ЛР-1 (отображается в левом боковом меню)
        public ObservableCollection<OperationViewModelBase> Operations { get; } = new();

        private OperationViewModelBase? _selectedOperation;
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
