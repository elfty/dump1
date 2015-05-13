using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpfexample
{
    /// <summary>
    /// Interaction logic for ByMaturity.xaml
    /// </summary>
    public partial class ByMaturity : Window
    {
        public ByMaturity()
        {
            InitializeComponent();
        }

        public List<PortByMat> _portbymatdisp;

        public ByMaturity(List<PortByMat> portbymat)
        {
            InitializeComponent();
            _portbymatdisp = portbymat;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            dataGrid1.ItemsSource = _portbymatdisp;
            ICollectionView view = CollectionViewSource.GetDefaultView(_portbymatdisp);
            view.SortDescriptions.Clear();
            SortDescription sd = new SortDescription("dt_mat", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);

            //dataGrid1.Items.Refresh();
        }
    }
}
