using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
namespace wpfexample
{
    class SkewAnalysis
    {
        private List<Position> _posArr;
        private List<Portfolio> _prtfArr;
        private List<VolParam> _volparamArr;
        private List<VolParam> _volparamshockArr;
        private Dictionary<int, Instrument> _instArr;
        private List<Rates> _ratesArr;
        private List<Dividend> _divArr;
        private List<IndexData> _indexdataArr = new List<IndexData>();
        private DBHelper _dbhelper = new DBHelper();
        private Dictionary<int, float> _instbeta = new Dictionary<int, float>();
        Dictionary<int, double?> tickerList = new Dictionary<int, double?>();
        Dictionary<string, int> tickernameList = new Dictionary<string, int>();
        public SkewAnalysis(List<Position> posArr, List<Portfolio> prtfArr, List<VolParam> volparamArr
            , Dictionary<int, Instrument> instArr, List<Rates> ratesArr, List<Dividend> divArr, List<IndexData> indexdataArr)
        {
            _posArr = posArr;
            _prtfArr = prtfArr;
            _volparamArr = volparamArr;
            _instArr = instArr;
            _ratesArr = ratesArr;
            _divArr = divArr;
            _indexdataArr = indexdataArr;
            _instbeta = _dbhelper.GetInstBeta();
            

            runSkewAnalysis();
            writeToFile("skewanalysis_initial.txt");
            _volparamArr = _dbhelper.GetVolParamShock(.001, .001);
            runSkewAnalysis();
            writeToFile("skewanalysis_shock.txt");
        }

        private void writeToFile(string filename)
        {
            FileHelper.CreateFile(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log", filename);
            foreach (Position p in _posArr)
            {
                Instrument item = _instArr[(int)p.id_imnt];
                FileHelper.WriteLine(filename, 
                    p.id_imnt + ","
                    + p.id_prtf + "," 
                    + p.pr_theo + "," 
                    + p.am_pos + "," 
                    + item.nm_imnt + "," 
                    + item.am_ct_sz);
            }
        }

        private void runSkewAnalysis()
        {

            Stopwatch sw = new Stopwatch();
            TimeSpan duration;
            sw.Restart();

            CreateTickerList();

            foreach (Portfolio prtf in _prtfArr)
            {
                //file.WriteLine(prtf.nm_prtf);
                try
                {
                    RecalcPrtf(prtf);
                }
                catch
                {

                }
            }

            duration = sw.Elapsed;
            sw.Stop();
        }

        private void RecalcPrtf(Portfolio prtf)
        {
            LoggingHelper.LogMemo("{1}: Recalc port: {0}", prtf.nm_prtf, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
            mhModels mhmodels = new mhModels();

            List<Position> position = new List<Position>();
            position = _posArr.Where(x => x.id_prtf == prtf.id_prtf).ToList();
            foreach (Position pos in position)
            {
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == pos.id_imnt);
                //Instrument item_und = instArr.FirstOrDefault(x => x.id_imnt == item.id_imnt_und);

                Instrument item = _instArr[(int)pos.id_imnt];
                Instrument item_und = _instArr[(int)item.id_imnt_und];

                if (item_und.id_typ_imnt != "IND")
                    prtf.pr_und = (float)tickerList[(int)item_und.id_imnt];

                switch (item.id_typ_imnt)
                {
                    case "STK":
                        pos.pr_und = (float)tickerList[(int)item.id_imnt];
                        pos.pr_imnt = (float)tickerList[(int)item.id_imnt];
                        pos.pr_theo = (float)tickerList[(int)item.id_imnt];
                        pos.am_delta = 1f;
                        pos.am_delta_imp = 1f;
                        continue;
                    case "FUT":
                        {
                            if (item.id_imnt == 29421 || item.id_imnt == 29422 || item.id_imnt == 29423)
                                continue;
                            IndexData indexpos = _indexdataArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                            pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                            pos.pr_imnt = (float)tickerList[(int)item.id_imnt];
                            mhmodels.CalcFut(pos, item, item_und, _ratesArr, (float)tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und && x.id_src == item_und.id_div_src).ToList()
                                , indexpos);
                            if (indexpos.id_front_mth == item.id_imnt_ric)
                                prtf.pr_und = pos.pr_theo;

                            continue;
                        }
                    case "OPT":
                        {
                            pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                            if ((int)item_und.id_imnt == 13959)
                                pos.pr_und = (float)(tickerList[(int)item_und.id_imnt] * .81);
                            float pr_theo = 0;
                            float am_borrow = 0;

                            IndexData indexpos = _indexdataArr.FirstOrDefault(y => y.id_imnt == prtf.id_imnt_und);

                            if (indexpos != null)
                            {

                                var dict = _instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos.id_front_mth);

                                Instrument item_fut = _instArr[(int)dict.Key];
                                Position pos_fut = _posArr.FirstOrDefault(x => x.id_imnt == item_fut.id_imnt);

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
                            mhmodels.CalcOpt(pos, item, item_und, _ratesArr, (float)tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , _volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , (float)tickerList[(int)item_und.id_imnt]
                                , (DateTime)prtf.dt_val, pr_theo, am_borrow);
                            pos.am_delta = mhmodels.mh_delta;
                            pos.am_gamma = mhmodels.mh_gamma;
                            pos.am_vega = mhmodels.mh_vega;
                            pos.am_theta = mhmodels.mh_theta;
                            pos.pr_theo = mhmodels.mh_thvalue;
                            pos.pr_imnt = mhmodels.mh_thvalue;
                            pos.am_vol = mhmodels.am_vol_db;
                            pos.am_delta_imp = mhmodels.mh_impdelta;
                            if ((int)item_und.id_imnt == 13959)
                                pos.pr_und = (float)tickerList[(int)item_und.id_imnt];
                            continue;
                        }


                }
            }
        }
        private void CreateTickerList()
        {
            foreach (Portfolio prtf in _prtfArr)
            {
                int i = 0;
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                Instrument item = _instArr[(int)prtf.id_imnt_und];
                LoggingHelper.LogMemo("{2}: Create ticker list: {0}, {1}", prtf.nm_prtf, item.id_imnt_ric, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                if (!tickerList.ContainsKey((int)prtf.id_imnt_und))
                {
                    tickernameList.Add(item.id_bberg, (int)prtf.id_imnt_und);
                    tickerList.Add((int)prtf.id_imnt_und, prtf.pr_und);
                    i++;
                }
            }

            //Add futures
            foreach (IndexData indexdata in _indexdataArr)
            {
                if (indexdata.id_imnt == 10002 || indexdata.id_imnt == 10013 || indexdata.id_imnt == 10145)
                {
                    //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_ric == indexdata.id_front_mth);

                    var dict = _instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexdata.id_next_mth);
                    Instrument item = _instArr[(int)dict.Key];
                    if (!tickerList.ContainsKey((int)item.id_imnt))
                    {
                        if (_posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt) == null)
                            tickerList.Add((int)item.id_imnt, 1);
                        else
                            tickerList.Add((int)item.id_imnt, _posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt).pr_imnt);
                        tickernameList.Add(item.id_bberg, (int)item.id_imnt);
                    }

                    dict = _instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexdata.id_front_mth);
                    item = _instArr[(int)dict.Key];
                    if (!tickerList.ContainsKey((int)item.id_imnt))
                    {
                        if (_posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt) == null)
                            tickerList.Add((int)item.id_imnt, 1);
                        else
                            tickerList.Add((int)item.id_imnt, _posArr.FirstOrDefault(x => x.id_imnt == item.id_imnt).pr_imnt);
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
    }
}
