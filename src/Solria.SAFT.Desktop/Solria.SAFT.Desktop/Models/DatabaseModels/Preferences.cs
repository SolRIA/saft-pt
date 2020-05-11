using Solria.SAFT.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Solria.SAFT.Desktop.Models.DatabaseModels
{
    public class Preferences
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static T Cast<T>(string value, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            
            object o;

            //the decimal is a special case, we must validate witch decimal point is used
            if (typeof(T) == typeof(decimal))
            {
                //remove the m specifier at the end of the string
                value = value.Replace("m", string.Empty);

                CultureInfo provider;
                //get the current culture of the input string
                if (value.Contains("."))
                    provider = CultureInfo.InvariantCulture;
                else
                    provider = new CultureInfo("pt-PT");

                o = decimal.Parse(value, provider);
            }
            else if (typeof(T) == typeof(int))
            {
                o = int.Parse(value);
            }
            else if (typeof(T) == typeof(bool))
            {
                if (value.Equals("1", StringComparison.OrdinalIgnoreCase))
                    o = true;
                else if (value.Equals("0", StringComparison.OrdinalIgnoreCase))
                    o = false;
                else
                    o = bool.Parse(value);
            }
            else
                o = Convert.ChangeType(value, typeof(T));

            return (T)o;
        }
    }
}
