namespace iSchedulerMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Vrh.LinqXMLProcessor.Base;
    using Vrh.Web.Common.Lib;

    #region iSchedulerXMLProcessor class

    public class iSchedulerXMLProcessor : LinqXMLProcessorBaseClass
    {
        #region Elements and Attributes private classes
        private class Elements
        {
            public const string DATABASECONNECTIONSTRING = "DatabaseConnectionString";
            public const string OBJECTTYPE = "ObjectType";
            public const string GROUPID = "GroupId";

            public const string MONITORSERVICE = "MonitorService";
            public const string CHECKINTERVAL = "CheckInterval";
        }

        private class Attributes
        {
        }
        #endregion Elements and Attributes private classes

        #region Properties

        /// <summary>
        /// Feldolgozás közben keletkezett hibaüzenetek.
        /// </summary>
        public string ErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Itt a model létrejöttekor egy fix érték: "en-US".
        /// </summary>
        public string LCID { get; private set; }

        /// <summary>
        /// Alkalmazott string konstasok listája.
        /// </summary>
        public List<NameValueXML> Strings { get; private set; }
        //public VariableCollection Strings { get; private set; }

        /// <summary>
        /// Kapcsolatok listája.
        /// Lehetséges, hogy string állandó van megadva a name attribútumban. 
        /// </summary>
        public List<NameValueXML> ConnectionStrings { get; private set; }
        //public VariableCollection ConnectionStrings { get; private set; }

        /// <summary>
        /// Az adatbázishoz való kapcsolódás adatai.
        /// A ConnectionStringStore által feloldott érték, ha nem tudta feloldani akkor ami az xml-ben van.
        /// </summary>
        public string DatabaseConnectionString { get; private set; }

        #region ObjectType property
        /// <summary>
        /// Az xml-ben található object type.
        /// Az ütemezések lekérdezéséhez szükséges.
        /// </summary>
        public string ObjectType
        {
            get
            {
                if (_ObjectType == null)
                {
                    _ObjectType = GetElementValue(GetXElement(Elements.OBJECTTYPE), "");
                    if (string.IsNullOrWhiteSpace(_ObjectType)) { throw new ApplicationException($"The <{Elements.OBJECTTYPE}> element is missing or empty!"); }
                }
                return _ObjectType;
            }
        }
        private string _ObjectType = null;
        #endregion ObjectType property

        #region GroupId property
        /// <summary>
        /// Az xml-ben található GroupId érték.
        /// Az ütemezések lekérdezéséhez szükséges.
        /// </summary>
        public string GroupId
        {
            get
            {
                if (_GroupId == null)
                {
                    _GroupId = GetElementValue(GetXElement(Elements.GROUPID), "");
                    if (string.IsNullOrWhiteSpace(_GroupId)) { throw new ApplicationException($"The <{Elements.GROUPID}> element is missing or empty!"); }
                    else if (_GroupId.Trim() == "*") { throw new ApplicationException($"The value of the <{Elements.GROUPID}> element can not equal '*'!"); }
                }
                return _GroupId;
            }
        }
        private string _GroupId = null;
        #endregion GroupId property

        #region CheckInterval property
        /// <summary>
        /// Az időzítések figyelése ennyi időközönként (másodperc) fog megtörténni.
        /// Minimum: 60 sec (1perc), maximum: 86400 sec (1nap).
        /// Ha a tulajdonság nem létezik, vagy értelmezhetetlen, akkor a minimum lesz.
        /// </summary>
        public int CheckInterval
        {
            get
            {
                if (_CheckInterval < CheckIntervalMinimum)
                {
                    string chckint = GetElementValue(GetXElement(Elements.MONITORSERVICE, Elements.CHECKINTERVAL), "");
                    if (string.IsNullOrEmpty(chckint))
                    {
                        _CheckInterval = CheckIntervalMinimum;
                    }
                    else
                    {
                        if (!int.TryParse(chckint, out _CheckInterval)) { _CheckInterval = CheckIntervalMinimum; }
                    }
                    _CheckInterval = Math.Min(Math.Max(_CheckInterval, CheckIntervalMinimum), CheckIntervalMaximum);
                }
                return _CheckInterval;
            }
        }
        private int _CheckInterval;
        #endregion CheckInterval properties

        /// <summary>
        /// Az időzítések figyelésének minimum értéke.
        /// </summary>
        public int CheckIntervalMinimum { get; private set; }

        /// <summary>
        /// Az időzítések figyelésének maximum értéke.
        /// </summary>
        public int CheckIntervalMaximum { get; private set; }

        public string XmlLocalPath { get; private set; }

        public string XmlRemotePath { get; private set; }

        #endregion Properties

        public iSchedulerXMLProcessor(string localPath, string remotePath = null) : base()
        {
            try
            {
                _xmlNameSpace = string.Empty;
                _xmlFileDefinition = "@" + localPath;

                LCID = "en-US";    //fix érték, mert nincs honnan megtudni a beállítást!
                CheckIntervalMinimum = 60; // 1 perc
                CheckIntervalMaximum = 86400; // 1 nap
                XmlLocalPath = localPath;
                XmlRemotePath = string.IsNullOrEmpty(remotePath) ? localPath : remotePath;

                this._CheckInterval = -1;   // annak jelzése, hogy a beállítás még nem történt meg.

                #region strings load
                Strings = new List<NameValueXML>();
                foreach (XElement item in GetAllXElements(NameValueXML.ElementNames.STRINGS, NameValueXML.ElementNames.STRING))
                {
                    NameValueXML nv = new NameValueXML()
                    {
                        Name = GetAttribute(item, NameValueXML.AttributeNames.STRING_NAME, ""),
                        LCID = GetAttribute(item, NameValueXML.AttributeNames.STRING_LCID, ""),
                        Value = item.Value,
                    };
                    if (string.IsNullOrEmpty(nv.Name)) { AddErr("The 'name' attribute is missing or empty in the <string> element!"); }
                    Strings.Add(nv);
                }
                #endregion strings load

                #region connectionstrings load and parser
                ConnectionStrings = new List<NameValueXML>();
                foreach (XElement item in GetAllXElements(NameValueXML.ElementNames.CONNECTIONSTRINGS, NameValueXML.ElementNames.CONNECTIONSTRING))
                {
                    NameValueXML se = new NameValueXML()
                    {
                        Name = GetAttribute(item, NameValueXML.AttributeNames.STRING_NAME, ""),
                        LCID = GetAttribute(item, NameValueXML.AttributeNames.STRING_LCID, ""),
                        Value = item.Value,
                    };
                    if (string.IsNullOrEmpty(se.Name)) { AddErr("The 'name' attribute is missing or empty in the <connectionstring> element!"); }
                    ConnectionStrings.Add(se);
                }
                foreach (NameValueXML nv in ConnectionStrings) { nv.Value = NameValueXML.FindString(Strings, nv.Value, LCID); }
                #endregion connectionstrings load and parser

                #region DatabaseConnectionString parser

                DatabaseConnectionString = GetElementValue(GetXElement(Elements.DATABASECONNECTIONSTRING), "");
                if (string.IsNullOrWhiteSpace(DatabaseConnectionString))
                {
                    AddErr($"The <{Elements.DATABASECONNECTIONSTRING}> element is missing or empty!");
                }
                else
                {
                    string constr = NameValueXML.FindString(ConnectionStrings, DatabaseConnectionString, LCID);
                    try { DatabaseConnectionString = VRH.ConnectionStringStore.VRHConnectionStringStore.GetConnectionString(constr, false); }
                    catch (Exception) { DatabaseConnectionString = constr; }
                    //this.DatabaseConnectionString = constr;
                }

                #endregion Database parser

                if (ErrorMessage != string.Empty) { throw new ApplicationException(ErrorMessage); }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error occured while xml processing!", ex);
            }
        }

        #region Private methods (LoadUrl, AddErr)

        private UrlElement LoadUrl(string one, string two)
        {
            UrlElement result = null;
            XElement xurl = GetXElement(one, two);
            if (xurl == null)
            {
                throw new ApplicationException($"The {one}.{two} element is missing or empty!");
            }
            else
            {
                result = new UrlElement(xurl);
                //ha nincs már hiba, akkor a Strings változókat keressük meg és helyettesítsük be!
                if (Strings != null && Strings.Count > 0)
                {   //előbb a nylvi változatot helyettesítsük be
                    if (!string.IsNullOrEmpty(LCID)) { foreach (NameValueXML nv in Strings.Where(x => x.LCID == LCID)) {result.Substitution(nv); } }
                    foreach (NameValueXML nv in Strings.Where(x => string.IsNullOrEmpty(x.LCID))) { result.Substitution(nv); }
                }
            }
            return result;
        }

        private void AddErr(string mess)
        {
            ErrorMessage += (ErrorMessage == "" ? "" : "\n") + mess;
        }

        #endregion Private methods
    }
    #endregion iSchedulerXMLProcessor class
}
