using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CalculatorMvc5.Services
{
    public class SqlStatments
    {
        public static string[] ParseSQLtoSubSQLs(string SQLStatement)
        {
            Regex regex = new Regex(";");
            return regex.Split(SQLStatement);
        }
    }
}