using System;
using System.Collections.Generic;

namespace Vrh.ApplicationContainer.Core
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
        public FatalException(string message, Exception innerException = null, params KeyValuePair<string, string>[] datas)
            : base($"{fatalPrefix}:{message}", innerException)
        {
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    Data.Add(data.Key, data.Value);
                }
            }
        }

        // TODO: ML
        private static readonly string fatalPrefix = "Fatal error occured!";
    }
}
