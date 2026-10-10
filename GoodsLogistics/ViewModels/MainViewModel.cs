using System.Collections.ObjectModel;
namespace SortingAggregator.ViewModels;

public class MainViewModel : Lab1.Infrastructure.ObservableObject
{
    // Коллекция доступных операций 
    public ObservableCollection<OperationViewModelBase> Operations { get; } = new();

    private OperationViewModelBase? _selectedOperation;
    public OperationViewModelBase? SelectedOperation
    {
        get => _selectedOperation;
        set => SetProperty(ref _selectedOperation, value);
    }

    public MainViewModel()
    {
        Operations = new ObservableCollection<OperationViewModelBase> {
        new MinMaxViewModel(), new SortViewModel(), new SumViewModel()};
    }
}
