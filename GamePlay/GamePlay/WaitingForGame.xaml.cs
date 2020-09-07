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
        private string userName;

        private ClientCallback clientCallback;
        private GameServiceClient connectionToServer;
        public delegate void UpdateUsers();
        public event UpdateUsers updateUsers;
        private List<string> userList;
        private bool deleteWindowRefrence = false;

        public WaitingForGame(string name, ClientCallback clientCallback, GameServiceClient connectionToServer)
        {
            this.userName = name;
            this.clientCallback = clientCallback;
            this.connectionToServer = connectionToServer;
            InitializeComponent();
            userList = connectionToServer.GetAvliableClientsForUser(this.userName).Keys.ToList();
            listOfAvliablePlayers.ItemsSource = connectionToServer.GetAvliableClientsForUser(this.userName).Keys.ToList();
            usrName.Content = "Hello " + name;
            initDelegates();
            GameWindowManger.Instance.WaitingForGameWindow = (this);
        }

        private void initDelegates()
        {
            this.clientCallback.startGame += startWithAnotherPlayer;
            this.clientCallback.updateUserList += updateUserList;
            this.clientCallback.confirmGame += confirmGame;
        }

        private void btnRefreshRivals_Click(object sender, RoutedEventArgs e)
        {

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

        private void windowClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connectionToServer.Disconnect(this.userName);
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
            GameWindow newGame = new GameWindow(this.userName, p1, this.connectionToServer,clientCallback);
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
    }
}
