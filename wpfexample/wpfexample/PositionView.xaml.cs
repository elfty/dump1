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
    /// Interaction logic for PositionView.xaml
    /// </summary>
    public partial class PositionView : Window
    {
        List<PositionDisplay> _posdisp;
        public PositionView()
        {
            InitializeComponent();
        }

        public PositionView(List<PositionDisplay> posdisp)
        {
            InitializeComponent();
            _posdisp = posdisp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid1.ItemsSource = _posdisp;
            ICollectionView view = CollectionViewSource.GetDefaultView(_posdisp);
            view.SortDescriptions.Clear();
            SortDescription sd = new SortDescription("id_typ_imnt", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);
            sd = new SortDescription("dt_mat", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);
            sd = new SortDescription("pr_strike", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);
            sd = new SortDescription("id_pc", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sd);
            //dataGrid1.Items.Refresh();
        }
    }
}
