using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVConnector.Plugin
{
    /// <summary>
    /// Paraméter adatait tartalmazó osztály
    /// </summary>
    internal class Parameter
    {
        /// <summary>
        /// Pozició
        /// </summary>
        public int No { get; set; } 

        /// <summary>
        /// Név
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Érték
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Visszadja a paraméter nevet a bejövő stringből 
        ///     van név kezelés: név=érték formátum --> return név
        ///     nincs névkezelés: érték formátum --> return null
        /// </summary>
        /// <param name="inputStr">ebből a stringből szedi ki</param>
        /// <param name="msgFormat">jelzi, hogy van-e név kezelés</param>
        /// <returns>a paraméter neve</returns>
        public static string GetName(string inputStr, MessageFormat msgFormat)
        {
            if (msgFormat == MessageFormat.Positional)
            {
                return null;
            }
            else
            {
                string[] nameValuePair = inputStr.Split("=".ToCharArray());
                if (nameValuePair.Length != 2)
                {
                    throw new Exception($"Bad name-value pair format: {inputStr}");
                }
                return nameValuePair[0];
            }
        }

        /// <summary>
        /// Visszadja a paraméter értéket a bejövő stringből 
        ///     van név kezelés: név=érték formátum --> return érték
        ///     nincs névkezelés: érték formátum --> return inputString
        /// </summary>
        /// <param name="inputStr">ebből a stringből szedi ki</param>
        /// <param name="msgFormat">jelzi, hogy van-e név kezelés</param>
        /// <returns>a paraméter értéke</returns>
        public static string GetValue(string inputStr, MessageFormat msgFormat)
        {
            if (msgFormat == MessageFormat.Positional)
            {
                return inputStr;
            }
            else
            {
                string[] nameValuePair = inputStr.Split("=".ToCharArray());
                if (nameValuePair.Length != 2)
                {
                    throw new Exception($"Bad name-value pair format: {inputStr}");
                }
                return nameValuePair[1];
            }
        }

    }
}
