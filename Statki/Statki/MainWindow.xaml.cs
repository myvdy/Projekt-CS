using System.Windows;

namespace Statki
{
    public partial class MainWindow : Window
    {
        public int level;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EasyButton_Click(object sender, RoutedEventArgs e)
        {
            level = 1; 
            OpenSinglePlayerShipsWindow();
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            level = 2; 
            OpenSinglePlayerShipsWindow();
        }

        private void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            level = 0;  
            OpenSinglePlayerShipsWindow();
        }

       
        private void OpenSinglePlayerShipsWindow()
        {
            SinglePlayerShips singlePlayerShips = new SinglePlayerShips(level);
            singlePlayerShips.Show();
            this.Close();  
        }
    }
}
