using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.AndonDataToRedis
{
    /// <summary>
    /// Embedded Resource-ként hozáadott fájl tartralmának olvasására szolgáló osztály
    /// 
    /// Feltételkezések:
    ///     - Ez az osztály az Embedded Resource-öt tartalmazó projekt defult névterében van
    /// ResourceReader rr = new ResourceReader();
    /// string resourcecontent = rr.GetFileContentAsString("Sample.xml");
    /// </summary>
    public class ResourceReader
    {
        /// <summary>
        /// stringként visszaadjas egy Embedded Resource-ként befordított fájl tartalmát
        /// </summary>
        /// <param name="fileName">Az Embedded Resource-ként hozzáadott fájl neve</param>
        /// <returns>Féjl tartalma</returns>
        public string GetFileContentAsString(string fileName)
        {
            Type myType = this.GetType();
            string result = string.Empty;
            using (Stream stream = myType.Assembly.
                       GetManifestResourceStream($"{myType.Namespace}.{fileName}"))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
