using System.Windows;
namespace GoodsLogistics
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }
        private void CloseHelp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
