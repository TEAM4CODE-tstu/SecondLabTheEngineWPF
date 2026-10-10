using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab1.Infrastructure;

/// <summary>
/// Реализует INotifyPropertyChanged, чтобы UI мог узнавать об изменениях свойств
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (Equals(field, value))
            return false;
        field = value;

        // UI сообщается что свойство объекта изменилось
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        return true;
    }
}