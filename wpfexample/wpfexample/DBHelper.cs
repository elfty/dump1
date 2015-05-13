using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

namespace wpfexample
{
    class DBHelper
    {
        private string connectionString = "Integrated Security=false;Data Source=ADSRV133.AD.MLP.COM;Initial Catalog=OPTIONS;Asynchronous Processing=true;User ID=volarb;Password=el3phant";

        internal List<Portfolio> GetPortfolio()
        {
            string sqlText = "SELECT * FROM PORTFOLIO where id_strat in (1,2,4) and id_prtf not in (908,900,926,106,321,812,1031,1032,371,725,310,147,734,1118,1150,434)";
            List<Portfolio> allPrtf = new List<Portfolio>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPrtf.Add(new Portfolio(row));
                    }
                }
                return allPrtf;
            }
        }

        internal List<Portfolio> GetPortfolioSpecials()
        {
            string sqlText = "SELECT * FROM PORTFOLIO where id_strat in (1,2,4)  and id_prtf not in (86,921,908,900,591,898,919,926,106,321,355,1014,812,1031,1032)";
            List<Portfolio> allPrtf = new List<Portfolio>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPrtf.Add(new Portfolio(row));
                    }
                }
                return allPrtf;
            }
        }

        internal List<Position> GetPosition()
        {
            string sqlText = "SELECT * FROM POSITION where am_pos <> 0";

            List<Position> allPos = new List<Position>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPos.Add(new Position(row));
                    }
                }
                return allPos;
            }
        }

        internal List<Position> GetPositionDiv()
        {
            string sqlText = "select p.* from POSITION p, INSTRUMENT i where id_prtf in ( "
                + " select p.id_prtf from PORTFOLIO p, DIVIDENDS d, INSTRUMENT i where p.id_imnt_und = d.id_imnt "
                + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1) and p.id_strat in (1,2,4) and i.id_imnt = p.id_imnt_und "
                + " and i.id_typ_imnt = 'STK' ) "
                + " and am_pos <> 0 "
                + " and p.id_imnt = i.id_imnt "
                + " and (i.id_pc = 'C')"
                + " order by i.id_imnt_und, i.dt_mat, i.pr_strike";


            List<Position> allPos = new List<Position>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPos.Add(new Position(row));
                    }
                }
                return allPos;
            }
        }

        internal string GetDivBBGvsImgnRec()
        {
            string results = "";
            string sqlText = "select i.id_imnt_ric, 'in bbg and not in imagine' from INSTRUMENT i, PORTFOLIO po where po.id_prtf in ( "
            + " select p.id_prtf from PORTFOLIO p, DIVIDENDS d, INSTRUMENT i where p.id_imnt_und = d.id_imnt "
            + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1) and p.id_strat in (1,2,4) and i.id_imnt = p.id_imnt_und "
            + " and i.id_typ_imnt = 'STK' ) "
            + " and po.id_imnt_und = i.id_imnt"
            + " and po.id_prtf not in (select po.id_prtf from INSTRUMENT i, PORTFOLIO po where po.id_prtf in ( "
            + " select p.id_prtf from PORTFOLIO p, DIVIDENDS_IMGN d, INSTRUMENT i where i.id_imnt_ric = d.id_imnt_ric "
            + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1) and p.id_strat in (1,2,4) and i.id_imnt = p.id_imnt_und "
            + " and i.id_typ_imnt = 'STK' and id_status = 'ANNOUNCED' ) "
            + " and po.id_imnt_und = i.id_imnt)"
            + " union all"
            + " select i.id_imnt_ric, 'in imagine and not in bbg' from INSTRUMENT i, PORTFOLIO po where po.id_prtf in ( "
            + " select p.id_prtf from PORTFOLIO p, DIVIDENDS_IMGN d, INSTRUMENT i where i.id_imnt_ric = d.id_imnt_ric "
            + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1) and p.id_strat in (1,2,4) and i.id_imnt = p.id_imnt_und "
            + " and i.id_typ_imnt = 'STK' and id_status = 'ANNOUNCED' ) "
            + " and po.id_imnt_und = i.id_imnt"
            + " and po.id_prtf not in (select po.id_prtf from INSTRUMENT i, PORTFOLIO po where po.id_prtf in ( "
            + " select p.id_prtf from PORTFOLIO p, DIVIDENDS d, INSTRUMENT i where p.id_imnt_und = d.id_imnt "
            + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1) and p.id_strat in (1,2,4) and i.id_imnt = p.id_imnt_und "
            + " and i.id_typ_imnt = 'STK' ) "
            + " and po.id_imnt_und = i.id_imnt)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        results = results + "<tr>"
                            + "<td>" + reader.GetValue(0).ToString() + "</td>"
                            + "<td>" + reader.GetValue(1).ToString() + "</td></tr>";
                    }
                }
                return results;
            }
        }

        internal string GetDivBBGvsImgnRecAmt()
        {
            string results = "";
            string sqlText = "select i.id_imnt_ric, d.am_div - di.am_div, d.am_div 'bbg div', di.am_div 'imgn div' from DIVIDENDS_IMGN di, DIVIDENDS d, INSTRUMENT i"
                + " where i.id_imnt_ric = di.id_imnt_ric and di.id_status = 'ANNOUNCED'"
                + " and di.dt_ex = (select dt_bus from SYS_DATE where am_days = 1)"
                + " and di.dt_ex = d.dt_ex"
                + " and i.id_imnt = d.id_imnt"
                + " and d.am_div - di.am_div <> 0";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        results = results + "<tr>" 
                            + "<td>" + reader.GetValue(0).ToString() + "</td>"
                            + "<td>" + Math.Round(reader.GetDouble(1), 4) + "</td>"
                            + "<td>" + Math.Round(reader.GetDouble(2), 4) + "</td>"
                            + "<td>" + Math.Round(reader.GetDouble(3), 4) + "</td></tr>";
                    }
                }
                return results;
            }
        }

        internal List<DividendRptStk> GetPositionDivStk()
        {
            string sqlText = " select p.*, d.* from POSITION p, INSTRUMENT i, DIVIDENDS d where "
                + " p.am_pos <> 0"
                + " and d.dt_ex = (select dt_bus from SYS_DATE where am_days = 1)"
                //+ " and d.dt_ex = '20131202'"
                + " and am_pos <> 0 "
                + " and p.id_imnt = i.id_imnt "
                + " and d.id_imnt = p.id_imnt"
                + " and i.id_typ_imnt = 'STK'";


            List<DividendRptStk> allPos = new List<DividendRptStk>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPos.Add(new DividendRptStk(row));
                    }
                }
                return allPos;
            }
        }

        internal List<GetPosition> GetPositionIntra()
        {
            string sqlText = "select *, "
                + " (select UnderlyingSC from REC..p5_und_mapping where p5_und_mapping.Underlying = rec..p5_bo_intra.Underlying) "
                + " from REC..p5_bo_intra "
                + " where time = (select MAX(time) from REC..p5_bo_intra)"
                + "and Trader not in ('SPECIAL_SIT', 'CHAN', 'JCKSN_SS')"
                + "and Instrument not in ('CUR','SS')"
                + "and (Position <> 0 or Opened <> 0)";
                //+ "and Underlying not in ('DGIT.O', 'SZMK.O','DGIT.O-Merged','BID.N-SP')";

            List<GetPosition> allPosIntra = new List<GetPosition>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        allPosIntra.Add(new GetPosition(row));
                    }
                }
                return allPosIntra;
            }
        }

        internal DateTime GetPositionIntraLastUpdate()
        {
            string sqlText = "select MAX(time) from REC..p5_bo_intra ";
            DateTime last_update = new DateTime();
            List<GetPosition> allPosIntra = new List<GetPosition>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        last_update = reader.GetDateTime(0);
                    }
                }
                return last_update;
            }
        }

        internal List<VolParam> GetVolParam()
        {
            string sqlText = "SELECT * FROM VOL_PARAM";

            List<VolParam> volParam = new List<VolParam>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        volParam.Add(new VolParam(row));
                    }
                }
                return volParam;
            }
        }


        internal List<Dividend> GetDividend()
        {
            string sqlText = "SELECT * FROM DIVIDENDS";

            List<Dividend> dividend = new List<Dividend>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        dividend.Add(new Dividend(row));
                    }
                }
                return dividend;
            }
        }

        internal List<Borrow> GetBorrow()
        {
            string sqlText = "SELECT * FROM REF_BORROW";

            List<Borrow> borrow = new List<Borrow>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        borrow.Add(new Borrow(row));
                    }
                }
                return borrow;
            }
        }

        internal List<Rates> GetRates()
        {
            string sqlText = "SELECT * FROM RATES order by am_yrs";

            List<Rates> rates = new List<Rates>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        rates.Add(new Rates(row));
                    }
                }
                return rates;
            }
        }

        internal Dictionary<int, Instrument> GetInst()
        {
            string sqlText = "SELECT * FROM INSTRUMENT";

            List<Instrument> inst = new List<Instrument>();
            Dictionary<int, Instrument> instDict = new Dictionary<int, Instrument>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        //inst.Add(new Instrument(row));
                        instDict.Add((int)row[0], new Instrument(row));

                    }
                }
                return instDict;
            }
        }

        internal List<IndexData> GetIndData()
        {
            string sqlText = "SELECT * FROM IND_VOL_IMNT_DATA";

            List<IndexData> indexdata = new List<IndexData>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        indexdata.Add(new IndexData(row));
                    }
                }
                return indexdata;
            }
        }

        internal Dictionary<int,float> GetPositionHist()
        {
            string sqlText = "SELECT * FROM POSITION_HIST where dt_trd = (select max(dt_trd) from POSITION_HIST)";

            Dictionary<int, float> poshist = new Dictionary<int,float>() ;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!poshist.ContainsKey(reader.GetInt32(3)))
                        {
                            poshist.Add(reader.GetInt32(3), (float)reader.GetDouble(7));
                        }
                    }
                }
                return poshist;
            }
        }

        internal Dictionary<int, float> GetInstBeta()
        {
            string sqlText = "SELECT id_imnt, am_beta FROM UND_PRICE where dt_bus = (select max(dt_bus) from UND_PRICE_CALC) and am_beta is not null";

            Dictionary<int, float> instbeta = new Dictionary<int, float>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!instbeta.ContainsKey(reader.GetInt32(0)))
                        {
                            instbeta.Add(reader.GetInt32(0), (float)reader.GetDouble(1));
                        }
                    }
                }
                return instbeta;
            }
        }

        internal void DeleteVegaSlide()
        {
            //Clear Table
            string sqlText = "DELETE FROM MIS..PROJECTION_PORT_OPTIONSDB where dt_bus = (select dt_bus from OPTIONS..SYS_DATE where id_date = 'TODAY')";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        internal void InsertVegaSlide(int am_pct_spot, double am_vega, double am_vega_ratio, double am_theta, double am_gamma, int beta_flag)
        {            
            //Insert Row
            string beta;
            if (beta_flag == 1)
                beta = "no beta";
            else
                beta = "beta";

            string sqlText = string.Format("insert into MIS..PROJECTION_VEGA values ('{0}','{1}',{2},{3},{4},{5},{6})",
                string.Format("{0:yyyyMMdd}", DateTime.Today), beta, am_pct_spot, am_vega, am_vega_ratio, am_theta, am_gamma);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        internal void InsertVegaSlide2(int am_pct_spot, string nm_typ_val, double am_val, double am_val_down_10,double am_val_down_25,double am_val_down_50,
            double am_val_down_5, double am_val_up_5, double am_val_up_10,double am_val_up_25, double am_val_up_50, int beta_flag)
        {
            //Insert Row
            string beta;
            if (beta_flag == 1)
                beta = "Beta Adj ex spxtail";
            else if (beta_flag == 2)
                beta = "No Beta ex spxtail";
            else if (beta_flag == 3)
                beta = "Beta Adj w/ spxtail";
            else
                beta = "No Beta w/ spxtail";

            string sqlText = string.Format("insert into MIS..PROJECTION_PORT_OPTIONSDB values ('{0}','{1}','{2}',{3},{4},{5},{6},{7},{8},{9},{10},{11},{12})",
                string.Format("{0:yyyyMMdd}", DateTime.Today), nm_typ_val, beta, am_pct_spot,
                am_val_down_10, am_val_down_5, am_val, am_val_up_5, am_val_up_10, 
                am_val_up_25, am_val_up_50, am_val_down_25, am_val_down_50);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        internal Dictionary<DateTime, int> GetUndPriceDate()
        {
            string sqlText = "SELECT id_ord, dt_bus FROM UND_PRICE_DATE";

            Dictionary<DateTime, int> undpricedate = new Dictionary<DateTime, int>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!undpricedate.ContainsKey(reader.GetDateTime(1)))
                        {
                            undpricedate.Add(reader.GetDateTime(1), reader.GetInt32(0));
                        }
                    }
                }
                return undpricedate;
            }
        }

        internal Dictionary<DateTime, float> GetSpxDiv(string id_imnt_ric)
        {

            string sqlText = "SELECT * FROM MIS..INDEX_DIV_YLD";

            if (id_imnt_ric == "SPX")
                sqlText = "SELECT Date,Div FROM MIS..INDEX_DIV_YLD";
            else if (id_imnt_ric == "NDX")
                sqlText = "SELECT Date,NdxDiv FROM MIS..INDEX_DIV_YLD";
            else
                sqlText = "SELECT Date,RutDiv FROM MIS..INDEX_DIV_YLD";

            Dictionary<DateTime, float> spxdiv = new Dictionary<DateTime, float>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!spxdiv.ContainsKey(reader.GetDateTime(0)))
                        {
                            if (reader.IsDBNull(1))
                                continue;
                            spxdiv.Add(reader.GetDateTime(0), reader.GetFloat(1));
                            
                        }
                    }
                }
                return spxdiv;
            }
        }

        internal List<Dividend_Hist> GetSPX500Div()
        {
            string sqlText = "SELECT * FROM MIS..dividends_ss_hist";

            List<Dividend_Hist> dividend = new List<Dividend_Hist>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        dividend.Add(new Dividend_Hist(row));
                    }
                }
                return dividend;
            }
        }

        internal Dictionary<int, string> GetGicsSect()
        {
            string sqlText = "SELECT * FROM OPTIONS..REF_GICS_SECTOR";

            Dictionary<int, string> gicssect = new Dictionary<int, string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        gicssect.Add(reader.GetInt32(0),reader.GetString(1));
                    }
                }
                return gicssect;
            }
        }

        internal Dictionary<int, string> GetGicsIndu()
        {
            string sqlText = "SELECT * FROM OPTIONS..REF_GICS_INDUSTRY_SUB";

            Dictionary<int, string> gicsindu = new Dictionary<int, string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        gicsindu.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                return gicsindu;
            }
        }

        internal List<VolData> GetVolData(DateTime dt_start_und, DateTime dt_end, float id_imnt_backtest_start, float id_imnt_backtest_end)
        {
            string sqlText = "select up.dt_bus,up.id_imnt,pr_close,pr_open,am_chg,am_ivol_1m_95,am_ivol_1m_100,am_ivol_1m_105,am_ivol_2m_95,am_ivol_2m_100,am_ivol_2m_105,am_ivol_3m_95,am_ivol_3m_100,am_ivol_3m_105" +
                " from UND_PRICE up, VOL_REL vr " +
                " where up.id_imnt = vr.id_imnt " +
                " and up.dt_bus = vr.dt_bus " +
                " and up.dt_bus >= '" + dt_start_und.ToString("yyyyMMdd") + "'" +
                " and up.dt_bus <= '" + dt_end.ToString("yyyyMMdd") + "'";

            if (id_imnt_backtest_end > 0)
                sqlText = sqlText + " and up.id_imnt >= " + id_imnt_backtest_start + " and up.id_imnt < " + id_imnt_backtest_end;
            
            sqlText = sqlText + " order by vr.dt_bus";

            List<VolData> voldata = new List<VolData>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        //inst.Add(new Instrument(row));
                        voldata.Add(new VolData(row));

                    }
                }
                return voldata;
            }
        }

        internal List<Rates> GetRatesHist()
        {
            string sqlText = "select id_rate, AVG(am_rate), convert(real,AVG(am_yrs)), DATEADD(dd, 0, DATEDIFF(dd, 0, dt_chg)), am_days "
                + "from hist..bu_RATES_HIST where id_rate = '3M' "
                + "group by id_rate, DATEADD(dd, 0, DATEDIFF(dd, 0, dt_chg)), am_days order by DATEADD(dd, 0, DATEDIFF(dd, 0, dt_chg)),AVG(am_yrs)";

            List<Rates> rates = new List<Rates>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        rates.Add(new Rates(row));
                    }
                }
                return rates;
            }
        }

        internal List<int> GetSPXComp(float id_imnt_backtest_start, float id_imnt_backtest_end)
        {
            string sqlText;
            if (id_imnt_backtest_start == 0 && id_imnt_backtest_end == 0)
            {
                sqlText = "select * from INSTRUMENT where ((id_typ_imnt = 'STK'"
                    + " and id_imnt not in (select id_imnt from INSTRUMENT_REF where etf_fl = 1)) "
                    + " or id_imnt in (10002, 10013, 10145)) "
                    + " and id_del = 0  "
                    + " order by id_imnt";
            }
            else
            {
                sqlText = "select * from INSTRUMENT where id_typ_imnt = 'STK'"
                    + " and id_imnt not in (select id_imnt from INSTRUMENT_REF where etf_fl = 1) "
                    + " and id_del = 0 "
                    //+ " and id_imnt in (select id_imnt from IND_COMP_SPX)"
                    + " and id_imnt >= " + id_imnt_backtest_start + " and id_imnt < " + id_imnt_backtest_end
                    + " order by id_imnt";
            }
            List<int> spxcomp_arr = new List<int>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        spxcomp_arr.Add(reader.GetInt32(0));
                    }
                }
                return spxcomp_arr;
            }
        }

        internal void InsertStraddlePL_Skew(DateTime dt_start, DateTime dt_end, float id_imnt_backtest_start, float id_imnt_backtest_end, string strPath, string strFile)
        {
            string sqlText;
            sqlText = "DELETE FROM MIS..VOL_HIST_IMPORT_STRADDLE_SKEW where dt_bus >= '" + dt_start.ToString("yyyyMMdd") + "'" 
                + " and dt_bus <= '" + dt_end.ToString("yyyyMMdd") + "'";
            if (id_imnt_backtest_end > 0)
                sqlText = sqlText + " and id_imnt >= " + id_imnt_backtest_start + " and id_imnt < " + id_imnt_backtest_end;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }      

            //sqlText = "BULK INSERT MIS..VOL_HIST_IMPORT_STRADDLE_SKEW FROM '" + strPath + strFile + "' " +
            //           "WITH ( FIRSTROW = 1, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')";

            sqlText = string.Format("BULK INSERT MIS..VOL_HIST_IMPORT_STRADDLE_SKEW FROM '{0}{1}' WITH ( FIRSTROW = 1, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n' )", strPath, strFile);
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            for (double i = .90; i <= 1.1; i += .05)
            {
                sqlText = "update upc "
                           + " set straddle_pl_" + i * 100 + " = i.am_pnl,"
                           + "  straddle_pl_" + i * 100 + "_ivol = i.am_ivol"
                           + " from UND_PRICE_CALC upc, MIS..VOL_HIST_IMPORT_STRADDLE_SKEW i"
                           + " where upc.id_imnt = i.id_imnt"
                           + " and upc.dt_bus = i.dt_bus"
                           + " and i.pr_strike = " + i
                           + " and i.am_pnl <> 'NaN'"
                           + " and upc.dt_bus >= '" + dt_start.ToString("yyyyMMdd") + "'"
                            + " and upc.dt_bus <= '" + dt_end.ToString("yyyyMMdd") + "'";
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        internal void InsertStraddlePL_Skew_1m(DateTime dt_start, DateTime dt_end, float id_imnt_backtest_start, float id_imnt_backtest_end, string strPath, string strFile)
        {
            string sqlText;
            sqlText = "DELETE FROM MIS..VOL_HIST_IMPORT_STRADDLE_SKEW_1M where dt_bus >= '" + dt_start.ToString("yyyyMMdd") + "'"
                + " and dt_bus <= '" + dt_end.ToString("yyyyMMdd") + "'";
            if (id_imnt_backtest_end > 0)
                sqlText = sqlText + " and id_imnt >= " + id_imnt_backtest_start + " and id_imnt < " + id_imnt_backtest_end;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            //sqlText = "BULK INSERT MIS..VOL_HIST_IMPORT_STRADDLE_SKEW FROM '" + strPath + strFile + "' " +
            //           "WITH ( FIRSTROW = 1, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')";

            sqlText = string.Format("BULK INSERT MIS..VOL_HIST_IMPORT_STRADDLE_SKEW_1M FROM '{0}{1}' WITH ( FIRSTROW = 1, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n' )", strPath, strFile);
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            for (double i = 1; i <= 1; i += .05)
            {
                sqlText = "update upc "
                           + " set straddle_pl_" + i * 100 + "_1m = i.am_pnl,"
                           + "  straddle_pl_" + i * 100 + "_1m_ivol = i.am_ivol"
                           + " from UND_PRICE_CALC upc, MIS..VOL_HIST_IMPORT_STRADDLE_SKEW_1M i"
                           + " where upc.id_imnt = i.id_imnt"
                           + " and upc.dt_bus = i.dt_bus"
                           + " and i.pr_strike = " + i
                           + " and i.am_pnl <> 'NaN'"
                           + " and upc.dt_bus >= '" + dt_start.ToString("yyyyMMdd") + "'"
                            + " and upc.dt_bus <= '" + dt_end.ToString("yyyyMMdd") + "'";
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        internal void InsertIntraPos(string strFile)
        {
            string sqlText = "BULK INSERT REC..p5_bo_intra FROM '" + strFile + "' " +
                       "WITH ( FIRSTROW = 1, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }          
        }

        internal void DeleteP5Intra()
        {
            string sqlText = "delete from REC..p5_bo_intra where time <> (select MAX(time) from REC..p5_bo_intra)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        internal List<VolParam> GetVolParamShock(double shockdn, double shockup)
        {
            string sqlText = "SELECT id_imnt, dt_mat, am_vol_atm " + 
                            ", am_skew_dn + ABS(sqrt(datediff(DD,dt_chg,dt_mat)/90.0) * " + shockdn + ") am_skew_dn " +
                            ", am_skew_up - ABS(sqrt(datediff(DD,dt_chg,dt_mat)/90.0) * " + shockup + ") am_skew_up " + 
                            ", pr_und, am_min, am_max, dt_chg, " +
                            " id_chg, skew_ts from VOL_PARAM";

            List<VolParam> volParamShock = new List<VolParam>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        volParamShock.Add(new VolParam(row));
                    }
                }
                return volParamShock;
            }
        }

        internal void InsertIntraPosCalc(string strFile)
        {
            string sqlText = "BULK INSERT OPTIONS..POSITION_LIVE FROM '" + strFile + "' " +
                       "WITH ( FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        internal void DeleteIntraPosCalc()
        {
            string sqlText = "delete from OPTIONS..POSITION_LIVE where dt_chg <> (select MAX(dt_chg) from OPTIONS..POSITION_LIVE)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        internal List<DateTime> getIdxExpyList(int id_imnt)
        {
            List<DateTime> expyList = new List<DateTime>();

            string sqlText = "select dt_mat from IND_OPTIONS_ACTIV where id_imnt_und = " + id_imnt + " group by dt_mat order by dt_mat";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlText, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expyList.Add(reader.GetDateTime(0));
                    }
                }
                return expyList;
            }


        }
    }
}
