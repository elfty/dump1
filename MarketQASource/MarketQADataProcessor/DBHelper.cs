using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace MarketQADataProcessor
{
	internal class DBHelper
	{
		private string connectionString = "Integrated Security=false;Data Source=VOLARBDB;Initial Catalog=Ivy;Asynchronous Processing=true;User ID=volarb;Password=el3phant";

		private const int NdxID = 102480;
		private const int RutID = 102434;
		private const int SpxID = 108105;


		private SqlCommand insertCommand;
		private SqlCommand insertCommand2;
		private SqlConnection connection;

		SqlParameter[] _insertSqlParameters;
		string _insertSqlText;

		private Dictionary<DateTime, int> _dateToRowID = new Dictionary<DateTime, int>();


		private SqlParameter[] GetInsertSqlParameters()
		{
			if (_insertSqlParameters == null)
			{
				_insertSqlParameters = new SqlParameter[]
			                            	{
			                            		new SqlParameter("@dt_bus", SqlDbType.DateTime),
			                            		new SqlParameter("@id_imnt", SqlDbType.Real),
			                            		new SqlParameter("@am_chg", SqlDbType.Real),
			                            		new SqlParameter("@am_rtn_1m", SqlDbType.Real),
			                            		new SqlParameter("@am_rtn_3m", SqlDbType.Real),
			                            		new SqlParameter("@am_rtn_6m", SqlDbType.Real),
			                            		new SqlParameter("@am_rtn_1y", SqlDbType.Real),
			                            		new SqlParameter("@am_range", SqlDbType.Real),
			                            		new SqlParameter("@am_range_5D", SqlDbType.Real),
			                            		new SqlParameter("@am_range_10D", SqlDbType.Real),
			                            		new SqlParameter("@am_range_21D", SqlDbType.Real),
			                            		new SqlParameter("@am_range_63D", SqlDbType.Real),
			                            		new SqlParameter("@am_range_126D", SqlDbType.Real),
			                            		new SqlParameter("@am_range_252D", SqlDbType.Real),
			                            		new SqlParameter("@am_var", SqlDbType.Real),
			                            		new SqlParameter("@am_corr", SqlDbType.Real),
			                            		new SqlParameter("@am_beta", SqlDbType.Real),
			                            		new SqlParameter("@am_cov", SqlDbType.Real),
			                            		new SqlParameter("@am_corr_ndx", SqlDbType.Real),
			                            		new SqlParameter("@am_corr_rty", SqlDbType.Real),
			                            		new SqlParameter("@am_mkt_cap", SqlDbType.Real)
			                            	};
			}

			return _insertSqlParameters;
		}

		private SqlParameter[] GetInsertSqlParameters2()
		{
			SqlParameter[] sqlParameters = new SqlParameter[]
			                               	{
			                               		new SqlParameter("@dt_bus", SqlDbType.DateTime),
			                               		new SqlParameter("@id_imnt", SqlDbType.Real),                                                
			                               		new SqlParameter("@am_vol_1m", SqlDbType.Real),
			                               		new SqlParameter("@am_vol_3m", SqlDbType.Real),
			                               		new SqlParameter("@am_vol_6m", SqlDbType.Real),
			                               		new SqlParameter("@am_vol_1y", SqlDbType.Real),
			                               		new SqlParameter("@am_vol_2y", SqlDbType.Real),
			                               		new SqlParameter("@am_vol_3y", SqlDbType.Real),
                                                new SqlParameter("@am_vol_5d", SqlDbType.Real),
                                                new SqlParameter("@am_vol_10d", SqlDbType.Real)
			                               	};

			return sqlParameters;
		}

		internal void InitializeInsertSqlCommand(string db, string schema, string table)
		{
			InitializeInsertSqlCommand(GetInsertSqlText(db, schema, table));
		}

		private void InitializeInsertSqlCommand(string sqlText)
		{
			if (connection == null)
			{
				connection = new SqlConnection(connectionString);
			}

			if (insertCommand == null)
			{
				insertCommand = new SqlCommand(sqlText, connection);

				SqlParameterCollection parameters = insertCommand.Parameters;
				parameters.AddRange(GetInsertSqlParameters());

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				insertCommand.Prepare();
			}
		}

		#region HACK quik workaround. Clean up later

		// HACK quik workaround. Clean up later
		internal void InitializeInsertSqlCommand2(string db, string schema, string table)
		{
			InitializeInsertSqlCommand2(string.Format("INSERT INTO {0}.{1}.{2} (dt_bus,id_imnt,am_vol_1m,am_vol_3m,am_vol_6m,am_vol_1y,am_vol_2y,am_vol_3y,am_vol_5d,am_vol_10d) VALUES (@dt_bus,@id_imnt,@am_vol_1m,@am_vol_3m,@am_vol_6m,@am_vol_1y,@am_vol_2y,@am_vol_3y,@am_vol_5d,@am_vol_10d)", db, schema, table));
		}
		private void InitializeInsertSqlCommand2(string sqlText)
		{
			if (connection == null)
			{
				connection = new SqlConnection(connectionString);
			}

			if (insertCommand2 == null)
			{
				insertCommand2 = new SqlCommand(sqlText, connection);

				SqlParameterCollection parameters = insertCommand2.Parameters;
				parameters.AddRange(GetInsertSqlParameters2());

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				insertCommand2.Prepare();
			}
		}

		#endregion


		internal void InsertIntoDB(int numOfRows, SecurityCalcedValues securityCalcedValues)
		{
			SqlParameterCollection parameters = insertCommand.Parameters;
			int securityIvyID = securityCalcedValues.SecurityNonIvyID;
			ResultsArrays results = securityCalcedValues.Results;


			for (int index = securityCalcedValues.Values.Count - numOfRows; index < securityCalcedValues.Values.Count; index++)
			{
				var calcedValues = securityCalcedValues.Values[index];
				int i = 0;
				parameters[i++].Value = calcedValues.Date;
                if (securityIvyID == 16862)
                {
                    securityIvyID = 16862;
                }

				parameters[i++].Value = securityIvyID;
				parameters[i++].Value = calcedValues.am_chg;
				parameters[i++].Value = double.IsNaN(calcedValues.am_rtn_1m) ? 0 : calcedValues.am_rtn_1m;
				parameters[i++].Value = double.IsNaN(calcedValues.am_rtn_3m) ? 0 : calcedValues.am_rtn_3m;
				parameters[i++].Value = double.IsNaN(calcedValues.am_rtn_6m) ? 0 : calcedValues.am_rtn_6m;
				parameters[i++].Value = double.IsNaN(calcedValues.am_rtn_1y) ? 0 : calcedValues.am_rtn_1y;
				parameters[i++].Value = calcedValues.am_range;
				parameters[i++].Value = calcedValues.am_range_5D;
				parameters[i++].Value = calcedValues.am_range_10D;
				parameters[i++].Value = calcedValues.am_range_1m;
				parameters[i++].Value = calcedValues.am_range_3m;
				parameters[i++].Value = calcedValues.am_range_6m;
				parameters[i++].Value = double.IsNaN(calcedValues.am_range_1y) ? 0 : calcedValues.am_range_1y;
				//parameters[i++].Value = double.IsNaN(results.Variance12Months_am_chg[index]) ? 0 : results.Variance12Months_am_chg[index];
                if (double.IsNaN(results.Variance12Months_am_chg[index]))
                {
                    parameters[i++].Value = 0;
                }
                else
                {
                    parameters[i++].Value = Math.Round(results.Variance12Months_am_chg[index],8);
                }				
                parameters[i++].Value = double.IsNaN(calcedValues.am_corr) ? 0 : calcedValues.am_corr;
				parameters[i++].Value = double.IsNaN(calcedValues.am_beta) ? 0 : calcedValues.am_beta;
                if (double.IsNaN(calcedValues.am_cov_sp))
                {
                    parameters[i++].Value = 0;
                }
                else
                {
                    parameters[i++].Value = Math.Round(calcedValues.am_cov_sp, 8);
                }

				//parameters[i++].Value = double.IsNaN(calcedValues.am_cov_sp) ? 0 : calcedValues.am_cov_sp;
				parameters[i++].Value = double.IsNaN(calcedValues.am_corr_ndx) ? 0 : calcedValues.am_corr_ndx;
				parameters[i++].Value = double.IsNaN(calcedValues.am_corr_rty) ? 0 : calcedValues.am_corr_rty;
				parameters[i++].Value = calcedValues.am_mkt_cap;
                
                //parameters[17].Value = 0;
				int rows = insertCommand.ExecuteNonQuery();
			}
		}

		internal void InsertIntoDB2(int numOfRows, SecurityCalcedValues securityCalcedValues)
		{
			SqlParameterCollection parameters = insertCommand2.Parameters;
			int securityIvyID = securityCalcedValues.SecurityNonIvyID;
			ResultsArrays results = securityCalcedValues.Results;


			for (int index = securityCalcedValues.Values.Count - numOfRows; index < securityCalcedValues.Values.Count; index++)
			{
				var calcedValues = securityCalcedValues.Values[index];
				int i = 0;
				parameters[i++].Value = calcedValues.Date;
				parameters[i++].Value = securityIvyID;
				parameters[i++].Value = results.am_vol_21[index];
				parameters[i++].Value = results.am_vol_63[index];
				parameters[i++].Value = results.am_vol_126[index];
				parameters[i++].Value = results.am_vol_252[index];
				parameters[i++].Value = results.am_vol_504[index];
				parameters[i++].Value = results.am_vol_756[index];
                parameters[i++].Value = results.am_vol_5[index];
                parameters[i++].Value = results.am_vol_10[index];

				int rows = insertCommand2.ExecuteNonQuery();
			}
		}
				
		private string GetInsertSqlText(string db, string schema, string table)
		{
			if (_insertSqlText == null)
			{
				_insertSqlText = string.Format("INSERT INTO {0}.{1}.{2} (dt_bus,id_imnt,am_chg,am_rtn_1m,am_rtn_3m,am_rtn_6m,am_rtn_1y,am_range,am_range_5D,am_range_10D,am_range_21D,am_range_63D,am_range_126D,am_range_252D,am_var,am_corr,am_beta,am_cov,am_corr_ndx,am_corr_rty,am_mkt_cap) VALUES (@dt_bus,@id_imnt,@am_chg,@am_rtn_1m,@am_rtn_3m,@am_rtn_6m,@am_rtn_1y,@am_range,@am_range_5D,@am_range_10D,@am_range_21D,@am_range_63D,@am_range_126D,@am_range_252D,@am_var,@am_corr,@am_beta,@am_cov,@am_corr_ndx,@am_corr_rty,@am_mkt_cap)", db, schema, table);
			}
	
			return _insertSqlText;
		}

		internal List<IndexData> GetIndexData(DateTime dateFrom, DateTime dateTo, int numOfRows, ref string errorMsg)
		{
			_dateToRowID.Clear();
				
			var values = new List<IndexData>(numOfRows);
			for (int i = 0; i < numOfRows; i++)
			{
				values.Add(new IndexData());
			}

			string databaseName = "Ivy";
			string schema = "";
			string table = "SECURITY_PRICE";
			string columns = " Date, SecurityID, TotalReturn ";
			string sqlSelect = "SELECT " + columns;
			string sqlWhere = "WHERE SecurityID={0} AND Date BETWEEN '{1}' AND '{2}'";
			string sqlOrderBy = "ORDER BY Date";

			// TODO Parameterize the query
			var data = new List<IndexRawData>(numOfRows);
			string sqlText = BuildSelectQuery(sqlSelect, databaseName, schema, table, true, string.Format(sqlWhere, SpxID, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd")), sqlOrderBy);
            errorMsg = null;
            GetIndexData(sqlText, data, ref errorMsg);
			if (errorMsg != null)
			{
				return values;
			}

			for (int i = 0; i < data.Count; i++)
			{
				values[i].SnP = data[i];
			}

			for (int i = 0; i < values.Count; i++)
			{
				_dateToRowID.Add(values[i].SnP.Date, i);
			}


			sqlText = BuildSelectQuery(sqlSelect, databaseName, schema, table, true, string.Format(sqlWhere, NdxID, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd")), sqlOrderBy);

			data.Clear();
			GetIndexData(sqlText, data, ref errorMsg);
			if (errorMsg != null)
			{
				return values;
			}

			for (int i = 0; i < data.Count; i++)
			{
				values[i].Nasdaq = data[i];
			}

			data.Clear();
			sqlText = BuildSelectQuery(sqlSelect, databaseName, schema, table, true, string.Format(sqlWhere, RutID, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd")), sqlOrderBy);
			GetIndexData(sqlText, data, ref errorMsg);
			if (errorMsg != null)
			{
				return values;
			}

			for (int i = 0; i < data.Count; i++)
			{
				values[i].Russell = data[i];
			}

			return values;
		}

		private string BuildSelectQuery(string select, string databaseName, string schema, string table, bool useNolock, string where, string orderBy)
		{
			// TODO Parameterize the query
			string sqlText = string.Format("{0} FROM {1}.{2}.{3} {4} {5} {6}"
										   , select
										   , databaseName
										   , schema
										   , table
										   , ((useNolock) ? "(NOLOCK)" : "")
										   , where
										   , orderBy);

			return sqlText;
		}


		internal void GetIndexData(string sqlText, List<IndexRawData> dataList, ref string errorMsg)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(sqlText, conn))
				{
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{

						while (reader.Read())
						{
							object[] data = new object[reader.FieldCount];
							reader.GetValues(data);
							dataList.Add(new IndexRawData(data));
						}
					}
				}
			}
			catch (Exception exception)
			{
				errorMsg = "Exception: " + exception.Message;
			}
		}

		internal DateTime GetFirstDate(string db, string schema, string table, int numOfRows, int securityID, ref string errorMsg)
		{
			try
			{
				string cmdText = string.Format("SELECT TOP 1 Date FROM (SELECT TOP " + numOfRows + " Date FROM {0}.{1}.{2} WHERE SecurityID={3} ORDER BY Date DESC) T ORDER BY Date", db, schema, table, securityID);

				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(cmdText, conn))
				{
					conn.Open();

					object obj = cmd.ExecuteScalar();
					return Convert.ToDateTime(obj);
				}
			}
			catch (Exception exception)
			{
				errorMsg = "Exception: " + exception.Message;
			}

			return DateTime.MinValue;
		}

		internal DateTime GetLastDate(string db, string schema, string table, int securityID, ref string errorMsg)
		{
			try
			{
				string cmdText = string.Format("SELECT TOP 1 Date FROM {0}.{1}.{2} WHERE SecurityID={3} ORDER BY Date DESC", db, schema, table, securityID);

				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(cmdText, conn))
				{
					conn.Open();

					object obj = cmd.ExecuteScalar();
					return Convert.ToDateTime(obj);
				}
			}
			catch (Exception exception)
			{
				errorMsg = "Exception: " + exception.Message;
			}

			return DateTime.MinValue;
		}

		internal int GetNumOfRows(string db, string schema, string table, int securityID, DateTime dateFrom, DateTime dateTo, ref string errorMsg)
		{
			int numOfRows = -1;
			try
			{
				string cmdText = string.Format("SELECT COUNT(*) FROM {0}.{1}.{2} WHERE SecurityID={3} AND Date BETWEEN '{4}' AND '{5}'", db, schema, table, securityID, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"));

				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(cmdText, conn))
				{
					conn.Open();

					object obj = cmd.ExecuteScalar();
					numOfRows = Convert.ToInt32(obj);
				}
			}
			catch (Exception exception)
			{
				errorMsg = "Exception: " + exception.Message;
			}

			return numOfRows;
		}

        internal int GetIvySecurityID(string db, string schema, string table, int id_imnt, ref string errorMsg)
        {
            int id_imnt_ivy = -1;
            try
            {
                string cmdText = string.Format("SELECT id_imnt_ivy FROM {0}.{1}.{2} WHERE id_imnt={3}", db, schema, table, id_imnt);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

                    object obj = cmd.ExecuteScalar();
                    id_imnt_ivy = Convert.ToInt32(obj);
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }

            return id_imnt_ivy;
        }

        internal double GetADRRatio(string db, string schema, string table, int id_imnt, ref string errorMsg)
        {
            int am_adr_ratio = -1;
            try
            {
                string cmdText = string.Format("SELECT am_adr_ratio FROM {0}.{1}.{2} WHERE id_imnt={3}", db, schema, table, id_imnt);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    conn.Open();

                    object obj = cmd.ExecuteScalar();
                    am_adr_ratio = Convert.ToInt32(obj);
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Exception: " + exception.Message;
            }

            return am_adr_ratio;
        }

		internal SecurityCalcedValues GetSecurityData(string db, string schema, string table, bool useNolock, int securityID, DateTime dateFrom, DateTime dateTo, int numOfRows, ref string errorMsg)
		{
			var values = new SecurityCalcedValues(securityID, numOfRows);

			try
			{
				// TODO Parameterize the query
				string cmdText = BuildSelectQuery("SELECT * ", db, schema, table, useNolock, "WHERE SecurityID=" + securityID + " AND Date BETWEEN '" + dateFrom + "' AND '" + dateTo + "' ", "ORDER BY Date");

				// Retrieve data
				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(cmdText, conn))
				{
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{

						int rowCount = 0;
						while (reader.Read())
						{
							var row = new object[reader.FieldCount];
							reader.GetValues(row);

							SecurityData data = new SecurityData(row);
							int pos = _dateToRowID[data.Date];	// S&P entries must be present for all days: dependencies in other places
							values.Add(pos, new CalcedValues(data));

							rowCount++;
						}
					}
				}
			}
			catch (Exception exception)
			{
				errorMsg = "Exception: " + exception.Message;
			}

			return values;
		}

		private List<ColumnInfo> GetColumnNames(string tableName)
		{
			string cmdText = "SELECT TOP 1 * FROM Ivy.." + tableName + " (NOLOCK)";
			List<ColumnInfo> columnInfos = null;

			try
			{
				using (SqlConnection conn = new SqlConnection(connectionString))
				using (SqlCommand cmd = new SqlCommand(cmdText, conn))
				{
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
					{
						DataTable table = reader.GetSchemaTable();

						var rows = table.Rows;

						columnInfos = new List<ColumnInfo>(rows.Count);
						StringBuilder sb = new StringBuilder(rows.Count);
						for (int j = 0; j < table.Rows.Count; j++)
						{
							DataRow row = table.Rows[j];
							sb.Append(row[table.Columns["ColumnName"]] + " : " + row[table.Columns["DataType"]]);
						}

						//textBox.Text = sb.ToString();
						//listboxMemo.Items.Add(textBox.Text);

						//For each field in the table...
						foreach (DataRow myField in table.Rows)
						{


							int i = 0;
							//For each property of the field...
							foreach (DataColumn myProperty in table.Columns)
							{



								//Display the field name and value.
								if (i == 0)
								{
									//baseFormControl1.AddMemoText(myProperty.ColumnName + " = " + myField[myProperty].ToString());
								}
								else
								{
									//baseFormControl1.AddMemoText("\t" + myProperty.ColumnName + " = " + myField[myProperty].ToString());
								}
								i++;
							}
							//baseFormControl1.AddMemoText("");
						}

						foreach (DataRow field in rows)
						{
							Console.WriteLine(field);
						}

						foreach (DataColumn column in table.Columns)
						{
							columnInfos.Add(new ColumnInfo() { Name = column.ColumnName, Type = column.DataType });
						}
					}
				}
			}
			catch (Exception e)
			{
				Trace.Write(e);
				throw;
			}

			return columnInfos;
		}

		internal void ClearTable(string db, string schema, string table)
		{
			string sqlText = string.Format("TRUNCATE TABLE {0}.{1}.{2}", db, schema, table);

			using (SqlConnection conn = new SqlConnection(connectionString))
			using (SqlCommand cmd = new SqlCommand(sqlText, conn))
			{
				conn.Open();

				cmd.ExecuteNonQuery();
			}
		}

		internal Dictionary<int, int> GetSecurities(string db, string schema, string table)
		{
            string sqlText = string.Format("SELECT id_imnt_ivy, id_imnt FROM {0}.{1}.{2} WHERE id_typ_imnt in ('IND', 'STK') and id_del <> 1 AND id_imnt_ivy IS NOT NULL AND id_imnt_ivy <> 0 ORDER By id_imnt_ivy", db, schema, table);
            //string sqlText = string.Format("SELECT id_imnt_ivy, id_imnt FROM {0}.{1}.{2} WHERE id_typ_imnt in ('IND', 'STK') and id_del = 2 AND id_imnt_ivy IS NOT NULL AND id_imnt_ivy <> 0  ORDER By id_imnt_ivy", db, schema, table);
			Dictionary<int, int> securities = new Dictionary<int, int>();

			using (SqlConnection conn = new SqlConnection(connectionString))
			using (SqlCommand cmd = new SqlCommand(sqlText, conn))
			{
				conn.Open();

				using (SqlDataReader reader = cmd.ExecuteReader())
				{

					while (reader.Read())
					{
						securities.Add(reader.GetInt32(0), reader.GetInt32(1));
					}
				}
			}

			return securities;
		}
	}

	internal class ColumnInfo
	{
		public string Name;
		public Type Type;
	}
}
