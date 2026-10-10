using System.Windows.Input;

namespace Lab1.Infrastructure;

public class RelayCommand : ICommand
{
    // Действие, которое будет выполнено при активации команды
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
    // Проверяет, разрешено ли сейчас выполнение команды
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    // Выполняет основное действие команды
    public void Execute(object? parameter) => _execute(parameter);
}