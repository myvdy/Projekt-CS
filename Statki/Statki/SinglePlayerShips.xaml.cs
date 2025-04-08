using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace Statki
{

    public class PositionConverter : IValueConverter
    {
        private readonly int index;
        private readonly bool isTop;

        public PositionConverter(int index, bool isTop)
        {
            this.index = index;
            this.isTop = isTop;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double gridDimension)
            {
                return index * (gridDimension / 11);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class BtnSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                return number * 0.325;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                double k = (parameter is double koef) ? koef : 0.325;
                return number / k;
            }
            return value;
        }
    }

    public class ShipSizeConverter : IValueConverter
    {
        int size;
        bool isWidth;
        int top;
        int left;
        public ShipSizeConverter(int size, bool isWidth)
        {
            this.size = size;
            this.isWidth = isWidth;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                return isWidth ? number * size / 11 : number / 11;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                double k = (parameter is double koef) ? koef : 0.375;
                return number;
            }
            return value;
        }
    }


    public class MarginConverter : IValueConverter
    {
        double currentSize;
        int size;
        int j;
        bool isLeft;
        public MarginConverter(double currentSize, int size, int j, bool isLeft)
        {
            this.currentSize = currentSize;
            this.size = size;
            this.j = j;
            this.isLeft = isLeft;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is double number)
            {
                if (isLeft)
                {
                    return currentSize + j * (size * currentSize + currentSize * 4 / 3 / size);
                }
                else
                {
                    return size * (currentSize + currentSize / 3);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    public partial class SinglePlayerShips : Window
    {

        int[][] board;
        bool isDrag = false;
        double top, left;
        System.Windows.Point dragStartPosition;
        Image currentShip;

        public SinglePlayerShips(int level)
        {
            InitializeComponent();
            window.StateChanged += SizeChanged;


            Grid Board = BoardGrid;
            Grid Board2 = BoardGrid2;
            Canvas leftCanvas = CanvasLeft;

            CanvasLeft.MouseMove += MoveShip;

            GenerateAndDisplayBoard(Board);
            GenerateAndDisplayBoard(Board2);
            Board.Loaded += (s, e) => AddShips(Board, CanvasLeft);
        }
        private void SizeChanged(object sender, EventArgs e)
        {
            double height = System.Windows.SystemParameters.PrimaryScreenHeight;
            double width = System.Windows.SystemParameters.PrimaryScreenWidth;
            window.Height = height;
            window.Width = width;
        }


        public void GenerateAndDisplayBoard(Grid Board)
        {
            board = new int[11][];

            for (int i = 0; i < 11; i++)
            {
                board[i] = new int[11];
                for (int j = 0; j < 11; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        board[i][j] = -1;
                    }
                    else
                    {
                        board[i][j] = 0;
                    }
                }
            }

            DisplayBoard(board, Board);
        }


        public void DisplayBoard(int[][] board, Grid Board)
        {
            BtnSizeConverter converter = new BtnSizeConverter();

            Binding bindHeight = new Binding("Height");
            Binding bindWidth = new Binding("Height");

            bindHeight.Source = window;
            bindHeight.Converter = converter;
            bindWidth.Source = Board;

            Board.SetBinding(HeightProperty, bindHeight);
            Board.SetBinding(WidthProperty, bindWidth);

            for (int i = 0; i < 11; i++)
            {
                Board.RowDefinitions.Add(new RowDefinition());
                Board.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    Button btn = new Button();
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);

                    if (board[i][j] == -1)
                    {
                        btn.Content = i == 0 && j != 0 ? Convert.ToChar(j + 64).ToString() : Convert.ToString(i);
                        btn.Style = (Style)Resources["field"];
                    }
                    else if (board[i][j] == 0)
                    {
                        btn.Style = (Style)Resources["field2"];
                        ImageBrush imgBrush = new ImageBrush();
                        imgBrush.ImageSource = new BitmapImage(new Uri("../../../water.png", UriKind.Relative));
                        btn.Background = imgBrush;
                    }

                    btn.MouseDown += Button_MouseDown;

                    Board.Children.Add(btn);
                }
            }
        }


        private void AddShip(Grid gridBoard, Canvas canvas, int X, int Y, int size)
        {
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

            for (int i = 0; i < ship.length; i++)
            {
                if (ship.isHorizontal)
                {
                    board[ship.X + i][ship.Y] = 1;
                }
                else
                {
                    board[ship.X][ship.Y + i] = 1;
                }
            }

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = new BitmapImage(new Uri("../../../ship.jpg", UriKind.Relative));

            Image shipVis = new Image
            {
                Source = imgBrush.ImageSource,
                Stretch = Stretch.Fill,
                Tag = ship
            };

            //double currentTop = Y;
            //double currentLeft = X;

            int nearestRow = Y;
            int nearestColumn = X;

            //int nearestRow = (int)Math.Round(currentTop / (BoardGrid.ActualHeight / 11));
            //int nearestColumn = (int)Math.Round(currentLeft / (BoardGrid.ActualWidth / 11));

            Binding topBinding = new Binding("ActualHeight")
            {
                Source = BoardGrid,
                Converter = new PositionConverter(nearestRow, true)
            };
            Binding leftBinding = new Binding("ActualWidth")
            {
                Source = BoardGrid,
                Converter = new PositionConverter(nearestColumn, false)
            };

            shipVis.SetBinding(Canvas.TopProperty, topBinding);
            shipVis.SetBinding(Canvas.LeftProperty, leftBinding);



            if (ship.isHorizontal)
            {
                shipVis.Width = gridBoard.ActualWidth / 11 * size;
                shipVis.Height = gridBoard.ActualHeight / 11;
            }
            else
            {
                shipVis.Width = gridBoard.ActualWidth / 11;
                shipVis.Height = gridBoard.ActualHeight / 11 * size;
            }

            shipVis.SetBinding(WidthProperty, WidthBind);
            shipVis.SetBinding(HeightProperty, HeightBind);

            shipVis.MouseLeftButtonDown += startDragging;
            shipVis.MouseLeftButtonUp += stopDragging;
            shipVis.MouseRightButtonDown += RotateShip;


            canvas.Children.Add(shipVis);
        }


        private void AddShips(Grid gridBoard, Canvas canvas)
        {
            int[][] shipsSizes = [[1, 1, 1, 1], [2, 2, 2], [3, 3], [4]];

            for (int i = 0; i < shipsSizes.Length; i++)
            {
                for (int j = 0; j < shipsSizes[i].Length; j++)
                {
                    int shipSize = shipsSizes[i][j];
                    double currentSize = gridBoard.Height / 11;

                    double x = currentSize;
                    double y = shipSize + gridBoard.Height / 11 * 12;

                    int nearestRow = (int)Math.Round(y / (BoardGrid.ActualHeight / 11));
                    int nearestColumn = (int)Math.Round(x / (BoardGrid.ActualWidth / 11));

                    AddShip(gridBoard, canvas, nearestColumn + j * (shipSize + 1), nearestRow + i * 2, shipSize);
                }
            }
        }
        private void RotateShip(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && sender is Image shipImage)
            {
                Ship ship = (Ship)shipImage.Tag;
                ship.isHorizontal = !ship.isHorizontal;

                double temp = shipImage.Width;
                shipImage.Width = shipImage.Height;
                shipImage.Height = temp;

                if (ship.isHorizontal)
                {
                    shipImage.Width = BoardGrid.ActualWidth / 11 * ship.length;
                    shipImage.Height = BoardGrid.ActualHeight / 11;
                }
                else
                {
                    shipImage.Width = BoardGrid.ActualWidth / 11;
                    shipImage.Height = BoardGrid.ActualHeight / 11 * ship.length;
                }

                isDrag = false;
                currentShip = null;

                Binding HeightBind = new Binding("Height");
                Binding WidthBind = new Binding("Height");

                HeightBind.Source = BoardGrid;
                HeightBind.Converter = new ShipSizeConverter(ship.length, !ship.isHorizontal);
                WidthBind.Source = BoardGrid;
                WidthBind.Converter = new ShipSizeConverter(ship.length, ship.isHorizontal);

                shipImage.SetBinding(WidthProperty, WidthBind);
                shipImage.SetBinding(HeightProperty, HeightBind);
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                MessageBox.Show($"Kliknięto na pole: Wiersz {Grid.GetRow(btn)}, Kolumna {Grid.GetColumn(btn)}");
                MessageBox.Show(Convert.ToString(BoardGrid.Height));
                MessageBox.Show(Convert.ToString(window.Height));
            }
        }


        private void startDragging(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image ship)
            {
                currentShip = ship;
                top = Canvas.GetTop(currentShip);
                left = Canvas.GetLeft(currentShip);


                dragStartPosition = e.GetPosition(CanvasLeft);
                isDrag = true;
            }
        }

        private void stopDragging(object sender, MouseButtonEventArgs e)
        {
            isDrag = false;

            if (currentShip != null)
            {
                double currentTop = Canvas.GetTop(currentShip);
                double currentLeft = Canvas.GetLeft(currentShip);

                int nearestRow = (int)Math.Round(currentTop / (BoardGrid.ActualHeight / 11));
                int nearestColumn = (int)Math.Round(currentLeft / (BoardGrid.ActualWidth / 11));

                Canvas.SetTop(currentShip, nearestRow * (BoardGrid.ActualHeight / 11));
                Canvas.SetLeft(currentShip, nearestColumn * (BoardGrid.ActualWidth / 11));

                Binding topBinding = new Binding("ActualHeight")
                {
                    Source = BoardGrid,
                    Converter = new PositionConverter(nearestRow, true)
                };
                Binding leftBinding = new Binding("ActualWidth")
                {
                    Source = BoardGrid,
                    Converter = new PositionConverter(nearestColumn, false)
                };
                currentShip.SetBinding(Canvas.TopProperty, topBinding);
                currentShip.SetBinding(Canvas.LeftProperty, leftBinding);
            }
        }


        private void MoveShip(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                System.Windows.Point coords = e.GetPosition(CanvasLeft);
                Canvas.SetLeft(currentShip, left + ((coords.X - left) - (dragStartPosition.X - left)));
                Canvas.SetTop(currentShip, top + ((coords.Y - top) - (dragStartPosition.Y - top)));
            }
        }


    }
}
