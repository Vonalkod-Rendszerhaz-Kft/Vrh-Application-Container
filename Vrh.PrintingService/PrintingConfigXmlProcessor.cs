using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.Web.Common.Lib;

namespace Vrh.PrintingService
{
    public class PrintingConfigXmlProcessor : XmlParser
    {
        #region Constructor
        /// <summary>
        /// XML paraméterező fájl feldolgozásának konstruktora.
        /// </summary>
        /// <param name="fileName">XmlParser.xml fájl a teljes elérési útvonalával együtt.</param>
        /// <param name="configName">Példányosításhoz meg kell adni az xmlFile fizikai helyét (teljes útvonallal együtt).</param>
        /// <param name="id">Ezt a definíció kell feldolgozni. Ha nem létezik az XML hiba.</param>
        /// <param name="lcid">Alapértelmezett nyelvi beállítás. Ha null vagy üres, akkor</param>
        public PrintingConfigXmlProcessor(string fileName, string configName, string lcid) 
            : base(fileName, configName, lcid)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FileManagerXmlParser START");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format($"FileManagerXmlParser Exception: {0}", ex));
            }
        }
        #endregion Constructor

        public XElement GetElement(params string[] elementPath)
        {
            return base.GetXElement(elementPath);
        }

        public new T GetValue<T>(string attributeName, XElement xelement, T defaultValue, bool isThrowException = false, bool isRequired = false)
        {
            return base.GetValue(attributeName, xelement, defaultValue, isThrowException, isRequired);
        }

        public new T GetValue<T>(XElement xelement, T defaultValue, bool isThrowException = false, bool isRequired = false)
        {
            return base.GetValue(xelement, defaultValue, isThrowException, isRequired);
        }
    }
}
