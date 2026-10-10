
using System.ComponentModel;
using System.Windows;

namespace GoodsLogistics
{
    public partial class MainWindow : Window
    {

        public MainWindow() => InitializeComponent();


        private bool _canClose = false;

        /// <summary>
        ///  Метод и предупреждение для выхода из приложения чтобы не по крестику
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            var helpWindow = new GoodsLogistics.HelpWindow();
            helpWindow.Owner = this;
            helpWindow.ShowDialog(); 
        }
    }
}