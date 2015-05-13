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
    /// Interaction logic for BySect.xaml
    /// </summary>
    public partial class BySect : Window
    {
        public List<PortBySector> _portbysectdisp;

        public BySect(List<PortBySector> portbysect)
        {
            InitializeComponent();
            _portbysectdisp = portbysect;
        }

          

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            dataGrid1.ItemsSource = _portbysectdisp;
            ICollectionView view = CollectionViewSource.GetDefaultView(_portbysectdisp);
            view.SortDescriptions.Clear();
            SortDescription sd = new SortDescription("nm_gics_sector", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);

            //dataGrid1.Items.Refresh();
        }

    }
}
