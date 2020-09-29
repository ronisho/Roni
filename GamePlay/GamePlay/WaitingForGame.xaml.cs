using GamePlay.GameServiceRef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for WaitingForGame.xaml
    /// </summary>
    public partial class WaitingForGame : Window
    {
        #region prop
        private string userName;
        private ClientCallback clientCallback;
        private GameServiceClient connectionToServer;
        private List<string> userList;
        #endregion prop

        public WaitingForGame(string name, ClientCallback clientCallback, GameServiceClient connectionToServer)
        {
            this.userName = name;
            this.clientCallback = clientCallback;
            this.connectionToServer = connectionToServer;
            InitializeComponent();
            userList = connectionToServer.GetAvliableClientsForUser(this.userName).Keys.ToList();
            listOfAvliablePlayers.Items.Clear();
            listOfAvliablePlayers.ItemsSource = null;
            listOfAvliablePlayers.ItemsSource = connectionToServer.GetAvliableClientsForUser(this.userName).Keys.ToList();
            usrName.Content = "Hello " + name;
            initDelegates();
        }

        private void initDelegates()
        {
            this.clientCallback.startGame += startWithAnotherPlayer;
            this.clientCallback.updateUserList += updateUserList;
            this.clientCallback.confirmGame += confirmGame;
        }

        internal bool confirmGame(string userToGame)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to play with " + userToGame + " ?", "Request Game" , MessageBoxButtons.YesNo);
            if (result.ToString() == "Yes")
            {
                return true;
            }
               
            return false;

        }

        public void updateUserList(string user,string action)
        {
            if (user.Equals(this.userName)) return;
            if (action.Equals("Add"))
            {
                userList.Add(user);
                listOfAvliablePlayers.ItemsSource = null;
                listOfAvliablePlayers.ItemsSource = userList;
                listOfAvliablePlayers.Items.Refresh();
            }
            else
            {
                userList.Remove(user);
                listOfAvliablePlayers.ItemsSource = null;
                listOfAvliablePlayers.ItemsSource = userList;
                listOfAvliablePlayers.Items.Refresh();
            }
   
        }

        private void startWithAnotherPlayer(string p1)
        {
            GameWindow newGame = new GameWindow(this.userName, p1, this.connectionToServer,clientCallback,this);
            this.userList.Remove(p1);
            listOfAvliablePlayers.ItemsSource = null;
            listOfAvliablePlayers.ItemsSource = userList;
            listOfAvliablePlayers.Items.Refresh();
            newGame.Show();
            this.Hide();
        }

        private void startGameClicked(object sender, RoutedEventArgs e)
        {

            string selectPlayer = listOfAvliablePlayers.SelectedItem.ToString();
            if (listOfAvliablePlayers.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Must pick a Rival to start new game", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bool result = this.connectionToServer.StartGame(this.userName, selectPlayer);
            if(result == true)
            {
                startWithAnotherPlayer(selectPlayer);
                connectionToServer.StartGameBetweenPlayers(this.userName, selectPlayer);
            }
            else
            {

            }
        }

        internal void imBack()
        {
            connectionToServer.PlayerRetrunToList(this.userName);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Thread t = new Thread(() => connectionToServer.Disconnect(this.userName));
            t.Start();
        }

        private void ClickLiveGame(object sender, RoutedEventArgs e)
        {
            List<string> gameList = connectionToServer.liveGamesList().ToList();
            if (gameList.Count == 0)
            {
                System.Windows.MessageBox.Show($"Sorry.. currently no live games", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LiveGame window = new LiveGame(gameList);
            window.ShowDialog();
        }

        private void ClickInfo(object sender, RoutedEventArgs e)
        {
            SearchInfo window = new SearchInfo(clientCallback, connectionToServer);
            window.ShowDialog();
        }

        private void ClickGamesHistory(object sender, RoutedEventArgs e)
        {
            List<string> historyList = connectionToServer.gamesHistory().ToList();
            if (historyList.Count == 0)
            {
                System.Windows.MessageBox.Show("No DATA available", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            GameList window = new GameList(historyList);
            window.ShowDialog();
        }

        private void ClickGamesHistoryUsers(object sender, RoutedEventArgs e)
        {
            List<string> usersList = connectionToServer.createPlayerData().ToList();
            if (usersList.Count == 0)
            {
                System.Windows.MessageBox.Show("No DATA available", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            HistoryBetween window = new HistoryBetween(usersList, clientCallback, connectionToServer);
            window.ShowDialog();
        }

        private void selectionUser(object sender, SelectionChangedEventArgs e)
        {
            if (listOfAvliablePlayers.SelectedItem != null)
            {
                string name = listOfAvliablePlayers.SelectedItem.ToString();
                var dataUser = connectionToServer.userData(name);
                tbName.Text = dataUser["User"];
                tbGame.Text = dataUser["Games"];
                tbWins.Text = dataUser["Wins"];
                tbLose.Text = dataUser["Losses"];
                tbPoint.Text = dataUser["Points"];

                float wins = float.Parse(dataUser["Wins"]);
                int games = int.Parse(dataUser["Games"]);

                if (games != 0)
                {
                    string per = (wins / games * 100).ToString();
                    if (per.Length > 4)
                        tbPer.Text = per.Substring(0, 4);
                    else tbPer.Text = per;
                }
                else
                {
                    tbPer.Text = "0";
                }
            }
            else
            {
                tbName.Clear();
                tbGame.Clear();
                tbWins.Clear();
                tbLose.Clear();
                tbPoint.Clear();
            }
        }
    }
}
