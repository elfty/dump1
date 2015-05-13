using System;
using System.Collections.Generic;
using System.Text;
using LRRELib;
using System.Linq;


namespace wpfexample
{

    class mhModels
    {
        public float mh_delta;
        public float mh_gamma;
        public float mh_impvol;
        public float mh_theta;
        public float mh_thvalue;
        public float mh_vega;
        public float am_vol_db;
        public float am_rate_db;
        public float am_time_db;
        public float mh_impdelta;
        
        public float calcRate(DateTime? dt_mat, List<Rates> ratesArr)
        {

            DateTime dt_val;
            Double am_time;

            int i = 0;

            dt_val = DateTime.Today;
            am_time = Convert.ToDouble(dt_mat.Value.Subtract(dt_val.AddDays(-1)).Days) / 365;

            if (am_time < 0)
                return 0;

            foreach (Rates rate in ratesArr)
            {
                if (Math.Round((double)ratesArr[i].am_yrs, 4) <= Math.Round(am_time,4) && (double)ratesArr[i + 1].am_yrs > am_time)
                    return (float)LinInterp((double)ratesArr[i].am_yrs, (double)ratesArr[i].am_rate, (double)ratesArr[i + 1].am_yrs, (double)ratesArr[i + 1].am_rate, am_time);
                i++;
            }

            return 0;
        }

        public void CalcFut(Position pos, Instrument item, Instrument item_und, List<Rates> ratesArr, float pr_imnt, List<Dividend> dividend, IndexData indexdata = null)
        {
            float am_time;
            float am_rate;
            float am_div = 0;
            DateTime dt_val = DateTime.Today;

            /* if (indexdata != null && item_und.id_typ_imnt == "IND")
            {
                //=IF(id_typ_und="c",C32,IF(id_imnt_ric = "-","",(C33+div_front_month)/C43))
                float am_adj_rate = (float)(1+((indexdata.am_rate_main-(indexdata.am_borrow/100))/100*(Convert.ToDouble(indexdata.dt_front_mth.Value.Subtract(dt_val.AddDays(-1)).Days))/365));
                divArray = calcDiv(dividendArr.Where(x => x.id_imnt == item.id_imnt_und && item_und.id_div_src == x.id_src).ToList(), item, indexdata);
                float divsum = 0;
                for (int i = 1; i < dividend.Length; i = i + 2)
                    divsum += (float)(double)divArray.GetValue(i);
                //TO DO NPV
                //am_div = Application.WorksheetFunction.NPV(calcRate(dt_ex, "P"), am_div)
                float pr_implied = (item_fut.pr_imnt + divsum) / am_adj_rate;
            }
            
            */

            am_time = ((DateTime)item.dt_mat - DateTime.Today).Days;
            am_rate = calcRate(item.dt_mat, ratesArr);
            if (am_time < 0)
                am_time = 0;
            
            if (dividend == null)
                am_div = 0;
            else
            {
                foreach (Dividend div in dividend)
                {
                    if (item_und.id_div_src == div.id_src)
                    {
                        if (div.dt_ex < item.dt_mat.Value.AddDays(1) && div.dt_ex > DateTime.Today)
                        {
                            //am_div = am_div + (float)div.am_div;
                            am_time = (((DateTime)div.dt_ex).Subtract(dt_val).Days) / 365f;
                            am_div = am_div + (float)(div.am_div * Math.Exp(-am_rate * am_time));
                        }

                    }
                }
            }

            float am_adj_rate = (float)(1+((indexdata.am_rate_main-(indexdata.am_borrow/100))/100*(Convert.ToDouble(indexdata.dt_front_mth.Value.Subtract(dt_val.AddDays(-1)).Days))/365));
            //(pr_lvl+div_front_month)/(1+(((am_rate_front_month-(am_rate_repo/100))/1)*am_days_front_month)/365))
            float pr_implied = (pos.pr_imnt + am_div) / am_adj_rate;
            //float pr_implied = (pos.pr_imnt+am_div)/(1+(((am_adj_rate-(am_rate_repo/100))/1)*am_days_front_month)/365))
            float pr_imnt1 = (float)(pr_imnt * (1 + (((am_rate + 0.01) * am_time) / 365)) - am_div);
            float pr_imnt2 = (float)(pr_imnt * (1 + (((am_rate - 0.01) * am_time) / 365)) - am_div);
            float am_rho = 0;

            if (pr_imnt != 0)
                am_rho = ((pr_imnt1 - pr_imnt2) / (2 * pr_imnt)) * 1000;

            pos.pr_theo = pr_implied;
            //pos.pr_imnt = pr_implied;
            pos.am_vol = 0;
            pos.am_vol_or = 0;
            pos.am_delta = 1;
            pos.am_delta_imp = 1;
            pos.am_gamma = 0;
            pos.am_vega = 0;
            pos.am_theta = 0;
            pos.am_rho = am_rho;
            pos.am_time = am_time / 365;
            pos.dt_chg = DateTime.Now;

        }

        public void CalcOpt(Position pos, Instrument item, Instrument item_und, List<Rates> ratesArr, float pr_imnt, List<Dividend> dividendArr, List<VolParam> volparamArr, float und_price, DateTime dt_val, float? pr_theo = null, float? am_borrow_rate = null, float und_price_bump = 0, float vol_bump = 0, float am_beta = 1)
        {

            Array divArray;
            
            __MIDL___MIDL_itf_LRRE_0001_0064_0001 id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American;
            if (item_und.id_typ_imnt == "IND")
                id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_European;

            float am_frrate = 0f;
            float am_rate = calcRate(item.dt_mat, ratesArr);
            float am_vol = calcVol(item, volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)), pos);

            if (pos.am_vol_or > 0)
                am_vol = pos.am_vol_or;

            am_vol = am_vol * (1 + vol_bump);

            float time = (float)(Convert.ToDouble(item.dt_mat.Value.AddDays(1).Subtract(dt_val).Days) / 365);
            float btime = (float)(Convert.ToDouble(item.dt_mat.Value.AddDays(1).Subtract(dt_val).Days) / 365);
            float pr_und = pos.pr_und * (1 + (und_price_bump * am_beta));

            if (pr_theo != null && item_und.id_typ_imnt == "IND")
            {
                pr_und = (float)pr_theo * (1 + (und_price_bump * am_beta));
                //add 1/3 to friday for expiry for expiry on closing print
                time = (float)((Convert.ToDouble(item.dt_mat.Value.Subtract(dt_val).Days) + .3333) / 365);
                btime = (float)((Convert.ToDouble(item.dt_mat.Value.Subtract(dt_val).Days) + .3333) / 365);
                id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_European;
                am_frrate = (float)am_borrow_rate / 100;
                divArray = calcDivIdx(dividendArr.Where(x => x.id_imnt == item.id_imnt_und && item_und.id_div_src == x.id_src).ToList(), item, ratesArr);
            }
            else
            {
                divArray = calcDiv(dividendArr.Where(x => x.id_imnt == item.id_imnt_und && item_und.id_div_src == x.id_src).ToList(), item);
                //divArray = calcDiv(dividendArr.Where(x => x.id_imnt == item.id_imnt_und).ToList(), item);
            }

            //TODO: BORROW for SS

            int am_days = (int)Math.Round((((Math.Round(time, 4) * 365) + 1) / 2), 0);
            int iterations = calcIter(am_days);
            //int iterations = calcIter(item.dt_mat.Value.Subtract(dt_val).Days + 1);

            //float price = (float)liveOptMidPrice;
            LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002 call_put;

            if (item.id_pc == "P")
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Put;
            else
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Call;

            Array userVarArray = new float[1];
            userVarArray.SetValue(0f, 0);

            Array pInputArray = new object[1];
            pInputArray.SetValue("", 0);

            string blah = ""; //filler
            float pr_opt = 0f;

            //FLOATING DELTAS

            /*float am_vol_path = 0;
            float am_strike_dist = 0;
            float am_vol_atm = volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)).am_vol_atm;
            float pr_strike_atm = volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)).pr_und;
            if (item_und.id_typ_imnt == "IND")
            {
                am_strike_dist = (float)(.25 * pr_strike_atm * am_vol_atm * Math.Sqrt(time));
                am_vol_path = 1.5f;
            }
            else
            {
                am_strike_dist = (float)(.25 * pr_strike_atm * am_vol_atm * Math.Sqrt(time));
                am_vol_path = 1f;
            }
            */
            //FLOATING DELTAS
            /*float am_vol_up = calcVol2((float)item.pr_strike + am_strike_dist, (DateTime)item.dt_mat, am_vol_path,volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)), pos);
            float am_vol_dn = calcVol2((float)item.pr_strike - am_strike_dist, (DateTime)item.dt_mat, am_vol_path,volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)), pos);
            float mh_thvalue_up = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike + am_strike_dist, am_vol_up, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
            float mh_thvalue_dn = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike - am_strike_dist, am_vol_dn, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
            
             
            mh_impdelta = (mh_thvalue_dn - mh_thvalue_up) / (am_strike_dist * 2);
            */
            //divArray = null;
            //FLOATING DELTA
            mh_impdelta = 1;
            LRREmdl bc = new LRREmdl();
            mh_delta = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Delta,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
            mh_gamma = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Gamma,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);

            /*mh_impvol = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ImpVol,
                 ref blah,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 (float)liveUndPrice, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, price, false, ref pInputArray);
            */
            mh_theta = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Theta,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);

            mh_thvalue = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
            
            if (mh_thvalue == -1)
                mh_thvalue = 999;

            mh_vega = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Vega,
                 ref blah,
                 id_ae,
                 LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                 call_put,
                 pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
            am_vol_db = am_vol;
            am_time_db = time;
            am_rate_db = am_rate;
        }

        public float CalcModel(DateTime dt_val, int exercise, float pr_und, float pr_strike, string id_pc, DateTime dt_mat, float am_rate, List<Dividend> dividendArr, float am_vol, string calc_type, float am_div_yld_12m = 0f, Array _divArray = null)
        {
            Array divArray = null;
            if (_divArray == null)
                divArray = calcDivIdxYld(pr_und, am_div_yld_12m, dt_val, dt_mat);
                //divArray = calcDivDtMat(dividendArr, dt_mat);
                //divArray = null;
            else
                divArray = _divArray;


            LRREmdl bc = new LRREmdl();
            //MHMODELSLib.Microhedge bc = new MHMODELSLib.Microhedge();
            __MIDL___MIDL_itf_LRRE_0001_0064_0001 id_ae = __MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American;
    

            if (exercise == 2)
                id_ae = __MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_European;
            else
                id_ae = __MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American;

            float am_frrate = 0f;
            //float am_rate = calcRate(dt_mat, ratesArr);
            //float am_rate = 0;
            //float am_vol = calcVol(item, volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)), pos) * (1 + am_vol_bump);

            //if (pos.am_vol_or != 0)
            //    am_vol = pos.am_vol_or;

            //DateTime dt_val = DateTime.Today;
            float time = (float)(Convert.ToDouble(dt_mat.Subtract(dt_val).Days) / 365);
            float btime = (float)(Convert.ToDouble(dt_mat.Subtract(dt_val).Days) / 365);

            //TODO: BORROW for SS
            //TODO: PV for Dividends?
            int am_days = (int) Math.Round((((Math.Round(time,4) * 365) + 1)),0);

            //int iterations = calcIter(item.dt_mat.Value.Subtract(dt_val).Days + 1);

            int iterations = calcIter(am_days);

            //float price = (float)liveOptMidPrice;
            __MIDL___MIDL_itf_LRRE_0001_0064_0002 call_put;

            if (id_pc == "P")
                call_put = __MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Put;
            else
                call_put = __MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Call;

            Array userVarArray = new float[1];
            userVarArray.SetValue(0f, 0);
            //userVarArray.SetValue(35f, 8);

            Array pInputArray = new object[1];
            pInputArray.SetValue("", 0);

            string blah = ""; //filler
            float pr_opt = 0f;

            //divArray = calcDiv(dividendArr.Where(x => x.id_imnt == item.id_imnt_und && item_und.id_div_src == x.id_src).ToList(), item);
            //divArray = null;
            //am_rate = .056f;
            float calc_results = 0;
            switch (calc_type)
            {
                case "Delta":

                    calc_results = (float)bc.Calculate(__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Delta,
                        ref blah,
                        id_ae,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Gamma":
                    calc_results = (float)bc.Calculate(__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Gamma,
                        ref blah,
                        id_ae,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Vega":
                    calc_results = (float)bc.Calculate(__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Vega,
                        ref blah,
                        id_ae,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Theta":
                    calc_results = (float)bc.Calculate(__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Theta,
                        ref blah,
                        id_ae,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Theo":
                    calc_results = (float)bc.Calculate(__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                        ref blah,
                        id_ae,
                        __MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
            }
            return calc_results;

        }

        public float CalcVega(Position pos, Instrument item, Instrument item_und, List<Rates> ratesArr, float pr_imnt, List<Dividend> dividendArr, List<VolParam> volparamArr, float und_price, string calc_type, float am_vol_bump, DateTime dt_val, float? pr_theo = null, float? am_borrow_rate = null)
        {

            Array divArray;
            LRREmdl bc = new LRREmdl();
            __MIDL___MIDL_itf_LRRE_0001_0064_0001 id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_American;

            if (item_und.id_typ_imnt == "IND")
                id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_European;

            float am_frrate = 0f;
            float am_rate = calcRate(item.dt_mat, ratesArr);
            float am_vol = calcVol(item, volparamArr.FirstOrDefault(x => x.dt_mat == item.dt_mat || x.dt_mat == ((DateTime)item.dt_mat).AddDays(1)), pos) * (1 + am_vol_bump);

            if (pos.am_vol_or != 0)
                am_vol = pos.am_vol_or;

            //DateTime dt_val = DateTime.Today.AddDays(5);
            float time = (float)(Convert.ToDouble(item.dt_mat.Value.AddDays(1).Subtract(dt_val).Days) / 365);
            float btime = (float)(Convert.ToDouble(item.dt_mat.Value.AddDays(1).Subtract(dt_val).Days) / 365);
            float pr_und = und_price;

            if (item_und.id_typ_imnt == "IND")
            {
                //pr_und = (float)pr_theo;
                //add 1/3 to friday for expiry for expiry on closing print
                time = (float)((Convert.ToDouble(item.dt_mat.Value.Subtract(dt_val).Days) + .3333) / 365);
                btime = (float)((Convert.ToDouble(item.dt_mat.Value.Subtract(dt_val).Days) + .3333) / 365);
                id_ae = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0001.MH_European;
                //am_frrate = (float)am_borrow_rate / 100;
            }

            //TODO: BORROW for SS
            //TODO: PV for Dividends?
            int am_days = (int)Math.Round((((Math.Round(time, 4) * 365) + 1) / 2), 0);

            //int iterations = calcIter(item.dt_mat.Value.Subtract(dt_val).Days + 1);

            int iterations = calcIter(am_days);

            //float price = (float)liveOptMidPrice;
            LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002 call_put;

            if (item.id_pc == "P")
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Put;
            else
                call_put = LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0002.MH_Call;

            Array userVarArray = new float[1];
            userVarArray.SetValue(0f, 0);

            Array pInputArray = new object[1];
            pInputArray.SetValue("", 0);

            string blah = ""; //filler
            float pr_opt = 0f;

            divArray = calcDiv(dividendArr.Where(x => x.id_imnt == item.id_imnt_und && item_und.id_div_src == x.id_src).ToList(), item);
            // divArray = null;
            float calc_results = 0;
            switch (calc_type)
            {
                case "Delta":
                    calc_results = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Delta,
                        ref blah,
                        id_ae,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Gamma":
                    calc_results = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Gamma,
                        ref blah,
                        id_ae,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Vega":
                    calc_results = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Vega,
                        ref blah,
                        id_ae,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Theta":
                    calc_results = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_Theta,
                        ref blah,
                        id_ae,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
                case "Theo":
                    calc_results = (float)bc.Calculate(LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0005.MH_Builtin,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0003.MH_ThValue,
                        ref blah,
                        id_ae,
                        LRRELib.__MIDL___MIDL_itf_LRRE_0001_0064_0004.MH_Equity,
                        call_put,
                        pr_und, (float)item.pr_strike, am_vol, am_rate, am_frrate, time, btime, ref divArray, ref userVarArray, iterations, pr_opt, false, ref pInputArray);
                    break;
            }
            return calc_results;

        }
        private int calcIter(int am_days)
        {
            int iterations = 0;
            //int am_days_2 = (int) Math.Floor(am_days/2.0);
            int am_days_2 = am_days;
            if (am_days_2 < 24)
                iterations = 24;
            else if (am_days_2 < 35)
                iterations = am_days_2;
            else
                iterations = 35;
            return iterations;

        }
        private Array calcDiv(List<Dividend> dividend, Instrument item, IndexData indexdata = null)
        {
            DateTime dt_val;
            DateTime dt_mat;
            double am_time;
            Array divArrayResults = new double[2];
            int i = 0;
            
            dt_val = DateTime.Today;
            if (indexdata == null)
                dt_mat = (DateTime)item.dt_mat;
            else
                dt_mat = (DateTime)indexdata.dt_front_mth;

            am_time = (dt_mat.Subtract(dt_val).Days) / 365f;

            if (am_time < 0 || dividend == null || dividend.Count == 0)
                return null;

            divArrayResults = new double[dividend.Where(x => x.dt_ex > DateTime.Today && x.dt_ex <= item.dt_mat).ToList().Count * 2];

            int j = 0;
            for (i = 0; i < dividend.Count; i++)
            {
                if (dividend[i].dt_ex > DateTime.Today && dividend[i].dt_ex <= item.dt_mat)
                {
                    am_time = (((DateTime)dividend[i].dt_ex).Subtract(dt_val).Days) / 365f;
                    divArrayResults.SetValue(am_time, j);
                    divArrayResults.SetValue(dividend[i].am_div, j+1);
                    j += 2;
                }
            }

            return divArrayResults;
        }

        private Array calcDivIdx(List<Dividend> dividend, Instrument item, List<Rates> ratesArr, IndexData indexdata = null)
        {
            DateTime dt_val;
            DateTime dt_mat;
            double am_time;
            double am_div = 0;
            double am_rate = 0;
            Array divArrayResults = new double[2];
            int i = 0;

            dt_val = DateTime.Today;
            if (indexdata == null)
                dt_mat = (DateTime)item.dt_mat;
            else
                dt_mat = (DateTime)indexdata.dt_front_mth;

            am_time = (dt_mat.Subtract(dt_val).Days) / 365f;
            am_rate = calcRate(dt_mat,ratesArr);

            if (am_time < 0 || dividend == null || dividend.Count == 0)
                return null;

            divArrayResults = new double[dividend.Count * 2];

            int j = 0;
            for (i = 0; i < dividend.Count; i++)
            {
                if (dividend[i].dt_ex > DateTime.Today && dividend[i].dt_ex <= item.dt_mat)
                {
                    
                    am_time = (((DateTime)dividend[i].dt_ex).Subtract(dt_val).Days) / 365f;
                    am_div = (double)dividend[i].am_div * Math.Exp(-am_rate * am_time);
                    divArrayResults.SetValue(am_time, j);
                    divArrayResults.SetValue(am_div, j + 1);
                    j += 2;
                }
            }

            return divArrayResults;
        }

        private Array calcDivIdxYld(float pr_close, float am_div_yld, DateTime dt_val, DateTime dt_mat)
        {
            float am_time;
            float am_div = 0;

            Array divArrayResults = new double[2];

            am_time = dt_mat.Subtract(dt_val.AddDays(1)).Days / 365f;
            am_div = (pr_close * am_div_yld) * am_time;
            divArrayResults.SetValue(am_time, 0);
            divArrayResults.SetValue(am_div, 1);

            return divArrayResults;
        }

        public Array calcDivDtMat(List<Dividend> dividend, DateTime dt_mat, IndexData indexdata = null)
        {
            DateTime dt_val;
            double am_time;
            Array divArrayResults = new double[2];
            int i = 0;

            dt_val = DateTime.Today;

            am_time = (dt_mat.Subtract(dt_val).Days) / 365f;

            if (am_time < 0 || dividend == null || dividend.Count == 0)
                return null;

            divArrayResults = new double[dividend.Count * 2];

            int j = 0;
            for (i = 0; i < dividend.Count; i++)
            {
                if (dividend[i].dt_ex > DateTime.Today && dividend[i].dt_ex <= dt_mat)
                {
                    am_time = (((DateTime)dividend[i].dt_ex).Subtract(dt_val).Days) / 365f;
                    divArrayResults.SetValue(am_time, j);
                    divArrayResults.SetValue(dividend[i].am_div, j + 1);
                    j += 2;
                }
            }

            return divArrayResults;
        }

        public float calcVol(Instrument item, VolParam volparam, Position pos)
        {
            DateTime? dt_mat;

            //dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);
            dt_mat = item.dt_mat;

            float am_vol_or;
            float am_skew;

            if (volparam == null)
                return 0;

            if (item.pr_strike <= volparam.pr_und)
                am_skew = volparam.am_skew_dn;
            else
                am_skew = volparam.am_skew_up;

            
            am_vol_or = (float)(volparam.am_vol_atm + am_skew * Math.Abs(volparam.pr_und - (float)item.pr_strike) / (0.1 * volparam.pr_und));

            //PARALLEL VOL
            //am_vol_or = (float)volparam.am_vol_atm;

            if (am_vol_or < volparam.am_min / 100)
                am_vol_or = volparam.am_min / 100;
            if (am_vol_or > volparam.am_max / 100)
                am_vol_or = volparam.am_max / 100;

            if (am_vol_or == 0 || am_vol_or == null) //if no vol then use implied vol from imagine
                am_vol_or = pos.am_vol;

            return am_vol_or;
        }

        public float calcVol2(float pr_strike, DateTime dt_mat, float am_vol_path, VolParam volparam, Position pos)
        {
            //DateTime? dt_mat;

            //dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);
            //dt_mat = item.dt_mat;

            float am_vol_or;
            float am_skew;

            if (volparam == null)
                return 0;

            if (pr_strike <= volparam.pr_und)
                am_skew = volparam.am_skew_dn * am_vol_path;
            else
                am_skew = volparam.am_skew_up * am_vol_path;

            am_vol_or = (float)(volparam.am_vol_atm + am_skew * Math.Abs(volparam.pr_und - (float)pr_strike) / (0.1 * volparam.pr_und));

            if (am_vol_or < volparam.am_min / 100)
                am_vol_or = volparam.am_min / 100;
            if (am_vol_or > volparam.am_max / 100)
                am_vol_or = volparam.am_max / 100;

            if (am_vol_or == 0 || am_vol_or == null) //if no vol then use implied vol from imagine
                am_vol_or = pos.am_vol;

            return am_vol_or;
        }

        public float calcVolNoInst(DateTime dt_mat,float pr_strike, VolParam volparam, Position pos)
        {

            //dt_mat = getExpiryDate(Convert.ToDateTime(leg.Term.Substring(0, 3) + " 01, 1900").Month, Convert.ToDateTime("Jan 01, " + leg.Term.Substring(3, 2)).Year);

            float am_vol_or;
            float am_skew;

            if (volparam == null)
                return 0;

            if (pr_strike <= volparam.pr_und)
                am_skew = volparam.am_skew_dn;
            else
                am_skew = volparam.am_skew_up;


            am_vol_or = (float)(volparam.am_vol_atm + am_skew * Math.Abs(volparam.pr_und - pr_strike) / (0.1 * volparam.pr_und));

            //PARALLEL VOL
            //am_vol_or = (float)volparam.am_vol_atm;

            if (am_vol_or < volparam.am_min / 100)
                am_vol_or = volparam.am_min / 100;
            if (am_vol_or > volparam.am_max / 100)
                am_vol_or = volparam.am_max / 100;

            if (am_vol_or == 0 || am_vol_or == null) //if no vol then use implied vol from imagine
                am_vol_or = pos.am_vol;

            return am_vol_or;
        }
        public double LinInterp(double x1, double y1, double x2, double y2, double X)
        {
            return ((X - x1) * y2 + (x2 - X) * y1) / (x2 - x1);
        }
    }
}
