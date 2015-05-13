using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
namespace wpfexample
{
    class Riskslide
    {
        private List<Position> _posArr;
        private List<Portfolio> _prtfArr;
        private List<VolParam> _volparamArr;
        private Dictionary<int, Instrument> _instArr;
        private List<Rates> _ratesArr;
        private List<Dividend> _divArr;
        List<IndexData> _indexdataArr = new List<IndexData>();
        private DBHelper _dbhelper = new DBHelper();
        private Dictionary<int, float> _instbeta = new Dictionary<int, float>();

        public Riskslide(List<Position> posArr, List<Portfolio> prtfArr, List<VolParam> volparamArr
            , Dictionary<int, Instrument> instArr, List<Rates> ratesArr, List<Dividend> divArr, List<IndexData> indexdataArr)
        {
            _posArr = posArr;
            _prtfArr = prtfArr;
            _volparamArr = volparamArr;
            _instArr = instArr;
            _ratesArr = ratesArr;
            _divArr = divArr;
            _instbeta = _dbhelper.GetInstBeta();
            _indexdataArr = indexdataArr;
            runVegaSlide();
        }

        private void runVegaSlide()
        {
            Stopwatch sw = new Stopwatch();
            TimeSpan duration;
            sw.Restart();

            mhModels mhmodels = new mhModels();
            DateTime dt_val = DateTime.Today.AddDays(0);
            //dt_val = dt_val.AddDays(0);
            List<string> greekList = new List<string>();
            greekList.Add("Vega");
            greekList.Add("Gamma");
            greekList.Add("Theta");
            greekList.Add("Delta");

            _dbhelper.DeleteVegaSlide();
            foreach (string greek in greekList)
            {
                for (int j = 1; j <= 4; j++)
                {
                    if (j == 1 || j == 2)                    
                        _prtfArr = _dbhelper.GetPortfolio();
                    else
                        _prtfArr = _dbhelper.GetPortfolioSpecials();

                    for (int i = -10; i <= 10; i++)
                    {
                        Dictionary<int, Vegaslide> _vegaslide = new Dictionary<int, Vegaslide>();
                        Dictionary<string, double> _vegaslide_name = new Dictionary<string, double>();
                        Dictionary<string, double> _vegaslidevol_name = new Dictionary<string, double>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_down_5 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_down_5 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_down_10 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_down_10 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_down_25 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_down_25 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_down_50 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_down_50 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_up_5 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_up_5 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_up_10 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_up_10 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_up_25 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_up_25 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_up_50 = new Dictionary<int, Vegaslide>();
                        Dictionary<int, Vegaslide> _vegaslide_vegavol_up_50 = new Dictionary<int, Vegaslide>();

                        double greektot = 0;
                        double greektot_down_5 = 0;
                        double greektot_down_10 = 0;
                        double greektot_down_25 = 0;
                        double greektot_down_50 = 0;
                        double greektot_up_5 = 0;
                        double greektot_up_10 = 0;
                        double greektot_up_25 = 0;
                        double greektot_up_50 = 0;
                        foreach (Portfolio prtf in _prtfArr)
                        {
                            double vega = 0;
                            double vega_down_5 = 0;
                            double vega_down_10 = 0;
                            double vega_down_25 = 0;
                            double vega_down_50 = 0;
                            double vega_up_5 = 0;
                            double vega_up_10 = 0;
                            double vega_up_25 = 0;
                            double vega_up_50 = 0;
                            double vegatot = 0;
                            double vegatot_down_5 = 0;
                            double vegatot_down_10 = 0;
                            double vegatot_down_25 = 0;
                            double vegatot_down_50 = 0;
                            double vegatot_up_5 = 0;
                            double vegatot_up_10 = 0;
                            double vegatot_up_25 = 0;
                            double vegatot_up_50 = 0;
                            double vegavol = 0;
                            double vegavol_down_5 = 0;
                            double vegavol_down_10 = 0;
                            double vegavol_down_25 = 0;
                            double vegavol_down_50 = 0;
                            double vegavol_up_5 = 0;
                            double vegavol_up_10 = 0;
                            double vegavol_up_25 = 0;
                            double vegavol_up_50 = 0;

                            List<Position> position = _posArr.Where(x => x.id_prtf == prtf.id_prtf).ToList();
                            List<Dividend> dividend = _divArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList();
                            List<VolParam> volparam = _volparamArr.Where(x => x.id_imnt == prtf.id_imnt_und).ToList();
                            foreach (Position pos in position)
                            {
                                Instrument item = _instArr[(int)pos.id_imnt];
                                if (item.dt_mat <= dt_val)
                                    continue;
                                Instrument item_und = _instArr[(int)item.id_imnt_und];
                                try
                                {
                                    
                                    if (item.id_typ_imnt != "IND" && item.id_typ_imnt != "STK" && item.id_typ_imnt != "FUT")
                                    {
                                        if (j == 1 || j == 3)
                                        {
                                            vega = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)((float)prtf.pr_und * (1 + i / 100.0)), greek, 0.0f,dt_val), 5);
                                            vega_up_5 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, 0.05f, dt_val), 5);
                                            vega_up_10 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, 0.1f, dt_val), 5);
                                            vega_up_25 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, 0.25f, dt_val), 5);
                                            vega_up_50 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, 0.50f, dt_val), 5);
                                            vega_down_5 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, -0.05f, dt_val), 5);
                                            vega_down_10 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, -0.1f, dt_val), 5);
                                            vega_down_25 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, -0.25f, dt_val), 5);
                                            vega_down_50 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), greek, -0.50f, dt_val), 5);

                                            //gamma = (mhmodels.CalcVega(pos, item, item_und, _ratesArr, pos.pr_imnt, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), "gamma") * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100.0;
                                            //theta = (mhmodels.CalcVega(pos, item, item_und, _ratesArr, pos.pr_imnt, dividend, volparam, (float)(pos.pr_und * (1 + i / 100.0)), "theta") * pos.am_pos * (float)item.am_ct_sz);
                                        }
                                        else
                                        {
                                            vega = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, 0.0f, dt_val), 5);
                                            vega_up_5 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, 0.05f, dt_val), 5);
                                            vega_up_10 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, 0.10f, dt_val), 5);
                                            vega_up_25 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, 0.25f, dt_val), 5);
                                            vega_up_50 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, 0.50f, dt_val), 5);
                                            vega_down_5 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, -0.05f, dt_val), 5);
                                            vega_down_10 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, -0.10f, dt_val), 5);
                                            vega_down_25 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, -0.25f, dt_val), 5);
                                            vega_down_50 = Math.Round(mhmodels.CalcVega(pos, item, item_und, _ratesArr, (float)prtf.pr_und, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), greek, -0.50f, dt_val), 5);
                                            //gamma = (mhmodels.CalcVega(pos, item, item_und, _ratesArr, pos.pr_imnt, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), "gamma") * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und)) * 100.0;
                                            //theta = (mhmodels.CalcVega(pos, item, item_und, _ratesArr, pos.pr_imnt, dividend, volparam, (float)(pos.pr_und + (pos.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))), "theta") * pos.am_pos * (float)item.am_ct_sz);
                                        }
                                        if (greek == "Vega")
                                        {
                                            vegatot += (vega * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol = vegavol + ((vega * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_up_5 += (vega_up_5 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_up_5 = vegavol_up_5 + ((vega_up_5 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_up_10 += (vega_up_10 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_up_10 = vegavol_up_10 + ((vega_up_10 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_up_25 += (vega_up_25 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_up_25 = vegavol_up_25 + ((vega_up_25 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_up_50 += (vega_up_50 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_up_50 = vegavol_up_50 + ((vega_up_50 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_down_5 += (vega_down_5 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_down_5 = vegavol_down_5 + ((vega_down_5 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_down_10 += (vega_down_10 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_down_10 = vegavol_down_10 = +((vega_down_10 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_down_25 += (vega_down_25 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_down_25 = vegavol_down_25 = +((vega_down_25 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                            vegatot_down_50 += (vega_down_50 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5));
                                            vegavol_down_50 = vegavol_down_50 = +((vega_down_50 * pos.am_pos * (float)item.am_ct_sz * Math.Pow((90.0 / (item.dt_mat.Value.Subtract(dt_val).Days)), .5)) * pos.am_vol_hedge);
                                        }
                                        else if (greek == "Gamma")
                                        {
                                            greektot += vega * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_down_5 += vega_down_5 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_down_10 += vega_down_10 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_down_25 += vega_down_25 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_down_50 += vega_down_50 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_up_5 += vega_up_5 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_up_10 += vega_up_10 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_up_25 += vega_up_25 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                            greektot_up_50 += vega_up_50 * pos.am_pos * pos.pr_und * .01 / (1 / pos.pr_und) * 100.0;
                                        }
                                        else if (greek == "Theta")
                                        {
                                            greektot += vega * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_down_5 += vega_down_5 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_down_10 += vega_down_10 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_down_25 += vega_down_25 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_down_50 += vega_down_50 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_up_5 += vega_up_5 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_up_10 += vega_up_10 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_up_25 += vega_up_25 * pos.am_pos * (float)item.am_ct_sz;
                                            greektot_up_50 += vega_up_50 * pos.am_pos * (float)item.am_ct_sz;
                                        }
                                        else if (greek == "Delta")
                                        {
                                            greektot += vega * pos.am_pos * pos.pr_und * (double) item.am_ct_sz;
                                            greektot_down_5 += vega_down_5 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_down_10 += vega_down_10 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_down_25 += vega_down_25 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_down_50 += vega_down_50 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_up_5 += vega_up_5 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_up_10 += vega_up_10 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_up_25 += vega_up_25 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                            greektot_up_50 += vega_up_50 * pos.am_pos * pos.pr_und * (double)item.am_ct_sz;
                                        }

                                    }
                                    else if (greek == "Delta")
                                    {
                                        if (item.id_typ_imnt == "STK")
                                        {
                                            pos.am_delta = 1f;
                                            pos.am_delta_imp = 1f;
                                        }
                                        else if (item.id_typ_imnt == "FUT")
                                        {
                                            IndexData indexpos = _indexdataArr.FirstOrDefault(x => x.id_imnt == prtf.id_imnt_und);
                                            mhmodels.CalcFut(pos, item, item_und, _ratesArr, pos.pr_und
                                            , _divArr.Where(x => x.id_imnt == prtf.id_imnt_und && x.id_src == item_und.id_div_src).ToList()
                                            , indexpos);
                                        }
                                        if (j == 1 || j == 3)
                                        {                                            
                                            greektot += pos.am_delta * pos.am_pos * (double) prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_down_5 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_down_10 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_down_25 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_down_50 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_up_5 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_up_10 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_up_25 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                            greektot_up_50 += pos.am_delta * pos.am_pos * (double)prtf.pr_und * (1 + i / 100.0) * (double)item.am_ct_sz;
                                        }
                                        else
                                        {
                                            greektot += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_down_5 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_down_10 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_down_25 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_down_50 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_up_5 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_up_10 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_up_25 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;
                                            greektot_up_50 += pos.am_delta * pos.am_pos * (float)(prtf.pr_und + (prtf.pr_und * _instbeta[(int)item_und.id_imnt] * (i / 100.0))) * (double)item.am_ct_sz;                                            
                                        }

                                    }
                                }
                                catch
                                {

                                }
                            }
                            _vegaslide_name.Add(prtf.nm_prtf, vegatot);
                            _vegaslide.Add((int)prtf.id_prtf, 
                                new Vegaslide(vegatot,vegavol));
                            _vegaslidevol_name.Add(prtf.nm_prtf, vegavol);
                            //_vegaslide_vegavol.Add((int)prtf.id_prtf, vegavol);
                            _vegaslide_up_5.Add((int)prtf.id_prtf, 
                                new Vegaslide(vegatot_up_5,vegavol_up_5));
                            //_vegaslide_vegavol_up_5.Add((int)prtf.id_prtf, vegavol_up_5);
                            _vegaslide_up_10.Add((int)prtf.id_prtf, 
                                new Vegaslide(vegatot_up_10,vegavol_up_10));
                            _vegaslide_up_25.Add((int)prtf.id_prtf,
                                new Vegaslide(vegatot_up_25, vegavol_up_25));
                            _vegaslide_up_50.Add((int)prtf.id_prtf,
                                new Vegaslide(vegatot_up_50, vegavol_up_50));
                            //_vegaslide_vegavol_up_10.Add((int)prtf.id_prtf, vegavol_up_10);
                            _vegaslide_down_5.Add((int)prtf.id_prtf, 
                                new Vegaslide(vegatot_down_5,vegavol_down_5));
                            //_vegaslide_vegavol_down_5.Add((int)prtf.id_prtf, vegavol_down_5);
                            _vegaslide_down_10.Add((int)prtf.id_prtf, 
                                new Vegaslide(vegatot_down_10,vegavol_down_10));
                            //_vegaslide_vegavol_down_10.Add((int)prtf.id_prtf, vegavol_down_10);
                            _vegaslide_down_25.Add((int)prtf.id_prtf,
                                new Vegaslide(vegatot_down_25, vegavol_down_25));
                            _vegaslide_down_50.Add((int)prtf.id_prtf,
                                new Vegaslide(vegatot_down_50, vegavol_down_50));
                        }

                        if (greek == "Vega")
                        {
                            Dictionary<int, double> resultDict = calcSumVega(_vegaslide,false);
                            Dictionary<int, double> resultDict_vegavol = calcSumVega(_vegaslide, true);

                            Dictionary<int, double> resultDict_down_5 = calcSumVega(_vegaslide_down_5,false);
                            Dictionary<int, double> resultDict_vegavol_down_5 = calcSumVega(_vegaslide_down_5, true);

                            Dictionary<int, double> resultDict_down_10 = calcSumVega(_vegaslide_down_10,false);
                            Dictionary<int, double> resultDict_vegavol_down_10 = calcSumVega(_vegaslide_down_10, true);

                            Dictionary<int, double> resultDict_down_25 = calcSumVega(_vegaslide_down_25, false);
                            Dictionary<int, double> resultDict_vegavol_down_25 = calcSumVega(_vegaslide_down_25, true);

                            Dictionary<int, double> resultDict_down_50 = calcSumVega(_vegaslide_down_50, false);
                            Dictionary<int, double> resultDict_vegavol_down_50 = calcSumVega(_vegaslide_down_50, true);

                            Dictionary<int, double> resultDict_up_5 = calcSumVega(_vegaslide_up_5,false);
                            Dictionary<int, double> resultDict_vegavol_up_5 = calcSumVega(_vegaslide_up_5, true);

                            Dictionary<int, double> resultDict_up_10 = calcSumVega(_vegaslide_up_10,false);
                            Dictionary<int, double> resultDict_vegavol_up_10 = calcSumVega(_vegaslide_up_10, true);

                            Dictionary<int, double> resultDict_up_25 = calcSumVega(_vegaslide_up_25, false);
                            Dictionary<int, double> resultDict_vegavol_up_25 = calcSumVega(_vegaslide_up_25, true);

                            Dictionary<int, double> resultDict_up_50 = calcSumVega(_vegaslide_up_50, false);
                            Dictionary<int, double> resultDict_vegavol_up_50 = calcSumVega(_vegaslide_up_50, true);

                            double minvega = minRatio(resultDict, resultDict_vegavol);
                            double minvega_down_5 = minRatio(resultDict_down_5, resultDict_vegavol_down_5);
                            double minvega_down_10 = minRatio(resultDict_down_10, resultDict_vegavol_down_10);
                            double minvega_down_25 = minRatio(resultDict_down_25, resultDict_vegavol_down_25);
                            double minvega_down_50 = minRatio(resultDict_down_50, resultDict_vegavol_down_50);
                            double minvega_up_5 = minRatio(resultDict_up_5, resultDict_vegavol_up_5);
                            double minvega_up_10 = minRatio(resultDict, resultDict_vegavol_up_10);
                            double minvega_up_25 = minRatio(resultDict_up_25, resultDict_vegavol_up_25);
                            double minvega_up_50 = minRatio(resultDict, resultDict_vegavol_up_50);
                            _dbhelper.InsertVegaSlide2(i, "Vega_ratio", minvega, minvega_down_10, minvega_down_5, minvega_down_25, minvega_down_50, minvega_up_5, minvega_up_10, minvega_up_25, minvega_up_50, j);
                            _dbhelper.InsertVegaSlide2(i, greek, resultDict[1] + resultDict[2], resultDict_down_10[1] + resultDict_down_10[2],
                            resultDict_down_5[1] + resultDict_down_5[2], resultDict_down_25[1] + resultDict_down_25[2], resultDict_down_50[1] + resultDict_down_50[2],
                            resultDict_up_5[1] + resultDict_up_5[2], resultDict_up_10[1] + resultDict_up_10[2],
                            resultDict_up_25[1] + resultDict_up_25[2], resultDict_up_50[1] + resultDict_up_50[2], j);
                            //825 822 66.67
                        }
                        else
                        {
                            _dbhelper.InsertVegaSlide2(i, greek, greektot,greektot_down_10,
                            greektot_down_25,greektot_down_50,greektot_down_5
                            ,greektot_up_5, greektot_up_10,greektot_up_25, greektot_up_50, j);
                        }
                    }
                }
            }
            duration = sw.Elapsed;
            sw.Stop();
        }


        private Dictionary<int, double> calcSumVega(Dictionary<int, Vegaslide> _vegaslide, bool isvegavol)
        {
            double long_vega = 0;
            double short_vega = 0;
            Dictionary<int, double> resultDict = new Dictionary<int, double>();
            foreach (var vs in _vegaslide)
            {
                if (vs.Value._am_vega > 1000)
                    if (isvegavol)
                        long_vega += vs.Value._am_vegavol;
                    else
                        long_vega += vs.Value._am_vega;

                if (vs.Value._am_vega < -1000)
                    if (isvegavol)
                        short_vega += vs.Value._am_vegavol;
                    else
                        short_vega += vs.Value._am_vega;
            }
            resultDict.Add(1, long_vega);
            resultDict.Add(2, short_vega);

            return resultDict;
        }

        private float minRatio(Dictionary<int, double> resultDict, Dictionary<int, double> resultDict_vegavol)
        {
            double long_vol = resultDict_vegavol[1] / resultDict[1];
            double short_vol = resultDict_vegavol[2] / resultDict[2];
            double vol_ratio = long_vol / short_vol;
            double correlation = Math.Pow(1 / vol_ratio, 2);
            return (float) ((Math.Min(1.5, vol_ratio) * resultDict[1]) + resultDict[2]);

        }
        private class Vegaslide
        {
            
            public double _am_vega { get; set; }
            public double _am_vegavol { get; set; }

            public Vegaslide(double am_vega, double am_vegavol)
            {
                _am_vega = am_vega;
                _am_vegavol = am_vegavol;
            }
        }
    }
}
