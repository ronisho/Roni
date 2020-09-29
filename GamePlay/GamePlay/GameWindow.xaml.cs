using GamePlay.GameServiceRef;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Shapes;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        #region prop
        private string actualPlayer;
        private string selectedPlayer;
        private GameServiceClient gameServer;
        private ClientCallback clientCallback;
        private Dictionary<int, string> colMap = new Dictionary<int, string>();
        private Dictionary<int, string> rowMap = new Dictionary<int, string>();
        private static int ROW = 7;
        private static int COL = 7;
        private char[,] board;
        private readonly WaitingForGame watingWindow;
        private char myChar = 'x';
        private bool userExit = true;
        private readonly int[] board_state = new int[ROW];
        #endregion prop
        #region disc sizes
        private double DISC_SIZE = 73;
        private double WIDTH_MARGIN = 0.08;
        private readonly static int HEIGHT_MARGIN = 8;
        private readonly static int BOTTOM_MARGIN = -1;
        #endregion disc sizes
     
        public GameWindow(string userName, string selectPlayer, GameServiceClient connectionToServer, ClientCallback clientCallback, WaitingForGame waitingForGame)
        {
            this.actualPlayer = userName;
            this.selectedPlayer = selectPlayer;
            this.gameServer = connectionToServer;
            this.clientCallback = clientCallback;
            this.clientCallback.playerMove += playerMove;
            this.watingWindow = waitingForGame;
            this.board = new char[ROW, COL];
            initBoard();
            initMaps();
            InitializeComponent();
            pvsp.Content = userName + " VS " + selectPlayer;
        }

        private void initBoard()
        {
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    this.board[i, j] = '\0';
                }
            }
        }

        private void initMaps()
        { 
            rowMap.Add(1, "r1");
            rowMap.Add(2, "r2");
            rowMap.Add(3, "r3");
            rowMap.Add(4, "r4");
            rowMap.Add(5, "r5");
            rowMap.Add(6, "r6");
            rowMap.Add(7, "r7");
            colMap.Add(1, "c1");
            colMap.Add(2, "c2");
            colMap.Add(3, "c3");
            colMap.Add(4, "c4");
            colMap.Add(5, "c5");
            colMap.Add(6, "c6");
        }

        private void handleMove(System.Windows.Point p, int col)
        {
            MoveResult moveResult = this.gameServer.ReportMove(col + 1, this.actualPlayer, p);
            if (moveResult == MoveResult.NotYourTurn)
            {
                System.Windows.MessageBox.Show("Its not your turn!");
                return;
            }

            else if (moveResult == MoveResult.UnlegalMove)
            {
                System.Windows.MessageBox.Show("Unlegal move try again!");
                return;
            }

            else if (moveResult == MoveResult.YouWon)
            {
                this.draw(p, col, System.Windows.Media.Brushes.Green);
                DialogResult result = System.Windows.Forms.MessageBox.Show("You win!Do you want to play again?", "Win message", MessageBoxButtons.YesNo);
                if (result.ToString() == "Yes")
                {
                    this.watingWindow.Show();
                    Thread t = new Thread(watingWindow.imBack);
                    t.Start();
                    this.userExit = false;
                    this.Close();
                }
                else
                {
                    this.watingWindow.Close();
                    this.Close();
                }
                return;
            }
            this.draw(p, col, System.Windows.Media.Brushes.Green);
        }

        private void draw(System.Windows.Point p, int col, SolidColorBrush color)
        {
            Ellipse el = new Ellipse
            {
                Fill = color,
                Height = DISC_SIZE,
                Width = DISC_SIZE
            };
            DiscGame newDisc = new DiscGame
            {
                Circle = el,
                Y = p.Y - el.Height / 2,
                Column = col,
                X = (col + WIDTH_MARGIN) / COL * gamePicture.ActualWidth
            };
            board_state[newDisc.Column]++;

            Canvas.SetTop(el, newDisc.Y);
            Canvas.SetLeft(el, newDisc.X);
            gamePicture.Children.Add(el);
            ThreadPool.QueueUserWorkItem(DisplayDisc, newDisc);
        }

        private void DisplayDisc(object disc)
        {
            DiscGame b = disc as DiscGame;
            while (true)
            {
                if (YOut(b))
                {
                    break;
                }
                Thread.Sleep(2);
                b.Y += 1;
                Dispatcher.Invoke(() =>
                {
                    Canvas.SetTop(b.Circle, b.Y);
                    Canvas.SetLeft(b.Circle, b.X);
                });
            }
        }

        private bool YOut(DiscGame d)
        {
            bool result = false;
            Dispatcher.Invoke(() =>
            {
                result = d.Y > gamePicture.ActualHeight - ((DISC_SIZE + HEIGHT_MARGIN) * board_state[d.Column]) + BOTTOM_MARGIN || d.Y < 0;
            });
            return result;
        }

        internal void playerMove(MoveResult moveResult, int row, int col, System.Windows.Point p)
        {

            if (moveResult == MoveResult.YouLose)
            {
                DialogResult resultDialog = System.Windows.Forms.MessageBox.Show("You lose!Do you want to play again?", "Lose message", MessageBoxButtons.YesNo);
                if (resultDialog.ToString() == "Yes")
                {
                    this.watingWindow.Show();
                    Thread t = new Thread(watingWindow.imBack);
                    t.Start();
                    //this.gameServer.PlayerRetrunToList(this.actualPlayer);
                    this.userExit = false;
                    this.Close();
                }
                else
                {
                    this.watingWindow.Close();
                    this.Close();
                }
                return;
            }
            this.draw(p, col, System.Windows.Media.Brushes.Red);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (userExit)
            {
                Thread t = new Thread(this.watingWindow.Close);
                t.Start();
            }

          
        }

        private void gamePicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(gamePicture);
            int col = (int)Math.Floor(p.X / gamePicture.ActualWidth * COL);
            if (!isInWIndowRange(p))
                return;
            handleMove(p, col);
        }

        private bool isInWIndowRange(System.Windows.Point p)
        {
            bool result = false;
            Dispatcher.Invoke(() =>
            {
                result = p.X > 0 &&
                p.X < gamePicture.ActualWidth &&
                p.Y > 0 &&
                p.Y < gamePicture.ActualHeight;
            });
            return result;
        }

    }
}
