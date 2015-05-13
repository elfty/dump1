using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace wpfexample
{
    class SABR
    {
        /*
        public SABR()
        {
            FincadFunctions.fc_app_init();
        }

        //*****************************************************************
        //  aaCalibrateOptions_SABR
        //*****************************************************************

        public void CalibrateOptionsSABR()
        {
            int status;
            double temp;
            int i;
            int j;
            int xrows;
            int xcols;
            string output = "";
            double price_u;
            double d_mkt;
            int smile_type;
            double[,] smile_tbl = new double[9, 3];
            double[,] df_crv_std = new double[20, 2];
            double[,] df_crv_hld = new double[20, 2];
            int intrp;
            double[,] param_rng = new double[2, 4];
            double[,] param_ini = new double[1, 4];
            int min_method;
            double[,] min_param = new double[1, 5];
            int error_metric;
            int weighting;
            int table_type;
            double[] Return_calib_tbl = new double[1];

            // Initialize the input variables
            price_u = 45;
            d_mkt = 39814;
            smile_type = 1;

            smile_tbl[0, 0] = 39995;
            smile_tbl[0, 1] = 25;
            smile_tbl[0, 2] = 0.33;
            smile_tbl[1, 0] = 39995;
            smile_tbl[1, 1] = 30;
            smile_tbl[1, 2] = 0.315;
            smile_tbl[2, 0] = 39995;
            smile_tbl[2, 1] = 35;
            smile_tbl[2, 2] = 0.3;
            smile_tbl[3, 0] = 39995;
            smile_tbl[3, 1] = 40;
            smile_tbl[3, 2] = 0.29;
            smile_tbl[4, 0] = 39995;
            smile_tbl[4, 1] = 45;
            smile_tbl[4, 2] = 0.28;
            smile_tbl[5, 0] = 39995;
            smile_tbl[5, 1] = 50;
            smile_tbl[5, 2] = 0.27;
            smile_tbl[6, 0] = 39995;
            smile_tbl[6, 1] = 55;
            smile_tbl[6, 2] = 0.265;
            smile_tbl[7, 0] = 39995;
            smile_tbl[7, 1] = 60;
            smile_tbl[7, 2] = 0.26;
            smile_tbl[8, 0] = 39995;
            smile_tbl[8, 1] = 65;
            smile_tbl[8, 2] = 0.255;


            df_crv_std[0, 0] = 39814;
            df_crv_std[0, 1] = 1;
            df_crv_std[1, 0] = 39995;
            df_crv_std[1, 1] = 0.976095768;
            df_crv_std[2, 0] = 40179;
            df_crv_std[2, 1] = 0.952380952;
            df_crv_std[3, 0] = 40360;
            df_crv_std[3, 1] = 0.929615017;
            df_crv_std[4, 0] = 40544;
            df_crv_std[4, 1] = 0.907029478;
            df_crv_std[5, 0] = 40725;
            df_crv_std[5, 1] = 0.885347635;
            df_crv_std[6, 0] = 40909;
            df_crv_std[6, 1] = 0.863837599;
            df_crv_std[7, 0] = 41091;
            df_crv_std[7, 1] = 0.8430755210000001;
            df_crv_std[8, 0] = 41275;
            df_crv_std[8, 1] = 0.82259251;
            df_crv_std[9, 0] = 41456;
            df_crv_std[9, 1] = 0.802929068;
            df_crv_std[10, 0] = 41640;
            df_crv_std[10, 1] = 0.783421438;
            df_crv_std[11, 0] = 41821;
            df_crv_std[11, 1] = 0.76469435;
            df_crv_std[12, 0] = 42005;
            df_crv_std[12, 1] = 0.746115655;
            df_crv_std[13, 0] = 42186;
            df_crv_std[13, 1] = 0.728280334;
            df_crv_std[14, 0] = 42370;
            df_crv_std[14, 1] = 0.710586339;
            df_crv_std[15, 0] = 42552;
            df_crv_std[15, 1] = 0.693507609;
            df_crv_std[16, 0] = 42736;
            df_crv_std[16, 1] = 0.676658438;
            df_crv_std[17, 0] = 42917;
            df_crv_std[17, 1] = 0.660483437;
            df_crv_std[18, 0] = 43101;
            df_crv_std[18, 1] = 0.6444366070000001;
            df_crv_std[19, 0] = 43282;
            df_crv_std[19, 1] = 0.629031845;


            df_crv_hld[0, 0] = 39814;
            df_crv_hld[0, 1] = 1;
            df_crv_hld[1, 0] = 39995;
            df_crv_hld[1, 1] = 0.976095768;
            df_crv_hld[2, 0] = 40179;
            df_crv_hld[2, 1] = 0.952380952;
            df_crv_hld[3, 0] = 40360;
            df_crv_hld[3, 1] = 0.929615017;
            df_crv_hld[4, 0] = 40544;
            df_crv_hld[4, 1] = 0.907029478;
            df_crv_hld[5, 0] = 40725;
            df_crv_hld[5, 1] = 0.885347635;
            df_crv_hld[6, 0] = 40909;
            df_crv_hld[6, 1] = 0.863837599;
            df_crv_hld[7, 0] = 41091;
            df_crv_hld[7, 1] = 0.8430755210000001;
            df_crv_hld[8, 0] = 41275;
            df_crv_hld[8, 1] = 0.82259251;
            df_crv_hld[9, 0] = 41456;
            df_crv_hld[9, 1] = 0.802929068;
            df_crv_hld[10, 0] = 41640;
            df_crv_hld[10, 1] = 0.783421438;
            df_crv_hld[11, 0] = 41821;
            df_crv_hld[11, 1] = 0.76469435;
            df_crv_hld[12, 0] = 42005;
            df_crv_hld[12, 1] = 0.746115655;
            df_crv_hld[13, 0] = 42186;
            df_crv_hld[13, 1] = 0.728280334;
            df_crv_hld[14, 0] = 42370;
            df_crv_hld[14, 1] = 0.710586339;
            df_crv_hld[15, 0] = 42552;
            df_crv_hld[15, 1] = 0.693507609;
            df_crv_hld[16, 0] = 42736;
            df_crv_hld[16, 1] = 0.676658438;
            df_crv_hld[17, 0] = 42917;
            df_crv_hld[17, 1] = 0.660483437;
            df_crv_hld[18, 0] = 43101;
            df_crv_hld[18, 1] = 0.6444366070000001;
            df_crv_hld[19, 0] = 43282;
            df_crv_hld[19, 1] = 0.629031845;

            intrp = 3;

            param_rng[0, 0] = 0.5;
            param_rng[0, 1] = 0.5;
            param_rng[0, 2] = -0.999;
            param_rng[0, 3] = 0;
            param_rng[1, 0] = 1;
            param_rng[1, 1] = 1;
            param_rng[1, 2] = 0;
            param_rng[1, 3] = 1;


            param_ini[0, 0] = 0.75;
            param_ini[0, 1] = 0.75;
            param_ini[0, 2] = -0.5;
            param_ini[0, 3] = 0.2;

            min_method = 2;

            min_param[0, 0] = 2000.5;
            min_param[0, 1] = 1e-010;
            min_param[0, 2] = 15;
            min_param[0, 3] = 0.8;
            min_param[0, 4] = 0.5;

            error_metric = 1;
            weighting = 2;
            table_type = 2;

            // Call the member function
            status = FincadFunctions.aaCalibrateOptions_SABR(price_u, d_mkt, smile_type, smile_tbl, df_crv_std, df_crv_hld, intrp, param_rng, param_ini, min_method, min_param, error_metric, weighting, table_type, ref Return_calib_tbl);
            if (status != 0)
            {
                output = "Calculation failed";
            }
            else
            {
                // Display the results
                output = "Calculation succeeded: \r\n";
                xrows = Return_calib_tbl.GetLength(0);
                // Limit results to 50 rows
                if (xrows > 50)
                {
                    xrows = 50;
                }

                for (i = 1; i <= xrows; ++i)
                {
                    temp = ((double[])Return_calib_tbl)[i - 1];
                    output = string.Concat(output, temp.ToString() + "\r\n", "");
                }
            }
            //MessageBox.Show(output, "FINCAD Analytics Suite for Developers", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }


        //*****************************************************************
        //  aaOption_SABR_eu_p
        //*****************************************************************
        public void OptionSABReup()
        {
            int status;
            double temp;
            int i;
            int xrows;
            string output = "";
            double price_u;
            int payoff_type;
            double[,] strike_tbl = new double[1, 1];
            double d_exp;
            double d_v;
            double[,] param_tbl = new double[1, 4];
            double[,] df_crv_std = new double[20, 2];
            double[,] df_crv_hld = new double[20, 2];
            int intrp;
            int[] stat = new int[10];
            double[] Return_stat = new double[1];

            // Initialize the input variables


            price_u = 50;
            payoff_type = 2;

            strike_tbl[0, 0] = 50;

            d_exp = 39995;
            d_v = 39814;

            param_tbl[0, 0] = 0.3;
            param_tbl[0, 1] = 1;
            param_tbl[0, 2] = -0.5;
            param_tbl[0, 3] = 0.2;


            df_crv_std[0, 0] = 39814;
            df_crv_std[0, 1] = 1;
            df_crv_std[1, 0] = 39995;
            df_crv_std[1, 1] = 0.976095768;
            df_crv_std[2, 0] = 40179;
            df_crv_std[2, 1] = 0.952380952;
            df_crv_std[3, 0] = 40360;
            df_crv_std[3, 1] = 0.929615017;
            df_crv_std[4, 0] = 40544;
            df_crv_std[4, 1] = 0.907029478;
            df_crv_std[5, 0] = 40725;
            df_crv_std[5, 1] = 0.885347635;
            df_crv_std[6, 0] = 40909;
            df_crv_std[6, 1] = 0.863837599;
            df_crv_std[7, 0] = 41091;
            df_crv_std[7, 1] = 0.8430755210000001;
            df_crv_std[8, 0] = 41275;
            df_crv_std[8, 1] = 0.82259251;
            df_crv_std[9, 0] = 41456;
            df_crv_std[9, 1] = 0.802929068;
            df_crv_std[10, 0] = 41640;
            df_crv_std[10, 1] = 0.783421438;
            df_crv_std[11, 0] = 41821;
            df_crv_std[11, 1] = 0.76469435;
            df_crv_std[12, 0] = 42005;
            df_crv_std[12, 1] = 0.746115655;
            df_crv_std[13, 0] = 42186;
            df_crv_std[13, 1] = 0.728280334;
            df_crv_std[14, 0] = 42370;
            df_crv_std[14, 1] = 0.710586339;
            df_crv_std[15, 0] = 42552;
            df_crv_std[15, 1] = 0.693507609;
            df_crv_std[16, 0] = 42736;
            df_crv_std[16, 1] = 0.676658438;
            df_crv_std[17, 0] = 42917;
            df_crv_std[17, 1] = 0.660483437;
            df_crv_std[18, 0] = 43101;
            df_crv_std[18, 1] = 0.6444366070000001;
            df_crv_std[19, 0] = 43282;
            df_crv_std[19, 1] = 0.629031845;


            df_crv_hld[0, 0] = 39814;
            df_crv_hld[0, 1] = 1;
            df_crv_hld[1, 0] = 39995;
            df_crv_hld[1, 1] = 0.976095768;
            df_crv_hld[2, 0] = 40179;
            df_crv_hld[2, 1] = 0.952380952;
            df_crv_hld[3, 0] = 40360;
            df_crv_hld[3, 1] = 0.929615017;
            df_crv_hld[4, 0] = 40544;
            df_crv_hld[4, 1] = 0.907029478;
            df_crv_hld[5, 0] = 40725;
            df_crv_hld[5, 1] = 0.885347635;
            df_crv_hld[6, 0] = 40909;
            df_crv_hld[6, 1] = 0.863837599;
            df_crv_hld[7, 0] = 41091;
            df_crv_hld[7, 1] = 0.8430755210000001;
            df_crv_hld[8, 0] = 41275;
            df_crv_hld[8, 1] = 0.82259251;
            df_crv_hld[9, 0] = 41456;
            df_crv_hld[9, 1] = 0.802929068;
            df_crv_hld[10, 0] = 41640;
            df_crv_hld[10, 1] = 0.783421438;
            df_crv_hld[11, 0] = 41821;
            df_crv_hld[11, 1] = 0.76469435;
            df_crv_hld[12, 0] = 42005;
            df_crv_hld[12, 1] = 0.746115655;
            df_crv_hld[13, 0] = 42186;
            df_crv_hld[13, 1] = 0.728280334;
            df_crv_hld[14, 0] = 42370;
            df_crv_hld[14, 1] = 0.710586339;
            df_crv_hld[15, 0] = 42552;
            df_crv_hld[15, 1] = 0.693507609;
            df_crv_hld[16, 0] = 42736;
            df_crv_hld[16, 1] = 0.676658438;
            df_crv_hld[17, 0] = 42917;
            df_crv_hld[17, 1] = 0.660483437;
            df_crv_hld[18, 0] = 43101;
            df_crv_hld[18, 1] = 0.6444366070000001;
            df_crv_hld[19, 0] = 43282;
            df_crv_hld[19, 1] = 0.629031845;

            intrp = 1;

            for (i = 1; i <= 10; i++)
            {
                stat[i - 1] = i;
            }


            // Call the member function
            status = FincadFunctions.aaOption_SABR_eu_p(price_u, payoff_type, strike_tbl, d_exp, d_v, param_tbl, df_crv_std, df_crv_hld, intrp, stat, ref Return_stat);
            if (status != 0)
            {
                output = "Calculation failed";
            }
            else
            {
                // Display the results
                output = "Calculation succeeded: \r\n";
                xrows = Return_stat.GetLength(0);
                // Limit results to 50 rows
                if (xrows > 50)
                {
                    xrows = 50;
                }

                for (i = 1; i <= xrows; ++i)
                {
                    temp = ((double[])Return_stat)[i - 1];
                    output = string.Concat(output, temp.ToString() + "\r\n", "");
                }
            }
            //MessageBox.Show(output, "FINCAD Analytics Suite for Developers", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        //*****************************************************************
        //  aaOption_SABR_iv
        //*****************************************************************
        private void OptionSABRiv()
        {
            int status;
            double temp;
            int i;
            int j;
            int xrows;
            int xcols;
            string output = "";
            double price_u;
            double[,] TK_tbl = new double[9, 2];
            double d_v;
            double[,] param_tbl = new double[1, 4];
            double[,] df_crv_std = new double[20, 2];
            double[,] df_crv_hld = new double[20, 2];
            int intrp;
            double[,] Return_output_smile_tbl = new double[1, 1];

            // Initialize the input variables
            price_u = 50;

            TK_tbl[0, 0] = 39995;
            TK_tbl[0, 1] = 46;
            TK_tbl[1, 0] = 39995;
            TK_tbl[1, 1] = 47;
            TK_tbl[2, 0] = 39995;
            TK_tbl[2, 1] = 48;
            TK_tbl[3, 0] = 39995;
            TK_tbl[3, 1] = 49;
            TK_tbl[4, 0] = 39995;
            TK_tbl[4, 1] = 50;
            TK_tbl[5, 0] = 39995;
            TK_tbl[5, 1] = 51;
            TK_tbl[6, 0] = 39995;
            TK_tbl[6, 1] = 52;
            TK_tbl[7, 0] = 39995;
            TK_tbl[7, 1] = 53;
            TK_tbl[8, 0] = 39995;
            TK_tbl[8, 1] = 54;

            d_v = 39814;

            param_tbl[0, 0] = 0.3;
            param_tbl[0, 1] = 1;
            param_tbl[0, 2] = -0.5;
            param_tbl[0, 3] = 0.2;


            df_crv_std[0, 0] = 39814;
            df_crv_std[0, 1] = 1;
            df_crv_std[1, 0] = 39995;
            df_crv_std[1, 1] = 0.976095768;
            df_crv_std[2, 0] = 40179;
            df_crv_std[2, 1] = 0.952380952;
            df_crv_std[3, 0] = 40360;
            df_crv_std[3, 1] = 0.929615017;
            df_crv_std[4, 0] = 40544;
            df_crv_std[4, 1] = 0.907029478;
            df_crv_std[5, 0] = 40725;
            df_crv_std[5, 1] = 0.885347635;
            df_crv_std[6, 0] = 40909;
            df_crv_std[6, 1] = 0.863837599;
            df_crv_std[7, 0] = 41091;
            df_crv_std[7, 1] = 0.8430755210000001;
            df_crv_std[8, 0] = 41275;
            df_crv_std[8, 1] = 0.82259251;
            df_crv_std[9, 0] = 41456;
            df_crv_std[9, 1] = 0.802929068;
            df_crv_std[10, 0] = 41640;
            df_crv_std[10, 1] = 0.783421438;
            df_crv_std[11, 0] = 41821;
            df_crv_std[11, 1] = 0.76469435;
            df_crv_std[12, 0] = 42005;
            df_crv_std[12, 1] = 0.746115655;
            df_crv_std[13, 0] = 42186;
            df_crv_std[13, 1] = 0.728280334;
            df_crv_std[14, 0] = 42370;
            df_crv_std[14, 1] = 0.710586339;
            df_crv_std[15, 0] = 42552;
            df_crv_std[15, 1] = 0.693507609;
            df_crv_std[16, 0] = 42736;
            df_crv_std[16, 1] = 0.676658438;
            df_crv_std[17, 0] = 42917;
            df_crv_std[17, 1] = 0.660483437;
            df_crv_std[18, 0] = 43101;
            df_crv_std[18, 1] = 0.6444366070000001;
            df_crv_std[19, 0] = 43282;
            df_crv_std[19, 1] = 0.629031845;


            df_crv_hld[0, 0] = 39814;
            df_crv_hld[0, 1] = 1;
            df_crv_hld[1, 0] = 39995;
            df_crv_hld[1, 1] = 0.976095768;
            df_crv_hld[2, 0] = 40179;
            df_crv_hld[2, 1] = 0.952380952;
            df_crv_hld[3, 0] = 40360;
            df_crv_hld[3, 1] = 0.929615017;
            df_crv_hld[4, 0] = 40544;
            df_crv_hld[4, 1] = 0.907029478;
            df_crv_hld[5, 0] = 40725;
            df_crv_hld[5, 1] = 0.885347635;
            df_crv_hld[6, 0] = 40909;
            df_crv_hld[6, 1] = 0.863837599;
            df_crv_hld[7, 0] = 41091;
            df_crv_hld[7, 1] = 0.8430755210000001;
            df_crv_hld[8, 0] = 41275;
            df_crv_hld[8, 1] = 0.82259251;
            df_crv_hld[9, 0] = 41456;
            df_crv_hld[9, 1] = 0.802929068;
            df_crv_hld[10, 0] = 41640;
            df_crv_hld[10, 1] = 0.783421438;
            df_crv_hld[11, 0] = 41821;
            df_crv_hld[11, 1] = 0.76469435;
            df_crv_hld[12, 0] = 42005;
            df_crv_hld[12, 1] = 0.746115655;
            df_crv_hld[13, 0] = 42186;
            df_crv_hld[13, 1] = 0.728280334;
            df_crv_hld[14, 0] = 42370;
            df_crv_hld[14, 1] = 0.710586339;
            df_crv_hld[15, 0] = 42552;
            df_crv_hld[15, 1] = 0.693507609;
            df_crv_hld[16, 0] = 42736;
            df_crv_hld[16, 1] = 0.676658438;
            df_crv_hld[17, 0] = 42917;
            df_crv_hld[17, 1] = 0.660483437;
            df_crv_hld[18, 0] = 43101;
            df_crv_hld[18, 1] = 0.6444366070000001;
            df_crv_hld[19, 0] = 43282;
            df_crv_hld[19, 1] = 0.629031845;

            intrp = 1;

            // Call the member function
            status = FincadFunctions.aaOption_SABR_iv(price_u, TK_tbl, d_v, param_tbl, df_crv_std, df_crv_hld, intrp, ref Return_output_smile_tbl);
            if (status != 0)
            {
                output = "Calculation failed";
            }
            else
            {
                // Display the results
                output = "Calculation succeeded: \r\n";
                xrows = Return_output_smile_tbl.GetLength(0);
                xcols = Return_output_smile_tbl.GetLength(1);
                // Limit results to 50 rows
                if (xrows > 50)
                {
                    xrows = 50;
                }

                for (i = 1; i <= xrows; ++i)
                {
                    for (j = 1; j <= xcols; ++j)
                    {
                        temp = ((double[,])Return_output_smile_tbl)[i - 1, j - 1];
                        output = string.Concat(output, temp.ToString(), "	");
                    }
                    output = string.Concat(output, "\r\n");
                }
            }
            MessageBox.Show(output, "FINCAD Analytics Suite for Developers", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

         
        public static DateTime FromExcelSerialDate(int SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug   
            return new DateTime(1899, 12, 31).AddDays(SerialDate);
            //DateTime dt = DateTime.FromOADate(39814)
            //DateTime dt = DateTime.Parse("8/25/2006 11:23:01").ToOADate(); 

        }
        */
    }
         
}
