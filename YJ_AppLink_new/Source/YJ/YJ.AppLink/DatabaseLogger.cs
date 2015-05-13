using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Data.SqlClient;
using YJ.AppLink.Pricing;

namespace YJ.AppLink
{
    ///public delegate void MessageLoggedHandler(object sender, MessageLoggedEventArgs args);

    /// <summary>
    /// A class that provides logging functionality for the AppLink
    /// </summary>
    public class DatabaseLogger
    {
        /// <summary>
		/// Occurs when a new message is received
        /// </summary>
        /// 
        const string connectionString = "Integrated Security=false;Data Source=VOLARBDB;Initial Catalog=MIS;Asynchronous Processing=true;User ID=volarb;Password=el3phant";
        const string connectionStringOptions = "Integrated Security=false;Data Source=VOLARBDB;Initial Catalog=OPTIONS;Asynchronous Processing=true;User ID=volarb;Password=el3phant";

        const string db = "MIS";
        const string dbOptions = "OPTIONS";
        const string schema = "dbo";
        const string table = "TRADE_LOG";
        /* TRADE_LOG Columns
         * id_imnt_ric,tx_buy_sell,,dt_mat,am_opt,pr_opt,pr_stk
         * am_delta,nm_broker,tx_init,tx_note,dt_trd,dt_time,tx_fill,id_imnt_und
         */

        public void insertNewMarket(MarketData md)
        {
            string errorMsg;
            string id_yj = "";
            string cmdText = "";

            /// Check if order already exists
            try
            {
                cmdText = string.Format("SELECT id_yj FROM {0}.{1}.{2} WHERE id_yj = '{3}'", db, schema, table, md.Id);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

                    object obj = cmd.ExecuteScalar();
                    id_yj = obj.ToString();
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }
            /// End
            
            /// Insert parent order
            double optPrc = 0;
            double am_opt = 0;
            string tx_buy_sell;
            string tx_init = "";

            // if there is a bid or offer
            if (double.IsNaN(md.BestBid))
            {
                optPrc = md.BestAsk;
                tx_buy_sell = "S";
                if (md.BestAskQuote == null)
                    am_opt = 0;
                else
                    am_opt = md.BestAskQuote.Quote.AskSize;
            }
            else
            {
                optPrc = md.BestBid;
                tx_buy_sell = "B";
                if (md.BestBidQuote == null)
                    am_opt = 0;
                else
                    am_opt = md.BestBidQuote.Quote.BidSize;
            }
            // check for NaN
            if (double.IsNaN(optPrc))
            {
                optPrc = 0;
                tx_buy_sell = "";
            }
            if (double.IsNaN(am_opt))
                am_opt = 0;

            // did we initiate
            if (md.LastQuote.Sender.ToString() == "schanconvexstrat" || md.LastQuote.Sender.ToString() == "tjackson1")
                tx_init = "Y";

            try
            {
                
                // check if market ID already exists in table
                if (id_yj != "")
                    cmdText = string.Format("UPDATE {0}.{1}.{2} set id_imnt_ric = '{3}',tx_buy_sell = '{4}',pr_opt = {5},nm_sender = '{6}',nm_description = '{7}', dt_update = '{8}', am_opt = {9} where id_yj = '{10}' and id_component = 0", 
                        db, schema, table, md.Options[0].StockSymbol, tx_buy_sell, optPrc, md.LastQuote.Sender, md.Description,DateTime.Now,am_opt,md.Id);
                else
                    cmdText = string.Format("INSERT INTO {0}.{1}.{2} (id_yj,id_imnt_ric,tx_buy_sell,pr_opt,dt_trd,nm_description,id_component,dt_update,nm_receiver,nm_sender,tx_init,am_opt) VALUES ('{3}','{4}','{5}',{6},'{7}','{8}',0,'{9}','{10}','{11}','{12}',{13})",
                        db, schema, table, md.Id, md.Options[0].StockSymbol, tx_buy_sell, optPrc, md.LastQuoteTime, md.Description, DateTime.Now, md.LastQuote.Receiver, md.LastQuote.Sender,tx_init,am_opt);
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }
            /// End

            // Insert Child Orders

            double id_component = 1;
            double cross_ratio;
            double cross_level;
            double ivol;
            foreach (Option opt in md.Options)
            {
                if (opt.Cross == null || double.IsNaN(opt.Cross.CrossLevel))
                    cross_level = 0;
                else
                    cross_level = opt.Cross.CrossLevel;

                if (opt.Cross == null || double.IsNaN(opt.Cross.Ratio))
                    cross_ratio = 0;
                else
                    cross_ratio = opt.Cross.Ratio;

                if (cross_ratio == 0)
                    cross_ratio = opt.Delta;

                if (double.IsNaN(opt.Vol))
                    ivol = 0;
                else
                    ivol = opt.Vol;

                try
                {
                    // check if market id already exists
                    if (id_yj != "")
                        cmdText = string.Format("UPDATE {0}.{1}.{2} set id_imnt_ric = '{3}', dt_mat = '{4}', nm_sender = '{5}',nm_receiver = '{6}',dt_trd = '{7}',nm_osi = '{8}',id_ratio= {9},nm_description= '{10}',pr_stk= {11},am_delta= {12}, dt_update = '{13}', am_ivol = {14} where id_yj = '{15}' and id_component = {16}",
                            db, schema, table, opt.StockSymbol, opt.Term + " " + opt.Strike + " " + opt.OptionType.Substring(0, 1), md.LastQuote.Sender, md.LastQuote.Receiver, md.LastQuoteTime, opt.OSICode, opt.Ratio * opt.Direction, md.Description, cross_level, cross_ratio, DateTime.Now,ivol, md.Id, id_component);
                    else
                        cmdText = string.Format("INSERT INTO {0}.{1}.{2} (id_yj,id_imnt_ric,dt_mat,nm_sender,nm_receiver,dt_trd,nm_osi,id_ratio,nm_description,pr_stk,am_delta,id_component,dt_update,tx_init,am_ivol) VALUES ('{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}',{12},{13},{14},'{15}','{16}',{17})",
                            db, schema, table, md.Id, opt.StockSymbol, opt.Term + " " + opt.Strike + " " + opt.OptionType.Substring(0, 1), md.LastQuote.Sender, md.LastQuote.Receiver, md.LastQuoteTime, opt.OSICode, opt.Ratio * opt.Direction, md.Description, cross_level, cross_ratio, id_component, DateTime.Now, tx_init, ivol);

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception exception)
                {
                    errorMsg = "Exception: " + exception.Message;
                }

                id_component++; // next leg
            }

            /// End
        }

        public double getUndPriceWE(Option leg)
        {
            string errorMsg;
            try
            {
                string cmdText = string.Format("select pr_imnt from OPTIONS..VOL_LIVE_WE where dt_chg = (select max(dt_chg) from OPTIONS..VOL_LIVE_WE) and id_imnt_ric = '{0}'", leg.StockSymbol);
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            return reader.GetDouble(0);
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }
            return 0;
        }
        public double getParamVol(Option leg)
        {
            ParamVol paramVol = new ParamVol();
            string cmdText;
            string errorMsg;
            DateTime? dt_mat;
            
            try
            {
                dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);
                cmdText = string.Format(" select * from {0}.{1}.VOL_PARAM vp, {0}.{1}.INSTRUMENT i where id_imnt_ric = '{3}' and vp.dt_mat > '{4}' and vp.dt_mat < '{5}' and vp.id_imnt = i.id_imnt", "OPTIONS", schema, table, leg.StockSymbol, dt_mat.Value.AddDays(-3).ToString(), dt_mat.Value.AddDays(3).ToString());

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{

						while (reader.Read())
						{

                            paramVol.am_vol_atm = reader.GetDouble(2);
                            paramVol.am_min = reader.GetDouble(6) / 100;
                            paramVol.am_max = reader.GetDouble(7) / 100;
                            paramVol.pr_und = reader.GetDouble(5);
                            paramVol.dt_chg = reader.GetDateTime(8);
                            paramVol.am_skew_dn = reader.GetDouble(3);
                            paramVol.am_skew_up = reader.GetDouble(4);
						}
					}
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }

            double am_vol_or;
            double am_skew;

            if (paramVol.am_vol_atm == null || paramVol.am_vol_atm == 0)
                return 0;

            if (leg.Strike <= paramVol.pr_und)
                am_skew = paramVol.am_skew_dn;
            else
                am_skew = paramVol.am_skew_up;

            am_vol_or = paramVol.am_vol_atm + am_skew * Math.Abs(paramVol.pr_und - leg.Strike) / (0.1 * paramVol.pr_und);

            if (am_vol_or < paramVol.am_min) 
                am_vol_or = paramVol.am_min;
            if (am_vol_or > paramVol.am_max)
                am_vol_or = paramVol.am_max;

            return am_vol_or; 
        }

        public WETheo getWETheo(Option leg)
        {
            WETheo wetheo = new WETheo();
            string cmdText;
            string errorMsg;
            DateTime? dt_mat;

            try
            {
                dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);
                cmdText = string.Format(" select * from {0}.{1}.WE_THEO wt where wt.id_imnt_und_ric = '{2}' and wt.dt_mat > '{3}' and wt.dt_mat < '{4}' and wt.pr_strike = {5} and wt.id_pc = '{6}'", "MIS", schema, leg.StockSymbol, dt_mat.Value.AddDays(-3).ToString(), dt_mat.Value.AddDays(3).ToString(), leg.Strike, leg.OptionType.Substring(0,1));

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            wetheo.dt_bus = reader.GetDateTime(0);
                            wetheo.id_imnt_ric = reader.GetString(2);
                            wetheo.dt_mat = reader.GetDateTime(3);
                            wetheo.pr_strike = (float) reader.GetDouble(4);
                            wetheo.id_pc = reader.GetString(5);
                            wetheo.pr_stk = (float)reader.GetDouble(19);
                            wetheo.pr_bid = (float)reader.GetDouble(7);
                            wetheo.pr_ask = (float)reader.GetDouble(8);
                            wetheo.pr_theo = (float)reader.GetDouble(18);
                            wetheo.am_delta = (float)reader.GetDouble(20);
                            wetheo.am_gamma = (float)reader.GetDouble(11);
                            wetheo.am_vega = (float)reader.GetDouble(12);
                            wetheo.am_theta = (float)reader.GetDouble(13);
                            wetheo.am_vol = (float)reader.GetDouble(16);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }

            return wetheo;
        }

        private Nullable<DateTime> getExpiryDate(int id_month, int id_year)
        {

            int ct_friday = new int();
            int i;
            DateTime dt_mat = new DateTime();
            DateTime dt;
            int id_month_curr;
            int id_year_curr;

            i = 1;
            id_month_curr = DateTime.Now.Month;
            id_year_curr = DateTime.Now.Year;

            if (id_month_curr > id_month)
            {
                if (id_year_curr == id_year)
                    id_year = id_year + 1;
            }

            do 
            {
                dt = new DateTime(id_year, id_month, i);
    
                if (dt.DayOfWeek == DayOfWeek.Friday)
                {
                    ct_friday ++;
                    dt_mat = dt;
                }
    
                if (i > 32)
                {
                    return null;
                }
    
                i++;

            } while (ct_friday < 3);

        return dt_mat.AddDays(1);
        }

        public Array fillArray(string sql)
        {
            List<double> resultsArray = new List<double>();
            string errorMsg;

            try
            {
                
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultsArray.Add(Convert.ToDouble(reader.GetValue(0)));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }
            return resultsArray.ToArray();
        }

        public List<divArray> fillArray2d(string sql)
        {
            var _divArray = new List<divArray>(); 
            string errorMsg;

            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _divArray.Add (new divArray {
                                dt_ex = reader.GetDateTime(0), 
                                am_div = reader.GetDouble(1) 
                            });
                            //resultsArray.Add(new double[] {Convert.ToDouble(reader.GetValue(0)), Convert.ToDouble(reader.GetValue(1))});
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }
            return _divArray;
        }

        public float getVega3m (string id_imnt_ric)
        {
            string sql = " select sum(p.am_vega * p.am_ct_sz * p.am_pos * sqrt(90/(convert(float,p.dt_mat - getdate() + 1)))) vega  from OPTIONS..POSITION_LIVE p, OPTIONS..INSTRUMENT i  "
                        + " where i.id_imnt = p.id_imnt_und  "
                        + " and i.id_imnt_ric = '" + id_imnt_ric + "' "
                        + "group by i.id_imnt, i.id_imnt_ric ";
                        
            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return (float) reader.GetDouble(0);                            
                        }
                    }
                }
            }



            catch (Exception exception)
            {
                //errorMsg = "Exception: " + exception.Message;
            }

            return 0;
        }

        public string getSectorName(string id_imnt_ric)
        {
            string sql = " select gics.nm_gics_sector  from OPTIONS..INSTRUMENT i, OPTIONS..REF_GICS_SECTOR gics"
                        + " where i.id_gics_sector = gics.id_gics_sector  "
                        + " and i.id_imnt_ric = '" + id_imnt_ric + "' ";

            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }



            catch (Exception exception)
            {
                //errorMsg = "Exception: " + exception.Message;
                return "";
            }

            return "";
        }

        public string[] getPrediction(string id_imnt_ric)
        {
            string[] resultsArr = new string[6];

            //get pred using walleye
            string sql = " select am_ivol_3m_pred, am_ivol_3m_100, am_vol_1m_fwd_pred,am_vol_1m_fwd_pred_logistic,am_vol_1m_fwd_pred_dual_rank,dt_bus from OPTIONS..INSTRUMENT i, OPTIONS..VOL_HIST_QA_IMPORT_INTRADAY_WE v "
                        + " where v.id_imnt = i.id_imnt "
                        + " and convert(varchar(8),v.dt_bus,112) = convert(varchar(8),GETDATE(),112) "
                        + " and i.id_imnt_ric = '" + id_imnt_ric + "' ";
            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToString(Math.Round(reader.GetFloat(4), 4)) == null)
                                continue;
                            resultsArr[0] = Convert.ToString(Math.Round(reader.GetFloat(0), 4));
                            resultsArr[1] = Convert.ToString(Math.Round(reader.GetFloat(1), 4));
                            resultsArr[2] = Convert.ToString(Math.Round(reader.GetFloat(2), 4));
                            resultsArr[3] = Convert.ToString(Math.Round(reader.GetFloat(3), 4));
                            resultsArr[4] = Convert.ToString(Math.Round(reader.GetFloat(4), 4));
                            resultsArr[5] = reader.GetDateTime(5).ToString("MM/dd/yyyy HH:mm");
                            //resultsArray.Add(new double[] {Convert.ToDouble(reader.GetValue(0)), Convert.ToDouble(reader.GetValue(1))});
                        }
                    }
                }

                //get pred using livevolpro
                if (resultsArr[4] == "0" || resultsArr[4] == null)
                {
                    sql = " select am_ivol_3m_pred, am_ivol_3m_100, am_vol_1m_fwd_pred,am_vol_1m_fwd_pred_logistic,am_vol_1m_fwd_pred_dual_rank,dt_bus from OPTIONS..INSTRUMENT i, OPTIONS..VOL_HIST_QA_IMPORT_INTRADAY v "
                        + " where v.id_imnt = i.id_imnt "
                        + " and convert(varchar(8),v.dt_bus,112) = convert(varchar(8),GETDATE(),112) "
                        + " and i.id_imnt_ric = '" + id_imnt_ric + "' ";
                    try
                    {

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            conn.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    resultsArr[0] = Convert.ToString(Math.Round(reader.GetFloat(0), 4));
                                    resultsArr[1] = Convert.ToString(Math.Round(reader.GetFloat(1), 4));
                                    resultsArr[2] = Convert.ToString(Math.Round(reader.GetFloat(2), 4));
                                    resultsArr[3] = Convert.ToString(Math.Round(reader.GetFloat(3), 4));
                                    resultsArr[4] = Convert.ToString(Math.Round(reader.GetFloat(4), 4));
                                    resultsArr[5] = reader.GetDateTime(5).ToString("MM/dd/yyyy HH:mm");
                                    //resultsArray.Add(new double[] {Convert.ToDouble(reader.GetValue(0)), Convert.ToDouble(reader.GetValue(1))});
                                }
                            }
                        }
                    }



                    catch (Exception exception)
                    {
                        //errorMsg = "Exception: " + exception.Message;
                    }
                }

                //get close prediction / implieds
                if (resultsArr[0] == "0" && resultsArr[1] == "0")
                {
                    sql = " select upc.am_ivol_3m_pred, vr.am_ivol_3m_100, upc.am_vol_1m_fwd_pred,upc.am_vol_1m_fwd_pred_logistic,upc.am_vol_1m_fwd_pred_dual_rank  "
                        + " from OPTIONS..INSTRUMENT i, OPTIONS..UND_PRICE_CALC upc, OPTIONS..VOL_REL vr "
                        + " where i.id_imnt = upc.id_imnt"
                        + " and vr.id_imnt = upc.id_imnt "
                        + " and upc.dt_bus = vr.dt_bus "
                        + " and vr.dt_bus = (select MAX(dt_bus) from OPTIONS..UND_PRICE_DATE) "
                        + " and i.id_imnt_ric = '" + id_imnt_ric + "' ";
                    try
                    {

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            conn.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    resultsArr[0] = Convert.ToString(Math.Round(reader.GetFloat(0), 4));
                                    resultsArr[1] = Convert.ToString(Math.Round(reader.GetFloat(1), 4));
                                    resultsArr[2] = Convert.ToString(Math.Round(reader.GetFloat(2), 4));
                                    resultsArr[3] = Convert.ToString(Math.Round(reader.GetFloat(3), 4));
                                    resultsArr[4] = Convert.ToString(Math.Round(reader.GetFloat(4), 4));
                                    resultsArr[5] = "close";
                                    //resultsArray.Add(new double[] {Convert.ToDouble(reader.GetValue(0)), Convert.ToDouble(reader.GetValue(1))});
                                }
                            }
                        }
                    }
                


                    catch (Exception exception)
                    {
                        //errorMsg = "Exception: " + exception.Message;
                    }
                }
                if (resultsArr[0] == "0" && resultsArr[1] == "0")
                {
                    resultsArr[0] = "-1";
                    resultsArr[1] = "-1";
                    resultsArr[2] = "-1";
                }

                
            }
            catch (Exception exception)
            {
                //errorMsg = "Exception: " + exception.Message;
                resultsArr[0] = "-1";
                resultsArr[1] = "-1";
                resultsArr[2] = "-1";
            }


            return resultsArr;
        }
        public class divArray
        {
            public DateTime dt_ex { get; set; }
            public double am_div { get; set; }

        }
        public class ParamVol
        {
            public double am_vol_atm;
            public double am_min;
            public double am_max;
            public double pr_und;
            public double am_skew_dn;
            public double am_skew_up;
            public DateTime dt_chg;
        }

    }


}