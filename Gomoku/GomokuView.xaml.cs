using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GomokuView : UserControl
    {
        public GomokuView()
        {
            InitializeComponent();
            Unloaded += OnGomokuViewUnloaded;
        }

        private const int GRID = 15;
        private const double CANVAS_SIZE = 460;
        private const double MARGIN = 20;           // 20px margin
        private const double StoneSize = 10;
        private readonly Brush GridColor = Brushes.SaddleBrown;
        private int counter = 0;
        private char[,] grid = new char[GRID, GRID];
        private MediaPlayer player = new MediaPlayer();
        private int p1score = 0, p2score = 0;
        private string tempAudioFilePath;

        public void StartGame()
        {
            stoneCanvas.Width = CANVAS_SIZE;
            stoneCanvas.Height = CANVAS_SIZE;
            FillGridCells();
            DrawGridLines();
            AddClickAreas();
            PlayBackground(); // This will now work because its context is established.
        }

        private void PlayBackground()
        {
            // This is the correct Pack URI for finding the resource inside the DLL.
            var resourceUri = new Uri("/Gomoku;component/Resources/background.mp3", UriKind.Relative);

            try
            {
                var streamInfo = Application.GetResourceStream(resourceUri);
                if (streamInfo == null)
                {
                    MessageBox.Show("Could not find the resource stream.");
                    return;
                }

                string tempDir = System.IO.Path.GetTempPath();
                // Use a unique name to avoid conflicts if multiple copies run.
                string tempFileName = "gomoku_" + Guid.NewGuid().ToString() + ".mp3";
                this.tempAudioFilePath = System.IO.Path.Combine(tempDir, tempFileName);

                using (var resourceStream = streamInfo.Stream)
                using (var fileStream = new FileStream(this.tempAudioFilePath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
                player.MediaOpened += (sender, e) => {
                    player.Play();
                    player.Volume = 0.7;
                };
                player.MediaFailed += (sender, e) => {
                    MessageBox.Show($"Media Failed with .mp3 extension: {e.ErrorException.Message}");
                };
                player.MediaEnded += (s, e) => {
                    player.Position = TimeSpan.Zero; // Loop
                    player.Play();
                    player.Volume = 0.7;
                };

                player.Open(new Uri(this.tempAudioFilePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while setting up the audio: {ex.Message}");
            }
        }

        // This method will run when the control is removed from the screen.
        private void OnGomokuViewUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop and close the player to release any file locks.
            player.Stop();
            player.Close();

            // Clean up the temporary file we created.
            if (!string.IsNullOrEmpty(this.tempAudioFilePath) && File.Exists(this.tempAudioFilePath))
            {
                try
                {
                    File.Delete(this.tempAudioFilePath);
                }
                catch
                {
                    // Ignore errors; the OS will clean the temp dir eventually.
                }
            }
        }

        private void FillGridCells()
        {
            double step = (CANVAS_SIZE - 2 * MARGIN) / (GRID - 1);

            for (int row = 0; row < GRID - 1; row++)
            {
                for (int col = 0; col < GRID - 1; col++)
                {
                    double x = MARGIN + col * step;
                    double y = MARGIN + row * step;

                    var cell = new Rectangle
                    {
                        Width = step,
                        Height = step,
                        Fill = Brushes.BurlyWood
                    };

                    Canvas.SetLeft(cell, x);
                    Canvas.SetTop(cell, y);
                    stoneCanvas.Children.Add(cell);
                }
            }
        }


        private void DrawGridLines()
        {
            double step = (CANVAS_SIZE - 2 * MARGIN) / (GRID - 1);

            // Vertical lines
            for (int i = 0; i < GRID; i++)
            {
                double x = MARGIN + i * step;
                var line = new Line
                {
                    X1 = x,
                    Y1 = MARGIN,
                    X2 = x,
                    Y2 = CANVAS_SIZE - MARGIN,
                    Stroke = GridColor,
                    StrokeThickness = 1
                };
                stoneCanvas.Children.Add(line);
            }

            // Horizontal lines
            for (int i = 0; i < GRID; i++)
            {
                double y = MARGIN + i * step;
                var line = new Line
                {
                    X1 = MARGIN,
                    Y1 = y,
                    X2 = CANVAS_SIZE - MARGIN,
                    Y2 = y,
                    Stroke = GridColor,
                    StrokeThickness = 1
                };
                stoneCanvas.Children.Add(line);
            }
        }

        private void AddClickAreas()
        {
            double step = (CANVAS_SIZE - 2 * MARGIN) / (GRID - 1);

            for (int row = 0; row < GRID; row++)
            {
                for (int col = 0; col < GRID; col++)
                {
                    var clickArea = new Ellipse
                    {
                        Width = StoneSize,
                        Height = StoneSize,
                        Fill = Brushes.Transparent,
                        Cursor = Cursors.Hand,
                        Tag = new Point(col, row),
                    };

                    clickArea.MouseLeftButtonDown += OnGridButtonClick;

                    double x = MARGIN + col * step - StoneSize / 2;
                    double y = MARGIN + row * step - StoneSize / 2;

                    Canvas.SetLeft(clickArea, x);
                    Canvas.SetTop(clickArea, y);
                    stoneCanvas.Children.Add(clickArea);
                }
            }
        }

        private void OnGridButtonClick(object sender, RoutedEventArgs e)
        {
            Ellipse btn = sender as Ellipse;
            Point coords = (Point)btn.Tag;
            int col = (int)coords.X;
            int row = (int)coords.Y;

            // Check if a stone is already placed here
            if (grid[row, col] != '\0')
            {
                MessageBox.Show("This spot is already taken!");
                return;
            }
            char stone;
            if (counter % 2 == 0)
            {
                btn.Fill = Brushes.Black;
                stone = 'b';
            }
            else
            {
                btn.Fill = Brushes.White;
                stone = 'w';
            }
            grid[row, col] = stone;
            counter++;

            if (Check(grid, stone))
            {
                string winner = stone == 'b' ? "1" : "2";
                MessageBox.Show($"Player {winner} won");
                if (winner == "1")
                {
                    p1score++;
                }
                else
                {
                    p2score++;
                }
            }
        }

        private bool Check(char[,] board, char symbol)
        {
            for (int row = 0; row < GRID; row++)
            {
                for (int col = 0; col < GRID; col++)
                {
                    if (board[row, col] != symbol)
                        continue;

                    // Check all 4 directions from current point
                    if (CountConsecutive(board, row, col, 0, 1, symbol) >= 5) return true; // →
                    if (CountConsecutive(board, row, col, 1, 0, symbol) >= 5) return true; // ↓
                    if (CountConsecutive(board, row, col, 1, 1, symbol) >= 5) return true; // ↘
                    if (CountConsecutive(board, row, col, 1, -1, symbol) >= 5) return true; // ↙
                }
            }
            return false;
        }

        private int CountConsecutive(char[,] board, int row, int col, int dRow, int dCol, char symbol)
        {
            int count = 1;

            // Forward direction
            int r = row + dRow;
            int c = col + dCol;
            while (IsInside(r, c) && board[r, c] == symbol)
            {
                count++;
                r += dRow;
                c += dCol;
            }

            // Backward direction
            r = row - dRow;
            c = col - dCol;
            while (IsInside(r, c) && board[r, c] == symbol)
            {
                count++;
                r -= dRow;
                c -= dCol;
            }

            return count;
        }

        private bool IsInside(int row, int col)
        {
            return row >= 0 && row < GRID && col >= 0 && col < GRID;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            // Reset the grid
            grid = new char[GRID, GRID];

            // Reset all Ellipses back to transparent
            foreach (var child in stoneCanvas.Children)
            {
                if (child is Ellipse ellipse && ellipse.Tag is Point)
                {
                    ellipse.Fill = Brushes.Transparent;
                }
            }

            // Reset the game round
            counter = 0;

            p1Score.Text = $"{p1score}";
            p2Score.Text = $"{p2score}";
        }
    }
}
