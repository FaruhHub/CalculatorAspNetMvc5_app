using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CalculatorMvc5.Services
{
    public static class Extensions
    {
        private static object _lock = new object();

        /// <summary>
        /// Method that creates a list of objects from the given data table
        /// </summary>
        /// <typeparam name="T">parameter takes a reference type.</typeparam>        
        /// <param name="tbl">parameter takes a DataTable class object.</param>
        /// <returns>returns a list of T type items</returns>
        public static List<T> CreateListFromTable<T>(this DataTable tbl) where T : new()
        {
            // making the method thread safe for multithreading purposes
            lock (_lock)
            {
                // define return list
                List<T> lst = new List<T>();

                // go through each row
                foreach (DataRow r in tbl.Rows)
                {
                    // add to the list
                    lst.Add(CreateItemFromRow<T>(r));
                }

                // return the list
                return lst;
            }
            
        }

        /// <summary>
        /// Method that creates an object from the given data row
        /// </summary>
        /// The <paramref name="T"/> parameter takes a reference type.
        /// <param name="row">parameter takes a DataRow class object.</param>
        /// <returns>returns a new item of input T object</returns>
        public static T CreateItemFromRow<T>(this DataRow row) where T : new()
        {
            // making the method threadsafe for multithreading purposes
            lock (_lock)
            {
                // create a new object
                T item = new T();

                // set the item
                SetItemFromRow(item, row);

                // return 
                return item;
            }
        }

        private static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            try
            {
                // go through each column
                foreach (DataColumn c in row.Table.Columns)
                {
                    // find the property for the column
                    PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                    // if exists, set the value
                    if (p != null && row[c] != DBNull.Value)
                    {
                        p.SetValue(item, row[c], null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLog("Error occured in the mehod SetItemFromRow: "+ex.Message + "\nStackTrace: " + ex.StackTrace);
            }

        }
    }
}