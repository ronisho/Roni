using GamePlay.GameServiceRef;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace GamePlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameServiceClient client;
        ClientCallback callback;
        public MainWindow()
        {
            InitializeComponent();
            callback = new ClientCallback();
            client = new GameServiceClient(new InstanceContext(callback));
        }

        private void signInClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(name.Text) && !string.IsNullOrEmpty(pass.Password))
            {
                try
                {
                    client.SingIn(name.Text.Trim(), ConvertPass(pass.Password.Trim()));
                    MessageBox.Show("roni signIn");
                    WaitingForGame waitingForGame = new WaitingForGame(name.Text.Trim(), callback, client);
                    waitingForGame.Show();
                    this.Hide();
                }
                catch (FaultException<ConnectedFault> err)
                {
                    MessageBox.Show(err.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException<WrongPassword> err)
                {
                    MessageBox.Show(err.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException<UnregisteredUser> err)
                {
                    MessageBox.Show(err.Detail.Details, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + "Type:" + ex.GetType() + "\n" + ex.InnerException, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("User name or password missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
         //  Application.
        }


        private void registerClicked(object sender, RoutedEventArgs e)
        {
            Register reg = new Register(callback, client);
            reg.Show();
            this.Hide();
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
