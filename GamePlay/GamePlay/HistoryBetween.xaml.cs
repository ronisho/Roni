using GamePlay.GameServiceRef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for HistoryBetween.xaml
    /// </summary>
    public partial class HistoryBetween : Window
    {
        ClientCallback callback;
        GameServiceClient client;
        public HistoryBetween(List<string> data, ClientCallback cc, GameServiceClient gsc)
        {
            InitializeComponent();
            callback = cc;
            client = gsc;
            player1ComboBox.ItemsSource = data;
            player2ComboBox.ItemsSource = data;
        }

        private void btnSearch_click(object sender, RoutedEventArgs e)
        {
            if (player1ComboBox.SelectedIndex == -1 || player2ComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Must select TWO players!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string player1 = player1ComboBox.SelectedItem.ToString();
            string player2 = player2ComboBox.SelectedItem.ToString();
            if (player1.Equals(player2))
            {
                MessageBox.Show("Must choose TWO different players!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<string> ret = client.gameDataBetween(player1, player2).ToList();
            if (ret.Count == 0)
            {
                lbSearchResults.ItemsSource = null;
                MessageBox.Show($"No match data found between {player1} and {player2}", "Game data info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                var p1wins = ret[0];
                var p2wins = ret[1];
                if (p1wins.Length > 4) p1wins = p1wins.Substring(0, 4);
                if (p2wins.Length > 4) p2wins = p2wins.Substring(0, 4);
                lbP1.Content = $"{player1} win {p1wins}%";
                lbP2.Content = $"{player2} win {p2wins}%";
                lbP1.Visibility = Visibility.Visible;
                lbP2.Visibility = Visibility.Visible;
                ret.RemoveAt(1);
                ret.RemoveAt(0);
                lbSearchResults.ItemsSource = ret;
            }
        }
    }
}
