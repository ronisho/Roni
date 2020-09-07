using GamePlay.GameServiceRef;
using System;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {

        public Register(ClientCallback cc, GameServiceClient gsc )
        {
            InitializeComponent();
            callback = cc;
            client = gsc;

        }
        ClientCallback callback;
        GameServiceClient client;

        private void signUpClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(name.Text) ||
               string.IsNullOrEmpty(pass.Password))
            {
                System.Windows.MessageBox.Show("Please fill all data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                string userName = name.Text.Trim();
                string password = ConvertPass(pass.Password.Trim());
                client.Register(userName, password);
                var list = client.GetAvliableClients(userName);
                WaitingForGame waitingForGame = new WaitingForGame(name.Text.Trim(), callback, client);
                waitingForGame.Show();
                this.Hide();
            }
            catch (FaultException<ConnectedFault> err)
            {
                System.Windows.MessageBox.Show(err.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\n" + "Type:" + ex.GetType() + "\n" + ex.InnerException, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private string ConvertPass(string pass)
        {
            using (SHA256 hashObj = SHA256.Create())
            {
                byte[] hashBytes = hashObj.ComputeHash(Encoding.UTF8.GetBytes(pass));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
