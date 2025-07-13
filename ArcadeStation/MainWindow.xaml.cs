using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gomoku;
using tictactoe;


namespace ArcadeStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<GameInfo> Games { get; set; }
        private UIElement _gameSelectionMenu;
        private MediaPlayer player = new MediaPlayer();
        public MainWindow()
        {
            InitializeComponent();
            PlayMenu();
            _gameSelectionMenu = GameScreen.Child;
            LoadGames();
            // This is crucial for data binding to work
            this.DataContext = this;
        }

        private void PlayMenu()
        {
            // Define the URI for the music file
            var musicUri = new Uri("Resources/Music/Menu.mp3", UriKind.Relative);

            player.MediaFailed += (s, e) => {
                MessageBox.Show($"Media failed to load: {e.ErrorException.Message}", "Media Player Error");
            };

            player.MediaEnded += (s, e) => {
                player.Position = TimeSpan.Zero;
                player.Play();
            };

            // Open and play the sound
            player.Open(musicUri);
            player.Play();
        }

        private void LoadGames()
        {
            // Create the list of games. You can add more here later!
            Games = new List<GameInfo>
            {
                new GameInfo
                {
                    Name = "Gomoku",
                    ThumbnailPath = "/ArcadeStation;component/Resources/Thumbnails/gomoku_thumb.png",
                    GameUserControlType = typeof(GomokuView) // Store the Type of the game's UserControl
                },
                new GameInfo
                {
                    Name = "Tic-Tac-Toe",
                    ThumbnailPath = "/ArcadeStation;component/Resources/Thumbnails/tictactoe_thumb.png",
                    GameUserControlType = typeof(TicTacToeView)
                }
            };
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
            var selectedGame = GameListBox.SelectedItem as GameInfo;
            if (selectedGame == null) return;

            // Dynamically create an instance of the selected game's UserControl
            var gameControl = Activator.CreateInstance(selectedGame.GameUserControlType) as UserControl;

            if (gameControl == null)
            {
                MessageBox.Show("Error: The selected game could not be loaded.");
                return;
            }

            // --- THIS IS THE KEY CHANGE ---
            // 1. Convert the generic UserControl to a 'dynamic' type.
            dynamic game = gameControl;

            // 2. Now you can call the StartGame method.
            // The check will happen at RUNTIME instead of compile time.
            try
            {
                // Make sure every game you create has a public StartGame(bool) method!
                game.StartGame();
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                MessageBox.Show($"Error: The game '{selectedGame.Name}' does not have a 'StartGame' method.");
                return;
            }

            // Place the game on the screen
            GameScreen.Child = gameControl;

            // Show the Back button and hide the Play button
            BackButton.Visibility = Visibility.Visible;
            GameListBox.Visibility = Visibility.Collapsed;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore the game selection menu
            GameScreen.Child = _gameSelectionMenu;
            player.Play();
            // Show the Play button and hide the Back button
            BackButton.Visibility = Visibility.Collapsed;
            PlayButton.Visibility = Visibility.Visible;
            GameListBox.Visibility = Visibility.Visible; // Show the game list again
        }
    }
}