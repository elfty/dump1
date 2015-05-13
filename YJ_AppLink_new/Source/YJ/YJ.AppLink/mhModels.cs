using System;
using System.Collections.Generic;
using System.Text;
using LRRELib;
using YJ.AppLink.Pricing;

namespace YJ.AppLink
{
    public class mhModels
    {
        public float mh_delta;
        public float mh_gamma;
        public float mh_impvol;
        public float mh_theta;
        public float mh_thvalue;
        public float mh_vega;

        public mhModels(Option leg, double undPrice, double liveUndPrice, double liveOptMidPrice)
        {
            DateTime? dt_mat;
            DateTime dt_val;
            dt_val = DateTime.Today;
            dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);

            LRREmdlClass bc = new LRREmdlClass();

            Array divArray;

            float underlyingPrice = (float)undPrice;
             
            /*if (leg.Cross == null)
                underlyingPrice = (float)undPrice;
            else if (double.IsNaN(leg.Cross.CrossLevel))
                underlyingPrice = (float)undPrice;
            else
                underlyingPrice = (float)leg.Cross.CrossLevel;*/

            float strike = (float) leg.Strike;
            float volatility = calcVol(leg);
            float intRate = calcRate(dt_mat);
            float frIntRate = 0f;

            float time = (float) (Convert.ToDouble(dt_mat.Value.Subtract(dt_val).Days) / 365);
            float btime = (float) (Convert.ToDouble(dt_mat.Value.Subtract(dt_val).Days) / 365);
            int iterations = 100;
            float price = (float) liveOptMidPrice;
            LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002 call_put;

            divArray = calcDiv(leg, dt_mat);

            if (leg.OptionType == "PUT")
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Put;
            else
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Call;

            Array userVarArray = new float[1];
            userVarArray.SetValue(0.0f, 0);

            Array pInputArray = new object[1];
            pInputArray.SetValue("", 0);

            string blah = ""; //filler
            
            /*divArray = new double[2];
            divArray.SetValue(.2356164, 0);
            divArray.SetValue(1, 1);

            /*divArray = null;*/

            mh_delta = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Delta,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 underlyingPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);

            mh_gamma = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Gamma,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 underlyingPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);

            mh_impvol = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ImpVol,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 (float)liveUndPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);

            mh_theta = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Theta,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 underlyingPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);

            mh_thvalue = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 underlyingPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);
            if (mh_thvalue == -1)
                mh_thvalue = 999;
            mh_vega = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Vega,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 underlyingPrice, strike, volatility, intRate, frIntRate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);

        }

        private float calcVol(Option leg)
        {
            DatabaseLogger dbLog = new DatabaseLogger();
            return (float) dbLog.getParamVol(leg);     
        }

        private float calcRate(DateTime? dt_mat)
        {
            DatabaseLogger dbLog = new DatabaseLogger();

            DateTime dt_val;
            Double am_time;
            string sql;
            Array ratesArray;
            Array yrsArray;
            int i = 0;

            dt_val = DateTime.Today;
            am_time = Convert.ToDouble(dt_mat.Value.Subtract(dt_val).Days) / 365;

            if (am_time < 0)
                return 0;
            
            sql = " select am_rate from OPTIONS..Rates order by am_yrs";
            ratesArray = dbLog.fillArray(sql);

            sql = " select am_yrs from OPTIONS..Rates order by am_yrs";
            yrsArray = dbLog.fillArray(sql);
            
            foreach (double rate in ratesArray)
            {
                if (Math.Round((double)yrsArray.GetValue(i), 4) <= am_time && (double)yrsArray.GetValue(i + 1) > am_time)
                    return (float) LinInterp((double)yrsArray.GetValue(i), (double)ratesArray.GetValue(i), (double)yrsArray.GetValue(i + 1), (double)ratesArray.GetValue(i + 1), am_time);
                i++;
            }

            return 0;
        }

        private Array calcDiv(Option leg, DateTime? dt_mat)
        {
            DatabaseLogger dbLog = new DatabaseLogger();

            DateTime dt_val;
            double am_time;
            string sql;
            var _divArray = new List<DatabaseLogger.divArray>();
            Array divArrayResults = new double[2];
            int i = 0;

            dt_val = DateTime.Today;
            am_time = (dt_mat.Value.Subtract(dt_val).Days) / 365;

            if (am_time < 0)
                return null;

            sql = "select d.dt_ex, d.am_div ";
            sql = sql + " from OPTIONS..DIVIDENDS d, OPTIONS..INSTRUMENT i ";
            sql = sql + " where i.id_imnt_ric = '" + leg.StockSymbol + "' ";
            sql = sql + " and d.id_src = i.id_div_src ";
            sql = sql + " and d.id_imnt = i.id_imnt ";
            sql = sql + " and d.dt_ex > '" + dt_val + "' ";
            sql = sql + " and d.dt_ex < '" + dt_mat.Value.AddDays(1) + "' ";
            sql = sql + " order by d.dt_ex";
            _divArray = dbLog.fillArray2d(sql);

            if (_divArray == null)
                return null;

            divArrayResults = new double[_divArray.Count * 2];

            int j = 0;
            for (i = 0; i < _divArray.Count; i++) 
            {
                am_time = (_divArray[i].dt_ex.Subtract(dt_val).Days) / 365f;
                divArrayResults.SetValue(am_time, j);
                divArrayResults.SetValue(_divArray[i].am_div, j+1);
                j += 2;
            }

            return divArrayResults;
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

    public double LinInterp(double x1, double y1, double x2, double y2, double X )
    {
        return ((X - x1) * y2 + (x2 - X) * y1) / (x2 - x1);
    }


    }
}
