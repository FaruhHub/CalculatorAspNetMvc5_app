using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace CalculatorMvc5.Services
{
    /// <summary>
    /// Provides the features for application development
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Gets the IP host address of the remote client.
        /// </summary>
        /// <returns>IP address</returns>
        public static string GetIP()
        {
            /* way № 1 */
            //string ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (string.IsNullOrEmpty(ipAddress))
            //{
            //    ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            //}

            /* way № 2 */
            string ip = HttpContext.Current.Request.UserHostAddress;//UserHostAddress - Gets the IP host address of the remote client.
            return ip;
        }

        /// <summary>
        /// Gets the IP host location information data deserialized from Json to specified T type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static T GetIPlocation<T>(string ipAddress) where T : new()
        {
            string url = string.Format("http://freegeoip.net/json/{0}", ipAddress);
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(url);
                T item = new JavaScriptSerializer().Deserialize<T>(json);
                return item;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the IP host location information data in Json format
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>Json location information</returns>
        public static string GetIPlocationJson(string ipAddress)
        {
            string url = string.Format("http://freegeoip.net/json/{0}", ipAddress);
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(url);
                return json;
            }
        }
    }
}