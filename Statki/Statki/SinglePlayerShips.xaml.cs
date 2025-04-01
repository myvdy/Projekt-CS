using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Globalization;

namespace Statki {
    public class BtnSizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double number) { 
                return number * 0.325;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double number) {
                double k = (parameter is double koef) ? koef : 0.325;
                return number / k;
            }
            return value;
        }
    }

    public class ShipSizeConverter : IValueConverter {
        int size;
        bool isWidth;
        int top;
        int left;
        public ShipSizeConverter(int size, bool isWidth) {
            this.size = size;
            this.isWidth = isWidth;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double number) {
                return isWidth? number * size / 11 : number / 11;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double number) {
                double k = (parameter is double koef) ? koef : 0.375;
                return number;
            }
            return value;
        }
    }

    /*
    public class MarginConverter : IValueConverter {
        int size;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double val) { 
                
            }
        }
    }*/


    public partial class SinglePlayerShips : Window {

        int[][] board;
        bool isDrag = false;
        double top, left;
        System.Windows.Point dragStartPosition;
        System.Windows.Shapes.Rectangle currentShip; 

        public SinglePlayerShips(int level) {
            InitializeComponent();

            Grid Board = BoardGrid;
            Grid Board2 = BoardGrid2;
            Canvas leftCanvas = CanvasLeft;

            CanvasLeft.MouseMove += MoveShip;

            GenerateAndDisplayBoard(Board2);
            GenerateAndDisplayBoard(Board);
            AddShips(Board, CanvasLeft);
        }


        public void GenerateAndDisplayBoard(Grid Board) {
            board = new int[11][];

            for (int i = 0; i < 11; i++) {
                board[i] = new int[11];
                for (int j = 0; j < 11; j++) {
                    if (i == 0 || j == 0) {
                        board[i][j] = -1;
                    } else {
                        board[i][j] = 0;
                    }
                }
            }

            DisplayBoard(board, Board);
        }


        public void DisplayBoard(int[][] board, Grid Board) {
            BtnSizeConverter converter = new BtnSizeConverter();

            Binding bindHeight = new Binding("Height");
            Binding bindWidth = new Binding("Height");

            bindHeight.Source = window;
            bindHeight.Converter = converter;
            bindWidth.Source = Board;

            Board.SetBinding(HeightProperty, bindHeight);
            Board.SetBinding(WidthProperty, bindWidth);

            for (int i = 0; i < 11; i++) {
                Board.RowDefinitions.Add(new RowDefinition());
                Board.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 11; i++) {
                for (int j = 0; j < 11; j++) {
                    Button btn = new Button();
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);

                    if (board[i][j] == -1) {
                        btn.Content = i == 0 && j != 0 ? Convert.ToChar(j + 64).ToString() : Convert.ToString(i);
                        btn.Style = (Style)Resources["field"];
                    } else if (board[i][j] == 0) {
                        btn.Style = (Style)Resources["field2"];
                    }

                    btn.MouseDown += Button_MouseDown;

                    Board.Children.Add(btn);
                }
            }
        }


        private void AddShip(Grid gridBoard, Canvas canvas, int X, int Y, int size) {
            int row = 0;
            int column = 0;

            Ship ship = new Ship(size, row, column, true);

            ShipSizeConverter WidthConverter = new ShipSizeConverter(size, true);
            ShipSizeConverter HeightConverter = new ShipSizeConverter(size, false);

            Binding HeightBind = new Binding("Height");
            Binding WidthBind = new Binding("Height");

            HeightBind.Source = gridBoard;
            HeightBind.Converter = HeightConverter;
            WidthBind.Source = gridBoard;
            WidthBind.Converter = WidthConverter;

            for (int i = 0; i < ship.length; i++) {
                if (ship.isHorizontal) {
                    board[ship.X + i][ship.Y] = 1;
                } else {
                    board[ship.X][ship.Y + i] = 1;
                }
            }

            System.Windows.Shapes.Rectangle shipVis = new System.Windows.Shapes.Rectangle {
                Fill = new SolidColorBrush(Colors.Black),
                Stroke = new SolidColorBrush(Colors.Red),
            };

            Canvas.SetTop(shipVis, Y + gridBoard.Height);
            Canvas.SetLeft(shipVis, X);

            shipVis.SetBinding(WidthProperty, WidthBind);
            shipVis.SetBinding(HeightProperty, HeightBind);

            /*if (ship.isHorizontal) {
                shipVis.Width = Convert.ToInt32(30 * size);
                shipVis.Height = Convert.ToInt32(30);
            } else {
                shipVis.Width = Convert.ToInt32(30);
                shipVis.Height = Convert.ToInt32(30 * size);
            }*/

            shipVis.MouseDown += startDragging;
            shipVis.MouseUp += stopDragging;

            canvas.Children.Add(shipVis);
        }


        private void AddShips(Grid gridBoard, Canvas canvas) {
            int[][] shipsSizes = [[1, 1, 1, 1], [2, 2, 2], [3, 3], [4]];

            for (int i = 0; i < shipsSizes.Length; i++) {
                for (int j = 0; j < shipsSizes[i].Length; j++) {
                    int shipSize = shipsSizes[i][j];
                    int x = 30 + j * (shipSize * 30 + 40 / shipSize);
                    int y = shipSize * (30 + 10);

                    AddShip(gridBoard, canvas, x, y, shipSize);
                }
            }
        }


        private void Button_MouseDown(object sender, MouseButtonEventArgs e) {
            Button btn = sender as Button;
            if (btn != null) {
                //MessageBox.Show($"Kliknięto na pole: Wiersz {Grid.GetRow(btn)}, Kolumna {Grid.GetColumn(btn)}");
                MessageBox.Show(Convert.ToString(BoardGrid.Height));
                MessageBox.Show(Convert.ToString(window.Height));
            }
        }


        private void startDragging(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource is System.Windows.Shapes.Rectangle ship) {
                currentShip = ship;
                top = Canvas.GetTop(currentShip);
                left = Canvas.GetLeft(currentShip);


                dragStartPosition = e.GetPosition(CanvasLeft);
                isDrag = true;
            }
            //System.Windows.Point coords = e.GetPosition(CanvasLeft);
            /*if (sender is System.Windows.Shapes.Rectangle ship) {
                Canvas.SetLeft(ship, coords.X);
                Canvas.SetTop(ship, coords.Y);
            }*/
        }

        private void stopDragging(object sender, MouseButtonEventArgs e) {
            isDrag = false;
        }

        private void MoveShip(object sender, MouseEventArgs e) {
            if (isDrag) {
                System.Windows.Point coords = e.GetPosition(CanvasLeft);
                Canvas.SetLeft(currentShip, left + ((coords.X - left) - (dragStartPosition.X - left)));
                Canvas.SetTop(currentShip, top + ((coords.Y - top) - (dragStartPosition.Y - top)));
                //Canvas.SetTop(currentShip, top + (coords.Y - left));
            }
        }
    }
}
