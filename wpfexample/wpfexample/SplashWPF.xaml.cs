using System;
using System.Collections.Generic;
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
using System.ComponentModel;

namespace wpfexample
{
    /// <summary>
    /// Interaction logic for SplashWPF.xaml
    /// </summary>
    public partial class SplashWPF : Window
    {

        private BackgroundWorker backgroundWorker_startup = new BackgroundWorker();

        public SplashWPF()
        {
            InitializeComponent();
            Main();
        }

        public void Main()
        {
            backgroundWorker_startup.WorkerSupportsCancellation = true;
            backgroundWorker_startup.WorkerReportsProgress = true;
            backgroundWorker_startup.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_startup_RunWorkerCompleted);
            backgroundWorker_startup.DoWork += backgroundWorker_startup_DoWork;
            backgroundWorker_startup.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_startup_ProgressChanged);
            backgroundWorker_startup.RunWorkerAsync();

        }


        private void backgroundWorker_startup_DoWork(object sender, DoWorkEventArgs e)
        {
            var newWindow = new MainWindow();
            newWindow.Show();
        }

        private void backgroundWorker_startup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker_startup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}
