using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace wpfexample
{
    class BorrowRpt
    {
        private List<Position> _posArr;
        private List<Portfolio> _prtfArr;
        private List<Borrow> _borrowArr;
        private Dictionary<int, Instrument> _instArr;
        private DBHelper _dbhelper = new DBHelper();
        private string strFile = @"\\samba\brok_tjackson\locates";
        private string email_body;
        private string email_body2;
        private string email_body_borrow;
        private string email_body_nofill;
        public BorrowRpt(List<Position> posArr, List<Portfolio> prtfArr
            , Dictionary<int, Instrument> instArr)
        {
            _posArr = posArr.Where(x => x.am_pos_adj < 0 && x.am_pos < 0).ToList(); //take only short stock
            _prtfArr = prtfArr;
            _instArr = instArr;
            _borrowArr = _dbhelper.GetBorrow();
            Emailer _emailer;
            email_body = @"<font size=""2"", font face = ""Calibri""><strong>Borrow Check</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0""><font size=""2"", font face = ""Calibri"">"
                + "<tr><th>Name</th><th>Intraday trade</th><th>shs available</th></tr>";
            email_body2 = @"<font size=""2"", font face = ""Calibri""><strong>Borrow rejects</strong></font><br><table border=""1"" BGCOLOR=""#ffffff"" CELLPADDING=""2"" CELLSPACING=""0""><font size=""2"", font face = ""Calibri"">"
                + "<tr><th>Name</th></tr>";
            runBorrowRpt();

            if (email_body_borrow != null && email_body_nofill != null)
            {
                email_body_borrow = email_body_borrow + "</font></table>";
                email_body_nofill = email_body_nofill + "</font></table>";

                email_body = email_body + email_body_borrow + "<br>" + email_body2 + email_body_nofill;
                _emailer = new Emailer("sherman.chan@mlp.com", "sherman.chan@mlp.com", "Borrow Checker", "Sherman Chan", "Borrow Checker " + DateTime.Today.ToString("MM/dd/yy"), email_body, true);
                _emailer.SendEmail();
            }
        }

        public void runBorrowRpt()
        {
            string dt = DateTime.Today.ToString("yyyMMdd");
            string[] files = Directory.GetFiles(strFile, "*.Locates." + dt + "*");

            List<BorrowType> borrowfile_arr = new List<BorrowType>();
            
            //file to array
            foreach (string file in files)
            {
                FileStream logFileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (StreamReader reader = new StreamReader(logFileStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split('\t');

                            if (parts[0] == "Symbol")
                                continue;

                            borrowfile_arr.Add(new BorrowType(parts[0], Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2]), parts[3], Convert.ToDouble(parts[4])));                            
                            if (Convert.ToDouble(parts[2]) == 0)
                            {
                                email_body_nofill = email_body_nofill + "<tr>"
                                + "<td>" + parts[0] + "</td>"
                                + "</tr>";

                            }
                        }
                        catch
                        {
                        }

                    }
                }
            }

            //check
            foreach (Position pos in _posArr)
            {
                Instrument imnt = _instArr[pos.id_imnt];
                double borrow_req;
                double borrow_ml;
                if (imnt.id_typ_imnt != "STK")
                    continue;

                try
                {
                    borrow_req = borrowfile_arr.FirstOrDefault(x => x.id_imnt_ric == imnt.id_bberg).am_qty_fill;
                }
                catch
                {
                    borrow_req = 0;
                }

                borrow_ml = (double)(float)_borrowArr.FirstOrDefault(x => x.id_imnt == imnt.id_imnt).am_shares_max;

                if (Math.Abs(pos.am_pos_adj) > (borrow_ml + borrow_req))
                {
                    email_body_borrow = email_body_borrow + "<tr>"
                        + "<td>" + imnt.id_imnt_ric + "</td>"
                        + "<td>" + pos.am_pos_adj.ToString("#,##0") + "</td>"
                        + "<td>" + (borrow_ml + borrow_req).ToString("#,##0") + "</td>"
                        + "</tr>";

                }
            }
        }

    }

    class BorrowType
    {
        public string id_imnt_ric { get; set; }
        public double am_qty_req { get; set; }
        public double am_qty_fill { get; set; }
        public string nm_req {get;set;}
        public double am_rate { get; set; }

        public BorrowType(string _id_imnt_ric, double _am_qty_req, double _am_qty_fill, string _nm_req, double _am_rate)
        {
            id_imnt_ric = _id_imnt_ric;
            am_qty_req = _am_qty_req;
            am_qty_fill = _am_qty_fill;
            nm_req = _nm_req;
            am_rate = _am_rate;
        }

    }
}
