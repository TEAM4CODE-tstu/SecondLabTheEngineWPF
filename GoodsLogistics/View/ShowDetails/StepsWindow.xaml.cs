using System.Windows;

namespace GoodsLogistics.Views
{
    public partial class StepsWindow : Window
    {
        public StepsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
