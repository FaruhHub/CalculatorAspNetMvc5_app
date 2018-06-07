using CalculatorMvc5.Models;
using CalculatorMvc5.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CalculatorMvc5.Repository
{
    public class CalculationsHistoryRepository
    {
        public List<CalculationsHistory> GetCalculationsHistoryByIPandCurrentDate(string clientIP)
        {
            DataSet ds = new DataSet();
            List<CalculationsHistory> calHistoryList = new List<CalculationsHistory>();

            DbServices.ExecuteStoreProcedure("sp_GetCalculationsHistoryByIPandCurrentDate", new string[] {clientIP }, "CalculationsHistory", ds);

            if (ds.Tables["CalculationsHistory"] != null && ds.Tables["CalculationsHistory"].Rows.Count > 0)
            {
                DataTable dt = ds.Tables["CalculationsHistory"];
                calHistoryList = dt.CreateListFromTable<CalculationsHistory>();
            }
            return calHistoryList;
        }

        public bool InsertCalculatedExpression(string clientIP, string expression)
        {
            try
            {
                DbServices.ExecuteInsertStoredProcedure("sp_InsertCalculatedExpressionByIP", new string[] { clientIP, expression });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }          

        }

        public bool DeleteCalculatedExpression(string Id)
        {
            try
            {
                DbServices.ExecuteDeleteStoredProcedure("sp_DeleteCalculatedExpression", new string[] { Id });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public T GetLastInsertedRow<T>(string tableName, string id = null) where T : new()
        {
            T item = new T();
            try
            {
                item = DbServices.GetLastInsertedDataFromTable<T>(tableName, id);
                return item;
            }
            catch (Exception ex)
            {
                return item;
            }

        }
    }
}