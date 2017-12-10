using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public static class ConfigHelper
    {
        public static string Get(string key, string defaultValue = "")
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value;
        }

        public static NameValueCollection AppSettings
        {
            get { return ConfigurationManager.AppSettings; }
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return ConfigurationManager.ConnectionStrings; }
        }

        public static int GetInt32(string name)
        {
            return Convert.ToInt32(Get(name));
        }

        public static bool GetBool(string name, bool defaultIfNull = false)
        {
            return DataHelper.GetBool(Get(name), defaultIfNull);
        }

        //public static string Get2(string name)
        //{
        //    return ConfigurationManager.AppSettings[name];
        //}
    }
}
