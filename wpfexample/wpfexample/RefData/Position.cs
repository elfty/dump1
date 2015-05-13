using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace wpfexample
{
    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    [Serializable]
    public class Position
    {
        public DateTime dt_trd { get; set; }
        public int id_pos { get; set; }
        public int id_prtf { get; set; }
        public int id_imnt { get; set; }
        public float am_pos { get; set; }
        public float am_pos_adj { get; set; }
        public float pr_imnt { get; set; }
        public float pr_imnt_close { get; set; }
        public float pr_theo { get; set; }
        public float pr_und { get; set; }
        public float am_pl { get; set; }
        public float am_pl_sum { get; set; }
        public float am_pl_nie { get; set; }
        public float am_pl_nie_sum { get; set; }
        public float am_vol { get; set; }
        public float am_vol_or { get; set; }
        public float am_vol_imp { get; set; }
        public float am_vol_close { get; set; }
        public float am_vol_mark { get; set; }
        public float am_vol_hedge { get; set; }
        public float am_delta { get; set; }
        public float am_gamma { get; set; }
        public float am_vega { get; set; }
        public float am_theta { get; set; }
        public float am_rho { get; set; }
        public float am_time { get; set; }
        public float am_rate { get; set; }
        public float am_div { get; set; }
        public DateTime dt_chg { get; set; }
        public float am_delta_imp { get; set; }
        public float pr_trd { get; set; } //not from POSITIONS table
        public float am_pl_day { get; set; } //not from POSITIONS table
        public float am_trd_buy { get; set; } //not from POSITIONS table
        public float am_trd_sell { get; set; } //not from POSITIONS table
        public float pr_avg_buy { get; set; } //not from POSITIONS table
        public float pr_avg_sell { get; set; } //not from POSITIONS table


        public Position(object[] positionRaw)
        {
            dt_trd = (DateTime)positionRaw[0];
            id_pos = (int)positionRaw[1];
            id_prtf = (int)positionRaw[2];
            id_imnt = (int)positionRaw[3];
            am_pos = (float)(double)positionRaw[4];
            am_pos_adj = (float)(double)positionRaw[5];
            pr_imnt = (float)(double)positionRaw[6];
            pr_imnt_close = (float)(double)positionRaw[7];
            pr_theo = (float)(double)positionRaw[8];
            pr_und = (float)(double)positionRaw[9];
            am_pl = (float)(double)positionRaw[10];
            am_pl_sum = (float)(double)positionRaw[11];
            am_pl_nie = (float)(double)positionRaw[12];
            am_pl_nie_sum = (float)(double)positionRaw[13];
            am_vol = (float)(double)positionRaw[14];
            am_vol_or = (float)(double)positionRaw[15];
            am_vol_imp = (float)(double)positionRaw[16];
            am_vol_close = (float)(double)positionRaw[17];
            am_vol_mark = (float)(double)positionRaw[18];
            am_vol_hedge = (float)(double)positionRaw[19];
            am_delta = (float)(double)positionRaw[20];
            am_gamma = (float)(double)positionRaw[21];
            am_vega = (float)(double)positionRaw[22];
            am_theta = (float)(double)positionRaw[23];
            am_rho = (float)(double)positionRaw[24];
            am_time = (float)(double)positionRaw[25];
            am_rate = (float)(double)positionRaw[26];
            am_div = (float)(double)positionRaw[27];
            dt_chg = (DateTime)positionRaw[28];
        }

        public void createPositionFile(List<Position> posArr, Dictionary<int,Instrument> instArr)
        {
            DBHelper _dbhelper = new DBHelper();
            DateTime time = DateTime.Now;
            FileHelper.CreateFile(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log", "intraposcalc_" + System.Environment.MachineName + ".txt");
            string sqlText;
            LoggingHelper.LogMemo("{1}: Live Positions Start: {0}","", String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
            List<Position> newList = new List<Position>();
            Dictionary<int, Instrument> newInstArr = new Dictionary<int, Instrument>();
            foreach (Position p in posArr)
            {
                newList.Add(ExtensionMethods.DeepClone(p));                
            }

            lock (instArr)
            {
                foreach (Position p in newList)
                {
                    try
                    {
                        sqlText = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41}",
                        string.Format("{0:yyyyMMdd}", DateTime.Today),
                        p.id_pos, p.id_prtf, p.id_imnt, p.am_pos, p.am_pos_adj, 
                        p.pr_imnt, p.pr_imnt_close, p.pr_theo, p.pr_und, p.am_pl, 
                        p.am_pl_sum, p.am_pl_nie, p.am_pl_nie_sum, p.am_vol, p.am_vol_or, 
                        p.am_vol_imp, p.am_vol_close, p.am_vol_mark, p.am_vol_hedge, 
                        p.am_delta, p.am_gamma, p.am_vega, p.am_theta, p.am_rho, 
                        p.am_time, p.am_rate, p.am_div, time.ToString("yyyyMMdd HH:mm:ss"), p.pr_trd, p.am_pl_day, instArr[p.id_imnt].id_imnt_und,
                        instArr[p.id_imnt].id_typ_imnt,instArr[p.id_imnt].id_imnt_ric,
                        instArr[p.id_imnt].pr_strike != null ? instArr[p.id_imnt].pr_strike.ToString() : DBNull.Value.ToString(),
                        instArr[p.id_imnt].dt_mat != null ? instArr[p.id_imnt].dt_mat.Value.ToString("yyyyMMdd") : DBNull.Value.ToString(),
                        instArr[p.id_imnt].id_pc != null ? instArr[p.id_imnt].id_pc.ToString() : DBNull.Value.ToString(),
                        instArr[p.id_imnt].am_ct_sz,                        
                        p.am_trd_buy,
                        p.am_trd_sell,
                        p.pr_avg_buy,
                        p.pr_avg_sell

                        );

                        FileHelper.WriteLine("intraposcalc_" + System.Environment.MachineName + ".txt", sqlText);
                        //LoggingHelper.LogMemo("{1}: Live Positions Insert: {0}", p.id_prtf, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                    }
                    catch
                    {
                        LoggingHelper.LogMemo("{1}: Live Positions Error: {0}", p.id_prtf, String.Format("{0:HH:mm:ss.fff}", DateTime.Now));
                    }
                }
            }

            FileHelper.Close("intraposcalc_" + System.Environment.MachineName + ".txt");
            _dbhelper.InsertIntraPosCalc(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log\intraposcalc_" + System.Environment.MachineName + ".txt");
            _dbhelper.DeleteIntraPosCalc();
            LoggingHelper.LogMemo("{1}: Live Positions End: {0}", "", String.Format("{0:HH:mm:ss.fff}", DateTime.Now));

        }
    }
}

