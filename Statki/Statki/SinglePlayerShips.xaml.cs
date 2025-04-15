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
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Diagnostics;

namespace Statki
{
    public partial class SinglePlayerShips : Window
    {
        private Stopwatch gameTimer = new Stopwatch();
        int[][] UserBoard;
        int[][] Opponentboard;
        bool isDrag = false;
        double top, left;
        System.Windows.Point dragStartPosition;
        Image currentShip;
        Ship[][] ships = [[new Ship(), new Ship(), new Ship(), new Ship()], [new Ship(), new Ship(), new Ship()], [new Ship(), new Ship()], [new Ship()]];
        private List<(string Move, bool Hit)> Player1moves = new List<(string, bool)>();
        private List<(string Move, bool Hit)> Player2moves = new List<(string, bool)>();


        public SinglePlayerShips(int level)
        {
            InitializeComponent();
            window.StateChanged += SizeChanged;
            gameTimer.Start();

            Grid Board = BoardGrid;
            Grid Board2 = BoardGrid2;
            Canvas leftCanvas = CanvasLeft;

            CanvasLeft.MouseMove += MoveShip;

            GenerateAndDisplayBoard(ref UserBoard, Board);
            GenerateAndDisplayBoard(ref Opponentboard, Board2);

            Board.Loaded += (s, e) => AddShips(Board, CanvasLeft, ref UserBoard, false, false);
            Board2.Loaded += (s, e) => AddShips(Board, CanvasRight, ref Opponentboard, true, true);

        }
        private void SizeChanged(object sender, EventArgs e)
        {
            double height = System.Windows.SystemParameters.PrimaryScreenHeight;
            double width = System.Windows.SystemParameters.PrimaryScreenWidth;
            window.Height = height;
            window.Width = width;
        }


        public void GenerateAndDisplayBoard(ref int[][] board, Grid Board)
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

            DisplayBoard(ref board, Board);
        }


        public void DisplayBoard(ref int[][] board, Grid Board)
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
                        imgBrush.ImageSource = new BitmapImage(new Uri("../../../Images/water.png", UriKind.Relative));
                        btn.Background = imgBrush;
                    }

                    Board.Children.Add(btn);
                }
            }
        }


        private void AddShip(Grid gridBoard, Canvas canvas, Ship ship, bool isEnemy)
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
            imgBrush.ImageSource = new BitmapImage(new Uri(ship.isHorizontal? "../../../Images/ship1.png" : "../../../Images/ship2.png", UriKind.Relative));

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
                shipVis.SetBinding(WidthProperty, WidthBind);
                shipVis.SetBinding(HeightProperty, HeightBind);
            }
            else
            {
                shipVis.SetBinding(WidthProperty, HeightBind);
                shipVis.SetBinding(HeightProperty, WidthBind);
            }

            if (!isEnemy) {
                shipVis.MouseLeftButtonDown += startDragging;
                shipVis.MouseLeftButtonUp += stopDragging;
                shipVis.MouseRightButtonDown += RotateShip;
            } else {
                shipVis.Visibility = Visibility.Hidden;
                //Dodać funkcję shoot tu
            }


            canvas.Children.Add(shipVis);
        }


        private void AddShips(Grid gridBoard, Canvas canvas, ref int[][] board, bool isRandomized, bool isEnemy)
        {
            int[][] shipsSizes = [[1, 1, 1, 1], [2, 2, 2], [3, 3], [4]];

            Random rand = new Random();

            for (int i = 0; i < shipsSizes.Length; i++)
            {
                for (int j = 0; j < shipsSizes[i].Length; j++)
                {
                    int shipSize = shipsSizes[i][j];

                    if (isRandomized) {

                        bool placed = false;

                        while (!placed) {
                            int desiredRow = rand.Next(1, 11);
                            int desiredCol = rand.Next(1, 11);
                            bool horizontal = Convert.ToBoolean(rand.Next(0, 2));

                            Ship ship = new Ship(shipSize, desiredRow, desiredCol, desiredRow, desiredCol, horizontal);

                            if (willShipFit(ref board, ship, desiredRow, desiredCol, horizontal)) {
                                changeStateUnderShip(ref board, ship, desiredRow, desiredCol, 1);
                                ships[i][j] = ship;
                                AddShip(gridBoard, canvas, ship, isEnemy);
                                placed = true;
                            }
                        }
                    } else {

                        double currentSize = gridBoard.Height / 11;

                        double x = currentSize;
                        double y = shipSize + gridBoard.Height / 11 * 12;

                        int nearestRow = (int)Math.Round(y / (BoardGrid.ActualHeight / 11));
                        int nearestColumn = (int)Math.Round(x / (BoardGrid.ActualWidth / 11));

                        int col = nearestColumn + j * (shipSize + 1);
                        int row = nearestRow + i * 2;

                        ships[i][j] = new Ship(shipSize, row, col, row, col, true);
                        AddShip(gridBoard, canvas, ships[i][j], isEnemy);
                    }
                }
            }
        }
        private void RotateShip(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && sender is Image shipImage && !isDrag) {
                Ship ship = (Ship)shipImage.Tag;

                if (!isOutOfBounds(ship.row, ship.column)) { 
                    changeStateUnderShip(ref UserBoard, ship, ship.row, ship.column, 0);
                    if (willShipFit(ref UserBoard, ship, ship.row, ship.column, !ship.isHorizontal)) {
                        ship.isHorizontal = !ship.isHorizontal;
                    }
                    changeStateUnderShip(ref UserBoard, ship, ship.row, ship.column, 1);
                }

                double temp = shipImage.Width;
                shipImage.Width = shipImage.Height;
                shipImage.Height = temp;

                if (isOutOfBounds(ship.row, ship.column)) {
                    ship.isHorizontal = !ship.isHorizontal;
                }

                if (ship.isHorizontal) {
                    shipImage.Source = new BitmapImage(new Uri("../../../Images/ship1.png", UriKind.Relative));
                } else {
                    shipImage.Source = new BitmapImage(new Uri("../../../Images/ship2.png", UriKind.Relative));
                }

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

        private void startDragging(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image ship)
            {
                currentShip = ship;
                top = Canvas.GetTop(currentShip);
                left = Canvas.GetLeft(currentShip);

                Ship shit = (Ship)currentShip.Tag;

                if (!(shit.column > 10 || shit.row > 10)) {
                    changeStateUnderShip(ref UserBoard, shit, shit.row, shit.column, 0);
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

                
                bool isShipFit = willShipFit(ref UserBoard, shit, nearestRow, nearestColumn, shit.isHorizontal);


                if (!isShipFit) {
                    
                    topBinding.Converter = new PositionConverter(shit.startingRow, true);
                    leftBinding.Converter = new PositionConverter(shit.startingColumn, false);

                    currentShip.SetBinding(Canvas.TopProperty, topBinding);
                    currentShip.SetBinding(Canvas.LeftProperty, leftBinding);

                } else {
                    changeStateUnderShip(ref UserBoard, shit, nearestRow, nearestColumn, 1);

                    currentShip.SetBinding(Canvas.TopProperty, topBinding);
                    currentShip.SetBinding(Canvas.LeftProperty, leftBinding);

                }

                shit.column = nearestColumn;
                shit.row = nearestRow;
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

        private bool willShipFit(ref int[][] board, Ship shit, int nearestRow, int nearestColumn, bool isHorizontal) {
            bool isHorizontalyFit = nearestColumn + shit.length - 1 <= 10 && isHorizontal;
            bool isVerticalyFit = nearestRow + shit.length - 1 <= 10 && isHorizontal == false;


            if (!(isHorizontalyFit ^ isVerticalyFit) || isOutOfBounds(nearestRow, nearestColumn)) {
                return false;
            }

            int startingPoint = isHorizontal ? nearestColumn : nearestRow;

            for (int i = startingPoint; i < startingPoint + shit.length; i++) {
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
                        if (j >= 1 && k >= 1 && j <= 10 && k <= 10) {
                            if (board[j][k] != 0) {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        private void changeStateUnderShip(ref int[][] board, Ship shit, int row, int column, int state) {
            for (int i = 0; i < shit.length; i++) {
                if (shit.isHorizontal) {
                    board[row][column + i] = state;
                } else {
                    board[row + i][column] = state;
                }
            }
        }

        private bool isOutOfBounds(int row, int column) {
            bool isRowOutOfBounds = row < 1 || row > 10;
            bool isColOutOfBounds = column < 1 || column > 10;

            return isRowOutOfBounds || isColOutOfBounds;
        }

        private bool AreAllShipsPlaced(ref int[][] board, int target)
        {
            int counter = 0;
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    if (board[i][j] == target)
                    {
                        counter += 1;
                    }
                }
            }
            if (counter == 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnReadyClicked(object sender, RoutedEventArgs e)
        {
            if (AreAllShipsPlaced(ref UserBoard, 1))
            {
                MessageBox.Show("Wszystkie statki poprawnie rozmieszczone!");
                StartGameButton.Visibility = Visibility.Hidden;

                foreach (UIElement child in CanvasLeft.Children)
                {
                    if (child is Image shipImage)
                    {
                        shipImage.MouseLeftButtonDown -= startDragging;
                        shipImage.MouseLeftButtonUp -= stopDragging;
                        shipImage.MouseRightButtonDown -= RotateShip;
                    }
                }

                CanvasLeft.MouseMove -= MoveShip;

                foreach (var child in BoardGrid2.Children)
                {
                    if (child is Button btn)
                    {
                        RoutedEventHandler shotHandler = (s, args) =>
                        {
                            HandleOpponentShot(btn);
                        };

                        btn.Click += shotHandler;

                        btn.Tag = shotHandler;
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie wszystkie statki są poprawnie rozmieszczone!");
            }
        }


        private void HandleOpponentShot(Button btn)
        {
            int gridRow = Grid.GetRow(btn);
            int gridColumn = Grid.GetColumn(btn);

            if (isOutOfBounds(gridRow, gridColumn))
            {
                MessageBox.Show("Strzał poza planszą!");
                return;
            }

            if (Opponentboard[gridRow][gridColumn] == 1)
            {
                Opponentboard[gridRow][gridColumn] = 2;
                btn.Background = new SolidColorBrush(Colors.Red);
                Player1moves.Add(($"{gridRow},{gridColumn}", true));

                if (IsShipSunk(gridRow, gridColumn))
                {
                    MessageBox.Show("Zatopiłeś statek!");
                    if (AreAllShipsPlaced(ref Opponentboard, 2))
                    {
                        MessageBox.Show("Wygrałeś!");
                        gameOver("Człowiek");
                    }
                }
                else
                {
                    MessageBox.Show("Trafiony!");
                }
            }
            else if (Opponentboard[gridRow][gridColumn] == 0)
            {
                Opponentboard[gridRow][gridColumn] = -1;
                btn.Background = new SolidColorBrush(Colors.Gray);
                Player1moves.Add(($"{gridRow},{gridColumn}", false));
                MessageBox.Show("Pudło!");
                return;
            }
            else
            {
                MessageBox.Show("Już strzelałeś w to miejsce!");
            }

            if (btn.Tag is RoutedEventHandler shotHandler)
            {
                btn.Click -= shotHandler;
            }
        }

        private string GetMovesAsString(bool who)
        {
            if (who)
            {
                var formattedMoves = Player1moves.Select(m => $"[{m.Move}, {m.Hit.ToString().ToLower()}]");
                return $"[{string.Join(", ", formattedMoves)}]";
            }
            else
            {
                var formattedMoves = Player2moves.Select(m => $"[{m.Move}, {m.Hit.ToString().ToLower()}]");
                return $"[{string.Join(", ", formattedMoves)}]";
            }
        }

        private void gameOver(string winner)
        {
            gameTimer.Stop();
            RemoveAllHandlers();
            string gameTime = gameTimer.Elapsed.ToString(@"hh\:mm\:ss");
            var connection = new DatabaseManager("localhost", "ships", "root", "");
            string Player1Moves = GetMovesAsString(true);
            string Player2Moves = GetMovesAsString(false);
            connection.ExecuteQuery($"INSERT INTO `shipgames`(`winner`, `player1_moves`, `player2_moves`, `game_time`) VALUES ('{winner}','{Player1Moves}','{Player2Moves}','{gameTime}');");
        }

        private void RemoveAllHandlers()
        {
            foreach (UIElement child in CanvasLeft.Children)
            {
                if (child is Image shipImage)
                {
                    shipImage.MouseLeftButtonDown -= startDragging;
                    shipImage.MouseLeftButtonUp -= stopDragging;
                    shipImage.MouseRightButtonDown -= RotateShip;
                }
            }

            CanvasLeft.MouseMove -= MoveShip;

            foreach (var child in BoardGrid2.Children)
            {
                if (child is Button btn && btn.Tag is RoutedEventHandler shotHandler)
                {
                    btn.Click -= shotHandler;
                    btn.Tag = null;
                }
            }
        }


        private bool IsShipSunk(int hitRow, int hitColumn)
        {
            foreach (var shipRow in ships)
            {
                foreach (var ship in shipRow)
                {
                    if (ship.isHorizontal)
                    {
                        if (hitRow == ship.row && hitColumn >= ship.column && hitColumn < ship.column + ship.length)
                        {
                            for (int i = 0; i < ship.length; i++)
                            {
                                if (Opponentboard[ship.row][ship.column + i] != 2)
                                {
                                    return false;
                                }
                            }
                            MarkSurroundingAsMiss(ship);
                            return true;
                        }
                    }
                    else
                    {
                        if (hitColumn == ship.column && hitRow >= ship.row && hitRow < ship.row + ship.length)
                        {
                            for (int i = 0; i < ship.length; i++)
                            {
                                if (Opponentboard[ship.row + i][ship.column] != 2)
                                {
                                    return false;
                                }
                            }
                            MarkSurroundingAsMiss(ship);
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private void MarkSurroundingAsMiss(Ship ship)
        {
            int startRow = ship.row - 1;
            int endRow = ship.isHorizontal ? ship.row + 1 : ship.row + ship.length;
            int startCol = ship.column - 1;
            int endCol = ship.isHorizontal ? ship.column + ship.length : ship.column + 1;

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startCol; j <= endCol; j++)
                {
                    if (i >= 1 && i <= 10 && j >= 1 && j <= 10 && Opponentboard[i][j] == 0)
                    {
                        Opponentboard[i][j] = -1;
                        Button btn = GetButtonFromGrid(BoardGrid2, i, j);
                        if (btn != null)
                        {
                            btn.Background = new SolidColorBrush(Colors.Gray);
                        }
                    }
                }
            }
        }

        private Button GetButtonFromGrid(Grid grid, int row, int column)
        {
            foreach (var child in grid.Children)
            {
                if (child is Button btn && Grid.GetRow(btn) == row && Grid.GetColumn(btn) == column)
                {
                    return btn;
                }
            }
            return null;
        }

    }
}
