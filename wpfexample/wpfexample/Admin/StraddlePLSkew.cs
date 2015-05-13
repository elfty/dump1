using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
namespace wpfexample
{
    class StraddlePLSkew
    {
        private DBHelper _dbhelper = new DBHelper();
        private Dictionary<DateTime, int> _undpricedate = new Dictionary<DateTime, int>();
        private List<Rates> _rateshistArr = new List<Rates>();
        private List<VolData> _voldataArr = new List<VolData>();
        private Dictionary<DateTime, float> _spxdiv = new Dictionary<DateTime, float>();
        private List<Dividend_Hist> _dividends = new List<Dividend_Hist>();
        private mhModels _mhmodel = new mhModels();
        private List<int> _spxcomp = new List<int>();
       // private SABR _sabr = new SABR();
        private string _fileDate = string.Format("-{0:yyyyMMdd_HHmmss}",
        DateTime.Now);
        //string _fileDate = "32673";
        List<VolParam> _volparamArr = new List<VolParam>();
        string str_file = "VOL_HIST_IMPORT_STRADDLE_SKEW.txt";
        string str_file_1m = "VOL_HIST_IMPORT_STRADDLE_SKEW_1M.txt";
        string str_path = @"\\adsrv133\volarb\MLP\ONProcess\StraddlePL_Skew";

        private float _id_imnt_backtest_start;

        public StraddlePLSkew(string dt_start, string dt_end, float id_imnt_backtest_start, float id_imnt_backtest_end)
        {
            _undpricedate = _dbhelper.GetUndPriceDate();
            _rateshistArr = _dbhelper.GetRatesHist();            
            _volparamArr = _dbhelper.GetVolParam();            
            _spxcomp = _dbhelper.GetSPXComp(id_imnt_backtest_start, id_imnt_backtest_end);
            _spxcomp.Add(10002);
            _id_imnt_backtest_start = id_imnt_backtest_start;
            _dividends = _dbhelper.GetSPX500Div();
            //str_file = str_file + id_imnt_backtest_end;
            DateTime _dt_start;
            DateTime _dt_end;
            if (dt_start != "0")
            {
                _dt_start = new DateTime(Convert.ToInt16(dt_start.Substring(0, 4)),
                    Convert.ToInt16(dt_start.Substring(4, 2)),
                    Convert.ToInt16(dt_start.Substring(6, 2)));
                _dt_end = new DateTime(Convert.ToInt16(dt_end.Substring(0, 4)),
                    Convert.ToInt16(dt_end.Substring(4, 2)),
                    Convert.ToInt16(dt_end.Substring(6, 2)));
            }
            else
            {
                _dt_end = _undpricedate.Max(x => x.Key);
                _dt_start = _dt_end.AddDays(-4);
            }

            DateTime dt_start_und = getFutureDate(_dt_start, -41);
            //DateTime dt_start_und = _dt_start;
            _spxdiv = _dbhelper.GetSpxDiv("SPX");
            str_file_1m = str_file_1m + id_imnt_backtest_end;
            _voldataArr = _dbhelper.GetVolData(dt_start_und, _dt_end, id_imnt_backtest_start, id_imnt_backtest_end);
            runBackTest1m(_dt_start, _dt_end);
            _dbhelper.InsertStraddlePL_Skew_1m(_dt_start, _dt_end, id_imnt_backtest_start, id_imnt_backtest_end, str_path + @"\", str_file_1m);

            dt_start_und = getFutureDate(_dt_start, -62);
            _voldataArr = _dbhelper.GetVolData(dt_start_und, _dt_end, id_imnt_backtest_start, id_imnt_backtest_end);
            runBackTest(_dt_start, _dt_end);
            _dbhelper.InsertStraddlePL_Skew(_dt_start, _dt_end, id_imnt_backtest_start, id_imnt_backtest_end, str_path + @"\", str_file);
        }

        
        private void runBackTest(DateTime dt_start, DateTime dt_end)
        {            

            if (FileHelper.FileExists(str_path, str_file))
            {
                FileHelper.CopyFile(str_path, str_file,
                    str_path + @"\hist", "VOL_HIST_IMPORT_STRADDLE_SKEW" + _fileDate + ".txt");
            }
            FileHelper.CreateFile(str_path, str_file);

            DateTime dt_start_und = getFutureDate(dt_start, -62);
            //DateTime dt_start_und = dt_start;
            DateTime dt_final = getFutureDate(dt_end, -62);

            foreach (int id_imnt_comp in _spxcomp)
            {
                if (id_imnt_comp == 10002)
                    _spxdiv = _dbhelper.GetSpxDiv("SPX");
                else if (id_imnt_comp == 10013)
                    _spxdiv = _dbhelper.GetSpxDiv("NDX");
                else if (id_imnt_comp == 10145)
                    _spxdiv = _dbhelper.GetSpxDiv("RUT");

                int id_imnt = id_imnt_comp; //basket
                //int id_imnt = 10145; //spx
                //_voldataArr = _dbhelper.GetVolData(id_imnt);
                 // DateTime dt_start = new DateTime(2013,01,01);
                 // DateTime dt_end = new DateTime(2013, 7,17);
                
              //DateTime dt_start_und = dt_start;
                Dictionary<DateTime, float> results = new Dictionary<DateTime, float>();
                

                DateTime dt_bus = new DateTime();
                List<VolData> _voldataList = _voldataArr.Where(x => x.id_imnt == id_imnt).ToList();
                _dividends = _dividends.Where(x => x.id_imnt == id_imnt).ToList();

                for (int k = 0; k <= 20; k += 5)
                {
                    #region for (int k = 0; k <= 20; k += 5)
                    //int k = 10;
                    
                    foreach (VolData vd in _voldataList)
                    {
                        #region foreach (VolData vd in _voldataList)
                        DateTime dt_bus_expiry = getFutureDate(vd.dt_bus, 62);
                        if (vd.dt_bus > dt_final) 
                            break;
                        List<VolData> _voldataCurr = _voldataList.Where(x => x.dt_bus >= vd.dt_bus && x.dt_bus <= dt_bus_expiry).ToList();
                        if (dt_bus_expiry != _voldataCurr[_voldataCurr.Count-1].dt_bus) 
                            goto SKIPLOOP;
                        float pr_strike = 0;
                        float am_cts = 0;
                        float am_cts_call = 0;
                        float am_cts_put = 0;
                        float am_theo_call_first = 0;
                        float am_theo_put_first = 0;
                        float stk_pl = 0;
                        float stk_call_pl = 0;
                        float stk_put_pl = 0;
                        float am_delta_prev = 0;
                        float am_delta_call_prev = 0;
                        float am_delta_put_prev = 0;
                        float pr_und = 0;
                        float am_ivol_3m_100 = 0;
                        float pr_theo_call = 0;
                        float pr_theo_put = 0;
                        float pr_theo_call_last = 0;
                        float pr_theo_put_last = 0;
                        float am_ivol_3m_100_init = 0;
                        float am_chg = 0;
                        float pr_close_clean = 0;
                        float pr_close_clean_last = 0;

                        bool option_exercised = false;
                        List<DateTime> date_arr = new List<DateTime>();
                        string[] stk_pl_arr = new string[64];
                        string[] opt_pl_arr = new string[64];
                        float opt_pnl = 0;
                        int exercise = 1;
                        if (id_imnt == 10002 || id_imnt == 10013 || id_imnt == 10145)
                            exercise = 2; //american/european

                        ArrayList volresults = new ArrayList();
                        for (int i = 0; i <= _voldataCurr.Count-1; i++)
                        {
                            //PNL BY DAY
                            opt_pnl = 0;
                            float am_rate = 0;
                            List<Rates> rateshistArr = _rateshistArr.Where(x => x.dt_chg == _voldataCurr[i].dt_bus).ToList();
                            try
                            {
                                am_rate = _rateshistArr.FirstOrDefault(x => x.dt_chg >= _voldataCurr[i].dt_bus && x.am_days == 90).am_rate;
                            }
                            catch
                            {
                                goto SKIPLOOP;
                            }

                            am_chg = _voldataCurr[i].am_chg == null ? 0f : (float) _voldataCurr[i].am_chg;
                            
                            pr_und = (float)_voldataCurr[i].pr_close;
                            dt_bus = _voldataCurr[i].dt_bus;
                            List<Dividend_Hist> _divs = _dividends.Where(x => x.dt_ex == getFutureDate(dt_bus, 1)).ToList(); 
                            Array divArray = calcDiv(_dividends, dt_bus, dt_bus_expiry);

                            if (i == 0)
                            {
                                pr_strike = pr_und * (.9f + (k / 100f));
                                pr_close_clean = pr_und;
                            }
                            else
                                pr_close_clean = pr_close_clean_last + pr_close_clean_last * am_chg;

                            try
                            {
                                am_ivol_3m_100 = calcVolSPX(i, pr_close_clean, pr_strike, _voldataCurr[i].dt_bus, _voldataCurr[i]);
                                
                            }
                            catch
                            {
                                goto SKIPLOOP;
                            }

                            if (i == 0)
                                am_ivol_3m_100_init = am_ivol_3m_100;

                            float am_time = (dt_bus_expiry.Subtract(_voldataCurr[i].dt_bus).Days) / 365f;

                            /// FLOATING
                            /// 
                            /*
                            float am_strike_dist = .1f * pr_strike * am_ivol_3m_100 * (float) Math.Sqrt(am_time);
                            float am_vol_up = calcVolSPX(i, (float)_voldataCurr[i].pr_close, pr_strike + am_strike_dist, _voldataCurr[i].dt_bus, vd);
                            float am_vol_dn = calcVolSPX(i, (float)_voldataCurr[i].pr_close, pr_strike - am_strike_dist, _voldataCurr[i].dt_bus, vd);

                            float am_theo_up = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, 2, (float)_voldataCurr[i].pr_close, pr_strike + am_strike_dist, "C", dt_bus_expiry, am_rate, null, am_vol_up, "Theo", _spxdiv[dt_bus]);
                            float am_theo_dn = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, 2, (float)_voldataCurr[i].pr_close, pr_strike - am_strike_dist, "C", dt_bus_expiry, am_rate, null, am_vol_dn, "Theo", _spxdiv[dt_bus]);

                            float am_delta_call_floating = (am_theo_dn - am_theo_up) / (am_strike_dist * 2f);
                            float am_delta_put_floating = am_delta_call_floating - 1f;

                            float am_delta_floating = -am_cts * (am_delta_call_floating + am_delta_put_floating) * 100f;
                            */
                            ///


                            float am_delta_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);
                            float am_delta_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);
                            volresults.Add(am_delta_put);
                            
                            //PNL BY DAY
                            //pr_theo_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            //pr_theo_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            

                            //pr_theo_put = _mhmodel.CalcModel(dt_bus, 2, pr_und, pr_strike, "P", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);
                            if (i == 0)
                            {
                                am_cts = 10000f / _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Vega", _spxdiv[dt_bus], divArray) / 100f / 2f;
                                am_cts_call = am_cts;
                                am_theo_call_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                                am_theo_put_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            }

                            if (exercise == 1 && _divs.Count > 0 && option_exercised == false && i < 62) //early exercise?
                            {
                                if (i < _voldataCurr.Count - 1 &&_voldataCurr[i + 1].dt_bus == _divs[0].dt_ex)
                                {
                                    pr_theo_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                                    if (_divs[0].am_div > pr_theo_put)
                                    {
                                        //early exercise
                                        pr_theo_call = pr_close_clean - pr_strike;
                                        am_cts_call = 0;
                                        option_exercised = true;
                                    }
                                }
                            }

                            float am_delta = -am_cts * (am_delta_call + am_delta_put) * 100;
                            float am_delta_call_cts = -am_cts_call * am_delta_call * 100;
                            float am_delta_put_cts = -am_cts *am_delta_put * 100;

                            /// FLOATING
                            //float am_delta = -am_cts * (am_delta_call_floating + am_delta_put_floating) * 100;
                            ///

                            if (i > 0)
                            {
                                stk_pl = stk_pl + (pr_close_clean - pr_close_clean_last) * am_delta_prev;
                                stk_call_pl = stk_call_pl + (pr_close_clean - pr_close_clean_last) * am_delta_call_prev;
                                stk_put_pl = stk_put_pl + (pr_close_clean - pr_close_clean_last) * am_delta_put_prev;
                                //stk_pl = ((float)_voldataCurr[i].pr_close - (float)_voldataCurr[i - 1].pr_close) * am_delta_prev;
                                //opt_pnl = opt_pnl + ((pr_theo_call + pr_theo_put) - (pr_theo_call_last + pr_theo_put_last)) * am_cts * 100;
                            }

                            pr_theo_call_last = pr_theo_call;
                            pr_theo_put_last = pr_theo_put;
                            am_delta_prev = am_delta;
                            am_delta_call_prev = am_delta_call_cts;
                            am_delta_put_prev = am_delta_put_cts;
                            pr_close_clean_last = pr_close_clean;
                            //stk_pl_arr[i] = (stk_pl + opt_pnl).ToString();
                            //opt_pl_arr[i] = opt_pnl.ToString();
                            //date_arr.Add(dt_bus);
                            
                            //FileHelper.WriteLine("floatingbacktest" + _id_imnt_backtest_start.ToString() + ".txt", id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + am_ivol_3m_100.ToString());
                            //FileHelper.WriteLine("backtest.txt", (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + getFutureDate(dt_bus_expiry, -63).ToString() + "," + opt_pnl.ToString() + "," + stk_pl.ToString());
                            //FileHelper.WriteLine(str_file, id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus_expiry, -63) + "," + i + "," + (opt_pnl + stk_call_pl + stk_put_pl).ToString() + "," + am_ivol_3m_100_init);
                        }
                        if (!option_exercised)
                            pr_theo_call = _mhmodel.CalcModel(dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);

                        pr_theo_put = _mhmodel.CalcModel(dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);

                        opt_pnl = ((pr_theo_call + pr_theo_put) - (am_theo_call_first + am_theo_put_first)) * am_cts * 100;
                        
                        /* DEBUG
                        if (Convert.ToDouble(stk_pl) == double.NaN)
                        {
                            int p = 1;
                        }
                        */ 

                        //FileHelper.WriteLine("floatingbacktest" + _fileDate.ToString() + ".txt", id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus, -62).ToString() + "," + (opt_pnl + stk_pl).ToString());
                        FileHelper.WriteLine(str_file, id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + (opt_pnl + stk_call_pl + stk_put_pl).ToString() + "," + am_ivol_3m_100_init);

                        /*
                        string file_string = "";
                        results.Add(getFutureDate(dt_bus, -63), opt_pnl + stk_pl);
                        for (int i = 0; i <= 63; i++)
                        {
                            file_string = file_string + "," + stk_pl_arr[i];                    
                        }
                        FileHelper.WriteLine("backtest3.txt", (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus_expiry, -63).ToString() + file_string);
                         */
                    SKIPLOOP:
                        continue;
                        #endregion
                    }
                    #endregion
                }
            }
            FileHelper.Close(str_file);
        
        }


        private void runBackTest1m(DateTime dt_start, DateTime dt_end)
        {

            if (FileHelper.FileExists(str_path, str_file_1m))
            {
                FileHelper.CopyFile(str_path, str_file_1m,
                    str_path + @"\hist", "VOL_HIST_IMPORT_STRADDLE_SKEW" + _fileDate + ".txt");
            }
            FileHelper.CreateFile(str_path, str_file_1m);

            DateTime dt_start_und = getFutureDate(dt_start, -41);
            //DateTime dt_start_und = dt_start;
            DateTime dt_final = getFutureDate(dt_end, -41);

            foreach (int id_imnt_comp in _spxcomp)
            {
                if (id_imnt_comp == 10002)
                    _spxdiv = _dbhelper.GetSpxDiv("SPX");
                else if (id_imnt_comp == 10013)
                    _spxdiv = _dbhelper.GetSpxDiv("NDX");
                else if (id_imnt_comp == 10145)
                    _spxdiv = _dbhelper.GetSpxDiv("RUT");

                int id_imnt = id_imnt_comp; //basket
                //int id_imnt = 28333; //spx
                //_voldataArr = _dbhelper.GetVolData(id_imnt);
                // DateTime dt_start = new DateTime(2013,01,01);
                // DateTime dt_end = new DateTime(2013, 7,17);

                //DateTime dt_start_und = dt_start;
                Dictionary<DateTime, float> results = new Dictionary<DateTime, float>();


                DateTime dt_bus = new DateTime();
                List<VolData> _voldataList = _voldataArr.Where(x => x.id_imnt == id_imnt).ToList();
                _dividends = _dividends.Where(x => x.id_imnt == id_imnt).ToList();

                for (int k = 10; k <= 10; k += 5)
                {
                    #region for (int k = 0; k <= 20; k += 5)
                    //int k = 10;

                    foreach (VolData vd in _voldataList)
                    {
                        #region foreach (VolData vd in _voldataList)
                        DateTime dt_bus_expiry = getFutureDate(vd.dt_bus, 41);
                        if (vd.dt_bus > dt_final)
                            break;
                        List<VolData> _voldataCurr = _voldataList.Where(x => x.dt_bus >= vd.dt_bus && x.dt_bus <= dt_bus_expiry).ToList();
                        if (dt_bus_expiry != _voldataCurr[_voldataCurr.Count - 1].dt_bus)
                            goto SKIPLOOP;
                        float pr_strike = 0;
                        float am_cts = 0;
                        float am_cts_call = 0;
                        float am_cts_put = 0;
                        float am_theo_call_first = 0;
                        float am_theo_put_first = 0;
                        float stk_pl = 0;
                        float stk_call_pl = 0;
                        float stk_put_pl = 0;
                        float am_delta_prev = 0;
                        float am_delta_call_prev = 0;
                        float am_delta_put_prev = 0;
                        float pr_und = 0;
                        float am_ivol_3m_100 = 0;
                        float pr_theo_call = 0;
                        float pr_theo_put = 0;
                        float pr_theo_call_last = 0;
                        float pr_theo_put_last = 0;
                        float am_ivol_3m_100_init = 0;
                        float am_chg = 0;
                        float pr_close_clean = 0;
                        float pr_close_clean_last = 0;
                        float am_pl_21 = 0;
                        float vega_pnl = 0;
                        bool option_exercised = false;
                        List<DateTime> date_arr = new List<DateTime>();
                        string[] stk_pl_arr = new string[64];
                        string[] opt_pl_arr = new string[64];
                        float opt_pnl = 0;
                        int exercise = 1;
                        if (id_imnt == 10002 || id_imnt == 10013 || id_imnt == 10145)
                            exercise = 2; //american/european

                        ArrayList volresults = new ArrayList();
                        float am_vega_21 = 0;
                        float am_ivol_3m_100_21 = 0;

                        for (int i = 0; i <= _voldataCurr.Count - 1; i++)
                        {
                            //PNL BY DAY
                            opt_pnl = 0;
                            float am_rate = 0;
                            List<Rates> rateshistArr = _rateshistArr.Where(x => x.dt_chg == _voldataCurr[i].dt_bus).ToList();
                            try
                            {
                                am_rate = _rateshistArr.FirstOrDefault(x => x.dt_chg >= _voldataCurr[i].dt_bus && x.am_days == 90).am_rate;
                            }
                            catch
                            {
                                goto SKIPLOOP;
                            }

                            am_chg = _voldataCurr[i].am_chg == null ? 0f : (float)_voldataCurr[i].am_chg;

                            pr_und = (float)_voldataCurr[i].pr_close;
                            dt_bus = _voldataCurr[i].dt_bus;
                            List<Dividend_Hist> _divs = _dividends.Where(x => x.dt_ex == getFutureDate(dt_bus, 1)).ToList();
                            Array divArray = calcDiv(_dividends, dt_bus, dt_bus_expiry);

                            if (i == 0)
                            {
                                pr_strike = pr_und * (.9f + (k / 100f));
                                pr_close_clean = pr_und;
                            }
                            else
                                pr_close_clean = pr_close_clean_last + pr_close_clean_last * am_chg;

                            try
                            {
                                am_ivol_3m_100 = calcVolSPX(i, pr_close_clean, pr_strike, _voldataCurr[i].dt_bus, _voldataCurr[i]);

                            }
                            catch
                            {
                                goto SKIPLOOP;
                            }

                            if (i == 0)
                                am_ivol_3m_100_init = am_ivol_3m_100;

                            float am_time = (dt_bus_expiry.Subtract(_voldataCurr[i].dt_bus).Days) / 365f;

                            float am_delta_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);
                            float am_delta_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);
                            volresults.Add(am_delta_put);

                            //PNL BY DAY
                            pr_theo_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            pr_theo_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            
                            if (i == 0)
                            {
                                am_cts = 10000f / _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Vega", _spxdiv[dt_bus], divArray) / 100f / 2f;
                                am_cts_call = am_cts;
                                am_theo_call_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                                am_theo_put_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            }

                            if (exercise == 1 && _divs.Count > 0 && option_exercised == false && i < 62) //early exercise?
                            {
                                if (i < _voldataCurr.Count - 1 && _voldataCurr[i + 1].dt_bus == _divs[0].dt_ex)
                                {
                                    pr_theo_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                                    if (_divs[0].am_div > pr_theo_put)
                                    {
                                        //early exercise
                                        pr_theo_call = pr_close_clean - pr_strike;
                                        am_cts_call = 0;
                                        option_exercised = true;
                                    }
                                }
                            }

                            float am_delta = -am_cts * (am_delta_call + am_delta_put) * 100;
                            float am_delta_call_cts = -am_cts_call * am_delta_call * 100;
                            float am_delta_put_cts = -am_cts * am_delta_put * 100;

                            if (i < 21)
                            {
                                stk_pl = stk_pl + (pr_close_clean - pr_close_clean_last) * am_delta_prev;
                                stk_call_pl = stk_call_pl + (pr_close_clean - pr_close_clean_last) * am_delta_call_prev;
                                stk_put_pl = stk_put_pl + (pr_close_clean - pr_close_clean_last) * am_delta_put_prev;
                                //stk_pl = ((float)_voldataCurr[i].pr_close - (float)_voldataCurr[i - 1].pr_close) * am_delta_prev;
                                //opt_pnl = opt_pnl + ((pr_theo_call + pr_theo_put) - (pr_theo_call_last + pr_theo_put_last)) * am_cts * 100;
                                pr_theo_call_last = pr_theo_call;
                                pr_theo_put_last = pr_theo_put;
                            }

                            
                            if (i == 20)
                            {
                                opt_pnl = ((pr_theo_call + pr_theo_put) - (am_theo_call_first + am_theo_put_first)) * am_cts * 100;
                                am_pl_21 = opt_pnl + stk_call_pl + stk_put_pl;
                                am_vega_21 = am_cts * 100 * _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Vega", _spxdiv[dt_bus], divArray) * 2;
                                am_ivol_3m_100_21 = am_ivol_3m_100;
                            }

                            if (i == 41)
                            {
                                float am_ivol_3m_100_final = calcVolSPX(i, pr_close_clean, pr_strike, _voldataCurr[i].dt_bus, _voldataCurr[i]);
                                vega_pnl = (am_ivol_3m_100_final - am_ivol_3m_100_21) * am_vega_21 * 100;
                            }

                            pr_theo_call_last = pr_theo_call;
                            pr_theo_put_last = pr_theo_put;
                            am_delta_prev = am_delta;
                            am_delta_call_prev = am_delta_call_cts;
                            am_delta_put_prev = am_delta_put_cts;
                            pr_close_clean_last = pr_close_clean;
                            //stk_pl_arr[i] = (stk_pl + opt_pnl).ToString();
                            //opt_pl_arr[i] = opt_pnl.ToString();
                            //date_arr.Add(dt_bus);

                            //FileHelper.WriteLine("floatingbacktest" + _id_imnt_backtest_start.ToString() + ".txt", id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + am_ivol_3m_100.ToString());
                            //FileHelper.WriteLine("backtest.txt", (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + getFutureDate(dt_bus_expiry, -63).ToString() + "," + opt_pnl.ToString() + "," + stk_pl.ToString());
                            //FileHelper.WriteLine(str_file, id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus_expiry, -63) + "," + i + "," + (opt_pnl + stk_call_pl + stk_put_pl).ToString() + "," + am_ivol_3m_100_init);
                        }
                        if (!option_exercised)
                            pr_theo_call = _mhmodel.CalcModel(dt_bus, exercise, pr_close_clean, pr_strike, "C", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);

                        pr_theo_put = _mhmodel.CalcModel(dt_bus, exercise, pr_close_clean, pr_strike, "P", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);

                        //opt_pnl = ((pr_theo_call + pr_theo_put) - (am_theo_call_first + am_theo_put_first)) * am_cts * 100;
                       

                        //FileHelper.WriteLine("floatingbacktest" + _fileDate.ToString() + ".txt", id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus, -62).ToString() + "," + (opt_pnl + stk_pl).ToString());
                        FileHelper.WriteLine(str_file_1m, id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + (am_pl_21 + vega_pnl).ToString() + "," + am_ivol_3m_100_init);

                    SKIPLOOP:
                        continue;
                        #endregion
                    }
                    #endregion
                }
            }
            FileHelper.Close(str_file_1m);

        }

        private float calcVolSPX(int timestep,float pr_und,float pr_strike, DateTime dt_bus, VolData vd)
        {
            float? am_vol_atm;
            float? am_vol_95;
            float? am_vol_105;
            float? am_skew;
            float am_skew_dn = 0;
            float am_skew_up = 0;
            float am_vol;
            if (vd.am_ivol_2m_100 == null) //interpolate between 1-3month
            {
                vd.am_ivol_2m_100 = (vd.am_ivol_3m_100 + vd.am_ivol_1m_100) / 2;
            }

            if (timestep >= 0 && timestep <= 20)
            {

                am_vol_atm = (vd.am_ivol_3m_100 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_100 * (timestep / 20f));

                //if ((vd.am_ivol_2m_100 == null || vd.am_ivol_2m_95 == null) && timestep > 0)
                //{
                if (timestep == 0)
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                }
                //}
                //else
                //{
                //    am_vol_95 = (vd.am_ivol_3m_95 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_95 * (timestep / 20f));
                //    am_vol_105 = (vd.am_ivol_3m_105 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_105 * (timestep / 20f));
                //}
            }
            else if (timestep >= 21 && timestep <= 41)
            {
      
                am_vol_atm = (vd.am_ivol_2m_100 * ((41f - timestep) / 20f)) + (vd.am_ivol_1m_100 * ((timestep - 21f) / 20f));
                //am_vol_95 = (vd.am_ivol_2m_95 * ((41 - timestep) / 20)) + (vd.am_ivol_1m_95 * ((timestep - 21) / 20));
                //am_vol_105 = (vd.am_ivol_2m_105 * ((41 - timestep) / 20)) + (vd.am_ivol_1m_105 * ((timestep - 21) / 20));
                /*if (timestep == 21)
                {
                    am_skew_dn = ((((float)vd.am_ivol_3m_95 + (float)vd.am_ivol_1m_95) / 2f) - (float)am_vol_atm);
                    am_skew_up = ((((float)vd.am_ivol_3m_105 + (float)vd.am_ivol_1m_105) / 2f) - (float)am_vol_atm);
                }
                else
                {
                    am_skew_dn = ((((float)vd.am_ivol_3m_95 + (float)vd.am_ivol_1m_95) / 2f) - (float)am_vol_atm) / (float)Math.Sqrt((float)(63f - timestep) / 42f);
                    am_skew_up = ((((float)vd.am_ivol_3m_105 + (float)vd.am_ivol_1m_105) / 2f) - (float)am_vol_atm) / (float)Math.Sqrt((float)(63f - timestep) / 42f);
                }
                 */
                if (timestep == 0)
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                }
            }
            else
            {
                am_vol_atm = (vd.am_ivol_1m_100) - ((vd.am_ivol_2m_100 - vd.am_ivol_1m_100) * ((timestep - 42f) / 20f));
                //am_vol_95 = (vd.am_ivol_1m_95) - ((vd.am_ivol_2m_95 - vd.am_ivol_1m_95) * ((timestep - 42) / 20));
                //am_vol_105 = (vd.am_ivol_1m_105) - ((vd.am_ivol_2m_105 - vd.am_ivol_1m_105) * ((timestep - 42) / 20));
                if (timestep == 42)
                {
                    am_skew_dn = ((float)vd.am_ivol_1m_95 - (float)vd.am_ivol_1m_100);
                    am_skew_up = ((float)vd.am_ivol_1m_105 - (float)vd.am_ivol_1m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_1m_95 - (float)vd.am_ivol_1m_100) / (float)Math.Sqrt((float)(63f - timestep) / 21f);
                    am_skew_up = ((float)vd.am_ivol_1m_105 - (float)vd.am_ivol_1m_100) / (float)Math.Sqrt((float)(63f - timestep) / 21f);
                }
            }

            /*
            if (am_vol_95 == null || am_vol_105 == null)
                am_skew = 0;
            else if (pr_strike <= pr_und)
                am_skew = am_vol_95 - am_vol_atm;
            else
                am_skew = am_vol_105 - am_vol_atm;
            */
            am_skew_dn = (((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100) * 2 - (timestep / 62f) * .015f) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
            am_skew_up = (((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100) * 2 + (timestep / 62f) * .015f) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
            /*
            if (timestep >= 58)
            {
                am_skew_dn = (((float)vd.am_ivol_1m_95 - (float)vd.am_ivol_1m_100) * 2f - (timestep / 62f) * .015f) / (float)Math.Sqrt((float)(63f - timestep) / 23f);
                am_skew_up = (((float)vd.am_ivol_1m_105 - (float)vd.am_ivol_1m_100) * 2f + (timestep / 62f) * .015f) / (float)Math.Sqrt((float)(63f - timestep) / 23f);
            }
            */
            if (pr_strike <= pr_und)
                am_skew = (float) Math.Min(am_skew_dn,.1);
            else
                am_skew = (float) Math.Max(am_skew_up, -.1);
            
            am_vol = (float)(am_vol_atm + am_skew * Math.Abs((float)pr_und - (float)pr_strike) / (0.1f * pr_und));

            if (am_vol <= 0.08) 
                am_vol = 0.08f;
            
            return am_vol;
        }

        private void runBackTestSABR()
        {
            //_sabr.OptionSABReup();
            FileHelper.CreateFile(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log", "backtestSABR" + _fileDate.ToString() + ".txt");
            int id_imnt = 10013; //spx

            //_voldataArr = _dbhelper.GetVolData(id_imnt);
            DateTime dt_start = new DateTime(1996, 01, 04);
            DateTime dt_end = new DateTime(2013, 03, 26);
            //DateTime dt_start_und = getFutureDate(dt_start, -63);
            DateTime dt_start_und = dt_start;
            Dictionary<DateTime, float> results = new Dictionary<DateTime, float>();
            DateTime dt_final = getFutureDate(dt_end, -63);

            double[] volSABR;

            DateTime dt_bus = new DateTime();
            List<VolData> _voldataList = _voldataArr.Where(x => x.dt_bus >= dt_start_und && x.dt_bus <= dt_final).ToList();
            _dividends = _dividends.Where(x => x.id_imnt == id_imnt).ToList();

            for (int k = 0; k <= 20; k += 5)
            {
                //int k = 10;

                foreach (VolData vd in _voldataList)
                {
                    DateTime dt_bus_expiry = getFutureDate(vd.dt_bus, 63);
                    List<VolData> _voldataCurr = _voldataArr.Where(x => x.dt_bus >= vd.dt_bus && x.dt_bus <= dt_bus_expiry).ToList();
                    float pr_strike = 0;
                    float am_cts = 0;
                    float am_theo_call_first = 0;
                    float am_theo_put_first = 0;
                    float stk_pl = 0;
                    float am_delta_prev = 0;
                    float pr_und = 0;
                    float am_ivol_3m_100 = 0;
                    float pr_theo_call = 0;
                    float pr_theo_put = 0;
                    float pr_theo_call_last = 0;
                    float pr_theo_put_last = 0;
                    List<DateTime> date_arr = new List<DateTime>();
                    string[] stk_pl_arr = new string[64];
                    string[] opt_pl_arr = new string[64];
                    float opt_pnl = 0;
                    int exercise = 1; //american/european

                    for (int i = 0; i <= 63; i++)
                    {

                        opt_pnl = 0;
                        float am_rate = 0;
                        //List<Rates> rateshistArr = _rateshistArr.Where(x => x.dt_chg == _voldataCurr[i].dt_bus).ToList();
                        try
                        {
                            am_rate = _rateshistArr.FirstOrDefault(x => x.dt_chg >= _voldataCurr[i].dt_bus && x.am_days == 90).am_rate;
                        }
                        catch
                        {
                            goto SKIPLOOP;
                        }
                        pr_und = (float)_voldataCurr[i].pr_close;
                        dt_bus = _voldataCurr[i].dt_bus;

                        Array divArray = calcDiv(_dividends, dt_bus, dt_bus_expiry);

                        if (i == 0)
                            pr_strike = pr_und * (.9f + (k / 100f));
                        try
                        {
                            volSABR = calcVolSABR(i, (float)_voldataCurr[i].pr_close, pr_strike, _voldataCurr[i].dt_bus, _voldataCurr[i], dt_bus_expiry);
                        }
                        catch
                        {
                            goto SKIPLOOP;
                        }
                        float am_time = (dt_bus_expiry.Subtract(_voldataCurr[i].dt_bus).Days) / 365f;

                        /// FLOATING
                        /// 
                        /*
                        float am_strike_dist = .1f * pr_strike * am_ivol_3m_100 * (float) Math.Sqrt(am_time);
                        float am_vol_up = calcVolSPX(i, (float)_voldataCurr[i].pr_close, pr_strike + am_strike_dist, _voldataCurr[i].dt_bus, vd);
                        float am_vol_dn = calcVolSPX(i, (float)_voldataCurr[i].pr_close, pr_strike - am_strike_dist, _voldataCurr[i].dt_bus, vd);

                        float am_theo_up = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, 2, (float)_voldataCurr[i].pr_close, pr_strike + am_strike_dist, "C", dt_bus_expiry, am_rate, null, am_vol_up, "Theo", _spxdiv[dt_bus]);
                        float am_theo_dn = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, 2, (float)_voldataCurr[i].pr_close, pr_strike - am_strike_dist, "C", dt_bus_expiry, am_rate, null, am_vol_dn, "Theo", _spxdiv[dt_bus]);

                        float am_delta_call_floating = (am_theo_dn - am_theo_up) / (am_strike_dist * 2f);
                        float am_delta_put_floating = am_delta_call_floating - 1f;

                        float am_delta_floating = -am_cts * (am_delta_call_floating + am_delta_put_floating) * 100f;
                        */
                        ///

                        float am_delta_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);
                        float am_delta_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", _spxdiv[dt_bus], divArray);

                        am_delta_call = GetSABRDelta(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Delta", volSABR, _spxdiv[dt_bus], divArray);
                        pr_theo_call = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                        pr_theo_put = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);

                        if (i == 0)
                        {
                            am_cts = 10000f / _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Vega", _spxdiv[dt_bus], divArray) / 100f / 2f;
                            am_theo_call_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "C", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                            am_theo_put_first = _mhmodel.CalcModel(_voldataCurr[i].dt_bus, exercise, (float)_voldataCurr[i].pr_close, pr_strike, "P", dt_bus_expiry, am_rate, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus], divArray);
                        }

                        float am_delta = -am_cts * (am_delta_call + am_delta_put) * 100;

                        //float am_delta = -am_cts * (am_delta_call_floating + am_delta_put_floating) * 100;

                        if (i > 0)
                        {
                            stk_pl = stk_pl + ((float)_voldataCurr[i].pr_close - (float)_voldataCurr[i - 1].pr_close) * am_delta_prev;
                            //stk_pl = ((float)_voldataCurr[i].pr_close - (float)_voldataCurr[i - 1].pr_close) * am_delta_prev;
                            opt_pnl = ((pr_theo_call + pr_theo_put) - (pr_theo_call_last + pr_theo_put_last)) * am_cts * 100;
                        }
                        pr_theo_call_last = pr_theo_call;
                        pr_theo_put_last = pr_theo_put;
                        am_delta_prev = am_delta;
                        stk_pl_arr[i] = (stk_pl + opt_pnl).ToString();
                        opt_pl_arr[i] = opt_pnl.ToString();
                        //date_arr.Add(dt_bus);

                        //FileHelper.WriteLine("backtest.txt", (.9f + (k / 100f)).ToString() + "," + dt_bus.ToString() + "," + getFutureDate(dt_bus_expiry, -63).ToString() + "," + opt_pnl.ToString() + "," + stk_pl.ToString());
                    }
                    pr_theo_call = _mhmodel.CalcModel(dt_bus, 2, pr_und, pr_strike, "C", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);
                    pr_theo_put = _mhmodel.CalcModel(dt_bus, 2, pr_und, pr_strike, "P", dt_bus_expiry, 0f, null, am_ivol_3m_100, "Theo", _spxdiv[dt_bus]);

                    opt_pnl = ((pr_theo_call + pr_theo_put) - (am_theo_call_first + am_theo_put_first)) * am_cts * 100;

                    FileHelper.WriteLine("backtestSABR" + _fileDate.ToString() + ".txt", id_imnt + "," + (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus, -63).ToString() + "," + (opt_pnl + stk_pl).ToString());
                /*
                string file_string = "";
                results.Add(getFutureDate(dt_bus, -63), opt_pnl + stk_pl);
                for (int i = 0; i <= 63; i++)
                {
                    file_string = file_string + "," + stk_pl_arr[i];                    
                }
                FileHelper.WriteLine("backtest3.txt", (.9f + (k / 100f)).ToString() + "," + getFutureDate(dt_bus_expiry, -63).ToString() + file_string);
                    */
                SKIPLOOP:
                    continue;
                }

            }


        }

        private double[] calcVolSABR(int timestep, float pr_und, float pr_strike, DateTime dt_bus, VolData vd, DateTime dt_bus_expiry)
        {
            /*
            float? am_vol_atm;
            float? am_vol_95;
            float? am_vol_105;
            float? am_skew;
            float am_skew_dn = 0;
            float am_skew_up = 0;
            float am_vol;
            if (vd.am_ivol_2m_100 == null) //interpolate between 1-3month
            {
                vd.am_ivol_2m_100 = (vd.am_ivol_3m_100 + vd.am_ivol_1m_100) / 2;
            }

            if (timestep >= 0 && timestep <= 20)
            {

                am_vol_atm = (vd.am_ivol_3m_100 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_100 * (timestep / 20f));

                //if ((vd.am_ivol_2m_100 == null || vd.am_ivol_2m_95 == null) && timestep > 0)
                //{
                if (timestep == 0)
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                }
                //}
                //else
                //{
                //    am_vol_95 = (vd.am_ivol_3m_95 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_95 * (timestep / 20f));
                //    am_vol_105 = (vd.am_ivol_3m_105 * ((20f - timestep) / 20f)) + (vd.am_ivol_2m_105 * (timestep / 20f));
                //}
            }
            else if (timestep >= 21 && timestep <= 41)
            {

                am_vol_atm = (vd.am_ivol_2m_100 * ((41f - timestep) / 20f)) + (vd.am_ivol_1m_100 * ((timestep - 21f) / 20f));

                if (timestep == 0)
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_3m_95 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                    am_skew_up = ((float)vd.am_ivol_3m_105 - (float)vd.am_ivol_3m_100) / (float)Math.Sqrt((float)(63f - timestep) / 63f);
                }
            }
            else
            {
                am_vol_atm = (vd.am_ivol_1m_100) - ((vd.am_ivol_2m_100 - vd.am_ivol_1m_100) * ((timestep - 42f) / 20f));
                //am_vol_95 = (vd.am_ivol_1m_95) - ((vd.am_ivol_2m_95 - vd.am_ivol_1m_95) * ((timestep - 42) / 20));
                //am_vol_105 = (vd.am_ivol_1m_105) - ((vd.am_ivol_2m_105 - vd.am_ivol_1m_105) * ((timestep - 42) / 20));
                if (timestep == 42)
                {
                    am_skew_dn = ((float)vd.am_ivol_1m_95 - (float)vd.am_ivol_1m_100);
                    am_skew_up = ((float)vd.am_ivol_1m_105 - (float)vd.am_ivol_1m_100);
                }
                else
                {
                    am_skew_dn = ((float)vd.am_ivol_1m_95 - (float)vd.am_ivol_1m_100) / (float)Math.Sqrt((float)(63f - timestep) / 21f);
                    am_skew_up = ((float)vd.am_ivol_1m_105 - (float)vd.am_ivol_1m_100) / (float)Math.Sqrt((float)(63f - timestep) / 21f);
                }
            }

            if (pr_strike <= pr_und)
                am_skew = (float)Math.Min(am_skew_dn, .1);
            else
                am_skew = (float)Math.Max(am_skew_up, -.1);

            am_vol = (float)(am_vol_atm + am_skew * Math.Abs((float)pr_und - (float)pr_strike) / (0.05f * pr_und));

            if (am_vol <= 0.08)
                am_vol = 0.08f;

            double[,] smile_tbl = new double[7, 3];

            smile_tbl[0, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[0, 1] = pr_und * .85;
            smile_tbl[0, 2] = (float)(am_vol_atm + am_skew_dn * Math.Abs((float)(pr_und) - (float)(pr_und * .85)) / (0.05f * (pr_und)));
            smile_tbl[1, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[1, 1] = pr_und * .90;
            smile_tbl[1, 2] = (float)(am_vol_atm + am_skew_dn * Math.Abs((float)(pr_und) - (float)(pr_und * .9)) / (0.05f * (pr_und)));
            smile_tbl[2, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[2, 1] = pr_und * .95;
            smile_tbl[2, 2] = (float)(am_vol_atm + am_skew_dn * Math.Abs((float)(pr_und) - (float)(pr_und * .95)) / (0.05f * (pr_und)));
            smile_tbl[3, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[3, 1] = pr_und * 1;
            smile_tbl[3, 2] = (float)am_vol_atm;         
            smile_tbl[4, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[4, 1] = pr_und * 1.05;
            smile_tbl[4, 2] = (float)(am_vol_atm + am_skew_up * Math.Abs((float)(pr_und) - (float)(pr_und * 1.05)) / (0.05f * (pr_und)));
            smile_tbl[5, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[5, 1] = pr_und * 1.1;
            smile_tbl[5, 2] = (float)(am_vol_atm + am_skew_up * Math.Abs((float)(pr_und) - (float)(pr_und * 1.1)) / (0.05f * (pr_und)));
            smile_tbl[6, 0] = DateTime.Parse(dt_bus_expiry.ToString()).ToOADate(); 
            smile_tbl[6, 1] = pr_und * 1.15;
            smile_tbl[6, 2] = (float)(am_vol_atm + am_skew_up * Math.Abs((float)(pr_und) - (float)(pr_und * 1.15)) / (0.05f * (pr_und)));

            double[,] df_crv_std = new double[1, 1];
            df_crv_std[0,0] = .05;
            double[,] df_crv_hld = new double[1, 1];
            df_crv_hld[0,0] = .05;

            int intrp = 3;
            double[,] param_rng = new double[2, 4];
            double[,] param_ini = new double[1, 4];
            int min_method;
            double[,] min_param = new double[1, 5];
            int error_metric;
            int weighting;
            int table_type;
            double[] Return_calib_tbl = new double[1];

            param_rng[0, 0] = 0.1;
            param_rng[1, 0] = .5;
            param_rng[0, 1] = 0;
            param_rng[1, 1] = .99;
            param_rng[0, 2] = -0.999;
            param_rng[1, 2] = 0;
            param_rng[0, 3] = 0;                                   
            param_rng[1, 3] = 2;

            param_ini[0, 0] = 0.75;
            param_ini[0, 1] = 0.75;
            param_ini[0, 2] = -0.5;
            param_ini[0, 3] = 0.2;

            min_method = 3;

            min_param[0, 0] = 2000.5;
            min_param[0, 1] = 1e-010;
            min_param[0, 2] = 15;
            min_param[0, 3] = 0.8;
            min_param[0, 4] = 0.5;

            error_metric = 1;
            weighting = 2;
            table_type = 2;

            double price_u = pr_und;
            double d_mkt = DateTime.Parse(dt_bus.ToString()).ToOADate();
            int smile_type = 1;
            FincadFunctions.fc_app_init();
            int status = FincadFunctions.aaCalibrateOptions_SABR(price_u, d_mkt, smile_type, smile_tbl, df_crv_std, df_crv_hld, intrp, param_rng, param_ini, min_method, min_param, error_metric, weighting, table_type, ref Return_calib_tbl);
            string output = "";
            int xrows;
            double temp;
            int i = 0;
            //int status = 1;
            if (status != 0)
            {
                output = "Calculation failed";
            }
            else
            {
                // Display the results
                output = "Calculation succeeded: \r\n";
                xrows = Return_calib_tbl.GetLength(0);
                // Limit results to 50 rows
                if (xrows > 50)
                {
                    xrows = 50;
                }

                for (i = 1; i <= xrows; ++i)
                {
                    temp = ((double[])Return_calib_tbl)[i - 1];
                    output = string.Concat(output, temp.ToString() + "\r\n", "");
                }
            }
            //FincadFunctions.fc_app_exit();
*/
            double[] Return_calib_tbl = new double[1];
            return Return_calib_tbl;

        }

        public float GetSABRDelta(DateTime dt_val, int exercise, float pr_und, float pr_strike, string id_pc, DateTime dt_mat, float am_rate, List<Dividend> dividendArr, float am_vol, string calc_type, double[] param_tbl, float am_div_yld_12m = 0f, Array _divArray = null)
        {
            double[,] strike_tbl = new double [1,1];
            double[,] df_crv_std = new double [1,1];
            double[,] df_crv_hld = new double [1,1];
            int[] stat = new int [1];

            int payoff_type = 0;
            if (id_pc == "C")
                payoff_type = 2;
            else
                payoff_type = 3;

            strike_tbl[0,0] = pr_strike;
            df_crv_std[0,0] = am_rate;
            df_crv_hld[0,0] = 0.0001;
            stat[0] = 2;

            return (float) OptionSABReup(Convert.ToDouble(pr_und), payoff_type, strike_tbl, DateTime.Parse(dt_mat.ToString()).ToOADate(), DateTime.Parse(dt_val.ToString()).ToOADate(), param_tbl, df_crv_std, df_crv_hld, 1, stat); ;
        }

        public double OptionSABReup(double price_u, int payoff_type, double[,] strike_tbl, double d_exp, double d_v, double[] param_tbl2, double[,] df_crv_std, double[,] df_crv_hld, int intrp, int[] stat)
        {
            //status = FincadFunctions.aaOption_SABR_eu_p(price_u, payoff_type, strike_tbl, d_exp, d_v, param_tbl, df_crv_std, df_crv_hld, intrp, stat, ref Return_stat);
            //double price_u;
            //int payoff_type;
            //double[,] strike_tbl = new double[1, 1];
            //double d_exp;
            //double d_v;
            double[,] param_tbl = new double[1, 4];
            //double[,] df_crv_std = new double[20, 2];
            //double[,] df_crv_hld = new double[20, 2];
            //int intrp;
            //int[] stat = new int[10];
            double[] Return_stat = new double[1];

            // Initialize the input variables
            param_tbl[0, 0] = param_tbl2[4];
            param_tbl[0, 1] = param_tbl2[5];
            param_tbl[0, 2] = param_tbl2[6];
            param_tbl[0, 3] = param_tbl2[7];

            #region old code
            /*
            price_u = 50;
            payoff_type = 2;

            strike_tbl[0, 0] = 50;

            d_exp = 39995;
            d_v = 39814;
            */


            /*
            df_crv_std[0, 0] = 39814;
            df_crv_std[0, 1] = 1;
            df_crv_std[1, 0] = 39995;
            df_crv_std[1, 1] = 0.976095768;
            df_crv_std[2, 0] = 40179;
            df_crv_std[2, 1] = 0.952380952;
            df_crv_std[3, 0] = 40360;
            df_crv_std[3, 1] = 0.929615017;
            df_crv_std[4, 0] = 40544;
            df_crv_std[4, 1] = 0.907029478;
            df_crv_std[5, 0] = 40725;
            df_crv_std[5, 1] = 0.885347635;
            df_crv_std[6, 0] = 40909;
            df_crv_std[6, 1] = 0.863837599;
            df_crv_std[7, 0] = 41091;
            df_crv_std[7, 1] = 0.8430755210000001;
            df_crv_std[8, 0] = 41275;
            df_crv_std[8, 1] = 0.82259251;
            df_crv_std[9, 0] = 41456;
            df_crv_std[9, 1] = 0.802929068;
            df_crv_std[10, 0] = 41640;
            df_crv_std[10, 1] = 0.783421438;
            df_crv_std[11, 0] = 41821;
            df_crv_std[11, 1] = 0.76469435;
            df_crv_std[12, 0] = 42005;
            df_crv_std[12, 1] = 0.746115655;
            df_crv_std[13, 0] = 42186;
            df_crv_std[13, 1] = 0.728280334;
            df_crv_std[14, 0] = 42370;
            df_crv_std[14, 1] = 0.710586339;
            df_crv_std[15, 0] = 42552;
            df_crv_std[15, 1] = 0.693507609;
            df_crv_std[16, 0] = 42736;
            df_crv_std[16, 1] = 0.676658438;
            df_crv_std[17, 0] = 42917;
            df_crv_std[17, 1] = 0.660483437;
            df_crv_std[18, 0] = 43101;
            df_crv_std[18, 1] = 0.6444366070000001;
            df_crv_std[19, 0] = 43282;
            df_crv_std[19, 1] = 0.629031845;


            df_crv_hld[0, 0] = 39814;
            df_crv_hld[0, 1] = 1;
            df_crv_hld[1, 0] = 39995;
            df_crv_hld[1, 1] = 0.976095768;
            df_crv_hld[2, 0] = 40179;
            df_crv_hld[2, 1] = 0.952380952;
            df_crv_hld[3, 0] = 40360;
            df_crv_hld[3, 1] = 0.929615017;
            df_crv_hld[4, 0] = 40544;
            df_crv_hld[4, 1] = 0.907029478;
            df_crv_hld[5, 0] = 40725;
            df_crv_hld[5, 1] = 0.885347635;
            df_crv_hld[6, 0] = 40909;
            df_crv_hld[6, 1] = 0.863837599;
            df_crv_hld[7, 0] = 41091;
            df_crv_hld[7, 1] = 0.8430755210000001;
            df_crv_hld[8, 0] = 41275;
            df_crv_hld[8, 1] = 0.82259251;
            df_crv_hld[9, 0] = 41456;
            df_crv_hld[9, 1] = 0.802929068;
            df_crv_hld[10, 0] = 41640;
            df_crv_hld[10, 1] = 0.783421438;
            df_crv_hld[11, 0] = 41821;
            df_crv_hld[11, 1] = 0.76469435;
            df_crv_hld[12, 0] = 42005;
            df_crv_hld[12, 1] = 0.746115655;
            df_crv_hld[13, 0] = 42186;
            df_crv_hld[13, 1] = 0.728280334;
            df_crv_hld[14, 0] = 42370;
            df_crv_hld[14, 1] = 0.710586339;
            df_crv_hld[15, 0] = 42552;
            df_crv_hld[15, 1] = 0.693507609;
            df_crv_hld[16, 0] = 42736;
            df_crv_hld[16, 1] = 0.676658438;
            df_crv_hld[17, 0] = 42917;
            df_crv_hld[17, 1] = 0.660483437;
            df_crv_hld[18, 0] = 43101;
            df_crv_hld[18, 1] = 0.6444366070000001;
            df_crv_hld[19, 0] = 43282;
            df_crv_hld[19, 1] = 0.629031845;
            
            intrp = 1;

            for (i = 1; i <= 10; i++)
            {
                stat[i - 1] = i;
            }
            */
            #endregion code

            // Call the member function
            /*
            status = FincadFunctions.aaOption_SABR_eu_p(price_u, payoff_type, strike_tbl, d_exp, d_v, param_tbl, df_crv_std, df_crv_hld, intrp, stat, ref Return_stat);
            if (status != 0)
            {
                output = "Calculation failed";
            }
            else
            {
                // Display the results
                output = "Calculation succeeded: \r\n";
                xrows = Return_stat.GetLength(0);
                // Limit results to 50 rows
                if (xrows > 50)
                {
                    xrows = 50;
                }

                for (i = 1; i <= xrows; ++i)
                {
                    temp = ((double[])Return_stat)[i - 1];
                    output = string.Concat(output, temp.ToString() + "\r\n", "");
                }
            }
            //MessageBox.Show(output, "FINCAD Analytics Suite for Developers", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            FincadFunctions.fc_app_exit();
             */
            return Return_stat[0];
        }

        private DateTime getFutureDate(DateTime dt_bus, int days)
        {
            int id_ord_start;
            int id_ord_next;

            id_ord_start = _undpricedate.FirstOrDefault(x => x.Key >= dt_bus).Value;
            id_ord_next = id_ord_start + days;

            return _undpricedate.FirstOrDefault(x => x.Value == id_ord_next).Key;
        }

        private Array calcDiv(List<Dividend_Hist> dividend, DateTime dt_val, DateTime dt_mat)
        {
            //DateTime dt_val;
            //DateTime dt_mat;
            double am_time;
            Array divArrayResults = new double[2];
            int i = 0;

            //dt_val = DateTime.Today;

            ////dt_mat = (DateTime)item.dt_mat;

            am_time = (dt_mat.Subtract(dt_val).Days) / 365f;

            if (am_time < 0 || dividend == null || dividend.Count == 0)
                return null;

            divArrayResults = new double[dividend.Count * 2];

            int j = 0;
            for (i = 0; i < dividend.Count; i++)
            {
                if (dividend[i].dt_ex > dt_val && dividend[i].dt_ex <= dt_mat)
                {
                    am_time = (((DateTime)dividend[i].dt_ex).Subtract(dt_val).Days) / 365f;
                    divArrayResults.SetValue(am_time, j);
                    divArrayResults.SetValue(dividend[i].am_div, j + 1);
                    j += 2;
                }
            }

            return divArrayResults;
        }
    }
}
