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
        Ship[][] ships = [[new Ship(), new Ship(), new Ship(), new Ship()], [new Ship(), new Ship(), new Ship()], [new Ship(), new Ship()], [new Ship()]];

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


        private void AddShip(Grid gridBoard, Canvas canvas, Ship ship)
        {
            int row = 0;
            int column = 0;


            ShipSizeConverter WidthConverter = new ShipSizeConverter(ship.length, true);
            ShipSizeConverter HeightConverter = new ShipSizeConverter(ship.length, false);

            Binding HeightBind = new Binding("Height");
            Binding WidthBind = new Binding("Height");

            HeightBind.Source = gridBoard;
            HeightBind.Converter = HeightConverter;
            WidthBind.Source = gridBoard;
            WidthBind.Converter = WidthConverter;

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = new BitmapImage(new Uri("../../../ship.jpg", UriKind.Relative));

            Image shipVis = new Image
            {
                Source = imgBrush.ImageSource,
                Stretch = Stretch.Fill,
                Tag = ship
            };

            int nearestRow = ship.row;
            int nearestColumn = ship.column;

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
                shipVis.Width = gridBoard.ActualWidth / 11 * ship.length;
                shipVis.Height = gridBoard.ActualHeight / 11;
            }
            else
            {
                shipVis.Width = gridBoard.ActualWidth / 11;
                shipVis.Height = gridBoard.ActualHeight / 11 * ship.length;
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

                    int col = nearestColumn + j * (shipSize + 1);
                    int row = nearestRow + i * 2;

                    ships[i][j] = new Ship(shipSize, row, col, row, col, true);
                    AddShip(gridBoard, canvas, ships[i][j]);
                }
            }
        }
        private void RotateShip(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && sender is Image shipImage && !isDrag)
            {
                Ship ship = (Ship)shipImage.Tag;

                changeStateUnderShip(ship, ship.row, ship.column, 0);

                if (!willShipFit(ship, ship.row, ship.column, !ship.isHorizontal)) {
                    changeStateUnderShip(ship, ship.row, ship.column, 1);
                    return;
                }


                double temp = shipImage.Width;
                shipImage.Width = shipImage.Height;
                shipImage.Height = temp;
                ship.isHorizontal = !ship.isHorizontal;
               

                changeStateUnderShip(ship, ship.row, ship.column, 1);

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

                Ship shit = (Ship)currentShip.Tag;

                if (!(shit.column > 10 || shit.row > 10)) {
                    changeStateUnderShip(shit, shit.row, shit.column, 0);
                }

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

                int nearestRow = getClosestCoord(currentTop);
                int nearestColumn = getClosestCoord(currentLeft);

                Binding topBinding = new Binding("ActualHeight") {
                    Source = BoardGrid,
                    Converter = new PositionConverter(nearestRow, true)
                };
                Binding leftBinding = new Binding("ActualWidth") {
                    Source = BoardGrid,
                    Converter = new PositionConverter(nearestColumn, false)
                };

                Ship shit = (Ship)currentShip.Tag;

                
                bool isShipFit = willShipFit(shit, nearestRow, nearestColumn, shit.isHorizontal);

                if (!isShipFit) {
                    topBinding.Converter = new PositionConverter(shit.startingRow, true);
                    leftBinding.Converter = new PositionConverter(shit.startingColumn, false);

                    currentShip.SetBinding(Canvas.TopProperty, topBinding);
                    currentShip.SetBinding(Canvas.LeftProperty, leftBinding);

                } else {
                    changeStateUnderShip(shit, nearestRow, nearestColumn, 1);

                    currentShip.SetBinding(Canvas.TopProperty, topBinding);
                    currentShip.SetBinding(Canvas.LeftProperty, leftBinding);

                    shit.column = nearestColumn;
                    shit.row = nearestRow;
                }

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



        private int getClosestCoord(double position) {
            return (int)Math.Round(position / (BoardGrid.ActualHeight / 11));
        }

        private bool willShipFit(Ship shit, int nearestRow, int nearestColumn, bool isHorizontal) {
            bool isRowOutOfBounds = nearestRow < 1 || nearestRow > 10;
            bool isColOutOfBounds = nearestColumn < 1 || nearestColumn > 10;
            bool isHorizontalyFit = nearestColumn + shit.length - 1 <= 10 && isHorizontal;
            bool isVerticalyFit = nearestRow + shit.length - 1 <= 10 && isHorizontal == false;

            int startingPoint = isHorizontal ? nearestColumn : nearestRow;

            if (isRowOutOfBounds || isColOutOfBounds || (isHorizontalyFit == false && isVerticalyFit == false)) {
                return false;
            }

            for (int i = startingPoint; i < startingPoint + shit.length; i++) {
                if (board[nearestRow][nearestColumn] == 0) {
                    int leftBorder = nearestColumn - 1,
                        rightBorder = leftBorder + 3,
                        topBorder = nearestRow - 1,
                        bottomBorder = topBorder + 3;

                    if (isHorizontal) {
                        leftBorder += i - startingPoint;
                        rightBorder += i - startingPoint;
                    } else {
                        topBorder += i - startingPoint;
                        bottomBorder += i - startingPoint;
                    }

                    for (int j = topBorder; j < bottomBorder; j++) {
                        for (int k = leftBorder; k < rightBorder; k++) {
                            if (j >= 0 && k >= 0 && j <= 10 && k <= 10) {
                                if (board[j][k] != 0) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private void changeStateUnderShip(Ship shit, int row, int column, int state) {
            for (int i = 0; i < shit.length; i++) {
                if (shit.isHorizontal) {
                    board[row][column + i] = state;
                } else {
                    board[row + i][column] = state;
                }
            }
        }

    }
}
