using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient; 

namespace Statki
{
    public partial class MainWindow : Window
    {
        public int level;

        public MainWindow()
        {
            InitializeComponent();
            Connect("localhost", "ships", "root", "");
        }

        public void Connect(string server, string database, string user, string password)
        {
            string connectionString = $"SERVER={server};DATABASE={database};UID={user};PASSWORD={password};";
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                MessageBox.Show("Połączono z bazą danych!", "Sukces");
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Błąd połączenia: {ex.Message}", "Błąd");
            }
            finally
            {

                connection.Close();
            }
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
