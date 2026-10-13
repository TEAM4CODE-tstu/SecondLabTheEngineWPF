using System.Windows;
using System.Windows.Controls;

namespace GoodsLogistics.Views
{
    /// <summary>
    /// Логика взаимодействия для FragmentConstructor.xaml
    /// </summary>
    public partial class FragmentConstructor 
    {
        public FragmentConstructor()
        {
            InitializeComponent();
        }
        private void OnPresetClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                PresetLabel.Text = button.Content?.ToString() ?? "Неизвестно";
            }
        }
    }
}
