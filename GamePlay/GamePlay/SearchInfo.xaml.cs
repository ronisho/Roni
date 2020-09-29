using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GamePlay.GameServiceRef;
using System.ServiceModel;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for SearchInfo.xaml
    /// </summary>
    public partial class SearchInfo : Window
    {
        #region prop
        List<string> options;
        ClientCallback callback;
        GameServiceClient client;
        #endregion prop
        public SearchInfo(ClientCallback cc, GameServiceClient gsc)
        {
            InitializeComponent();
            callback = cc;
            client = gsc;

            options = new List<string> { "Username", "Number of Games", "Number of Wins", "Number of Losses", "Number of Points" };
            sortingOpt.ItemsSource = options;
        }
        private void sortClicked(object sender, RoutedEventArgs e)
        {
            string sortBy = sortingOpt.SelectedItem.ToString();
            lbSortResults.ItemsSource = null;
            lbSortResults.ItemsSource = client.getSortedList(sortBy);
            return;
        }
    }

}
