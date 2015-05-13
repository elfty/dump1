using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloomberglp.Blpapi.Examples;
namespace wpfexample
{
    class DividendRpt
    {
        private List<DividendRptStk> _posArrDivStk;
        private List<Position> _posArrDiv;
        private List<Position> _posArr;
        private List<Portfolio> _prtfArr;
        private List<VolParam> _volparamArr;
        private Dictionary<int, Instrument> _instArr;
        private List<Rates> _ratesArr;
        private List<Dividend> _divArr;
        private DBHelper _dbhelper = new DBHelper();
        private Dictionary<int, float> _instbeta = new Dictionary<int, float>();
        Dictionary<int, double?> _tickerList = new Dictionary<int, double?>();
        List<IndexData> _indexdataArr = new List<IndexData>();
        private string email_body;

        public DividendRpt(List<Position> posArr, List<Portfolio> prtfArr, List<VolParam> volparamArr
            , Dictionary<int, Instrument> instArr, List<Rates> ratesArr, List<Dividend> divArr
            , Dictionary<int, double?> tickerList, List<IndexData> indexdataArr)
        {
            _posArrDiv = _dbhelper.GetPositionDiv();
            _posArrDivStk = _dbhelper.GetPositionDivStk();
            _posArr = posArr;
            _prtfArr = prtfArr;
            _volparamArr = volparamArr;
            _instArr = instArr;
            _ratesArr = ratesArr;
            _divArr = divArr;
            //_instbeta = _dbhelper.GetInstBeta();
            _tickerList = tickerList;
            _indexdataArr = indexdataArr;
            Emailer _emailer;
            string results = _dbhelper.GetDivBBGvsImgnRec();
            string results2 = _dbhelper.GetDivBBGvsImgnRecAmt();

            if (results != "")
            {
                email_body = @"<font size=""2"", font face = ""Calibri""><strong>Imagine vs BBG Announced Div Check</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0"">"
                    + @"<font size=""2"", font face = ""Calibri"">"
                    + "<tr><th>Symbol</th><th>Problem</th>"
                    + results + "</font></table>";
            }

            if (results2 != "")
            {
                email_body = email_body + "<br>"
                    + @"<font size=""2"", font face = ""Calibri""><strong>Imgn vs BBG Div Amt Check</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0"">"
                    + @"<font size=""2"", font face = ""Calibri"">"
                    + "<tr><th>Symbol</th><th>Div Diff</th>"
                    + "<th>BBG Div</th><th>Imgn Div</th>"
                    + results2 + "</font></table>";
            }
            runDividendRpt();
            if (email_body == "")
                _emailer = new Emailer("sherman.chan@mlp.com", "sherman.chan@mlp.com", "Div Checker", "Sherman Chan", "Div Checker " + DateTime.Today.ToString("MM/dd/yy") + " None", "", false);
            else
                _emailer = new Emailer("sherman.chan@mlp.com", "sherman.chan@mlp.com", "Div Checker", "Sherman Chan", "Div Checker " + DateTime.Today.ToString("MM/dd/yy"), email_body, true);
            _emailer.SendEmail();
        }

        private void runDividendRpt()
        {
            mhModels mhmodels = new mhModels();
            DateTime dt_val = DateTime.Today.AddDays(0);
            BbgPricer _bbgpricer = new BbgPricer();
            List<DividendRptType> divrptarr = new List<DividendRptType>();
            string email_body_early_exercise;
            string email_body_long_dividend;

            email_body_long_dividend = @"<font size=""2"", font face = ""Calibri""><strong>Dividend Rolls</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0"">"
                + @"<font size=""2"", font face = ""Calibri"">"
                + "<tr><th>Symbol</th><th>Position</th>"
                + "<th>Delta Shares</th><th>Net Shares</th><th>source</th><th>div</th><th>div withheld(20%)</th><th>$div amount</th>" 
                + "<th>$amount withheld(20%)</th>"
                + "<th>annc/proj</th>";

            email_body_early_exercise = @"<font size=""2"", font face = ""Calibri""><strong>Early Exercise</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0""><font size=""2"", font face = ""Calibri""><tr><th>Symbol</th><th>Position</th><th>early exercise?</th><th>call theo</th><th>call delta</th>" +
                "<th>put theo</th><th>und price</th><th>put bid</th><th>put offer</th><th>total div</th><th>annc/proj</th>";

            /// Early Exercise
            /// 

            foreach (Position pos in _posArrDiv)
            {

                double call_delta = 0;
                double call_theo = 0;
                double put_theo = 0;
                
                Instrument item = _instArr[(int)pos.id_imnt];
                Instrument item_und = _instArr[(int)item.id_imnt_und];

                DateTime dt_mat = ((DateTime)(item.dt_mat)).AddDays(1);

                double pr_und_bid = (float)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US EQUITY", "BID");
                double pr_und_ask = (float)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US EQUITY", "ASK");
                pos.pr_und = (float)(pr_und_bid + pr_und_ask) / 2;

                _tickerList[(int)item_und.id_imnt] = pos.pr_und;

                if (item.id_typ_imnt == "OPT")
                {

                    if (pos.pr_und < item.pr_strike)
                        continue;

                    List<Dividend> dividend = _divArr.Where(x => x.id_imnt == item.id_imnt_und && x.dt_ex <= dt_mat && x.id_src.TrimEnd() == item_und.id_div_src.TrimEnd()).ToList();
                    
                    List<VolParam> volparam = _volparamArr.Where(x => x.id_imnt == item.id_imnt_und).ToList();
                    call_delta = (double)mhmodels.CalcModel(dt_val, 1, pos.pr_und, (float)item.pr_strike, item.id_pc, dt_mat, mhmodels.calcRate(dt_mat, _ratesArr), null, mhmodels.calcVolNoInst(dt_mat, (float)item.pr_strike, volparam.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == dt_mat), pos), "Delta", 0, mhmodels.calcDivDtMat(dividend, (DateTime)item.dt_mat));
                    call_theo = (double)mhmodels.CalcModel(dt_val, 1, pos.pr_und, (float)item.pr_strike, item.id_pc, dt_mat, mhmodels.calcRate(dt_mat, _ratesArr), null, mhmodels.calcVolNoInst(dt_mat, (float)item.pr_strike, volparam.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == dt_mat), pos), "Theo", 0, mhmodels.calcDivDtMat(dividend, (DateTime)item.dt_mat));
                    put_theo = (double)mhmodels.CalcModel(dt_val, 1, pos.pr_und, (float)item.pr_strike, "P", dt_mat, mhmodels.calcRate(dt_mat, _ratesArr), dividend, mhmodels.calcVolNoInst(dt_mat, (float)item.pr_strike, volparam.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat ==dt_mat), pos), "Theo");

                    double pr_put_bid = (double)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US " + ((DateTime)item.dt_mat).AddDays(1).ToString("MM/dd/yy") + " " + "P" + item.pr_strike + " EQUITY", "BID");
                    if (pr_put_bid == -1)
                        pr_put_bid = 0;
                    double pr_put_ask = (double)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US " + dt_mat.ToString("MM/dd/yy") + " " + "P" + item.pr_strike + " EQUITY", "ASK");
                    bool early_exercise = isEarlyExercise((double)pos.pr_und, (double)item.pr_strike, call_theo, pr_put_bid, dividend);

                    divrptarr.Add(new DividendRptType(item, call_delta, call_theo, put_theo, pos.pr_und, pr_put_bid, pr_put_ask, dividend, early_exercise));
                    if (dividend.Count != 0)
                        email_body_early_exercise = email_body_early_exercise + "<tr>" + "<td>" + item_und.id_bberg + " " + dt_mat.ToString("MM/dd/yy") + " " + "C" + item.pr_strike
                            + "</td><td>" + pos.am_pos + "</td>"
                            + "<td>" + early_exercise + "</td><td>" + Math.Round(call_theo, 2) + "</td><td>" + Math.Round(call_delta, 2) + "</td>"
                            + "<td>" + Math.Round(put_theo, 2) + "</td><td>" + pos.pr_und + "</td><td>" + pr_put_bid + "</td><td>" + pr_put_ask + "</td>"
                            + "<td>" + dividend.Sum(x => x.am_div) + "</td>"
                            + "<td>" + dividend[0].tx_proj + "</td></tr>";
                }
            }
            email_body_early_exercise = email_body_early_exercise + "</font></table>";

            /// Long Dividend
            /// 

            foreach (DividendRptStk pos in _posArrDivStk)
            {
                double am_delta_port = 0;
                Instrument item = _instArr[(int)pos.id_imnt];
                Instrument item_und = _instArr[(int)item.id_imnt_und];

                double pr_und_bid = (float)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US EQUITY", "BID");
                double pr_und_ask = (float)_bbgpricer.BbgPricerTicker(item_und.id_bberg + " US EQUITY", "ASK");
                pos.pr_und = (float)(pr_und_bid + pr_und_ask) / 2;

                _tickerList[(int)item_und.id_imnt] = pos.pr_und;

                if (pos.am_pos < 0)
                    continue;

                Portfolio prtf = _prtfArr.FirstOrDefault(x => x.id_prtf == pos.id_prtf);
                RecalcPrtf(prtf);
                List<Position> _posArr3 = _posArr.Where(x => x.id_prtf == pos.id_prtf).ToList();
                foreach (Position position in _posArr3)
                {
                    Instrument inst = _instArr[(int)position.id_imnt];
                    am_delta_port = am_delta_port + (position.am_delta * position.am_pos * (float)inst.am_ct_sz);

                }
                List<Dividend> dividend = _divArr.Where(x => x.id_imnt == item.id_imnt_und).ToList();

                email_body_long_dividend = email_body_long_dividend + "<tr>" + "<td>" + item_und.id_bberg
                    + "</td>"
                    + "<td>" + pos.am_pos + "</td>"
                    + "<td>" + Math.Round(am_delta_port / 100, 0) * 100 + "</td>"
                    + "<td>" + (pos.am_pos - Math.Round(am_delta_port / 100, 0) * 100) + "</td>";
                    if (item_und.id_div_src == pos.id_src)
                        email_body_long_dividend = email_body_long_dividend + "<td><strong>*" + pos.id_src + "*</strong></td>";
                    else
                        email_body_long_dividend = email_body_long_dividend + "<td>" + pos.id_src + "</td>";

                    email_body_long_dividend = email_body_long_dividend + "<td>" + pos.am_div + "</td><td>" + Math.Round((double)(dividend[0].am_div * .2), 2) + "</td>"
                    + "<td>" + Math.Round((double)(pos.am_div * ((pos.am_pos - Math.Round(am_delta_port / 100, 0) * 100))),0) + "</td>"
                    + "<td>" + Math.Round((double)(pos.am_div * ((pos.am_pos - Math.Round(am_delta_port / 100, 0) * 100)) * .2), 0) + "</td>"
                    + "<td>" + dividend[0].tx_proj + "</td></tr>";
                
            }
            
            email_body_long_dividend = email_body_long_dividend + "</font></table>";
            email_body_long_dividend = email_body_long_dividend + @"<font size=""2"", font face = ""Calibri"">*= Active Dividend</font>";
            email_body = email_body + "<br>" + email_body_early_exercise + "<br>" + email_body_long_dividend;

        }

        private bool isEarlyExercise(double pr_und, double pr_strike, double call_theo, double pr_put_bid, List<Dividend> dividend)
        {
            if (Math.Abs((pr_und - (double)pr_strike)- call_theo) > .01)
            {
                return false;

            }
            if (pr_put_bid >= dividend.Sum(x => x.am_div))
            {
                return false;
            }

            return true;

        }

        private void RecalcPrtf(Portfolio prtf)
        {
            DateTime dt_val = DateTime.Today;

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
                    prtf.pr_und = (float)_tickerList[(int)item_und.id_imnt];

                switch (item.id_typ_imnt)
                {
                    case "STK":
                        pos.pr_und = (float)_tickerList[(int)item.id_imnt];
                        pos.pr_imnt = (float)_tickerList[(int)item.id_imnt];
                        pos.pr_theo = (float)_tickerList[(int)item.id_imnt];
                        pos.am_delta = 1f;
                        pos.am_delta_imp = 1f;
                        continue;
                    case "FUT":
                        {
                            if (item.id_imnt == 29421 || item.id_imnt == 29422 || item.id_imnt == 29423)
                                continue;
                            IndexData indexpos = _indexdataArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                            pos.pr_und = (float)_tickerList[(int)item_und.id_imnt];
                            pos.pr_imnt = (float)_tickerList[(int)item.id_imnt];
                            mhmodels.CalcFut(pos, item, item_und, _ratesArr, (float)_tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und && x.id_src == item_und.id_div_src).ToList()
                                , indexpos);
                            if (indexpos.id_front_mth == item.id_imnt_ric)
                                prtf.pr_und = pos.pr_theo;

                            continue;
                        }
                    case "OPT":
                        {
                            pos.pr_und = (float)_tickerList[(int)item_und.id_imnt];
                            if ((int)item_und.id_imnt == 13959)
                                pos.pr_und = (float)(_tickerList[(int)item_und.id_imnt] * .81);
                            float pr_theo = 0;
                            float am_borrow = 0;

                            IndexData indexpos = _indexdataArr.FirstOrDefault(y => y.id_imnt == prtf.id_imnt_und);

                            if (indexpos != null)
                            {

                                var dict = _instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos.id_front_mth);

                                Instrument item_fut = _instArr[(int)dict.Key];
                                Position pos_fut = _posArrDiv.FirstOrDefault(x => x.id_imnt == item_fut.id_imnt);

                                //Instrument item_fut = instArr.FirstOrDefault(x => x.id_imnt_ric == indexpos.id_front_mth);
                                if (pos_fut == null)
                                {
                                    pr_theo = (float)_tickerList[(int)item_und.id_imnt];
                                    //pos_fut.pr_und = (float)tickerList[(int)item_und.id_imnt];
                                }
                                else
                                {
                                    pr_theo = pos_fut.pr_theo;
                                    pos_fut.pr_und = pos_fut.pr_theo;
                                }
                                am_borrow = (float)indexpos.am_borrow;
                            }
                            mhmodels.CalcOpt(pos, item, item_und, _ratesArr, (float)_tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , _volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , (float)_tickerList[(int)item_und.id_imnt]
                                , dt_val, pr_theo, am_borrow);
                            pos.am_delta = mhmodels.mh_delta;
                            pos.am_gamma = mhmodels.mh_gamma;
                            pos.am_vega = mhmodels.mh_vega;
                            pos.am_theta = mhmodels.mh_theta;
                            pos.pr_theo = mhmodels.mh_thvalue;
                            pos.pr_imnt = mhmodels.mh_thvalue;
                            pos.am_vol = mhmodels.am_vol_db;
                            pos.am_delta_imp = mhmodels.mh_impdelta;
                            if ((int)item_und.id_imnt == 13959)
                                pos.pr_und = (float)_tickerList[(int)item_und.id_imnt];
                            continue;
                        }
                }


            }

        }
    }

    class DividendRptType
    {
        Instrument item { get; set; }
        double call_delta { get; set; } 
        double call_theo { get; set; }
        double put_theo { get; set; }
        double pr_und { get; set; }
        double pr_put_bid { get; set; }
        double pr_put_ask { get; set; }
        List<Dividend> dividend { get; set; }
        bool early_exercise { get; set;}

        public DividendRptType(Instrument _item, double _call_delta, double _call_theo, double _put_theo, double _pr_und, double _pr_put_bid, double _pr_put_ask, List<Dividend> _dividend, bool _early_exercise)
        {
            item = _item;
            call_delta = _call_delta;
            call_theo = _call_theo;
            put_theo = _put_theo;
            pr_und = _pr_und;
            pr_put_bid = _pr_put_bid;
            pr_put_ask = _pr_put_ask;
            dividend = _dividend;
            early_exercise = _early_exercise;
        }

        
    }
}
