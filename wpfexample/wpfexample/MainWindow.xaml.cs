using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Threading;
using System.ComponentModel;
using Bloomberglp.Blpapi.Examples;
using System.Collections.ObjectModel;
using Microsoft.Office.Core;

namespace wpfexample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBHelper _dbhelper = new DBHelper();
        GetPosition _getposhelper = new GetPosition();
        System.Timers.Timer _recalcprtftimer = new System.Timers.Timer(5000);
        System.Timers.Timer _positiontimer = new System.Timers.Timer(20000);
        System.Timers.Timer _positionuploadtimer = new System.Timers.Timer(20000);
        private BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker2 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker_startup = new BackgroundWorker();

        Thread pricesThread;

        List<Portfolio> prtfArr = new List<Portfolio>();
        List<Position> posArr = new List<Position>();
        List<VolParam> volparamArr = new List<VolParam>();
        List<Dividend> divArr = new List<Dividend>();
        List<Rates> ratesArr = new List<Rates>();
        Dictionary<int, Instrument> instArr = new Dictionary<int, Instrument>();
        List<IndexData> indexdataArr = new List<IndexData>();
        ObservableCollection<PortfolioDisplay> portdispArr = new ObservableCollection<PortfolioDisplay>();
        SortableObservableCollection<PortfolioDisplay> sortportdisp;
        Dictionary<int, float> poshistArr = new Dictionary<int, float>();
        Dictionary<int, string> gicssectArr = new Dictionary<int, string>();
        Dictionary<int, string> gicsinduArr = new Dictionary<int, string>();
        private Dictionary<int, float> instbeta = new Dictionary<int, float>();

        BbgPricer bbgpricer = new BbgPricer();

        Dictionary<int, double?> tickerList;
        Dictionary<string, int> tickernameList;

        bool isLivePrices = false;
        bool isRecalcPort = false;
        bool isRiskSlide = false;
        bool isRollCheck = false;
        bool isStraddlePLSkew = false;
        bool isDivCheck = false;
        bool isRunningIntraPos = false;
        bool isBorrowCheck = false;
        bool isP5Position = false;

        float id_imnt_backtest_start;
        float id_imnt_backtest_end;
        float und_price_bump = 0;
        float am_vol_bump = 0;

        string dt_bus_backtest_start;
        string dt_bus_backtest_end;
        DateTime dt_val;
        public AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public MainWindow()
        {
            
            LoggingHelper.LogMemo("{0}: Starting..", String.Format("{0:HH:mm:ss.fff}", DateTime.Now));

            InitializeComponent();
            //backgroundWorker_startup.WorkerSupportsCancellation = true;
            //backgroundWorker_startup.WorkerReportsProgress = true;
            //backgroundWorker_startup.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_startup_RunWorkerCompleted);
            //backgroundWorker_startup.DoWork += backgroundWorker_startup_DoWork;
            //backgroundWorker_startup.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_startup_ProgressChanged);
            //backgroundWorker_startup.RunWorkerAsync();
            Main();
            
            //bbgpricer.BbgHist();
        }

        public void Main()
        {

            ProcessCommandLineArgs();

            //backgroundWorker_startup.ReportProgress(0);
            
            LoadArrays();

            Riskslide riskslide;
            Rollcheck rollcheck;
            StraddlePLSkew straddleplskew;
            DividendRpt divrpt;
            BorrowRpt borrowrpt;

            if (isBorrowCheck)
            {
                _getposhelper.getP5Position(posArr, instArr, prtfArr, true);
                borrowrpt = new BorrowRpt(posArr, prtfArr, instArr);
                Environment.Exit(0);
            }

            if (isDivCheck)
            {
                CreateTickerList();
                divrpt = new DividendRpt(posArr, prtfArr, volparamArr, instArr, ratesArr, divArr, tickerList, indexdataArr);
                Environment.Exit(0);
            }

            if (isStraddlePLSkew)
            {
                straddleplskew = new StraddlePLSkew(dt_bus_backtest_start, dt_bus_backtest_end, id_imnt_backtest_start, id_imnt_backtest_end);
                Environment.Exit(0);
            }

            if (isRollCheck)
            {
                CreateTickerList();
                rollcheck = new Rollcheck(posArr, prtfArr, volparamArr, instArr, ratesArr, divArr, indexdataArr, tickerList);
                Environment.Exit(0);
            }

            if (isRiskSlide)
            {
                riskslide = new Riskslide(posArr, prtfArr, volparamArr, instArr, ratesArr, divArr, indexdataArr);
                Environment.Exit(0);
            }

            if (isP5Position)
            {
                isRunningIntraPos = false;
                UpdatePosition();
            }

            if (!isRiskSlide)
            {
                statusbartext.Dispatcher.Invoke((Action)(() => statusbartext.Text = "Getting portfolio"));
                

                //check if another instance is running getP5PositionAll
                /*
                DateTime last_run = _dbhelper.GetPositionIntraLastUpdate();
               
                if (last_run > DateTime.Now.AddMinutes(-1))
                {
                    isRunningIntraPos = true;
                }
                 */

                _getposhelper.getP5Position(posArr, instArr, prtfArr, isRunningIntraPos);
                CreateTickerList();
                statusbartext.Dispatcher.Invoke((Action)(() => statusbartext.Text = "Recalc Book"));
                RecalcBook();

                CreatePortDisp();

                sortportdisp = new SortableObservableCollection<PortfolioDisplay>(portdispArr);

                dataGrid1.Dispatcher.Invoke((Action)(() => dataGrid1.ItemsSource = sortportdisp));
                RefreshView();
                
                UpdatePosition();
                UpdatePositionUpload();

            }
        }

        /// <summary>
        /// Process command line arguments
        /// </summary>
        private void ProcessCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                string[] kv = arg.Split(':');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "-riskslide":
                            isRiskSlide = true;
                            break;
                        case "-rollcheck":
                            isRollCheck = true;
                            break;
                        case "-divcheck":
                            isDivCheck = true;
                            break;
                        case "-borrowcheck":
                            isBorrowCheck = true;
                            break;
                        case "-straddleplskew":
                            isStraddlePLSkew = true;
                            dt_bus_backtest_start = kv[1];
                            dt_bus_backtest_end = kv[2];
                            id_imnt_backtest_start = (float) Convert.ToDouble(kv[3]);
                            id_imnt_backtest_end = (float) Convert.ToDouble(kv[4]);                            
                            break;
                        case "-getportfolio":
                            isP5Position = true;
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Update Arrays
        /// </summary>
        private void LoadArrays()
        {
            prtfArr = _dbhelper.GetPortfolio();
            posArr = _dbhelper.GetPosition();
            volparamArr = _dbhelper.GetVolParam();
            divArr = _dbhelper.GetDividend();
            ratesArr = _dbhelper.GetRates();
            instArr = _dbhelper.GetInst();
            indexdataArr = _dbhelper.GetIndData();
            poshistArr = _dbhelper.GetPositionHist();
            gicssectArr = _dbhelper.GetGicsSect();
            gicsinduArr = _dbhelper.GetGicsIndu();
            instbeta = _dbhelper.GetInstBeta();
        }

        /// <summary>
        /// Initialize Portfolio Display Object with Data
        /// </summary>
        private void CreatePortDisp()
        {
            //System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\temp\\risk_logger.txt");

            float am_delta_tot = 0;
            float am_impdelta_tot = 0;
            float am_gamma_tot = 0;
            float am_vega_tot = 0;
            float am_theta_tot = 0;
            float am_pl_tot = 0;
            float am_pl_day_tot = 0;
            int prtf_ct = 1;

            portdispArr.Clear();
            //try
            //{
            foreach (Portfolio prtf in prtfArr)
            {
                Instrument inst_und = instArr[(int)prtf.id_imnt_und];
                //Instrument inst_und = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                List<Position> pos = new List<Position>();
                pos = posArr.Where(x => x.id_prtf == prtf.id_prtf).ToList();
                float am_delta_port = 0;
                float am_impdelta_port = 0;
                float am_gamma_port = 0;
                float am_vega_port = 0;
                float am_theta_port = 0;
                float am_pl_theo_port = 0;
                float am_pl_day_port = 0;
                float index_mult = 1;
                //if (inst_und.id_imnt_ric == "UA")
                //    am_delta_port = 1;

                foreach (Position position in pos)
                {
                                        
                    //Instrument inst = instArr.FirstOrDefault(x => x.id_imnt == position.id_imnt);
                    Instrument inst = instArr[(int)position.id_imnt];
                    LoggingHelper.LogMemo("{2}: Create port disp: {0}, {1}", prtf.nm_prtf, inst.id_imnt_ric, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                    if (position.id_prtf != 460 && position.id_prtf != 958 && position.id_prtf != 725 && position.id_prtf != 771) // exclude deltas
                        am_delta_port = am_delta_port + (position.am_delta * position.am_pos * position.pr_und * (float)inst.am_ct_sz);
                    am_impdelta_port = am_impdelta_port + (position.am_delta_imp * position.am_pos * position.pr_und * (float)inst.am_ct_sz);
                    am_gamma_port = (float)(am_gamma_port + (position.am_gamma * position.am_pos * position.pr_und * .01 / (1 / position.pr_und)) * 100.0);
                    am_vega_port = am_vega_port + (position.am_vega * position.am_pos * (float)inst.am_ct_sz);
                    am_theta_port = am_theta_port + (position.am_theta * position.am_pos * (float)inst.am_ct_sz);
                    am_pl_day_port = am_pl_day_port + position.am_pl_day;
                    if (inst.id_typ_imnt == "FUT")
                        am_pl_theo_port = am_pl_theo_port + ((position.pr_imnt - position.pr_imnt_close) * position.am_pos * (float)inst.am_ct_sz);
                    else if (!poshistArr.ContainsKey(position.id_imnt))
                        am_pl_theo_port = am_pl_theo_port + ((position.pr_theo - position.pr_imnt_close) * position.am_pos * (float)inst.am_ct_sz);
                    else
                        am_pl_theo_port = am_pl_theo_port + ((position.pr_theo - poshistArr[position.id_imnt]) * position.am_pos * (float)inst.am_ct_sz);

                    if (position.pr_theo == 999)
                        am_pl_theo_port = 0;
                }

                //check index
                if (prtf.id_imnt_und == 10002)
                    index_mult = 50;
                else if (prtf.id_imnt_und == 10013)
                    index_mult = 20;
                else if (prtf.id_imnt_und == 10145)
                    index_mult = 100;

                portdispArr.Add(new PortfolioDisplay
                {
                    id_prtf = prtf.id_prtf,
                    prtf_ct = prtf_ct,
                    nm_prtf = prtf.nm_prtf,
                    pr_und = prtf.pr_und,
                    am_pct_chg = (prtf.pr_und - prtf.pr_und_close) / prtf.pr_und_close,
                    am_delta = am_delta_port,
                    am_delta_abs = Math.Abs(am_delta_port),
                    am_delta_ct = am_delta_port / prtf.pr_und / (float)inst_und.am_ct_sz / index_mult,
                    am_gamma = am_gamma_port,
                    am_vega = am_vega_port,
                    am_theta = am_theta_port,
                    am_pl = am_pl_theo_port,
                    am_impdelta = am_impdelta_port
                });

                am_delta_tot = am_delta_tot + am_delta_port;
                am_impdelta_tot = am_impdelta_tot + am_impdelta_port;
                am_gamma_tot = am_gamma_tot + am_gamma_port;
                am_vega_tot = am_vega_tot + am_vega_port;
                am_theta_tot = am_theta_tot + am_theta_port;
                am_pl_tot = am_pl_tot + am_pl_theo_port;
                am_pl_day_tot = am_pl_day_tot + am_pl_day_port;
                prtf_ct++;

                

                //this.Invoke((MethodInvoker)delegate { textBox_delta.Text = am_delta_tot.ToString("0,000"); });
                //this.Invoke((MethodInvoker)delegate { textBox_gamma.Text = am_gamma_tot.ToString("0,000"); });
                //this.Invoke((MethodInvoker)delegate { textBox_vega.Text = am_vega_tot.ToString("0,000"); });
                //this.Invoke((MethodInvoker)delegate { textBox_theta.Text = am_theta_tot.ToString("0,000"); });

                /*this.textBox_delta.Text = am_delta_tot.ToString("0,000");
                this.textBox_gamma.Text = am_gamma_tot.ToString("0,000");
                this.textBox_vega.Text = am_vega_tot.ToString("0,000");
                this.textBox_theta.Text = am_theta_tot.ToString("0,000");
                 */
            }
            //catch
            //{ file.Close(); }
        }
        

        /// <summary>
        /// Refresh Portfolio Display Array
        /// </summary>
        private void RefreshView()
        {
            float am_delta_tot = 0;
            float am_impdelta_tot = 0;
            float am_gamma_tot = 0;
            float am_vega_tot = 0;
            float am_theta_tot = 0;
            float am_pl_tot = 0;
            float am_pl_day_tot = 0;
            float am_pl_trd_tot = 0;
            float am_vega_long = 0;
            float am_vega_short = 0;
            float am_vegavol_long = 0;
            float am_vegavol_short = 0;
            float am_edge_tot = 0;
            Dictionary<int, double> _vegaslide = new Dictionary<int, double>();
            Dictionary<int, double> _vegaslide_vegavol = new Dictionary<int, double>();
            Dictionary<string, double> _vegaslide_name = new Dictionary<string, double>();
            try{

                lock (prtfArr)
                {
                    if (prtfArr.Count != portdispArr.Count)
                    {
                        CreatePortDisp();
                        sortportdisp = null;
                        sortportdisp = new SortableObservableCollection<PortfolioDisplay>(portdispArr);
                        PortfolioDisplay portdisp2 = sortportdisp.FirstOrDefault(x => x.nm_prtf == "FAS");
                        float blah = (float) portdisp2.am_delta;
                    }
                    foreach (Portfolio prtf in prtfArr)
                    {
                        //Instrument inst_und = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                        Instrument inst_und = instArr[(int)prtf.id_imnt_und];
                        PortfolioDisplay portdisp = portdispArr.FirstOrDefault(x => x.id_prtf == prtf.id_prtf);
                        List<Position> pos = new List<Position>();

                        double vegavol = 0;
                        pos = posArr.Where(x => x.id_prtf == prtf.id_prtf).ToList();
                        lock (pos)
                        {

                            float am_delta_port = 0;
                            float am_impdelta_port = 0;
                            float am_gamma_port = 0;
                            float am_vega_port = 0;
                            float am_theta_port = 0;
                            float am_pl_theo_port = 0;
                            float am_pl_day_port = 0;
                            float index_mult = 1;
                            float am_vegavol_port = 0;
                            float am_vegatot_port = 0;
                            float am_pl_trd_port = 0;
                            float am_edge_port = 0;
                            //if (inst_und.id_imnt_ric == "UA")
                            //    am_delta_port = 1;

                            foreach (Position position in pos)
                            {
                                //Instrument inst = instArr.FirstOrDefault(x => x.id_imnt == position.id_imnt);
                                Instrument inst = instArr[(int)position.id_imnt];

                                //LoggingHelper.LogMemo("{2}: Refresh view: {0}, {1}", prtf.nm_prtf, inst.id_imnt_ric, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                                if (position.id_prtf != 460 && position.id_prtf != 958 && position.id_prtf != 725 && position.id_prtf != 771) //exclude deltas
                                    am_delta_port = am_delta_port + (position.am_delta * position.am_pos * position.pr_und * (float)inst.am_ct_sz);
                                am_impdelta_port = am_impdelta_port + (position.am_delta_imp * position.am_pos * position.pr_und * (float)inst.am_ct_sz);
                                am_gamma_port = (float)(am_gamma_port + (position.am_gamma * position.am_pos * position.pr_und * .01 / (1 / position.pr_und)) * 100.0);
                                am_vega_port = am_vega_port + (float)(Math.Round(position.am_vega, 5) * position.am_pos * (float)inst.am_ct_sz);
                                if (inst.dt_mat > DateTime.Today && inst.id_typ_imnt != "IND" && inst.id_typ_imnt != "STK" && inst.id_typ_imnt != "FUT")
                                {
                                    am_vegatot_port += (float)(Math.Round(position.am_vega, 5) * position.am_pos * (float)inst.am_ct_sz * Math.Pow((90.0 / (inst.dt_mat.Value.Subtract(DateTime.Today).Days)), .5));
                                    am_vegavol_port = (float)(am_vegavol_port + ((Math.Round(position.am_vega, 5) * position.am_pos * (float)inst.am_ct_sz * Math.Pow((90.0 / (inst.dt_mat.Value.Subtract(DateTime.Today).Days)), .5)) * position.am_vol_hedge));
                                }
                                am_theta_port = am_theta_port + (position.am_theta * position.am_pos * (float)inst.am_ct_sz);
                                am_pl_day_port = am_pl_day_port + position.am_pl_day;
                                if (position.am_trd_buy != 0 || position.am_trd_sell != 0)
                                {
                                    if (position.pr_avg_buy == 0)
                                        position.pr_avg_buy = position.pr_imnt;
                                    if (position.pr_avg_sell == 0)
                                        position.pr_avg_sell = position.pr_imnt;

                                    am_pl_trd_port = am_pl_trd_port + ((position.pr_imnt - position.pr_avg_buy) * position.am_trd_buy * (float)inst.am_ct_sz) + ((position.pr_imnt - position.pr_avg_sell) * position.am_trd_sell * (float)inst.am_ct_sz);
                                    //am_pl_trd_port = am_pl_trd_port + ((position.pr_imnt - position.pr_trd) * position.am_pos_adj * (float)inst.am_ct_sz);
                                }

                                if (inst.id_typ_imnt == "OPT")
                                {
                                    am_edge_port = am_edge_port + (position.pr_imnt - position.pr_theo) * position.am_pos * (float)inst.am_ct_sz;
                                }

                                if (inst.id_typ_imnt == "FUT")
                                    am_pl_theo_port = am_pl_theo_port + ((position.pr_imnt - position.pr_imnt_close) * position.am_pos * (float)inst.am_ct_sz);
                                else if (!poshistArr.ContainsKey(position.id_imnt))
                                    am_pl_theo_port = am_pl_theo_port + ((position.pr_theo - position.pr_imnt_close) * (position.am_pos - position.am_pos_adj) * (float)inst.am_ct_sz);
                                else
                                    am_pl_theo_port = am_pl_theo_port + ((position.pr_theo - poshistArr[position.id_imnt]) * (position.am_pos - position.am_pos_adj) * (float)inst.am_ct_sz);

                                if (position.pr_theo == 999)
                                    am_pl_theo_port = 0;
                            }

                            //check index
                            if (prtf.id_imnt_und == 10002)
                                index_mult = 50;
                            else if (prtf.id_imnt_und == 10013)
                                index_mult = 20;
                            else if (prtf.id_imnt_und == 10145)
                                index_mult = 100;

                            portdisp.pr_und = prtf.pr_und;
                            portdisp.am_pct_chg = ((prtf.pr_und - prtf.pr_und_close) / prtf.pr_und_close);
                            portdisp.am_delta = am_delta_port;
                            portdisp.am_delta_abs = Math.Abs(am_delta_port);
                            portdisp.am_delta_ct = am_delta_port / prtf.pr_und / (float)inst_und.am_ct_sz / index_mult;
                            portdisp.am_gamma = am_gamma_port;
                            portdisp.am_vega = am_vega_port;
                            portdisp.am_theta = am_theta_port;
                            portdisp.am_pl = am_pl_theo_port + am_pl_trd_port;
                            portdisp.am_impdelta = am_impdelta_port;
                            portdisp.am_pl_trd = am_pl_trd_port;
                            portdisp.am_pl_day = am_pl_day_port;
                            portdisp.am_edge = am_edge_port;

                            am_delta_tot = am_delta_tot + am_delta_port;
                            am_gamma_tot = am_gamma_tot + am_gamma_port;
                            am_vega_tot = am_vega_tot + am_vega_port;
                            am_theta_tot = am_theta_tot + am_theta_port;
                            am_pl_tot = am_pl_tot + am_pl_theo_port + am_pl_trd_port;
                            am_impdelta_tot = am_impdelta_tot + am_impdelta_port;
                            am_pl_trd_tot = am_pl_trd_tot + am_pl_trd_port;
                            am_pl_day_tot = am_pl_day_tot + am_pl_day_port;
                            am_edge_tot = am_edge_tot + am_edge_port;
                            //prtf_ct++;

                            if (am_vegatot_port > 1000)
                            {
                                am_vegavol_long += am_vegavol_port;
                                am_vega_long += am_vegatot_port;
                            }
                            if (am_vegatot_port < -1000)
                            {
                                am_vegavol_short += am_vegavol_port;
                                am_vega_short += am_vegatot_port;
                            }

                            _vegaslide.Add((int)prtf.id_prtf, am_vegatot_port);
                            _vegaslide_vegavol.Add((int)prtf.id_prtf, am_vegavol_port);
                            _vegaslide_name.Add(prtf.nm_prtf, am_vegatot_port);

                            portdisp.am_vega15 = am_vegatot_port;
                            portdisp.am_vegavol15 = am_vegavol_port;
                            //this.Invoke((MethodInvoker)delegate { textBox_delta.Text = am_delta_tot.ToString("0,000"); });
                            //this.Invoke((MethodInvoker)delegate { textBox_gamma.Text = am_gamma_tot.ToString("0,000"); });
                            //this.Invoke((MethodInvoker)delegate { textBox_vega.Text = am_vega_tot.ToString("0,000"); });
                            //this.Invoke((MethodInvoker)delegate { textBox_theta.Text = am_theta_tot.ToString("0,000"); });

                            /*this.textBox_delta.Text = am_delta_tot.ToString("0,000");
                            this.textBox_gamma.Text = am_gamma_tot.ToString("0,000");
                            this.textBox_vega.Text = am_vega_tot.ToString("0,000");
                            this.textBox_theta.Text = am_theta_tot.ToString("0,000");
                             */
                        }
                    }


                    double long_vol = am_vegavol_long / am_vega_long;
                    double short_vol = am_vegavol_short / am_vega_short;
                    double vol_ratio = long_vol / short_vol;
                    double correlation = Math.Pow(1 / vol_ratio, 2);
                    float resultszz = (float)((Math.Min(1.5, vol_ratio) * am_vega_long) + am_vega_short);

                    dataGrid1.Dispatcher.Invoke((Action)(() => am_delta_port_textBox.Text = am_delta_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_gamma_port_textBox.Text = am_gamma_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_vega_port_textBox.Text = am_vega_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_theta_port_textBox.Text = am_theta_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_pl_theo_port_textBox.Text = am_pl_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_vegaratio_port_textBox.Text = resultszz.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_pl_trd_port_textBox.Text = am_pl_trd_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_impdelta_port_textBox.Text = am_impdelta_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_edge_port_textBox.Text = am_edge_tot.ToString("#,###")));
                    dataGrid1.Dispatcher.Invoke((Action)(() => am_pl_day_port_textBox.Text = am_pl_day_tot.ToString("#,###")));

                    //dataGrid1.ItemsSource = portdispArr;

                    //SortableBindingList<PortfolioDisplay> filenamesList = new SortableBindingList<PortfolioDisplay>(portdispArr); // <-- BindingList      
                    //Bind BindingList directly to the DataGrid, no need of BindingSource     
                    //dataGridView1.DataSource = typeof(PortfolioDisplay);
                    //dataGridView1.DataSource = filenamesList;

                    //dataGridView1.Sort(this.am_delta_abs, ListSortDirection.Descending);

                    //dataGridView1.Invoke((Action)(() => dataGridView1.DataSource = typeof(PortfolioDisplay)));
                    //dataGrid1.Invoke((Action)(() => dataGrid1.ItemsSource = portdispArr));
                    //dataGrid1.Dispatcher.Invoke((Action)(() => dataGrid1.ItemsSource = portdispArr));
                    //dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1.Sort = am_theta_tot.ToString("0,000"); });
                    //dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Sort = ListSortDirection.Descending));
                    //dataGrid1.ColumnFromDisplayIndex(2).SortDirection = ListSortDirection.Descending;


                    sortportdisp.Sort(x => x.am_delta_abs, ListSortDirection.Descending);
                    dataGrid1.Dispatcher.Invoke((Action)(() => dataGrid1.Items.Refresh()));
                    dataGrid1.Dispatcher.Invoke((Action)(() => dataGrid1.ItemsSource = sortportdisp));
                    //dataGrid1.Columns[2].SortDirection = ListSortDirection.Descending;
                

                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// Initialize Ticker List for bloomberg/live prices
        /// </summary>
        private void CreateTickerList()
        {
            tickernameList = new Dictionary<string, int>();
            tickerList = new Dictionary<int, double?>();
            
            foreach (Portfolio prtf in prtfArr)
            {
                int i = 0;
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                Instrument item = instArr[(int)prtf.id_imnt_und];
                LoggingHelper.LogMemo("{2}: Create ticker list: {0}, {1}", prtf.nm_prtf, item.id_imnt_ric, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                if (!tickerList.ContainsKey((int)prtf.id_imnt_und))
                {
                    tickernameList.Add(item.id_bberg, (int)prtf.id_imnt_und);
                    tickerList.Add((int)prtf.id_imnt_und, prtf.pr_und);
                    i++;
                }
            }

            //Add futures
            foreach (IndexData indexdata in indexdataArr)
            {
                if (indexdata.id_imnt == 10002 || indexdata.id_imnt == 10013 || indexdata.id_imnt == 10145)
                {
                    //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_ric == indexdata.id_front_mth);
                  
                    var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexdata.id_next_mth);
                    Instrument item = instArr[(int) dict.Key];
                    if (!tickerList.ContainsKey((int)item.id_imnt))
                    {
                        if (posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt) == null)
                            tickerList.Add((int)item.id_imnt, 1);
                        else
                            tickerList.Add((int)item.id_imnt, posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt).pr_imnt);
                        tickernameList.Add(item.id_bberg, (int)item.id_imnt);
                    }

                    dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexdata.id_front_mth);
                    item = instArr[(int)dict.Key];
                    if (!tickerList.ContainsKey((int)item.id_imnt))
                    {
                        if (posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt) == null)
                            tickerList.Add((int)item.id_imnt, 1);
                        else
                            tickerList.Add((int)item.id_imnt, posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt).pr_imnt);
                        tickernameList.Add(item.id_bberg, (int)item.id_imnt);
                    }
                    //tickerList.Add(29421, 1);
                    //tickernameList.Add("RTAM3", 29421);
                    //tickerList.Add(29422, 1);
                    //tickernameList.Add("NQM3", 29422);
                    //tickerList.Add(29423, 1);
                    //tickernameList.Add("ESM3", 29423);
                    /*dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexdata.id_next_mth);
                    item = instArr[(int)dict.Key];
                    if (!tickerList.ContainsKey((int)item.id_imnt))
                    {
                        tickerList.Add((int)item.id_imnt, posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt).pr_imnt);
                    }
                     */
                }


            }
        }

        /// <summary>
        /// Subscribes to live bloomberg prices
        /// </summary>
        private void RefreshPrices()
        {
            //Stopwatch sw = new Stopwatch();
            //TimeSpan duration;
            
            //ItgPricer itgpricer = new ItgPricer();

            int i = 0;

            //sw.Restart();
            foreach (Portfolio prtf in prtfArr)
            {
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                Instrument item = instArr[(int) prtf.id_imnt_und];
                if (!tickerList.ContainsKey((int)item.id_imnt))
                {
                    tickerList.Add((int)item.id_imnt, 10.0);
                    //tickerList.Add(item.id_bberg, bbgpricer.BbgPricerTicker(item.id_bberg));
                    //tickerList.Add(item.id_bberg, 10);
                }
                i++;
            }

            bbgpricer.run(tickerList, instArr, tickernameList);
            //itgpricer.run(tickerList, instArr);
            //duration = sw.Elapsed;
        }

        /// <summary>
        /// Update position on timer
        /// </summary>
        private void UpdatePosition()
        {
            _positiontimer.Elapsed += new ElapsedEventHandler(_positiontimer_Elapsed);
            _positiontimer.Enabled = true;
        }

        /// <summary>
        /// Update positionupload on timer
        /// </summary>
        private void UpdatePositionUpload()
        {
            _positionuploadtimer.Elapsed += new ElapsedEventHandler(_positionuploadtimer_Elapsed);
            _positionuploadtimer.Enabled = true;
        }

        /// <summary>
        /// Update position event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _positiontimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _getposhelper.getP5Position(posArr, instArr, prtfArr, isRunningIntraPos);
        }

        /// <summary>
        /// Update position upload event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _positionuploadtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            posArr[1].createPositionFile(posArr, instArr);
        }

        /// <summary>
        /// Recalc Portfolio on timer
        /// </summary>
        private void RecalcPortTimer()
        {
            _recalcprtftimer.Elapsed += new ElapsedEventHandler(_recalcprtftimer_Elapsed);
            _recalcprtftimer.Enabled = true;
        }

        /// <summary>
        /// Recalc Portfolio event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _recalcprtftimer_Elapsed(object sender, ElapsedEventArgs e)
        {
     
            //tickerList = bbgpricer.getTickerList();
            //lock (tickerList)
            //{
            dt_val = dt_val_datePicker.DisplayDate;
            foreach (Portfolio prtf in prtfArr)
            {
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                Instrument item = instArr[(int)prtf.id_imnt_und];
                //if (item.id_imnt_ric == "UA")
                //    i = 3;
                //SetStatusBox("Port: " + prtf.nm_prtf);

                //SetControlPropertyThreadSafe(textBox_delta, "Text", prtf.nm_prtf);
                //this.Invoke((MethodInvoker)delegate { toolStripStatusLabel1.Text = "Port: " + prtf.nm_prtf; });

                //if (prtf.pr_und != (float)tickerList[item.id_bberg]) //if undprice is different
                //{
                RecalcPrtf(prtf);
                //}

            }
            RefreshView();
        }

        /// <summary>
        /// Recalc entire book
        /// </summary>
        private void RecalcBook()
        {
            //System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\temp\\risk_logger.txt");
            dt_val = dt_val_datePicker.DisplayDate;

            lock (tickerList)
            {
                int i = 1;
                foreach (Portfolio prtf in prtfArr)
                {
                    //file.WriteLine(prtf.nm_prtf);
                    try
                    {
                        RecalcPrtf(prtf);
                    }
                    catch
                    {

                    }
                    i++;
                }
            }
            
            lock (posArr)
            {
                posArr[1].createPositionFile(posArr, instArr);
            }
            
            //file.Close();
        }

        /// <summary>
        /// Recalc individual portfolio
        /// </summary>
        /// <param name="prtf"></param>
        private void RecalcPrtf(Portfolio prtf)
        {
            LoggingHelper.LogMemo("{1}: Recalc port: {0}", prtf.nm_prtf, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
            lock (tickerList)
            {
                mhModels mhmodels = new mhModels();

                List<Position> position = new List<Position>();
                position = posArr.Where(x => x.id_prtf == prtf.id_prtf).ToList();
                foreach (Position pos in position)
                {
                    //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == pos.id_imnt);
                    //Instrument item_und = instArr.FirstOrDefault(x => x.id_imnt == item.id_imnt_und);

                    Instrument item = instArr[(int)pos.id_imnt];
                    Instrument item_und = instArr[(int)item.id_imnt_und];
                    float am_beta = 1;
                    if (instbeta.ContainsKey((int)prtf.id_imnt_und))
                         am_beta = (float)instbeta[(int)prtf.id_imnt_und];      


                    if (bbgpricer.isrunning && tickerList.ContainsKey((int)item_und.id_imnt))
                    {
                        if (item_und.id_typ_imnt != "IND")
                            prtf.pr_und = (float)tickerList[(int)item_und.id_imnt];
                    }
                    else
                    {
                        prtf.pr_und = pos.pr_und * (1 + (und_price_bump * am_beta));
                    }

                    switch (item.id_typ_imnt)
                    {
                        case "STK":
                            if (bbgpricer.isrunning)
                            {
                                pos.pr_und = (float)tickerList[(int)item.id_imnt] * (1 + (und_price_bump * am_beta));
                                pos.pr_imnt = (float)tickerList[(int)item.id_imnt] * (1 + (und_price_bump * am_beta));
                                pos.pr_theo = (float)tickerList[(int)item.id_imnt] * (1 + (und_price_bump * am_beta));
                            }
                            else
                            {
                                prtf.pr_und = pos.pr_imnt * (1 + (und_price_bump * am_beta));
                                pos.pr_und = pos.pr_imnt * (1 + (und_price_bump * am_beta));
                                pos.pr_theo = pos.pr_imnt * (1 + (und_price_bump * am_beta));
                            }
                            pos.am_delta = 1f;
                            pos.am_delta_imp = 1f;
                            continue;
                        case "FUT":
                            {
                                if (item.id_imnt == 29421 || item.id_imnt == 29422 || item.id_imnt == 29423)
                                    continue;
                                IndexData indexpos = indexdataArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                                if (bbgpricer.isrunning)
                                {
                                    pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                                    pos.pr_imnt = (float)tickerList[(int)item.id_imnt];
                                }
                                mhmodels.CalcFut(pos, item, item_und, ratesArr, pos.pr_und 
                                    , divArr.Where(x => x.id_imnt == prtf.id_imnt_und && x.id_src == item_und.id_div_src).ToList()
                                    , indexpos);
                                if (indexpos.id_front_mth == item.id_imnt_ric)
                                    prtf.pr_und = pos.pr_theo;

                                continue;
                            }
                        case "OPT":
                            {
                                if (bbgpricer.isrunning && tickerList.ContainsKey((int)item_und.id_imnt))
                                {
                                    pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                                }
                                
                                //if ((int)item_und.id_imnt == 13959)
                                //    pos.pr_und = (float)(tickerList[(int)item_und.id_imnt] * .81);

                                float pr_theo = 0;
                                float am_borrow = 0;
                                                          

                                IndexData indexpos = indexdataArr.FirstOrDefault(y => y.id_imnt == prtf.id_imnt_und);

                                if (indexpos != null)
                                {

                                    var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos.id_front_mth);

                                    Instrument item_fut = instArr[(int)dict.Key];
                                    Position pos_fut = posArr.FirstOrDefault(x => x.id_imnt == item_fut.id_imnt);

                                    //Instrument item_fut = instArr.FirstOrDefault(x => x.id_imnt_ric == indexpos.id_front_mth);
                                    if (pos_fut == null)
                                    {
                                        pr_theo = (float)tickerList[(int)item_und.id_imnt];
                                        //pos_fut.pr_und = (float)tickerList[(int)item_und.id_imnt];
                                    }
                                    else
                                    {
                                        pr_theo = pos_fut.pr_theo;
                                        pos_fut.pr_und = pos_fut.pr_theo;
                                    }
                                    am_borrow = (float)indexpos.am_borrow;
                                }
                                /*mhmodels.CalcOpt(pos, item, item_und, ratesArr, (float)tickerList[(int)item_und.id_imnt]
                                    , divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                    , volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                    , (float)tickerList[(int)item_und.id_imnt]
                                    , dt_val, pr_theo, am_borrow);
                                 * */
                                
                                mhmodels.CalcOpt(pos, item, item_und, ratesArr, pos.pr_und
                                    , divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                    , volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                    , pos.pr_und
                                    , dt_val, pr_theo, am_borrow,und_price_bump, am_vol_bump, am_beta);
                                pos.am_delta = mhmodels.mh_delta;
                                pos.am_gamma = mhmodels.mh_gamma;
                                pos.am_vega = mhmodels.mh_vega;
                                pos.am_theta = mhmodels.mh_theta;
                                pos.pr_theo = mhmodels.mh_thvalue;
                                //pos.pr_imnt = mhmodels.mh_thvalue;
                                pos.am_vol = mhmodels.am_vol_db;
                                pos.am_delta_imp = mhmodels.mh_impdelta;
                                pos.am_vol_hedge = mhmodels.am_vol_db;

                                if ((int)item_und.id_imnt == 13959)
                                    pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                                continue;
                            }
                    }

                }
            }
            
        }

        /// <summary>
        /// Buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopLp_Click(object sender, EventArgs e)
        {
            isLivePrices = false;
            _recalcprtftimer.Enabled = false;
        }

        private void RecalcPort_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);
            backgroundWorker2.DoWork += backgroundWorker2_DoWork;
            backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker2_ProgressChanged);
            backgroundWorker2.RunWorkerAsync();                                   
        }

        private void RecalcPort_run()
        {
            
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!backgroundWorker2.CancellationPending)
            {
                string und_price_bump_text = "";
                string am_vol_bump_text = "";
                bool? bbg_ischecked = false;
                Stopwatch sw = new Stopwatch();
                TimeSpan duration;
                sw.Restart();

                dt_val_datePicker.Dispatcher.Invoke((Action)(() => dt_val = (DateTime) dt_val_datePicker.SelectedDate));
                BbgPrices_CheckBox.Dispatcher.Invoke((Action)(() => bbg_ischecked = BbgPrices_CheckBox.IsChecked));

                und_price_bump_textBox.Dispatcher.Invoke((Action)(() => und_price_bump_text = und_price_bump_textBox.Text));
                am_vol_bump_textBox.Dispatcher.Invoke((Action)(() => am_vol_bump_text = am_vol_bump_textBox.Text));

                if (bbg_ischecked == true && bbgpricer.isrunning == false)
                {
                    bbgpricer.isrunning = true;
                    pricesThread = new Thread(new ThreadStart(RefreshPrices));
                    pricesThread.Start();
                }

                if (bbg_ischecked == false && bbgpricer.isrunning == true)
                {
                    bbgpricer.isrunning = false;
                    pricesThread.Abort();
                }
                //if (und_price_bump_text[0] == '-' || und_price_bump_text[0] == '.' || (Convert.ToInt32(und_price_bump_text[0]) >= 48 && Convert.ToInt32(und_price_bump_text[0]) <= 57))
                    

                //if (am_vol_bump_text[0] == '-' || am_vol_bump_text[0] == '.' || (Convert.ToInt32(am_vol_bump_text[0]) >= 48 && Convert.ToInt32(am_vol_bump_text[0]) <= 57))
                try
                {
                    und_price_bump = (float)Convert.ToDouble(und_price_bump_text);
                    am_vol_bump = (float)Convert.ToDouble(am_vol_bump_text);
                }
                catch
                {
                }

                lock (tickerList)
                {
                    foreach (Portfolio prtf in prtfArr.ToList())
                    {
                        Instrument item = instArr[(int)prtf.id_imnt_und];

                        //if (item.id_imnt_ric == "UA")
                        //    i = 3;

                        //if (prtf.pr_und != (float)tickerList[(int)item.id_imnt]) //if undprice is different

                        RecalcPrtf(prtf);

                    }

                }
                backgroundWorker2.ReportProgress(0);
                duration = sw.Elapsed;
                //posArr[1].createPositionFile(posArr);
                statusbartext.Dispatcher.Invoke((Action)(() => statusbartext.Text = "Calc done "
                    + String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + " - "
                    + duration.TotalSeconds.ToString("00.000")
                    + " - Value Date: " + dt_val.ToString("MM/dd/yy")
                    + " - Spot Bump: " + und_price_bump
                    + " - Vol Bump: " + am_vol_bump));

                //Thread.Sleep(3000);
            }

        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RefreshView();
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopPositionTimer();
            bbgpricer.isrunning = false;
            if (BbgPrices_CheckBox.IsChecked == true)
                pricesThread.Abort();

            lock (volparamArr)
            {
                volparamArr = null;
                volparamArr = _dbhelper.GetVolParam();
            }
            lock (divArr)
            {
                divArr = null;
                divArr = _dbhelper.GetDividend();
            }
            lock (ratesArr)
            {
                ratesArr = null;
                ratesArr = _dbhelper.GetRates();
            }
            /*lock (prtfArr)
            {
                prtfArr = null;
                prtfArr = _dbhelper.GetPortfolio();
            }
            lock (posArr)
            {
                posArr = null;
                posArr = _dbhelper.GetPosition();
            }*/

            CreateTickerList();
            Thread.Sleep(20000);
            bbgpricer.isrunning = true;
            //GRAB PRICES
            if (BbgPrices_CheckBox.IsChecked == true)
            {
                bbgpricer.isrunning = true;
                pricesThread = new Thread(new ThreadStart(RefreshPrices));
                pricesThread.Start();
            }
            _getposhelper.getP5Position(posArr, instArr, prtfArr, isRunningIntraPos);

            backgroundWorker2.RunWorkerAsync();         
            UpdatePosition();         

           
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshPrices();
        }

        private void backgroundWorker_startup_DoWork(object sender, DoWorkEventArgs e)
        {
            Main();
        }

        private void backgroundWorker_startup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusbartext.Dispatcher.Invoke((Action)(() => statusbartext.Text = "Loading"));
        }

        private void backgroundWorker_startup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void GrabPrices_Btn_Click(object sender, RoutedEventArgs e)
        {
            //backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            //backgroundWorker1.RunWorkerAsync();

            //GRAB PRICES BBG
            if (BbgPrices_CheckBox.IsChecked == true)
            {
                bbgpricer.isrunning = false;
                pricesThread = new Thread(new ThreadStart(RefreshPrices));
                pricesThread.Start();
            }

            //RECALC thread
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);
            backgroundWorker2.DoWork += backgroundWorker2_DoWork;
            backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker2_ProgressChanged);
            backgroundWorker2.RunWorkerAsync();   
            
        }

        private void RefreshView_Btn_Click(object sender, RoutedEventArgs e)
        {
            RefreshView();
        }

        private void dataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            List<PositionDisplay> posdispArr = new List<PositionDisplay>();
            PortfolioDisplay portdisp = (PortfolioDisplay)dataGrid1.SelectedItem;
            if (portdisp != null)
            {
                posdispArr = CreatePosDisp();
                var newWindow = new PositionView(posdispArr.Where(x => x.id_prtf == portdisp.id_prtf).ToList());
                newWindow.Show();
            }
        }

        /// <summary>
        /// Initialize Position Display Object with Data
        /// </summary>
        private List<PositionDisplay> CreatePosDisp()
        {
            List<PositionDisplay> posdispArr = new List<PositionDisplay>();
            foreach (Position pos in posArr)
            {
                Instrument inst = instArr[(int)pos.id_imnt];
                Instrument inst_und = instArr[(int)inst.id_imnt_und];

                float _am_pl = 0;
                if (!poshistArr.ContainsKey(pos.id_imnt))
                    _am_pl = pos.pr_theo - pos.pr_imnt * pos.am_pos * (float)inst.am_ct_sz;
                else
                    _am_pl = (pos.pr_theo - poshistArr[pos.id_imnt]) * pos.am_pos * (float)inst.am_ct_sz;

                if (pos.pr_theo == 999)
                    _am_pl = 0;

                posdispArr.Add(new PositionDisplay
                {
                    id_prtf = pos.id_prtf,
                    id_imnt = pos.id_imnt,
                    id_imnt_typ = inst.id_typ_imnt,
                    nm_imnt = inst.nm_imnt,
                    id_imnt_ric = inst.id_imnt_ric,
                    am_pos = pos.am_pos,
                    am_pos_adj = pos.am_pos_adj,
                    pr_strike = inst.pr_strike,
                    dt_mat = inst.dt_mat,
                    id_pc = inst.id_pc,
                    pr_imnt = pos.pr_imnt,
                    pr_imnt_close = pos.pr_imnt_close,
                    am_vol_imp = pos.am_vol_imp,
                    am_vol_mark = pos.am_vol_mark,
                    pr_und = pos.pr_und,
                    pr_theo = pos.pr_theo,
                    am_vol = pos.am_vol,
                    am_vol_hedge = pos.am_vol_hedge,
                    am_delta = pos.am_delta,
                    am_delta_dollar = pos.am_delta * pos.am_pos * pos.pr_und * (float) inst.am_ct_sz,
                    am_gamma_dollar = (float)(pos.am_gamma * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100,
                    am_vega_dollar = pos.am_vega * pos.am_pos * (float)inst.am_ct_sz,
                    am_theta_dollar = pos.am_theta * pos.am_pos * (float)inst.am_ct_sz,
                    am_pl = _am_pl,
                    am_impdelta = pos.am_delta_imp * pos.am_pos * pos.pr_und * (float) inst.am_ct_sz
                });
            }
            return posdispArr;
        }

        private void RefreshData_Btn_Click(object sender, RoutedEventArgs e)
        {
            //StopPositionTimer();
            StopRecalcBook();

        }

        private void StopPositionTimer()
        {
            _positiontimer.Elapsed -= new ElapsedEventHandler(_positiontimer_Elapsed);
            _positiontimer.Enabled = false;
        }

        private void StopRecalcBook()
        {
            backgroundWorker2.CancelAsync();            
        }

        private void StartRecalcBook()
        {
            backgroundWorker2.RunWorkerAsync();
        }
        private void Exit_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (bbgpricer.isrunning)
                pricesThread.Abort();
            bbgpricer.isrunning = false;                        

            Environment.Exit(0);
            
        }

        private void manualCopy_btn_Click(object sender, RoutedEventArgs e)
        {
            dataGrid1.SelectionMode = DataGridSelectionMode.Extended;
            dataGrid1.SelectAllCells();
            dataGrid1.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dataGrid1);
            Clipboard.GetData(DataFormats.CommaSeparatedValue);
            dataGrid1.UnselectAllCells();

            string test = Clipboard.GetText();
        }

        private List<PortByMat> viewByMat()
        {
            //Get all maturities
            //RecalcBook();

            SortedSet<DateTime> dt_mat_arr = new SortedSet<DateTime>();
            foreach (Position pos in posArr)
            {
                if (pos.am_pos == 0)
                    continue;
                Instrument inst = instArr[(int)pos.id_imnt];

                if (inst.id_typ_imnt != "OPT")
                    continue;

                dt_mat_arr.Add((DateTime) inst.dt_mat);

            }            

            //add sorted maturities to dictionary
            Dictionary<DateTime, PortByMat> portbymat = new Dictionary<DateTime, PortByMat>();
            foreach (DateTime dt in dt_mat_arr)
                portbymat.Add(dt, new PortByMat{
                    dt_mat = dt,
                am_gamma = 0f,
                am_theta = 0f,
                am_vega = 0f,
                am_vega_3m = 0f});

            foreach (Position pos in posArr)
            {
                if (pos.am_pos == 0)
                    continue;
                Instrument inst = instArr[(int)pos.id_imnt];

                if (inst.id_typ_imnt != "OPT")
                    continue;

                portbymat[(DateTime)inst.dt_mat].am_gamma = (float)(portbymat[(DateTime)inst.dt_mat].am_gamma + (pos.am_gamma * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100.0);
                portbymat[(DateTime)inst.dt_mat].am_vega = portbymat[(DateTime)inst.dt_mat].am_vega + (float)(Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz);
                portbymat[(DateTime)inst.dt_mat].am_vega_3m = (float)(portbymat[(DateTime)inst.dt_mat].am_vega_3m + ((Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz * Math.Pow((90.0 / (inst.dt_mat.Value.AddDays(1).Subtract(DateTime.Today).Days)), .5))));
                portbymat[(DateTime)inst.dt_mat].am_theta = portbymat[(DateTime)inst.dt_mat].am_theta + (pos.am_theta * pos.am_pos * (float)inst.am_ct_sz);
            }

            List<PortByMat> portbymatdisp = new List<PortByMat>();
            foreach (KeyValuePair<DateTime, PortByMat> port in portbymat)
            {
                portbymatdisp.Add(port.Value);
            }

            return portbymatdisp;
        }

        private void byMat_btn_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new ByMaturity(viewByMat());
            newWindow.Show();            
        }

        private List<PortBySector> viewBySect()
        {
            Dictionary<int, PortBySector> portbysect = new Dictionary<int, PortBySector>();
            foreach (KeyValuePair<int, string> dt in gicssectArr)
                portbysect.Add(dt.Key, new PortBySector
                {
                    nm_gics_sector = dt.Value,
                    am_gamma = 0f,
                    am_theta = 0f,
                    am_vega = 0f,
                    am_vega_3m = 0f
                });

            foreach (Position pos in posArr)
            {
                if (pos.am_pos == 0)
                    continue;
                Instrument inst = instArr[(int)pos.id_imnt];
                Instrument inst_und = instArr[(int)inst.id_imnt_und];
                if (inst.id_typ_imnt != "OPT")
                    continue;

                portbysect[(int)inst_und.id_gics_sector].am_gamma = (float)(portbysect[(int)inst_und.id_gics_sector].am_gamma + (pos.am_gamma * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100.0);
                portbysect[(int)inst_und.id_gics_sector].am_vega = portbysect[(int)inst_und.id_gics_sector].am_vega + (float)(Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz);
                portbysect[(int)inst_und.id_gics_sector].am_vega_3m = (float)(portbysect[(int)inst_und.id_gics_sector].am_vega_3m + ((Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz * Math.Pow((90.0 / (inst.dt_mat.Value.AddDays(1).Subtract(DateTime.Today).Days)), .5))));
                portbysect[(int)inst_und.id_gics_sector].am_theta = portbysect[(int)inst_und.id_gics_sector].am_theta + (pos.am_theta * pos.am_pos * (float)inst.am_ct_sz);
            }

            List<PortBySector> portbysectdisp = new List<PortBySector>();
            foreach (KeyValuePair<int, PortBySector> port in portbysect)
            {
                portbysectdisp.Add(port.Value);
            }

            return portbysectdisp;
        }

        private void bySector_btn_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new BySect(viewBySect());
            newWindow.Show();
        }

        private List<PortByGicsIndu> viewByGicsIndu()
        {
            Dictionary<int, PortByGicsIndu> portbygicsindu = new Dictionary<int, PortByGicsIndu>();
            foreach (KeyValuePair<int, string> dt in gicsinduArr)
                portbygicsindu.Add(dt.Key, new PortByGicsIndu
                {
                    nm_gics_industry_sub = dt.Value,
                    am_gamma = 0f,
                    am_theta = 0f,
                    am_vega = 0f,
                    am_vega_3m = 0f
                });

            foreach (Position pos in posArr)
            {
                if (pos.am_pos == 0)
                    continue;
                Instrument inst = instArr[(int)pos.id_imnt];
                Instrument inst_und = instArr[(int)inst.id_imnt_und];
                if (inst.id_typ_imnt != "OPT")
                    continue;

                portbygicsindu[(int)inst_und.id_gics_industry_sub].am_gamma = (float)(portbygicsindu[(int)inst_und.id_gics_industry_sub].am_gamma + (pos.am_gamma * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100.0);
                portbygicsindu[(int)inst_und.id_gics_industry_sub].am_vega = portbygicsindu[(int)inst_und.id_gics_industry_sub].am_vega + (float)(Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz);
                portbygicsindu[(int)inst_und.id_gics_industry_sub].am_vega_3m = (float)(portbygicsindu[(int)inst_und.id_gics_industry_sub].am_vega_3m + ((Math.Round(pos.am_vega, 5) * pos.am_pos * (float)inst.am_ct_sz * Math.Pow((90.0 / (inst.dt_mat.Value.AddDays(1).Subtract(DateTime.Today).Days)), .5))));
                portbygicsindu[(int)inst_und.id_gics_industry_sub].am_theta = portbygicsindu[(int)inst_und.id_gics_industry_sub].am_theta + (pos.am_theta * pos.am_pos * (float)inst.am_ct_sz);
            }

            List<PortByGicsIndu> portbygicsindudisp = new List<PortByGicsIndu>();
            foreach (KeyValuePair<int, PortByGicsIndu> port in portbygicsindu)
            {
                portbygicsindudisp.Add(port.Value);
            }

            return portbygicsindudisp;
        }


        private void byGicsIndu_btn_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new ByGicsInduView(viewByGicsIndu());
            newWindow.Show();
        }

        private void am_vol_bump_textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            am_vol_bump_textBox.Text = "";
        }

        private void und_price_bump_textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            und_price_bump_textBox.Text = "";
        }

        private void und_price_bump_textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (und_price_bump_textBox.Text == "")
            {
                und_price_bump_textBox.Text = "Spot Bump";
                und_price_bump = 0;
            }
        }

        private void am_vol_bump_textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (am_vol_bump_textBox.Text == "")
            {
                am_vol_bump_textBox.Text = "Vol Bump";
                am_vol_bump = 0;
            }
        }

        private void BbgPrices_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bbgpricer.isrunning = false;
            pricesThread.Abort();            
        }

    }
}


