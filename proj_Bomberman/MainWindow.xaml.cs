using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace proj_Bomberman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameController gameController;

        private Dictionary<string, bool> keyPressed;
        private bool keySpaceFirstPress;

        private DispatcherTimer input_tim;
        private int input_count;
        private int input_delay;

        public MainWindow()
        {
            InitializeComponent();

            RestartButton.Focusable = false;

            StartGame();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            //run inside ifs only when first press
            if (!keyPressed["Up"] && e.Key == Key.Up)
            {
                keyPressed["Up"] = true;
                keyPressed["Down"] = false;
                keyPressed["Left"] = false;
                keyPressed["Right"] = false;
                input_count = 0;
            } else if (!keyPressed["Down"] && e.Key == Key.Down)
            {
                keyPressed["Up"] = false;
                keyPressed["Down"] = true;
                keyPressed["Left"] = false;
                keyPressed["Right"] = false;
                input_count = 0;
            } else if (!keyPressed["Left"] && e.Key == Key.Left)
            {
                keyPressed["Up"] = false;
                keyPressed["Down"] = false;
                keyPressed["Left"] = true;
                keyPressed["Right"] = false;
                input_count = 0;
            } else if (!keyPressed["Right"] && e.Key == Key.Right)
            {
                keyPressed["Up"] = false;
                keyPressed["Down"] = false;
                keyPressed["Left"] = false;
                keyPressed["Right"] = true;
                input_count = 0;
            }

            if (!keyPressed["Space"] && e.Key == Key.Space)
            {
                keyPressed["Space"] = true;
                keySpaceFirstPress = true;
            }
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                keyPressed["Up"] = false;
            }
            if (e.Key == Key.Down)
            {
                keyPressed["Down"] = false;
            }
            if (e.Key == Key.Left)
            {
                keyPressed["Left"] = false;
            }
            if (e.Key == Key.Right)
            {
                keyPressed["Right"] = false;
            }
            if (e.Key == Key.Space)
            {
                keyPressed["Space"] = false;
                keySpaceFirstPress = false;
            }
        }

        private void MainWindow_PassInput_Tick(object? sender, EventArgs e)
        {
            if (input_count > 0)
            {
                input_count--;
            } else
            {
                gameController.ReadInput(keyPressed["Up"], keyPressed["Down"], keyPressed["Left"], keyPressed["Right"]);
                input_count = input_delay;
            }

            if (keySpaceFirstPress && keyPressed["Space"])
            {
                gameController.ReadInput(keyPressed["Space"]);
                keySpaceFirstPress = false;
            }
        }

        private void UpdatePlayerStats(Player obj)
        {
            MaxBomb.Content = obj.MaxBomb.ToString();
            BombRange.Content = obj.BombRange.ToString();
            KeyStatus.Content = obj.HasKey ? 1.ToString() : 0.ToString();
        }

        private void UpdateGameState(string result)
        {
            ProjectWindow.KeyUp -= MainWindow_OnKeyUp;
            ProjectWindow.KeyDown -= MainWindow_OnKeyDown;
            
            input_tim.Stop();

            GameStatus.Content = result;
            GameStatus.FontWeight = FontWeights.Bold;

            if (result == "You Win!!!")
                GameStatus.Foreground = Brushes.DarkGoldenrod;
            else
                GameStatus.Foreground = Brushes.Red;
        }

        private void StartGame()
        {
            gameController = new GameController(MainCanvas, 19, 19, 7, 9, UpdatePlayerStats, UpdateGameState);
            gameController.InitGame();

            keyPressed = new Dictionary<string, bool>();
            keyPressed["Up"] = false;
            keyPressed["Down"] = false;
            keyPressed["Left"] = false;
            keyPressed["Right"] = false;
            keyPressed["Space"] = false;
            keySpaceFirstPress = false;

            input_delay = 15;
            input_count = input_delay;

            input_tim = new DispatcherTimer();
            input_tim.Tick += new EventHandler(MainWindow_PassInput_Tick);
            input_tim.Interval = new TimeSpan(0, 0, 0, 0, 10);
            input_tim.Start();

            ProjectWindow.KeyUp += MainWindow_OnKeyUp;
            ProjectWindow.KeyDown += MainWindow_OnKeyDown;

            GameStatus.Content = "Playing...";
            GameStatus.FontWeight = FontWeights.Normal;
            GameStatus.Foreground = Brushes.Black;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateGameState("Restarting...");
            gameController.ClearAll();

            StartGame();
        }
    }
}
