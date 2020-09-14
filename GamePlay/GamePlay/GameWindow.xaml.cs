using GamePlay.GameServiceRef;
using System;
using System.Collections.Generic;
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
using System.Drawing;
using System.Windows.Navigation;
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
        private string actualPlayer;
        private string selectedPlayer;
        private GameServiceClient gameServer;
        private ClientCallback clientCallback;
        private Dictionary<int, string> colMap = new Dictionary<int, string>();
        private Dictionary<int, string> rowMap = new Dictionary<int, string>();
        private static int ROW = 7;
        private static int COL = 7;
        private char[,] board;
        private WaitingForGame watingWindow;
        private char myChar = 'x';
        private char playerChar = 'y';
        private bool userExit = true;

        private double DISC_SIZE = 73;

        private double WIDTH_MARGIN = 0.08;
        private readonly static int HEIGHT_MARGIN = 8;
        private readonly static int BOTTOM_MARGIN = -1;
        private readonly int[] board_state = new int[ROW];

        public GameWindow(string userName, string selectPlayer, GameServiceClient connectionToServer, ClientCallback clientCallback)
        {
            this.actualPlayer = userName;
            this.selectedPlayer = selectPlayer;
            this.gameServer = connectionToServer;
            this.clientCallback = clientCallback;
            GameWindowManger.Instance.GameWindow = (this);
            this.watingWindow = GameWindowManger.Instance.WaitingForGameWindow;
            this.clientCallback.playerMove += playerMove;
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

        private void updateMyWindowAfterMove(int col)
        {
            int rowTodraw = ROW -1;
            for(int i = 1; i < ROW; i++)
            {
                if (this.board[i, col -1] != '\0')
                { 
                    rowTodraw = i -1;
                    
                    break;
                }
                   
            }
            this.board[rowTodraw, col -1] = myChar;
            string result = rowMap[rowTodraw +1] + colMap[col];
            System.Windows.Controls.Button l = (System.Windows.Controls.Button)this.FindName(result);
            l.Background = new SolidColorBrush(Colors.Yellow);


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
                    this.gameServer.PlayerRetrunToList(this.actualPlayer);
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
                Thread t = new Thread(GameWindowManger.Instance.WaitingForGameWindow.Close);
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


        private void updatefinishGameDB(string user1 , int point)
        {

        }
    }
}
