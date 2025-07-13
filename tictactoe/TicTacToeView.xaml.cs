using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace tictactoe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TicTacToeView : UserControl
    {
        private int row, col;
        private int clicked;
        char[,] nums = { { ' ', ' ', ' ' }, { ' ', ' ', ' ' }, { ' ', ' ', ' ' } };

        public TicTacToeView()
        {
            InitializeComponent();
        }

        public void StartGame()
        {
            Reset();
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            string name = bt.Name;

            // Determine which cell
            row = (int)Char.GetNumericValue(name[1]);
            col = (int)Char.GetNumericValue(name[2]);

            // Alternate turns based on clicks
            char symbol = (clicked % 2 == 0) ? 'O' : 'X';
            Brush color = (clicked % 2 == 0) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
            // Update UI and board
            bt.Content = symbol;
            bt.Foreground = color;
            bt.FontSize = 24;
            nums[row, col] = symbol;
            bt.IsEnabled = false;
            clicked++;

            // Check for win
            if (Check(nums, symbol))
            {
                MessageBox.Show($"Player {(symbol == 'O' ? "1" : "2")} won!");
                Reset();
            }

            // Check for draw
            if (clicked == 9)
            {
                MessageBox.Show("DRAW");
                Reset();
            }
        }


        private bool Check(char[,] nums, char symbol)
        {
            // Check rows and columns
            for (int i = 0; i < 3; i++)
            {
                if ((nums[i, 0] == symbol && nums[i, 1] == symbol && nums[i, 2] == symbol) ||
                    (nums[0, i] == symbol && nums[1, i] == symbol && nums[2, i] == symbol))
                {
                    return true;
                }
            }

            // Check diagonals
            if ((nums[0, 0] == symbol && nums[1, 1] == symbol && nums[2, 2] == symbol) ||
                (nums[0, 2] == symbol && nums[1, 1] == symbol && nums[2, 0] == symbol))
            {
                return true;
            }

            return false;
        }

        private void Reset()
        {
            // Reset the board array
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nums[i, j] = ' ';
                }
            }

            // Reset the UI buttons
            foreach (var child in GameGrid.Children)
            {
                if (child is Border border && border.Child is Button btn)
                {
                    btn.Content = "";
                    btn.IsEnabled = true;
                }
            }

            // Reset the move counter
            clicked = 0;
        }
    }
}
