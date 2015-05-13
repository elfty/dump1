using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace wpfexample
{
    class GetPosition
    {
        DBHelper _dbhelper = new DBHelper(); 
        public string id_acc_mlp { get; set; } //Trader
        public string id_bbg_key_mlp { get; set; } //BB_KEY 
        public string id_imnt_ric_mlp { get; set; } //opra_code
        public float am_pos { get; set; } //Position
        public float pr_avg { get; set; } //Trd AvgPr
        public float am_pos_sod { get; set; } //Opened
        public float pr_imnt { get; set; } //Price
        public float pr_imnt_close {get; set;} //Prev Mark
        public string id_typ_imnt_mlp { get; set; } //Instrument
        public string id_sub_acc_mlp { get; set; } //Micro
        public string id_imnt_und_old { get; set; } //Underlying
        public string id_imnt_und { get; set; } //UnderlyingSC
        public float pr_und { get; set; } //umkt
        public float am_pnl_day { get; set; } //Day PL
        public float am_trd_buy { get; set; } //Bot
        public float am_trd_sell { get; set; } //Sold
        public float pr_avg_buy{ get; set; } //Bot AvgPX
        public float pr_avg_sell { get; set; } //Sold AvgPx

        public Dictionary<int, double?> tickerListNewPrtf = new Dictionary<int, double?>();
        public Dictionary<string, int> tickernameListNewPrtf = new Dictionary<string, int>();

        public GetPosition()
        {}

        public GetPosition(object[] parts)
        {
            id_acc_mlp = parts[20].ToString();
            id_bbg_key_mlp = parts[1].ToString();
            if (parts[11] == " " || parts[11] == null)
                id_imnt_ric_mlp = parts[0].ToString();
            else
                id_imnt_ric_mlp = parts[11].ToString();

            am_pos = (float)Convert.ToDouble(parts[3]);
            pr_imnt = parts[9].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[9]);
            pr_imnt_close = parts[10].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[10]);
            am_pos_sod = parts[12].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[12]);
            am_trd_sell = parts[21].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[21]);
            am_trd_buy = parts[22].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[22]);
            id_typ_imnt_mlp = parts[5].ToString();
            id_sub_acc_mlp = parts[17].ToString();
            id_imnt_und_old = parts[2].ToString();
            id_imnt_und = parts[28].ToString();
            pr_avg = parts[23].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[23]);
            pr_und = parts[24].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[24]);
            pr_avg_buy = parts[26].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[26]);
            pr_avg_sell = parts[27].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[27]);
            am_pnl_day = parts[14].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[14]);
        }

        public GetPosition(string[] parts)
        {
            id_acc_mlp = parts[20];
            id_bbg_key_mlp = parts[1];
            if (parts[11] == " " || parts[11] == null)
                id_imnt_ric_mlp = parts[0];
            else
                id_imnt_ric_mlp = parts[11];

            am_pos = (float)Convert.ToDouble(parts[3]);
            pr_imnt = parts[9].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[9]);
            pr_imnt_close = parts[10].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[10]);
            am_pos_sod = parts[12].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[12]);
            id_typ_imnt_mlp = parts[5];
            id_sub_acc_mlp = parts[17];
            id_imnt_und_old = parts[2];
            pr_avg = parts[23].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[23]);
            am_pnl_day = parts[23].ToString().Length == 0 ? 0 : (float)Convert.ToDouble(parts[14]);
            id_imnt_und = parts[28].ToString();
                //(float)Convert.ToDouble(parts[23]);

        }
        internal void PositionAllCreateFile(string[] parts, DateTime time)
        {
            string sql;
            //sql = " insert rec..p5_bo_intra values (";
            sql = "" + parts[0] + ""; //security
            sql = sql + "," + parts[1] + ""; //bb_key
            sql = sql + "," + parts[2] + "";//underlying
            sql = sql + "," + parts[3] + ""; //position
            if (parts[4] == "")
                sql = sql + "," + 0 + "";   //contractsize
            else
                sql = sql + "," + parts[4] + ""; //contractsize

            sql = sql + "," + parts[5] + ""; //instrument
            if (parts[6] == "")
                sql = sql + ",";  //expir_date
            else
                sql = sql + "," + parts[6] + ""; //expir_date

            if (parts[7] == "")
                sql = sql + "," + 0 + "";   //strike_prc
            else
                sql = sql + "," + parts[7] + ""; //strike_prc

            if (parts[8] == "")
                sql = sql + ",";   //strike_prc
            else
                sql = sql + "," + parts[8] + ""; //putcallind

            sql = sql + "," + parts[9] + ""; //price
            sql = sql + "," + parts[10] + ""; //prev mark
            sql = sql + "," + parts[11] + ""; //sm_ext_sec_id
            sql = sql + "," + parts[12] + ""; //opened
            sql = sql + "," + parts[13] + ""; //trd
            sql = sql + "," + parts[14] + ""; //pnl
            sql = sql + "," + parts[15] + ""; //mtd
            sql = sql + "," + parts[16] + ""; //ytd
            sql = sql + "," + parts[17] + ""; //micro
            sql = sql + "," + parts[18] + ""; //account
            sql = sql + "," + parts[19] + ""; //currency
            sql = sql + "," + parts[20] + ""; //trader
            sql = sql + "," + parts[21] + ""; //sld
            sql = sql + "," + parts[22] + ""; //bot
            if (parts[23] == "")
                sql = sql + "," + 0 + "";   //contractsize
            else
                sql = sql + "," + parts[23] + ""; //contractsize
            if (parts[24] == "")
                sql = sql + "," + 0 + ""; //umkt
            else
                sql = sql + "," + parts[24] + ""; //umkt
            sql = sql + "," + time.ToString("yyyyMMdd HH:mm:ss") + ""; //time
            sql = sql + "," + parts[25] + ""; //Bot_AvgPx
            sql = sql + "," + parts[26] + ""; //Sold_AvgPx
            FileHelper.WriteLine("intrapos_" + System.Environment.MachineName + ".txt", sql);
        }
        internal void getP5PositionAll()
        {

            FileHelper.CreateFile(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log", "intrapos_" + System.Environment.MachineName + ".txt");

            string strFile = "\\\\adsrv133\\volarb\\mlp\\ONProcess\\P5Positions\\intra\\jackson_all_" + System.Environment.MachineName + ".csv";
            string strBatFile = "\\\\adsrv133\\volarb\\MLP\\VTDev\\apps\\P5Positions\\bin\\jackson_run_intra_all.bat";
            DateTime time = DateTime.Now;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = strBatFile;
            proc.Start();
            proc.WaitForExit();

            FileStream logFileStream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (StreamReader reader = new StreamReader(logFileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        string[] parts = line.Split(',');

                        if (parts[0] == "Security" || parts[0] == "EUR")
                            continue;

                        PositionAllCreateFile(parts, time);
                    }
                    catch
                    {
                    }

                }
            }

            FileHelper.Close("intrapos_" + System.Environment.MachineName + ".txt");
            _dbhelper.InsertIntraPos(@"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log\intrapos_" + System.Environment.MachineName + ".txt");
            _dbhelper.DeleteP5Intra();
            
        }
        internal void getP5Position(List<Position> posArr, Dictionary<int, Instrument> instArr, List<Portfolio> prtfArr, bool isRunningIntraPos)
        {

            List<GetPosition> getposArr = new List<GetPosition>();
            List<GetPosition> getposArrIntra = new List<GetPosition>();
            List<GetPosition> getposArrPort = new List<GetPosition>();
            string strFile = "\\\\adsrv133\\volarb\\mlp\\ONProcess\\P5Positions\\intra\\jackson.csv";
            string strBatFile = "\\\\adsrv133\\volarb\\MLP\\VTDev\\apps\\P5Positions\\bin\\jackson_run_intra.bat";
            DateTime time = DateTime.Now;

            //CHECK IF INTRA POS IS RUNNING
            if (!isRunningIntraPos)
                getP5PositionAll();

            getposArr = _dbhelper.GetPositionIntra();

            /*
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = strBatFile;
            proc.Start();
            proc.WaitForExit();           

            FileStream logFileStream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (StreamReader reader = new StreamReader(logFileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        string[] parts = line.Split(',');

                        if (parts[0] == "Security" || parts[0] == "EUR")
                            continue;

                        if (parts[17] != "SPECIAL_SIT" && parts[17] != "CHAN" && parts[20] != "JCKSN_SS")
                            getposArr.Add(new GetPosition(parts));
                    }
                    catch
                    {
                    }

                }

            }
            */

            //getposArrIntra = getposArr.Where(x => x.am_pos - x.am_pos_sod != 0).ToList();
            //getposArrPort = getposArr.Where(x => (x.am_pos != 0 || x.am_pos_sod != 0) && x.id_imnt_und == "CYH.N").ToList();
            //getposArrIntra = getposArr;

            foreach (GetPosition getpos in getposArr)
            {

                try
                {
                    string id_opra_imnt_ric_mlp;
                    int id_imnt = 0;
                    //if (getpos.id_imnt_ric_mlp.IndexOf("NEM") >= 0)
                    //    continue;
                    if (getpos.id_typ_imnt_mlp == "SS" || getpos.id_imnt_ric_mlp == "LVNAR.O" || getpos.id_sub_acc_mlp != "OPTION_VOL_DOMESTIC")
                        continue;

                    string id_typ_imnt = getInstType(getpos.id_typ_imnt_mlp);

                    if (id_typ_imnt == "OPT")
                    {
                        if (getpos.id_imnt_ric_mlp == "")
                            id_imnt = 0;
                        else
                        {
                            id_opra_imnt_ric_mlp = getpos.id_imnt_ric_mlp.Substring(0, (getpos.id_imnt_ric_mlp.Length - 12))
                                + "".PadRight(5 - (getpos.id_imnt_ric_mlp.Length - 12), ' ')
                                + getpos.id_imnt_ric_mlp.Substring(getpos.id_imnt_ric_mlp.Length - 12, 12);
                            id_imnt = getImntID(id_opra_imnt_ric_mlp, getpos.id_bbg_key_mlp, id_typ_imnt, instArr);
                        }
                    }
                    else if (id_typ_imnt == "OTC")
                    {
                        if (getpos.id_imnt_ric_mlp == "JBLU_6.5_130320_C.OTC")
                            id_imnt = 29739;
                    }
                    else
                    {   //Left(id_bbg_imnt_ric_mlp, InStr(id_bbg_imnt_ric_mlp, " ") - 1)
                        if (id_typ_imnt == "FUT" && getpos.id_bbg_key_mlp.Substring(0, 3) == "RTA")
                            id_imnt = getImntID("TFS" + getpos.id_bbg_key_mlp.Substring(0, getpos.id_bbg_key_mlp.IndexOf(" ")).Substring(getpos.id_bbg_key_mlp.Substring(0, getpos.id_bbg_key_mlp.IndexOf(" ")).Length - 2, 2), getpos.id_bbg_key_mlp, id_typ_imnt, instArr);
                        else
                            id_imnt = getImntID(getpos.id_bbg_key_mlp.Substring(0, getpos.id_bbg_key_mlp.IndexOf(" ")), getpos.id_bbg_key_mlp, id_typ_imnt, instArr);
                        //id_imnt = getImntID(id_bbg_key_mlp, id_bbg_key_mlp, id_typ_imnt, instArr);
                    }
                    if (id_imnt == 0)
                    {
                        id_imnt = 0;
                        LoggingHelper.LogError("{0}: Booking Error {1}. {2}", String.Format("{0:hh:mm:ss.fff}", DateTime.Now), getpos.id_imnt_ric_mlp, getpos.am_pos);
                        continue;
                    }

                    //Instrument item = instArr.FirstOrDefault(x => x.id_imnt == id_imnt); //get instrument information
                    //Instrument item_und = instArr.FirstOrDefault(x => x.id_imnt == item.id_imnt_und);

                    Instrument item = instArr[(int)id_imnt]; //get instrument information
                    Instrument item_und = instArr[(int)item.id_imnt_und];

                    if (item_und.id_typ_imnt == "IND")
                    {
                        //update index imnt id
                    }

                    //Update Position
                    LoggingHelper.LogMemo("{1}: Booking Trade: {0}, {2}", item.id_imnt_ric, String.Format("{0:HH:mm:ss.fff}", DateTime.Now), (getpos.am_pos - getpos.am_pos_sod));
                    updatePosition(item, getpos, posArr, prtfArr, item_und);
                }
                catch
                {
                }


            }
        }

        internal void updatePosition(Instrument inst, GetPosition getposDb, List<Position> posArr, List<Portfolio> prtfArr, Instrument inst_und)
        {
            Position pos = posArr.FirstOrDefault(x => x.id_imnt == inst.id_imnt);
            Portfolio prtf = prtfArr.FirstOrDefault(x => x.id_imnt_und == inst.id_imnt_und);

            if (prtf == null)
            {
                //insert new portfolio
                var newPrtf = new object[24];
                newPrtf[0] = prtfArr.Max(x => x.id_prtf) + 1;
                newPrtf[1] = inst_und.id_imnt_ric;
                newPrtf[2] = inst.id_imnt_und;
                newPrtf[3] = 0.0;
                newPrtf[4] = 0.0;
                newPrtf[5] = 0.0;
                newPrtf[6] = 0.0;
                newPrtf[7] = 0.0;
                newPrtf[8] = 0.0;
                newPrtf[9] = 0.0;
                newPrtf[10] = 0.0;
                newPrtf[11] = 0.0;
                newPrtf[12] = 0.0;
                newPrtf[13] = 0.0;
                newPrtf[14] = 0.0;
                newPrtf[15] = 0.0;
                newPrtf[16] = "";
                newPrtf[17] = 4;
                newPrtf[18] = DateTime.Now;
                newPrtf[19] = 0;
                newPrtf[20] = 0;
                newPrtf[21] = 0;
                newPrtf[22] = 0;
                newPrtf[23] = 0.0;
                //TODO Fix adding new portfolio
                //LoggingHelper.LogError("{0}: Portfolio does not exist {1}", String.Format("{0:hh:mm:ss.fff}", DateTime.Now));
                
                prtfArr.Add(new Portfolio(newPrtf));
            }

            prtf = prtfArr.FirstOrDefault(x => x.id_imnt_und == inst.id_imnt_und);

            if (pos == null)
            {
                //insert new position
                var newPos = new object[31];
                newPos[0] = DateTime.Today;
                newPos[1] = posArr.Max(x => x.id_pos) + 1;
                newPos[2] = prtf.id_prtf;
                newPos[3] = inst.id_imnt;
                newPos[4] = (double)getposDb.am_pos;
                newPos[5] = (double)(getposDb.am_pos - getposDb.am_pos_sod);
                newPos[6] = (double)getposDb.pr_avg;
                newPos[7] = (double)getposDb.pr_imnt_close;
                newPos[8] = (double)getposDb.pr_imnt;
                newPos[9] = (double)getposDb.pr_und;
                newPos[10] = 0.0;
                newPos[11] = 0.0;
                newPos[12] = 0.0;
                newPos[13] = 0.0;
                newPos[14] = 0.0;
                newPos[15] = 0.0;
                newPos[16] = 0.0;
                newPos[17] = 0.0;
                newPos[18] = 0.0;
                newPos[19] = 0.0;
                newPos[20] = 0.0;
                newPos[21] = 0.0;
                newPos[22] = 0.0;
                newPos[23] = 0.0;
                newPos[24] = 0.0;
                newPos[25] = 0.0;
                newPos[26] = 0.0;
                newPos[27] = 0.0;
                newPos[28] = DateTime.Now;

                posArr.Add(new Position(newPos));

                Position pos_new = posArr.FirstOrDefault(x => x.id_imnt == inst.id_imnt);

                pos_new.pr_trd = getposDb.pr_avg; //avg price
                pos_new.am_pl_day = getposDb.am_pnl_day;
                pos.am_trd_buy = getposDb.am_trd_buy;
                pos.am_trd_sell = getposDb.am_trd_sell;
                pos.pr_avg_buy = getposDb.pr_avg_buy;
                pos.pr_avg_sell = getposDb.pr_avg_sell;
                pos.pr_imnt = getposDb.pr_imnt;
                pos.pr_imnt_close = getposDb.pr_imnt_close;
                pos.pr_und = getposDb.pr_und;
            }
            else
            {
                

                pos.am_pos = getposDb.am_pos;
                pos.am_pos_adj = (getposDb.am_pos - getposDb.am_pos_sod);
                pos.pr_trd = getposDb.pr_avg; //avg price
                pos.am_pl_day = getposDb.am_pnl_day;
                pos.am_trd_buy = getposDb.am_trd_buy;
                pos.am_trd_sell = getposDb.am_trd_sell;
                pos.pr_avg_buy = getposDb.pr_avg_buy;
                pos.pr_avg_sell = getposDb.pr_avg_sell;
                pos.pr_imnt = getposDb.pr_imnt;
                pos.pr_imnt_close = getposDb.pr_imnt_close;
                pos.pr_und = getposDb.pr_und;

                if (inst.id_typ_imnt == "STK")
                    pos.pr_und = getposDb.pr_imnt;
            }

        }
        internal int getImntID(string id_opra_imnt_ric_mlp, string id_bbg_key_mlp, string id_typ_imnt, Dictionary<int, Instrument> instArr)
        {
            int id_imnt = 0;
            if (id_typ_imnt == "STK")
            {
                if (id_opra_imnt_ric_mlp == "CHL.N")
                    id_opra_imnt_ric_mlp = "CHL";
                if (id_opra_imnt_ric_mlp == "BRKB")
                    id_opra_imnt_ric_mlp = "BRK/B";
                if (id_opra_imnt_ric_mlp == "LVNTA.O")
                    id_opra_imnt_ric_mlp = "LVNTA";
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_ric == id_opra_imnt_ric_mlp);
                var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == id_opra_imnt_ric_mlp);
                Instrument item = instArr[(int)dict.Key];

                return (int)item.id_imnt;
            }

            else if (id_typ_imnt == "OPT")
            {
                id_imnt = getDeriv(id_opra_imnt_ric_mlp, id_bbg_key_mlp, instArr);
            }
            else if (id_typ_imnt == "")
                id_imnt = 0;
            else
            {
                //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_ric == id_opra_imnt_ric_mlp);
                var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == id_opra_imnt_ric_mlp);
                Instrument item = instArr[(int)dict.Key];
                return (int)item.id_imnt;
            }
            return id_imnt;
        }

        internal int getDeriv(string id_imnt_ric_mlp, string id_bberg, Dictionary<int,Instrument> instArr)
        {
            int id_imnt = 0;
            string id_pc;
            string id_imnt_reuters = id_imnt_ric_mlp;

            //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_reuters == id_imnt_reuters);
            var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_reuters == id_imnt_reuters);

            if (dict.Value != null)
            {
                Instrument item = instArr[(int)dict.Key];
                return (int)item.id_imnt;
            }
            string id_imnt_ric = cleanTicker(id_imnt_ric_mlp.Substring(0, 5).Replace(" ", ""));
            if (id_imnt_ric == "HSH1")
                id_imnt_ric = "HSH";



            int id_month = (int)(id_imnt_ric_mlp.Substring(5, 1))[0];
            if (id_month <= 76) //L
            {
                id_pc = "C";
                id_month = id_month - 64;
            }
            else
            {
                id_pc = "P";
                id_month = id_month - 76;
            }

            int id_day = Convert.ToInt32(id_imnt_ric_mlp.Substring(6, 2));
            int id_year = Convert.ToInt32("20" + id_imnt_ric_mlp.Substring(8, 2));
            float pr_strike = (float)Convert.ToDouble(id_imnt_ric_mlp.Substring(11, 6));

            if (id_imnt_ric_mlp.Substring(10, 1) == "A")
            {
                pr_strike = pr_strike / 10;
            }
            else if (id_imnt_ric_mlp.Substring(10, 1) == "B")
            {
                pr_strike = pr_strike / 100;
            }
            else if (id_imnt_ric_mlp.Substring(10, 1) == "C")
            {
                pr_strike = pr_strike / 1000;
            }
            else if (id_imnt_ric_mlp.Substring(10, 1) == "D")
            {
                pr_strike = pr_strike / 10000;
            }
            else if (id_imnt_ric_mlp.Substring(10, 1) == "E")
            {
                pr_strike = pr_strike / 100000;
            }
            else if (id_imnt_ric_mlp.Substring(10, 1) == "F")
            {
                pr_strike = pr_strike / 1000000;
            }

            if (id_imnt_ric == "BRKB")
                id_imnt_ric = "BRK/B";
            if (id_imnt_ric == "SPXW")
                id_imnt_ric = "SPX";
            if (id_imnt_ric == "SPXQ")
                id_imnt_ric = "SPX";
            if (id_imnt_ric == "VIAB")
                id_imnt_ric = "VIA/B";


            string nm_month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(id_month).Substring(0, 3).ToUpper();

            if (id_bberg == "" || id_bberg == " ")
                id_bberg = id_imnt_ric + " US " + new DateTime(id_year, id_month, id_day).ToString("MM/dd/yyyy")
                    + " " + id_pc + pr_strike;
            else
            {
                if (id_bberg.IndexOf(" Equity") > 0)
                    id_bberg = id_bberg.Substring(0, id_bberg.IndexOf(" Equity"));
                else
                    id_bberg = id_bberg.Substring(0, id_bberg.IndexOf(" Index"));
                /*If InStr(id_bberg, " Equity") > 0 Then
                    id_bberg = Left(id_bberg, InStr(id_bberg, " Equity") - 1)
                Else
                    id_bberg = Left(id_bberg, InStr(id_bberg, " Index") - 1)
                End If*/
            }

            if (new DateTime(id_year, id_month, id_day).DayOfWeek == DayOfWeek.Thursday ||
                new DateTime(id_year, id_month, id_day).DayOfWeek == DayOfWeek.Friday)
            {
                id_bberg = id_imnt_reuters;
                id_imnt = createDeriv(id_imnt_ric, id_imnt_reuters, id_bberg, nm_month, pr_strike, id_pc, id_year, instArr, 0, id_day);
            }
            else
                id_imnt = createDeriv(id_imnt_ric, id_imnt_reuters, id_bberg, nm_month, pr_strike, id_pc, id_year, instArr);

            return id_imnt;
            /*


            If Weekday(CDate(id_month & "/" & id_day & "/" & id_year)) = 6 Or Weekday(CDate(id_month & "/" & id_day & "/" & id_year)) = 5 Then
                id_bberg = id_imnt_reuters
                id_imnt = createDeriv(id_imnt_ric, nm_month, pr_strike, id_pc, id_year, , id_day)
            Else
                id_imnt = createDeriv(id_imnt_ric, nm_month, pr_strike, id_pc, id_year)
            End If

            If id_imnt = 0 Then GoTo skip_this*/
        }

        internal int createDeriv(string id_imnt_ric, string id_imnt_reuters, string id_bberg, string nm_month, float pr_strike, string id_pc, int id_year, Dictionary<int, Instrument> instArr, int id_month = 0, int id_day = 0)
        {
            string[] _months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            if (id_month == 0)
                id_month = Array.IndexOf<string>(_months, nm_month) + 1;
            else
                nm_month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(id_month).Substring(0, 3).ToUpper();

            //TODO if cannot find underlying symbol
            //Instrument item = instArr.FirstOrDefault(x => x.id_imnt_ric == id_imnt_ric);
            var dict = instArr.FirstOrDefault(x => x.Value.id_imnt_ric == id_imnt_ric);
            Instrument item = instArr[(int)dict.Key];
            string id_der_ric = "";
            string nm_der = "";
            DateTime dt_mat;

            if (id_day > 0) //is weekly option
            {
                id_der_ric = id_imnt_ric + nm_month + id_day + id_year.ToString().Substring(2, 2) + "-" + pr_strike + id_pc;
                dt_mat = new DateTime(id_year, id_month, id_day);
            }
            else
            {
                id_der_ric = id_imnt_ric + nm_month + id_year.ToString().Substring(2, 2) + "-" + pr_strike + id_pc;
                dt_mat = (DateTime)getExpiryDate(id_month, id_year);
            }

            //TO DO FIX THIS
            /*if (instArr.FirstOrDefault(x => x.Value.id_imnt_ric == id_der_ric && x.Value.id_del == 0) != null)
            {
                return (int)instArr.FirstOrDefault(x => x.id_imnt_ric == id_der_ric).id_imnt;
            }
             */

            if (id_day > 0) //is weekly option
                nm_der = id_imnt_ric + nm_month + id_day + id_year.ToString().Substring(2, 2) + "-" + pr_strike + id_pc;
            else
                nm_der = id_imnt_ric + nm_month + id_year.ToString().Substring(2, 2) + "-" + pr_strike + id_pc;
            var newInst = new object[24];

            var dictmax = instArr.Max(x => x.Value.id_imnt);
            Instrument itemmax = instArr[(int)dict.Key];

            newInst[0] = dictmax + 1;
            newInst[1] = id_der_ric;
            newInst[2] = id_der_ric;
            newInst[3] = id_der_ric;
            newInst[4] = "0";
            newInst[5] = "OPT";
            newInst[6] = "na";
            newInst[7] = 0;
            newInst[8] = 0;
            newInst[9] = "na";
            newInst[10] = (double)100.0;
            newInst[11] = (double)1;
            newInst[12] = item.id_imnt_und;
            newInst[13] = (double)pr_strike;
            newInst[14] = dt_mat;
            newInst[15] = id_pc;
            newInst[16] = 1;
            newInst[17] = id_imnt_reuters;
            newInst[18] = 0;
            newInst[19] = id_bberg;
            newInst[20] = "";
            newInst[21] = DateTime.Now;
            newInst[22] = 0;
            newInst[23] = "schan";
            instArr.Add((int)newInst[0], new Instrument(newInst));

            return (int)newInst[0];
        }

        internal Nullable<DateTime> getExpiryDate(int id_month, int id_year)
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
                    ct_friday++;
                    dt_mat = dt;
                }

                if (i > 32)
                {
                    return null;
                }

                i++;

            } while (ct_friday < 3);

            return dt_mat;
        }

        internal Nullable<DateTime> getExpiryDate2(int id_month, int id_year)
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
                    id_month = id_month + 1;
            }

            do
            {
                dt = new DateTime(id_year, id_month, i);

                if (dt.DayOfWeek == DayOfWeek.Friday)
                {
                    ct_friday++;
                    dt_mat = dt;
                }

                if (i > 32)
                {
                    return null;
                }

                i++;

            } while (ct_friday < 3);

            return dt_mat;
        }

        internal string cleanTicker(string tx)
        {
            string str;

            if (tx == null || tx == "")
                str = tx;

            else if (tx.Substring(0, 1) == ".")
                str = tx.Replace(",", "");
            else
            {
                str = tx.Replace(".N", "");
                //str = tx.Replace(str, "\"\"", "");
                str = tx.Replace(".OQ", "");
                str = tx.Replace(".O", "");
            }

            return str;

        }

        internal string getInstType(string id_typ_imnt_mlp)
        {
            string id_typ_imnt = "";

            if (id_typ_imnt_mlp == "S" || id_typ_imnt_mlp == "ADR")
                id_typ_imnt = "STK";
            else if (id_typ_imnt_mlp == "O" || id_typ_imnt_mlp == "ADRO" ||
                id_typ_imnt_mlp == "BO" || id_typ_imnt_mlp == "ER" ||
                id_typ_imnt_mlp == "W" || id_typ_imnt_mlp == "IdxOption" ||
                id_typ_imnt_mlp == "EqtyWarrRight")
                id_typ_imnt = "OPT";
            else if (id_typ_imnt_mlp == "F")
                id_typ_imnt = "FUT";
            else if (id_typ_imnt_mlp == "OTC")
                id_typ_imnt = "OTC";

            return id_typ_imnt;
        }

    }
}
