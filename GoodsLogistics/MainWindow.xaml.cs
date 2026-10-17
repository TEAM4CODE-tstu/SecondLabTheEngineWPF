
using SortingAggregator.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GoodsLogistics
{
    public partial class MainWindow : Window
    {

        public MainWindow() => InitializeComponent();


        private bool _canClose = false;

        /// <summary>
        ///  Метод и предупреждение для выхода из приложения чтобы не по крестику
        /// </summary>
        /// <param name = "sender" ></ param >
        //< param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
                MessageBox.Show("Используйте кнопку 'Выход' для корректного закрытия приложения!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _canClose = true; 
            Application.Current.Shutdown();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                var helpWindow = new HelpWindow();

                helpWindow.DataContext = vm;

                helpWindow.ShowDialog();
            }
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var tabControl = sender as TabControl;
                if (tabControl != null && tabControl.SelectedItem is TabItem selectedTab)
                {
                    // Передаем заголовок вкладки в ViewModel
                    vm.UpdateHelpText(selectedTab.Header.ToString());
                }
            }
        }
    }
}