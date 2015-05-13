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
    /// Interaction logic for ByGicsInduView.xaml
    /// </summary>
    public partial class ByGicsInduView : Window
    {
        public List<PortByGicsIndu> _portbygicsindudisp;

        public ByGicsInduView(List<PortByGicsIndu> portbygicsindu)
        {
            InitializeComponent();
            _portbygicsindudisp = portbygicsindu;
        }

          

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            dataGrid1.ItemsSource = _portbygicsindudisp;
            ICollectionView view = CollectionViewSource.GetDefaultView(_portbygicsindudisp);
            view.SortDescriptions.Clear();
            SortDescription sd = new SortDescription("nm_gics_industry_sub", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);

            //dataGrid1.Items.Refresh();
        }
    }
}
