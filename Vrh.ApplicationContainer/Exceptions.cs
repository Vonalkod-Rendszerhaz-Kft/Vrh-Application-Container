using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Fatal Exception exception class (összeálítás funkcionálását megakadályozó hibák)
    /// </summary>
    public class FatalException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">hibaüzenet</param>
        /// <param name="innerException">belső exception</param>
        /// <param name="datas">kiegészítő adatok</param>
        public FatalException(String message, Exception innerException = null, params KeyValuePair<string, string>[] datas)
            : base(String.Format("{0}: {1}", fatalPrefix, message), innerException)
        {
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    this.Data.Add(data.Key, data.Value);
                }
            }
        }

        // TODO: ML
        static private string fatalPrefix = "Fatal error occured!";
    }
}
