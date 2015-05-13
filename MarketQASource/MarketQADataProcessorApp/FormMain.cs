using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using MarketQADataProcessor;
using Toolkit.Logging;

namespace MarketQADataProcessorApp
{
	// TODO Add parameters: date to start processing from, rows to process (currently 3730)
	// TODO Option to use the parameters in reverse: last date
	
	
	public partial class formMain : Form
	{ 
		//static readonly ILog Log = LogService.GetLog();

		private bool isTestMode;
        private int id_imnt_ivy;
        private int testSecurityID;
        private int testSecurityIvyID;
		//private const int testSecurityID = 106203;
		//private const int testSecurityIvyID = 10012;

        //private const int testSecurityID = 134657;
        //private const int testSecurityIvyID = 24013;

        int referenceSecurity;	// id_imnt_ivy of SPX

		string importsTable;
		string importsTable2;

		int numOfRows;

		// TODO Temporary data storage...
		List<string> logInsufficientData = new List<string>();
		List<string> logMemo = new List<string>();

		private List<int> _securitiesWithNoData = new List<int>();
		private bool isExecuting;

		DBHelper _dbHelper = new DBHelper();

		private bool _sendEmail;
		private string _emailServer;
		private int _emailServerPort;
		private string _emailFrom;
		private string _emailFromName;
		private string _emailTo;
		private string _emailToName;
		private string _emailSubject;
		private string _emailBody;
		private bool _isBodyHtml;

        

		public formMain()
		{
			InitializeComponent();

			Global.FormMain = this;

			Reset();
		}

        public int InsertNumOfRows { get; set; }
        public bool AutoRun { get; set; }

		private void Reset()
		{
			_dbHelper = new DBHelper();
		}
		
		//baseFormControl1.AddMemoText("Exception: ", exception);
		

		private List<IndexData> PopulateIndexData(DateTime dateFrom, DateTime dateTo, int rowsToRetrieve, ref string errorMsg)
		{
			return _dbHelper.GetIndexData(dateFrom, dateTo, rowsToRetrieve, ref errorMsg);
		}



        private void buttonRun_Click(object sender, EventArgs e)
        {
            RunningState(true);

            // TODO Move to a function
            isTestMode = checkboxTestMode.Checked;
            int insertNumOfRows = (int)numericInsertRows.Value;
            string securitiesDB = "Ivy";
            string importsDB = "OPTIONS";
            string importsSchema = "dbo";
            string tableSecurityPrice = "SECURITY_PRICE";

            string errorMsg1 = null;
            //int counter = 0;
 
            _dbHelper.ClearTable(importsDB, importsSchema, importsTable);

            _dbHelper.ClearTable(importsDB, importsSchema, importsTable2);
            // TODO Get a list of securities
            string securitiesTable = "INSTRUMENT";
            Dictionary<int, int> securities = _dbHelper.GetSecurities(importsDB, importsSchema, securitiesTable);


            /*List<ColumnInfo> names = GetColumnNames(tableSecurityPrice);
            StringBuilder sb = new StringBuilder(names.Count);
            foreach (var columnInfo in names)
            {
                baseFormControl1.AddMemoText("Name: {0}. Type: {1}.", columnInfo.Name, columnInfo.Type);
            }*/
            
            //RUN MANY STOCKS
            List<int> imntarray = new List<int>(new[]{ 
                13506



                });

            int many_stocks_counter = 1;

            if (isTestMode)
                many_stocks_counter = imntarray.Count;


            for (int counter = 0; counter < many_stocks_counter; counter++)
            {
            

                string schema = "";

                DateTime dateFrom;
                DateTime dateTo;

                if (isTestMode)
                {
                    //testSecurityID = Convert.ToInt16(id_imnt.Text);
                    testSecurityID = Convert.ToInt32(imntarray[counter]); //RUN MANY STOCKS
                    testSecurityIvyID = _dbHelper.GetIvySecurityID(importsDB, importsSchema, securitiesTable, testSecurityID, ref errorMsg1);
                    insertNumOfRows = 10000;	// To match the rows in the reference spreadsheet. 
                    numOfRows = FixedValues.DaysIn36Months + insertNumOfRows + 1;
                    dateFrom = _dbHelper.GetFirstDate(securitiesDB, schema, tableSecurityPrice, numOfRows, testSecurityIvyID, ref errorMsg1); // using SPX
                    //dateTo = new DateTime(2010, 10, 25);
                    //dateTo = _dbHelper.GetLastDate(securitiesDB, schema, tableSecurityPrice, referenceSecurity, ref errorMsg1); 
                    dateTo = _dbHelper.GetLastDate(securitiesDB, schema, tableSecurityPrice, testSecurityIvyID, ref errorMsg1);
                    numOfRows = _dbHelper.GetNumOfRows(securitiesDB, schema, tableSecurityPrice, referenceSecurity, dateFrom, dateTo, ref errorMsg1); // using SPX
                }
                else
                {
                    //insertNumOfRows = 10000;	
                    numOfRows = FixedValues.DaysIn36Months + insertNumOfRows + 1;
                    dateFrom = _dbHelper.GetFirstDate(securitiesDB, schema, tableSecurityPrice, numOfRows+2, referenceSecurity, ref errorMsg1); // using SPX
                    dateTo = _dbHelper.GetLastDate(securitiesDB, schema, tableSecurityPrice, referenceSecurity, ref errorMsg1);
                    numOfRows = _dbHelper.GetNumOfRows(securitiesDB, schema, tableSecurityPrice, referenceSecurity, dateFrom, dateTo, ref errorMsg1); // using SPX
                    //dateTo = new DateTime(2012, 07, 12);
                    //numOfRows = _dbHelper.GetNumOfRows(securitiesDB, schema, tableSecurityPrice, referenceSecurity, dateFrom, dateTo, ref errorMsg1); // using SPX
                }


                //bool useNolock = true;

               int offset = (int)dateFrom.ToOADate();

                List<IndexData> indexData = PopulateIndexData(dateFrom, dateTo, numOfRows, ref errorMsg1);
                numOfRows = indexData.Count;

                // TODO Loop through each security and update the data cache
                // TODO Insert into the DB with a transaction


                // TODO Remove after testing
                if (isTestMode)
                {

                    securities.Clear();
                    securities.Add(testSecurityIvyID, testSecurityID);
                    //numOfRows = insertNumOfRows + 1;
                }

                // TODO Changes: Keep the index data outside the values list: need to reuse it
                // TODO Changes: Parallelize

                Dictionary<int, SecurityCalcedValues> allSecurities = new Dictionary<int, SecurityCalcedValues>(numOfRows);

                LoggingHelper.LogMemo("Retrieving data for {0} securities...", securities.Count);
                AddMemoText("Retrieving data for {0} securities...", securities.Count);

                //			securities.Clear();
                //			securities.Add(testSecurityID, testSecurityIvyID);


                foreach (var kvp in securities)
                {
  
                    SecurityCalcedValues values = _dbHelper.GetSecurityData(securitiesDB, schema, tableSecurityPrice, true, kvp.Key, dateFrom, dateTo, numOfRows, ref errorMsg1);
                    values.SecurityNonIvyID = kvp.Value;

                    // Debugging
                    if (values.Values.Count != numOfRows)
                    {
                        // TODO Log
                        logInsufficientData.Add(string.Format("Insufficient number of rows. IvySecurityID: {0}; SecurityID: {1}; rows: {2}", values.SecurityID, values.SecurityNonIvyID, values.Values.Count));
                    }

                    if (values.Values.Count > 0)
                    {
                        allSecurities.Add(kvp.Key, values);
                    }
                    else
                    {
                        _securitiesWithNoData.Add(kvp.Key);
                    }
                }
                

                LoggingHelper.LogMemo("Processing...");
                AddMemoText("Processing...");


                _dbHelper.InitializeInsertSqlCommand(importsDB, importsSchema, importsTable);
                _dbHelper.InitializeInsertSqlCommand2(importsDB, importsSchema, importsTable2);

                int count = 0;
                int total = allSecurities.Values.Count;
                TimeSpan duration;
                Stopwatch sw = new Stopwatch();
                Stopwatch swTotalRun = new Stopwatch();
                swTotalRun.Restart();

                foreach (var calcedValues in allSecurities.Values)
                {
                    sw.Restart();

                    count++;

                    string errorMsg = CalculateSecurityData(calcedValues, indexData);
                    Console.WriteLine(calcedValues.SecurityID);

                    int messages = logMemo.Count;

                    //				if (messages > 0)
                    //				{
                    //					StringBuilder sb = new StringBuilder(messages);
                    //					for (int i = 0; i < logMemo.Count; i++)
                    //					{
                    //						sb.AppendLine(logMemo[i]);
                    //					}
                    //
                    //					baseFormControl1.AddMemoText(sb.ToString());
                    //				}

                    if (errorMsg == null)
                    {
                        _dbHelper.InsertIntoDB(Math.Min(insertNumOfRows, numOfRows), calcedValues);
                        _dbHelper.InsertIntoDB2(Math.Min(insertNumOfRows, numOfRows), calcedValues);
                    }
                    else
                    {
                        AddMemoText("Ivy Security ID: {1}; Security ID: {0}; count: {2}. Text: {3}", calcedValues.SecurityNonIvyID, calcedValues.SecurityID, count, errorMsg);
                        LoggingHelper.LogWarn("Ivy Security ID: {1}; Security ID: {0}; count: {2}. Text: {3}", calcedValues.SecurityNonIvyID, calcedValues.SecurityID, count, errorMsg);
                    }

                    duration = sw.Elapsed;
                    calcedValues.ClearResults();

                    AddMemoText("Processed security ID {0}, Ivy ID: {1}. Index {2} out of {3} in {4}", calcedValues.SecurityNonIvyID, calcedValues.SecurityID, count, total, duration);
                    LoggingHelper.LogMemo("Processed security ID {0}, Ivy ID: {1}. Index {2} out of {3} in {4}", calcedValues.SecurityNonIvyID, calcedValues.SecurityID, count, total, duration);
                }

                if (logInsufficientData.Count > 0)
                {
                    LoggingHelper.LogWarn("Insufficient number of rows for the complete data set: {0}",
                                          string.Join(",", logInsufficientData));
                }

                LoggingHelper.LogMemo("Completed in: {0}", swTotalRun.Elapsed);
                AddMemoText("Completed in: {0}", swTotalRun.Elapsed);
                List<string> logFiles = GetLogFileNames();

                LoggingHelper.CloseLogs();

                // HACK Quick workaround to let the file system save the files.
                Thread.Sleep(5000);

                if (_sendEmail)
                {
                    EmailLogs(logFiles);
                }

                AddMemoText("Completed");
                RunningState(false);

            } //RUN MANY STOCKS
        }

		private List<string> GetLogFileNames()
		{
			List<string> names = new List<string>();

			names.Add(LoggingHelper.GetFullFilePath(LoggingHelper.DebugFileName));
			names.Add(LoggingHelper.GetFullFilePath(LoggingHelper.ErrorFileName));
			names.Add(LoggingHelper.GetFullFilePath(LoggingHelper.MemoFileName));
			names.Add(LoggingHelper.GetFullFilePath(LoggingHelper.WarningsFileName));

			return names;
		}

		private void EmailLogs(List<string> logFileNames)
		{
			Emailer emailer = new Emailer(_emailServer, _emailServerPort, _emailFrom, _emailFromName, _emailTo, _emailToName, _emailSubject, _emailBody, _isBodyHtml);

			foreach (string logFileName in logFileNames)
			{
				emailer.AddAttachment(logFileName);	
			}

			emailer.SendEmail();
		}
		

		private class StateObject
		{
			public SqlCommand Cmd;
			public int SecurityID;
		}

		delegate void CrossThread(string text);
		/*private void InsertCompleted(IAsyncResult c1)
		{
			try
			{
				StateObject state = (StateObject)c1.AsyncState;
				int rows = state.Cmd.EndExecuteNonQuery(c1);

				CrossThread ct = new CrossThread(UpdateProgress);
				this.Invoke(ct, "Inserted " + rows + " rows for security ID " + state.SecurityID);

			}

			catch (Exception e)
			{

			}
			finally
			{
				isExecuting = false;
				if (connection != null)
				{
					connection.Close();
				}
			}
		}*/

//		void UpdateProgress(string text)
//		{
//			textBox.Text = text;
//		}

		private void WriteToFile(SecurityCalcedValues values, List<IndexData> indexData)
		{
            FileHelper.CreateFile(@"C:\Temp\MarketQA", "FileCalced.txt");


			FileHelper.WriteLine("FileCalced.txt", "SecurityID,Date,BidLow,AskHigh,ClosePrice,Volume,TotalReturn,AdjustmentFactor,OpenPrice,SharesOutstanding,AdjustmentFactor2,AdjustedLow,AdjustedHigh,AdjustedClose,am_chg,am_vol_21,am_vol_63,am_vol_126,am_vol_252,am_vol_504,am_vol_756,am_rtn_1m,am_rtn_3m,am_rtn_6m,am_rtn_1y,am_range,am_range_5D,am_range_10D,am_range_1m,am_range_3m,am_range_6m,am_range_1y,am_var,am_corr,am_beta,am_cov_sp,am_corr_ndx,am_corr_rty,am_mkt_cap,am_var_spx,am_chg_sp,am_chg_ndx,am_chg_rty");

			ResultsArrays results = values.Results;
			SourceData source = values.Source;
			

			int i = 0;
			int row = 0;
			foreach (var calcedValues in values.Values)
			{
				FileHelper.Write("FileCalced.txt", calcedValues.RawData);
				IndexData iData = indexData[i++];
				//results.Average21_am_chg, results.StdDev21_am_chg, results.am_vol_21
				FileHelper.WriteLine("FileCalced.txt",
									 "," + calcedValues.AdjustedLow
									 + "," + calcedValues.AdjustedHigh
									 + "," + calcedValues.AdjustedClose
									 + "," + calcedValues.am_chg
//									 + "," + results.Average21_am_chg[row]
//									 + "," + results.StdDev21_am_chg[row]
									 + "," + results.am_vol_21[row]
									 + "," + results.am_vol_63[row]
									 + "," + results.am_vol_126[row]
									 + "," + results.am_vol_252[row]
									 + "," + results.am_vol_504[row]
									 + "," + results.am_vol_756[row]
									 + "," + calcedValues.am_rtn_1m
									 + "," + calcedValues.am_rtn_3m
									 + "," + calcedValues.am_rtn_6m
									 + "," + calcedValues.am_rtn_1y
									 + "," + calcedValues.am_range
									 + "," + calcedValues.am_range_5D
									 + "," + calcedValues.am_range_10D
									 + "," + calcedValues.am_range_1m
									 + "," + calcedValues.am_range_3m
									 + "," + calcedValues.am_range_6m
									 + "," + calcedValues.am_range_1y
									 + "," + results.Variance12Months_am_chg[row]
									 + "," + results.Pearson_am_corr[row]
									 + "," + results.am_beta[row]
									 + "," + results.am_cov_sp[row]
									 + "," + results.Pearson_am_corr_ndx[row]
									 + "," + results.Pearson_am_corr_rty[row]
									 + "," + calcedValues.am_mkt_cap
									 + "," + calcedValues.am_var_spx
									 + "," + source.am_chg_spx[row]
									 + "," + source.am_chg_ndx[row]
									 + "," + source.am_chg_rut[row]
					);

				row++;
			}

			FileHelper.Close("FileCalced.txt");
		}

		private string CalculateSecurityData(SecurityCalcedValues values, List<IndexData> indexData)
		{
			string successful = null;

			try
			{
				
                
                values.SetIndexData(indexData);
                successful = values.Calculate(indexData);
				
				if (isTestMode)
				{
					WriteToFile(values, indexData);
				}
				
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			return successful;
		}

		

		
		private void LogMemo(string format, params object[] args)
		{
			if (args.Length > 0)
			{
				logMemo.Add(string.Format(format, args));
			}
			else
			{
				logMemo.Add(format);
			}
		}


		private void RunningState(bool isRunning)
		{
			buttonRun.Enabled = !isRunning;
		}


		delegate void AddTextDelegate(string format, object[] args);

//		public void AddText(string format, params object[] args)
//		{
//			if (textBox.InvokeRequired)
//			{
//				Invoke(new AddTextDelegate(AddText), new object[] { format, args });
//			}
//			else
//			{
//				textBox.Text += (string.Format(format, args));
//			}
//
//		}

/*		delegate void AddMemoTextDelegate(string format, object[] args);

		public void AddMemoText(string format, params object[] args)
		{
			if (listviewMemo.InvokeRequired)
			{
				Invoke(new AddMemoTextDelegate(AddMemoText), new object[] { format, args });
			}
			else
			{
				listviewMemo.Items.Add(string.Format(format, args));
			}

			listviewMemo.EnsureVisible(listviewMemo.Items.Count - 1);
		}*/


		delegate void AddMemoTextDelegate(string format, object[] args);

		internal void AddMemoText(string format, params object[] args)
		{
			if (listboxMemo.InvokeRequired)
			{
				Invoke(new AddMemoTextDelegate(AddMemoText), new object[] { format, args });
			}
			else
			{
				listboxMemo.Items.Add(string.Format(format, args));
			}

			//listboxMemo.EnsureVisible(listboxMemo.Items.Count - 1);
		}



		private XDocument GetSettingsFile()
		{
			string fullPath = Assembly.GetExecutingAssembly().Location;
			string basePath = Path.GetDirectoryName(fullPath);
			string assemblyName = Path.GetFileName(fullPath);

			string configFile = Path.Combine(basePath, "MarketQADataProcessor" + ".config");
			return XDocument.Load(configFile);
		}

		private bool IsNullOrEmpty(XElement node, string name)
		{
			return (node == null
			        || node.Attribute(name) == null
			        || string.IsNullOrEmpty(node.Attribute(name).Value));
		}

		private string ReadAttribute(XElement node, string name)
		{
			return IsNullOrEmpty(node, name) ? null : node.Attribute(name).Value;
		}

		private string ReadStringAttribute(XElement node, string name, string defaultValue)
		{
			return ReadAttribute(node, name) ?? defaultValue;
		}

		private int ReadIntAttribute(XElement node, string name, int defaultValue)
		{
			if (IsNullOrEmpty(node, name))
			{
				return defaultValue;
			}

			int value;
			if (int.TryParse(node.Attribute(name).Value, out value))
			{
				return value;
			}

			return defaultValue;
		}

		private double ReadDoubleAttribute(XElement node, string name, double defaultValue)
		{
			if (IsNullOrEmpty(node, name))
			{
				return defaultValue;
			}

			double value;
			if (double.TryParse(node.Attribute(name).Value, out value))
			{
				return value;
			}

			return defaultValue;
		}

		private bool ReadBoolAttribute(XElement node, string name, bool defaultValue)
		{
			if (IsNullOrEmpty(node, name))
			{
				return defaultValue;
			}

			bool value;
			if (bool.TryParse(node.Attribute(name).Value, out value))
			{
				return value;
			}

			return defaultValue;
		}

		private void ReadDefaults(XElement moduleRootNode)
		{
			var configNode = (moduleRootNode == null) ? null : moduleRootNode.Element("Defaults");
			if (configNode == null)
			{
				LoggingHelper.LogError("Cannot find configuration node");
				return;
			}

			importsTable = ReadStringAttribute(configNode, "importsTable", "UND_PRICE_QA_IMPORT");
			importsTable2 = ReadStringAttribute(configNode, "importsTable2",  "VOL_HIST_QA_IMPORT");
			referenceSecurity = ReadIntAttribute(configNode, "referenceSecurity",  108105);

			_emailServer = ReadStringAttribute(configNode, "emailServer", "ny-relay1");
			_emailServerPort = ReadIntAttribute(configNode, "emailServerPort", 25);
			_emailFrom = ReadStringAttribute(configNode, "emailFrom", "schan@walleyetrading.net");
			_emailFromName = ReadStringAttribute(configNode, "emailFromName", "MarketQA App");
            _emailTo = ReadStringAttribute(configNode, "emailTo", "schan@walleyetrading.net");
			//_emailToName = ReadStringAttribute(configNode, "emailToName", "Ruslan Bartsits");
			_emailSubject = ReadStringAttribute(configNode, "emailSubject", "MarketQA Logs");
			_isBodyHtml = ReadBoolAttribute(configNode, "isBodyHtml", false);
			_sendEmail = ReadBoolAttribute(configNode, "sendEmail", true);
		}

		private void LoadConfiguration()
		{
			try
			{
				XDocument xDoc = GetSettingsFile();
				XElement element = xDoc.Element("configuration").Element("MarketQADataProcessor");

				//ReadTestModeSettings(element);
				ReadDefaults(element);
				//ReadEmbeddedResources();
			}
			catch (Exception e)
			{
				LoggingHelper.LogError("Error during initialization! {0}", e.Message);
			}
		}

		private void formMain_Load(object sender, EventArgs e)
		{
            LoadConfiguration();

            InsertNumOfRows = 10;
            ProcessCommandLineArgs();
            numericInsertRows.Value = InsertNumOfRows;

            if (AutoRun)
            {
                buttonRun.PerformClick();
                Environment.Exit(0);          
            }
		}

		private void formMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (isExecuting)
			{
				MessageBox.Show(this, "Cannot close the form until the pending asynchronous command has completed. Please wait...");
				e.Cancel = true;
			}
		}

        private void ProcessCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                string[] kv = arg.Split(':');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "-numofrows":
                            if (kv.Length < 2)
                            {
                                continue;
                            }

                            int value;
                            if (int.TryParse(kv[1], out value))
                            {
                                InsertNumOfRows = value;
                            }
                            break;

                        case "-autorun":
                            AutoRun = true;
                            break;
                    }
                }
            }
        }
	}


	

	internal static class Global
	{
		internal static formMain FormMain;
	}
}
