using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloomberglp.Blpapi.Examples;

namespace wpfexample
{
    class Rollcheck
    {
        private List<Position> _posArr;
        private List<Portfolio> _prtfArr;
        private List<VolParam> _volparamArr;
        private Dictionary<int, Instrument> _instArr;
        private List<Rates> _ratesArr;
        private List<Dividend> _divArr;
        private DBHelper _dbhelper = new DBHelper();
        private GetPosition getpos = new GetPosition();
        private mhModels _mhmodels = new mhModels();
        private BbgPricer _bbgpricer = new BbgPricer();
        private string roll_alert = "";
        private string email_body = "";
        private string email_body_div = "";
        private bool is_negative_global;
        private bool roll_alert_global;

        List<IndexData> _indexdataArr = new List<IndexData>();
        Dictionary<int, double?> _tickerList = new Dictionary<int, double?>();

        public Rollcheck(List<Position> posArr, List<Portfolio> prtfArr, List<VolParam> volparamArr
            , Dictionary<int, Instrument> instArr, List<Rates> ratesArr, List<Dividend> divArr, List<IndexData> indexdataArr
            , Dictionary<int, double?> tickerList)
        {
            email_body = "index theo bid/ask combo borrow rate" + System.Environment.NewLine;
            _posArr = posArr;
            _prtfArr = prtfArr;
            _volparamArr = volparamArr;
            _instArr = instArr;
            _ratesArr = ratesArr;
            _divArr = divArr;
            _tickerList = tickerList;
            _indexdataArr = indexdataArr;
            email_body = @"<font size=""2"", font face = ""Calibri""><strong>Roll Prices</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0""><font size=""2"", font face = ""Calibri"">"
                + "<tr><th>Index</th><th>Theo</th><th>Bid</th><th>Ask"
                + "<th>1m Roll</th>" + "<th>2m Roll</th>"
                + "</th><th>Combo</th>"
                + "<th>Borrow</th><th>Rate</th><th>Fix by</th>";
            email_body_div = @"<br><font size=""2"", font face = ""Calibri""><strong>Gross Div Check</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0""><font size=""2"", font face = ""Calibri"">"
                + "<tr><th>Index</th><th>Maturity</th><th>Gross Div</th><th>BBG Gross Div"
                + "<th>Diff</th>" + "<th>Negative?</th>" + "<th>Theo</th>" + "<th>Basis/Day</th>" + "<th>Quoted Roll</th>";
            runGrossDivCheck(10002);
            runGrossDivCheck(10013);
            runGrossDivCheck(10145);
            email_body_div = email_body_div + "</font></table>";
            runRollCheck(10002);
            runRollCheck(10013);
            runRollCheck(10145);
            if (is_negative_global)
                roll_alert = "negative index div, ";
            if (roll_alert_global)
                roll_alert = roll_alert + "roll diff";

            email_body = email_body + "</font></table>";
            Emailer _emailer = new Emailer("sherman.chan@mlp.com", "sherman.chan@mlp.com", "Roll Checker", "Sherman Chan", "Roll Checker: " + roll_alert, email_body + email_body_div, true);
            _emailer.SendEmail();
        }

        internal void runGrossDivCheck(int id_imnt)
        {
            List<VolParam> volparam = _volparamArr.Where(x => x.id_imnt == id_imnt && x.dt_mat >= DateTime.Today).ToList();
            List<DividendGross> divCheckArr = new List<DividendGross>();
            IndexData indexpos2 = _indexdataArr.FirstOrDefault(x => x.id_imnt == id_imnt);
            Instrument item_fut2 = _instArr[(int)_instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos2.id_front_mth).Key];
            Instrument item_fut_next = _instArr[(int)_instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos2.id_next_mth).Key];
            Instrument item_und2 = _instArr[id_imnt];
            DateTime dt_val = DateTime.Today;

            double am_borrow = (double)indexpos2.am_borrow;

            double front_month_price = (double)(_bbgpricer.BbgPricerTicker(item_fut2.id_bberg + " Index", "BID") + _bbgpricer.BbgPricerTicker(item_fut2.id_bberg + " Index", "ASK")) / 2;

            double am_div = 0;
            double theo = front_month_price;
            double theo_last = front_month_price;
            double theo_regular_expiry = front_month_price;
            bool first_regular_expiry = false;

            foreach (VolParam vp in volparam)
            {
                bool is_negative = false;
                List<Dividend> div = _divArr.Where(x => x.id_imnt == id_imnt && x.id_src == "MAN" && x.dt_ex >= DateTime.Today).ToList();
                List<Dividend> div_bbg = _divArr.Where(x => x.id_imnt == id_imnt && x.id_src == "BB " && x.dt_ex >= DateTime.Today).ToList();
                double div_amt = 0;
                double div_amt_bbg = 0;

                double am_dte = vp.dt_mat.Subtract(dt_val).Days;
                double am_rate = _mhmodels.calcRate(vp.dt_mat, _ratesArr);

                

                foreach (Dividend d in div)
                {
                    if (d.dt_ex >= DateTime.Today && d.dt_ex <= vp.dt_mat)
                        div_amt = div_amt + (double)d.am_div;
                }

                foreach (Dividend d in div_bbg)
                {
                    if (d.dt_ex >= DateTime.Today && d.dt_ex <= vp.dt_mat)
                        div_amt_bbg = div_amt_bbg + (double)d.am_div;
                }

                if (div_amt < 0)
                {
                    is_negative = true;
                    is_negative_global = true;
                }

                theo = (front_month_price - div_amt) * Math.Pow(1 + (am_rate - (am_borrow / 100)), (am_dte / 365));
                float quoted_roll = 0;

                if (getExpiryDate2(vp.dt_mat.Month, vp.dt_mat.Year) == vp.dt_mat)
                {
                    quoted_roll = (float) (theo - theo_regular_expiry);
                    if (!first_regular_expiry)
                    {
                        theo_regular_expiry = theo;
                        first_regular_expiry = true;
                    }
                    if (vp.dt_mat.Month == 3 || vp.dt_mat.Month == 6 || vp.dt_mat.Month == 9 || vp.dt_mat.Month == 12)
                        theo_regular_expiry = theo;
                }
                else
                {
                }
                theo_last = theo;

                email_body_div = email_body_div + "<tr>"
                        + "<td>" + _instArr[id_imnt].id_imnt_ric + "</td>"
                        + "<td>" + vp.dt_mat.ToString("MM/dd/yy") + "</td>"
                        + "<td>" + String.Format("{0:0.0000}", Math.Round(div_amt, 4)) + "</td>"
                        + "<td>" + String.Format("{0:0.0000}", Math.Round(div_amt_bbg,4)) + "</td>"
                        + "<td>" + String.Format("{0:0.0000}", Math.Round((div_amt - div_amt_bbg), 4)) + "</td>"
                        + "<td>" + is_negative+ "</td>"
                        + "<td>" + Math.Round(theo,2) + "</td>"
                        + "<td>" + Math.Round((theo - front_month_price) / am_dte,3) + "</td>"
                        + "<td>" + Math.Round(quoted_roll,2) + "</td>"
                        + "</tr>";

                
                divCheckArr.Add(new DividendGross(vp.dt_mat, div_amt, div_amt_bbg, (div_amt - div_amt_bbg), is_negative));
            }
           

        }

        internal void runRollCheck(int id_imnt)
        {
            IndexData indexpos2 = _indexdataArr.FirstOrDefault(x => x.id_imnt == id_imnt);
            Instrument item_fut2 = _instArr[(int)_instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos2.id_front_mth).Key];
            Instrument item_fut_next = _instArr[(int)_instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos2.id_next_mth).Key];
            Instrument item_und2 = _instArr[id_imnt];

            double pr_future = 0;

            Portfolio prtf = _prtfArr.FirstOrDefault(x => x.id_imnt_und == id_imnt);
            List<Position> position = new List<Position>();
            position = _posArr.Where(x => x.id_imnt == (int)item_fut2.id_imnt || x.id_imnt == (int)item_fut_next.id_imnt).ToList();
            if (position.Count == 0)
            {
                double fut_price_bid = (double)_bbgpricer.BbgPricerTicker(item_fut2.id_bberg + " Index", "BID");
                getpos.updatePosition(item_fut2, new GetPosition { am_pos = 0, am_pos_sod = 0, pr_imnt_close = (float)fut_price_bid, pr_imnt = (float)fut_price_bid }, _posArr, _prtfArr, item_und2);
                position.Add(_posArr.FirstOrDefault(x => x.id_imnt == item_fut2.id_imnt));
            }
            DateTime date_1 = getExpiryDate2(DateTime.Today.Month, DateTime.Today.Year);
            DateTime date_2 = getExpiryDate2(date_1.AddMonths(1).Month, date_1.AddMonths(1).Year);
            DateTime date_3 = getExpiryDate2(date_2.AddMonths(1).Month, date_2.AddMonths(1).Year);
            int id_imnt_call_1 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_1.ToString("MMM").ToUpper(), (float)prtf.pr_und, "C", date_1.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_call_1], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_call_1));
            int id_imnt_put_1 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_1.ToString("MMM").ToUpper(), (float)prtf.pr_und, "P", date_1.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_put_1], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_put_1));
            int id_imnt_call_2 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_2.ToString("MMM").ToUpper(), (float)prtf.pr_und, "C", date_2.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_call_2], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_call_2));
            int id_imnt_put_2 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_2.ToString("MMM").ToUpper(), (float)prtf.pr_und, "P", date_2.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_put_2], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_put_2));
            int id_imnt_call_3 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_3.ToString("MMM").ToUpper(), (float)prtf.pr_und, "C", date_3.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_call_3], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_call_3));
            int id_imnt_put_3 = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, date_3.ToString("MMM").ToUpper(), (float)prtf.pr_und, "P", date_3.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_put_3], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_put_3));

            int id_imnt_front_call = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, indexpos2.nm_front_mth, (float)prtf.pr_und, "C", indexpos2.dt_front_mth.Value.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_front_call], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_front_call));
            int id_imnt_front_put = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, indexpos2.nm_front_mth, (float)prtf.pr_und, "P", indexpos2.dt_front_mth.Value.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_front_put], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_front_put));
            int id_imnt_next_call = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, indexpos2.nm_next_mth, (float)prtf.pr_und, "C", indexpos2.dt_next_mth.Value.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_next_call], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_next_call));
            int id_imnt_next_put = getpos.createDeriv(item_und2.id_imnt_ric, item_und2.id_imnt_ric, item_und2.id_imnt_ric, indexpos2.nm_next_mth, (float)prtf.pr_und, "P", indexpos2.dt_next_mth.Value.Year, _instArr);
            getpos.updatePosition(_instArr[id_imnt_next_put], new GetPosition { am_pos = 1, am_pos_sod = 1 }, _posArr, _prtfArr, item_und2);
            position.Add(_posArr.FirstOrDefault(x => x.id_imnt == id_imnt_next_put));
            
            //add next fut
            
           
            foreach (Position pos in position)
            {
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
                            IndexData indexpos = _indexdataArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                            pos.pr_und = (float)_tickerList[(int)item_und.id_imnt];
                            
                            if ((float)_tickerList[(int)item.id_imnt] != 1)
                                pos.pr_imnt = (float)_tickerList[(int)item.id_imnt];
                            pos.pr_imnt = pos.pr_imnt_close;
                            pr_future = pos.pr_imnt_close;
                            

                            _mhmodels.CalcFut(pos, item, item_und, _ratesArr, (float)_tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und && x.id_src == item_und.id_div_src).ToList()
                                , indexpos);
                            if (indexpos.id_front_mth == item.id_imnt_ric)
                            {
                                prtf.pr_und = pos.pr_theo;
                                _tickerList[(int)item_und.id_imnt] = prtf.pr_und;
                            }
                            continue;
                        }
                    case "OPT":
                        {
                            pos.pr_und = (float)_tickerList[(int)item_und.id_imnt];
                            float pr_theo = 0;
                            float am_borrow = 0;

                            IndexData indexpos = _indexdataArr.FirstOrDefault(y => y.id_imnt == prtf.id_imnt_und);

                            if (indexpos != null)
                            {

                                var dict = _instArr.FirstOrDefault(x => x.Value.id_imnt_ric == indexpos.id_front_mth);

                                Instrument item_fut = _instArr[(int)dict.Key];
                                Position pos_fut = _posArr.FirstOrDefault(x => x.id_imnt == item_fut.id_imnt);

                                //Instrument item_fut = instArr.FirstOrDefault(x => x.id_imnt_ric == indexpos.id_front_mth);
                                if (pos_fut == null || pos_fut.am_pos == 0)
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
                            _mhmodels.CalcOpt(pos, item, item_und, _ratesArr, (float)_tickerList[(int)item_und.id_imnt]
                                , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , _volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList()
                                , (float)_tickerList[(int)item_und.id_imnt]
                                , DateTime.Today, pr_theo, am_borrow);
                            pos.pr_theo = _mhmodels.mh_thvalue;

                            continue;
                        }
                }

            }

            //DateTime? dt_mat_march = getpos.getExpiryDate(3, 2013);

            double roll_price = position.FirstOrDefault(x => x.id_imnt == id_imnt_front_call).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_front_put).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_next_call).pr_theo +
                position.FirstOrDefault(x => x.id_imnt == id_imnt_next_put).pr_theo;
            double roll_price_1m = position.FirstOrDefault(x => x.id_imnt == id_imnt_call_1).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_put_1).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_call_2).pr_theo +
                position.FirstOrDefault(x => x.id_imnt == id_imnt_put_2).pr_theo;
            double roll_price_2m = position.FirstOrDefault(x => x.id_imnt == id_imnt_call_1).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_put_1).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_call_3).pr_theo +
                position.FirstOrDefault(x => x.id_imnt == id_imnt_put_3).pr_theo;


            double combo_price = pr_future - ((double)_instArr[id_imnt_front_call].pr_strike - (position.FirstOrDefault(x => x.id_imnt == id_imnt_front_put).pr_theo -
                position.FirstOrDefault(x => x.id_imnt == id_imnt_front_call).pr_theo));
            string id_spread_ric = "";
            string tx_warning = "";
            if (id_imnt == 10145)
                id_spread_ric = "RTARTA Index";
            else if (id_imnt == 10013)
                id_spread_ric = "NQNQ Index";
            else
                id_spread_ric = "ESES Index";

            double spread_price_bid = (double)_bbgpricer.BbgPricerTicker(id_spread_ric, "BID");
            double spread_price_ask = (double)_bbgpricer.BbgPricerTicker(id_spread_ric, "ASK");

            if (roll_price > -spread_price_bid + .2)
            {
                tx_warning = "lower div or borrow";
                roll_alert_global = true;
            }
            else if (roll_price < -spread_price_ask - .2)
            {
                tx_warning = "higher div or borrow";
                roll_alert_global = true;
            }
            else
                tx_warning = "none";

            /*email_body = email_body + item_und2.id_imnt_ric + " " 
                + String.Format("{0:0.00}", Math.Round(roll_price, 2)) + " "
                + String.Format("{0:0.00}", spread_price_bid) + "/" + String.Format("{0:0.00}", spread_price_ask) + " "
                + String.Format("{0:0.00}", Math.Round(combo_price,2)) + " "
                + String.Format("{0:0.00}", indexpos2.am_borrow) + " "
                + String.Format("{0:0.00}", indexpos2.am_rate_main) + " "
                + tx_warning + " "
                + '\n' + '\n';
             * */
            email_body = email_body + "<tr>" + "<td>" + item_und2.id_imnt_ric
                        + "</td><td>" + String.Format("{0:0.00}", Math.Round(roll_price, 2)) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", spread_price_bid) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", spread_price_ask) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", roll_price_1m) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", roll_price_2m) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", Math.Round(combo_price, 2)) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", indexpos2.am_borrow) + "</td>"
                        + "<td>" + String.Format("{0:0.00}", indexpos2.am_rate_main) + "</td>"
                        + "<td>" + tx_warning + "</td></tr>";
        }


        internal DateTime getExpiryDate2(double id_month, double id_year)
        {
            double ct_friday;
            double i, j;
            DateTime dt_mat = DateTime.Today;
            DateTime dt;
            double id_month_curr;
            double id_year_curr;
            double id_day_curr;

            ct_friday = 0;
            id_month_curr = DateTime.Today.Month;
            id_year_curr = DateTime.Today.Year;
            id_day_curr = DateTime.Today.Day;
            i = 1;

            if (id_month_curr > id_month)
            {
                if (id_year_curr == id_year)
                    id_month = id_month + 1;
            }

            do
            {
                dt = new DateTime((int)id_year, (int)id_month, (int)i);

                if ((int)dt.DayOfWeek == 5)
                {
                    ct_friday = ct_friday + 1;
                    dt_mat = dt;
                }

                if (i > 29)
                {
                    id_month = id_month + 1;
                    i = 0;
                    ct_friday = 0;
                }
                i = i + 1;
            } while (ct_friday < 3);

            if (DateTime.Today > dt_mat)
                return getExpiryDate2(dt_mat.AddMonths(1).Month, dt_mat.AddMonths(1).Year);
            else
                return dt_mat.AddDays(1);


        }

        class DividendGross
        {
            public DateTime dt_mat;
            public double am_div_man;
            public double am_div_bbg;
            public double am_div_diff;
            public bool is_negative;

            public DividendGross(DateTime _dt_mat, double _am_div_man, double _am_div_bbg, double _am_div_diff, bool _is_negative)
            {
                dt_mat = _dt_mat;
                am_div_man = _am_div_man;
                am_div_bbg = _am_div_bbg;
                am_div_diff = _am_div_diff;
                is_negative = _is_negative;
            }
        }
    }
}

