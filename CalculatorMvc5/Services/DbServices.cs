using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CalculatorMvc5.Services
{
    public sealed class DbServices
    {
        // Member fields
        private static readonly DbServices m_instance = new DbServices();
        private static SqlConnection m_connection;
        private static SqlCommand m_command;
        private static SqlDataAdapter m_adapter;        
        private const string NAME_VALUE_PARAMETER = "=>";

        // Constructors
        private DbServices()
        {
            try
            {
                m_connection = new SqlConnection(ConnectionString());
                m_connection.Open();
                m_command = new SqlCommand("", m_connection);
                m_adapter = new SqlDataAdapter();
            }
            catch (Exception ex)
            {
                LogFile.WriteToLog("Opening DB connection. Database is not accessible, please check Database connection!\n" + ex.Message);
            }
        }

        public static string ConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ToString();
            return connectionString;
        }

        private static void log(String sql)
        {
            LogFile.WriteToLog("## Executing SQL: " + sql);
        }

        public static DbServices Instance
        {
            get
            {
                return m_instance;

                //after DB interaction this fields needs to be closed
                //adapter.Dispose();
                //command.Dispose();
                //connection.Close();
            }
        }
        
        
        public static Boolean ExecuteSQL(string sqlStatment, string srcTable, DataSet ds)
        {
            return ExecuteSQL(sqlStatment, srcTable, ds, true);
        }


        public static Boolean ExecuteSQL(string sqlStatment, string srcTable, DataSet ds, bool showMessage)
        {
            lock (m_adapter)
            {
                //Remove last statement
                if (sqlStatment.EndsWith(";"))
                    sqlStatment = sqlStatment.Substring(0, sqlStatment.Length - 1);

                if (m_adapter == null)
                    return false;
                DbServices.m_adapter.SelectCommand = DbServices.m_command;
                DbServices.m_adapter.SelectCommand.CommandText = sqlStatment;
                DbServices.m_adapter.SelectCommand.CommandTimeout = Int32.MaxValue;
                log(sqlStatment);
                try
                {
                    //DbServices.adapter.AcceptChangesDuringFill = true;
                    DbServices.m_adapter.Fill(ds, srcTable);
                    return true;
                }
                catch (Exception ex)
                {
                    LogFile.WriteToLog("Error executing sql : " + sqlStatment + ", error: " + ex.Message);
                    return false;
                }
            }
        }
        

        public static DataSet ExtractData(string SqlStatement)
        {
            if (m_adapter == null)
                return null;
            DbServices.m_adapter.SelectCommand = DbServices.m_command;
            log(SqlStatement);
            DbServices.m_adapter.SelectCommand.CommandText = SqlStatement;
            DataSet ds = new DataSet();
            DbServices.m_adapter.Fill(ds);
            return ds;
        }

        public static Boolean ExecuteUpdateSQL(string sqlStatement, bool showError = true)
        {
            if (m_adapter == null) return false;
            DbServices.m_adapter.UpdateCommand = DbServices.m_command;
            DbServices.m_adapter.UpdateCommand.CommandText = sqlStatement;
            try
            {
                log(sqlStatement);
                DbServices.m_adapter.UpdateCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running SQL: " + sqlStatement + " ,Error: " + ex.Message);
                return false;
            }
        }

        public static Boolean ExecuteSqlWithBeginTransaction(string sqlStatement, bool showError = true)
        {
            if (m_adapter == null) return false;
            SqlCommand cmd = null;
            try
            {
                log(sqlStatement);
                cmd = new SqlCommand(sqlStatement, m_connection);
                if (cmd.Transaction == null)
                    cmd.Transaction = cmd.Connection.BeginTransaction();
                cmd.ExecuteNonQuery();
                cmd.Transaction.Commit();
                cmd.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running SQL: " + sqlStatement + " ,Error: " + ex.Message);
                return false;
            }
        }

        public static Boolean InsertData(string sqlStatement)
        {
            SqlCommand cmd = null;
            try
            {
                log(sqlStatement);
                cmd = new SqlCommand(sqlStatement, m_connection);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                log("Error running specified sql:\n" + sqlStatement + "\nError Message: " + ex.Message);
                return false;
            }
        }

        public static Boolean CreateTable(string sqlStatement)
        {
            if (m_adapter == null)
                return false;

            SqlCommand cmd = null;

            try
            {
                log(sqlStatement);
                cmd = new SqlCommand(sqlStatement, m_connection);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                log("Error running specified sql:\n" + sqlStatement + "\nError Message: " + ex.Message);
            }
            return false;
        }

        public static Boolean ExecuteBatch(string sql, bool showError = false)
        {
            SqlStatments st = new SqlStatments();
            string[] sqls = SqlStatments.ParseSQLtoSubSQLs(sql);
            int i, size = sqls != null ? sqls.Length : 0;
            string temp;
            bool success = true;
            bool executeWithParameters = false;

            for (i = 0; i < size; i++)
            {
                temp = sqls[i].Trim();
                if (temp.Length > 0)
                {
                    // do not execute comments
                    if (temp.StartsWith("--"))
                    {
                        log("Not running: " + temp);
                        continue;
                    }
                    if (temp.ToLower().StartsWith("exec"))
                    {
                        if (temp.IndexOf(NAME_VALUE_PARAMETER) >= 0 || executeWithParameters)
                        {
                            success &= ExecuteStoredProcedureWithParameters(temp);
                            executeWithParameters = true;
                        }
                        else
                            success &= ExecuteStoreProcedure(temp, showError);
                    }
                    else
                        success &= ExecuteUpdateSQL(temp, showError);
                }
            }

            return success;
        }

        // run a stored procedure that takes a parameter
        public static Boolean ExecuteStoredProcedureWithParameters(string storedProcedure)
        {
            String originalName = storedProcedure;
            SqlCommand command;
            SqlParameter SqlParameter;
            string[] parameters;
            // parameter name and value
            string[] temp;
            string name, value;

            try
            {
                storedProcedure = storedProcedure.Replace("exec", "");
                storedProcedure = storedProcedure.Replace("'", "");
                storedProcedure = storedProcedure.Replace(")", "");
                parameters = storedProcedure.Split('(', ',');

                command = m_connection.CreateCommand();
                command.CommandText = parameters[0];
                command.CommandType = CommandType.StoredProcedure;

                log("----------");
                log("Executing stored procedure with parameters: " + originalName);
                log("Procedure name: " + parameters[0]);
                log("Parameters: " + (parameters.Length - 1));

                for (int i = 1; i < parameters.Length; i++)
                {
                    temp = parameters[i].Split(new string[] { NAME_VALUE_PARAMETER }, StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length != 2)
                    {
                        log("Invalid parameter, must be in the format name=>value , parameter: " + parameters[i]);
                        return false;
                    }

                    SqlParameter = new SqlParameter();
                    name = temp[0].Trim();
                    value = temp[1].Trim();

                    if (value != null && value.ToLower().Equals("null"))
                        value = null;

                    SqlParameter.ParameterName = name;
                    SqlParameter.Value = value;

                    log("Parameter name: " + name + " , value: " + value);

                    command.Parameters.Add(SqlParameter);
                }

                command.ExecuteNonQuery();

                command.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                log("Error executing SQL: " + ex.Message + " , SQL : " + storedProcedure);
                return false;
            }
        }

        public static Boolean ExecuteStoredProcedureWithParameters(string storedProcedure, string columnName, string tableName)
        {
            SqlCommand command;

            try
            {
                command = m_connection.CreateCommand();
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter inputParam1 = new SqlParameter
                {
                    ParameterName = "columnName",
                    Value = columnName,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(inputParam1);
                SqlParameter inputParam2 = new SqlParameter
                {
                    ParameterName = "tableName",
                    Value = tableName,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(inputParam2);

                SqlParameter returnParam = new SqlParameter
                {
                    ParameterName = "isExist",
                    Direction = ParameterDirection.Output,
                    Size = 1
                };
                command.Parameters.Add(returnParam);

                command.ExecuteNonQuery();

                //command.Parameters["isExist"].Direction = ParameterDirection.Output;
                string returnValue = (string)command.Parameters["isExist"].Value;
                command.Dispose();

                if (returnValue == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log("Error running specified store procedure: " + storedProcedure + "\nError Message: " + ex.Message);
                return false;
            }
        }

        public static Boolean ExecuteStoreProcedure(string storeProcedure, bool showError = true)
        {
            storeProcedure = storeProcedure.ToLower().Replace("exec", "");
            //storeProcedure = storeProcedure.Replace("'", "");
            storeProcedure = storeProcedure.Replace(")", "");
            string[] storeProcedureStrings = storeProcedure.Split('(', ',');

            try
            {
                log("----------");
                log("Stored Procedure: " + storeProcedureStrings[0]);
                log("Parameters: " + (storeProcedureStrings.Length - 1));
                SqlDataAdapter adapter = new SqlDataAdapter(storeProcedureStrings[0], m_connection);
                adapter.UpdateCommand = new SqlCommand(storeProcedureStrings[0], m_connection);
                adapter.UpdateCommand.CommandText = storeProcedureStrings[0];
                adapter.UpdateCommand.CommandType = CommandType.StoredProcedure;
                String param;

                // derive parameters only when the procure has them
                if (storeProcedureStrings.Length > 1)
                    SqlCommandBuilder.DeriveParameters(adapter.UpdateCommand);

                for (int i = 0; i < adapter.UpdateCommand.Parameters.Count; i++)
                {
                    if (i + 1 <= storeProcedureStrings.Length - 1)
                        param = storeProcedureStrings[i + 1];
                    else
                        param = null;
                    log("Parameter: " + param);

                    if (param != null && param.ToLower() != "null")
                        adapter.UpdateCommand.Parameters[i].Value = param;
                    else
                        adapter.UpdateCommand.Parameters[i].Value = null;
                }

                adapter.UpdateCommand.ExecuteNonQuery();
                adapter.Dispose();
                log("----------");
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running SQL: " + ex.Message + " SQL:" + storeProcedure);
                return false;
            }
        }

        /// <summary>
        /// Selects specified data from DataBase
        /// </summary>
        /// <param name="storeProcedure">calling StoredProcedure name</param>
        /// <param name="spParams">StoredProcedure parameters</param>
        /// <param name="srcTable">specify a table name for your retrieved data</param>
        /// <param name="ds">DataSet container instance for retrieved data</param>
        /// <returns>Will return True if successful otherwice False</returns>
        public static Boolean ExecuteStoreProcedure(string storeProcedure, string[] spParams, string srcTable, DataSet ds)
        {
            storeProcedure = storeProcedure.ToLower().Replace("exec", "");
            string[] storeProcedureStrings = spParams;

            try
            {
                log("----------");
                log("Stored Procedure: " + storeProcedure);
                log("Parameters: " + (storeProcedureStrings.Length));
                SqlDataAdapter adapter = new SqlDataAdapter(storeProcedure, m_connection);
                adapter.SelectCommand = new SqlCommand(storeProcedure, m_connection);
                adapter.SelectCommand.CommandText = storeProcedure;   // procedure name
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                String param;

                // derive parameters only when the procedure has them
                if (storeProcedureStrings.Length > 0)
                {
                    SqlCommandBuilder.DeriveParameters(adapter.SelectCommand);
                    if (adapter.SelectCommand.Parameters.Count > 0)
                        adapter.SelectCommand.Parameters.RemoveAt(0);   //Will be removed Parameter: "@RETURN_VALUE"
                }

                for (int i = 0; i < adapter.SelectCommand.Parameters.Count; i++)
                {
                    if (i <= storeProcedureStrings.Length - 1)
                        param = storeProcedureStrings[i];
                    else
                        param = null;
                    log("Parameter: " + param);

                    if (param != null && param.ToLower() != "null")
                        adapter.SelectCommand.Parameters[i].Value = param;
                    else
                        adapter.SelectCommand.Parameters[i].Value = null;
                }

                adapter.Fill(ds, srcTable);
                adapter.Dispose();
                log("----------");
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running StoredProcedure: " + storeProcedure + " ErrorMessage: " + ex.Message);
                return false;
            }            

        }

        /// <summary>
        /// Inserts specified data to DataBase
        /// </summary>
        /// <param name="storeProcedure">calling StoredProcedure name</param>
        /// <param name="spParams">StoredProcedure parameters</param>
        /// <returns>Will return True if successful otherwice False</returns>
        public static Boolean ExecuteInsertStoredProcedure(string storeProcedure, string[] spParams)
        {
            storeProcedure = storeProcedure.ToLower().Replace("exec", "");
            string[] storeProcedureStrings = spParams;

            try
            {
                log("----------");
                log("Stored Procedure: " + storeProcedure);
                log("Parameters: " + (storeProcedureStrings.Length));
                SqlDataAdapter adapter = new SqlDataAdapter(storeProcedure, m_connection);
                adapter.InsertCommand = new SqlCommand(storeProcedure, m_connection);
                adapter.InsertCommand.CommandText = storeProcedure;   // procedure name
                adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                String param;

                // derive parameters only when the procedure has them
                if (storeProcedureStrings.Length > 0)
                {
                    SqlCommandBuilder.DeriveParameters(adapter.InsertCommand);
                    if (adapter.InsertCommand.Parameters.Count > 0)
                        adapter.InsertCommand.Parameters.RemoveAt(0);   //Will be removed Parameter: "@RETURN_VALUE"
                }

                for (int i = 0; i < adapter.InsertCommand.Parameters.Count; i++)
                {
                    if (i <= storeProcedureStrings.Length - 1)
                        param = storeProcedureStrings[i]; 
                    else
                        param = null;
                    log("Parameter: " + param);

                    if (param != null && param.ToLower() != "null")
                        adapter.InsertCommand.Parameters[i].Value = param;
                    else
                        adapter.InsertCommand.Parameters[i].Value = null;
                }

                adapter.InsertCommand.ExecuteNonQuery();
                adapter.Dispose();
                log("----------");
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running StoredProcedure: " + storeProcedure + " ErrorMessage: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes specified data from DataBase
        /// </summary>
        /// <param name="storeProcedure">calling StoredProcedure name</param>
        /// <param name="spParams"></param>
        /// <returns>Will return True if successful otherwice False</returns>
        public static Boolean ExecuteDeleteStoredProcedure(string storeProcedure, string[] spParams)
        {
            storeProcedure = storeProcedure.ToLower().Replace("exec", "");
            string[] storeProcedureStrings = spParams;

            try
            {
                log("----------");
                log("Stored Procedure: " + storeProcedure);
                log("Parameters: " + (storeProcedureStrings.Length));
                SqlDataAdapter adapter = new SqlDataAdapter(storeProcedure, m_connection);
                adapter.DeleteCommand = new SqlCommand(storeProcedure, m_connection);
                adapter.DeleteCommand.CommandText = storeProcedure;   // procedure name
                adapter.DeleteCommand.CommandType = CommandType.StoredProcedure;
                String param;

                // derive parameters only when the procedure has them
                if (storeProcedureStrings.Length > 0)
                {
                    SqlCommandBuilder.DeriveParameters(adapter.DeleteCommand);
                    if (adapter.DeleteCommand.Parameters.Count > 0)
                        adapter.DeleteCommand.Parameters.RemoveAt(0);   //Will be removed Parameter: "@RETURN_VALUE"
                }

                for (int i = 0; i < adapter.DeleteCommand.Parameters.Count; i++)
                {
                    if (i <= storeProcedureStrings.Length - 1)
                        param = storeProcedureStrings[i];
                    else
                        param = null;
                    log("Parameter: " + param);

                    if (param != null && param.ToLower() != "null")
                        adapter.DeleteCommand.Parameters[i].Value = param;
                    else
                        adapter.DeleteCommand.Parameters[i].Value = null;
                }

                adapter.DeleteCommand.ExecuteNonQuery();
                adapter.Dispose();
                log("----------");
                return true;
            }
            catch (Exception ex)
            {
                log("ERROR running StoredProcedure: " + storeProcedure + " ErrorMessage: " + ex.Message);
                return false;
            }
        }


        public static Boolean ExecuteScript(string storeProcedures)
        {
            SqlDataAdapter adapter;
            string[] statements = storeProcedures.Split(';');


            foreach (string statement in statements)
            {
                if (!string.IsNullOrEmpty(statement))
                {
                    string prefix = statement.TrimStart().ToLower();
                    //if statement is store procedure- call store procedure method                  
                    if (prefix.StartsWith("exec"))
                    {
                        bool isSuccess = ExecuteStoreProcedure(statement);
                        if (!isSuccess)
                            return false;
                        continue;
                    }
                    if (prefix.StartsWith("begin"))
                    {
                        bool isSuccess = ExecuteUpdateSQL(statement);
                        if (!isSuccess)
                            return false;
                        continue;
                    }
                    else
                    {
                        adapter = new SqlDataAdapter(statement, m_connection);
                        adapter.SelectCommand = new SqlCommand(statement, m_connection);
                        adapter.SelectCommand.CommandType = CommandType.Text;
                        log(statement);
                    }


                    try
                    {
                        adapter.SelectCommand.ExecuteNonQuery();
                        adapter.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log("Error running specified store procedure: " + statement + "\nError Message: " + ex.Message);
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValidSQL(string sql, out string message)
        {
            DataSet ds = new DataSet();
            message = string.Empty;

            if (m_adapter == null)
                return false;

            string[] sqls = SqlStatments.ParseSQLtoSubSQLs(sql);

            foreach (string statement in sqls)
            {
                if (!string.IsNullOrEmpty(statement))
                {
                    sql = "EXPLAIN PLAN  FOR " + statement;

                    DbServices.m_adapter.SelectCommand = DbServices.m_command;
                    DbServices.m_adapter.SelectCommand.CommandText = sql;


                    try
                    {
                        DbServices.m_adapter.Fill(ds, "table");

                    }
                    catch (SqlException ex)
                    {
                        message = ex.Message;

                        return false;
                    }
                }
            }
            return true;
        }

        public static T GetLastInsertedDataFromTable<T>(string tableName, string id = null) where T : new()
        {
            string sql = string.Format("SELECT TOP(1) * FROM {0} ORDER BY {1} DESC", tableName, id ?? "0");
            DataSet ds = new DataSet();
            DataRow dr;
            try
            {
                DbServices.ExecuteSQL(sql, tableName, ds);
                if (ds.Tables[tableName] != null && ds.Tables[tableName].Rows.Count > 0)
                {
                    dr = ds.Tables[tableName].Rows[0];
                    T item = dr.CreateItemFromRow<T>();
                    return item;
                }
                else
                    return default(T);
            }
            catch (Exception ex)
            {
                log(ex.Message + "\nError path: " + ex.StackTrace);
                return default(T);
            }
            
        }

        

    }
}