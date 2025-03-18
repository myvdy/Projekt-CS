using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Statki
{
    public partial class SinglePlayerShips : Window
    {
        private int level;

        public SinglePlayerShips(int level)
        {
            InitializeComponent();
            this.level = level;
            GenerateAndDisplayBoard();
        }

        public void GenerateAndDisplayBoard()
        {
            int[][] board = new int[11][];

            for (int i = 0; i < 11; i++)
            {
                board[i] = new int[11];
                for (int j = 0; j < 11; j++)
                {
                    if (i == 0 || j == 0) {
                        board[i][j] = -1;
                    }
                    else {
                        board[i][j] = 0; 
                    }
                }
            }

            DisplayBoard(board);
        }

        public void DisplayBoard(int[][] board)
        {
            BoardGrid.Children.Clear();

            for (int i = 0; i < 11; i++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {


                    if (board[i][j] == -1 && i == 0)
                    {
                        Button btn = new Button
                        {
                            Content = Convert.ToChar(j + 64).ToString(),
                            Background = Brushes.White,
                            BorderBrush = Brushes.White


                        };
                        btn.IsEnabled = false;
                        Grid.SetRow(btn, i);
                        Grid.SetColumn(btn, j);
                        BoardGrid.Children.Add(btn);
                    }
                    if (board[i][j] == -1 && j == 0)
                    {
                        Button btn = new Button
                        {
                            Content = Convert.ToString(i),
                            Background = Brushes.White,
                            BorderBrush = Brushes.White


                        };
                        btn.IsEnabled = false;
                        Grid.SetRow(btn, i);
                        Grid.SetColumn(btn, j);
                        BoardGrid.Children.Add(btn);
                    }
                    if(board[i][j] == 0)
                    {
                        Button btn = new Button
                        {
                            Background = Brushes.LightBlue,
                            BorderBrush = Brushes.Black,

                        };

                        Grid.SetRow(btn, i);
                        Grid.SetColumn(btn, j);
                        BoardGrid.Children.Add(btn);
                    }
                    if (i == 0 && j == 0)
                    {
                        Button btn = new Button
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.White

                        };
                        btn.IsEnabled = false;
                        Grid.SetRow(btn, i);
                        Grid.SetColumn(btn, j);
                        BoardGrid.Children.Add(btn);
                    }
                }
            }
        }
    }
}
// gffdgffgfggfkdfkfdkdfkdfkdf