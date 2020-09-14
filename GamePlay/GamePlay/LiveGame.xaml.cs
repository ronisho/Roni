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
using System.Windows.Shapes;

namespace GamePlay
{
    /// <summary>
    /// Interaction logic for LiveGame.xaml
    /// </summary>
    public partial class LiveGame : Window
    {
        public LiveGame(List<string> data)
        {
            InitializeComponent();
            updateData(data);
        }

        private void updateData(List<string> data)
        {
            lbGameList.ItemsSource = null;
            lbGameList.ItemsSource = data;
        }
    }
}
