using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Statki {
    public class DatabaseManager {
        private string connectionString;
        public DatabaseManager(string Server, string Database, string User, string Password) {
            connectionString = $"SERVER={Server};DATABASE={Database};UID={User};PASSWORD={Password};";
        }

        public MySqlConnection GetConnection() {
            try {
                return new MySqlConnection(connectionString);
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd podczas nawiązywania połączenia z bazą danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        public DataTable ExecuteQuery(string query) {
            var connection = GetConnection();
            {
                try {
                    connection.Open();
                    var command = new MySqlCommand(query, connection);
                    var adapter = new MySqlDataAdapter(command);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
                catch (Exception ex) {
                    MessageBox.Show($"Błąd podczas wykonywania zapytania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }
    }
}