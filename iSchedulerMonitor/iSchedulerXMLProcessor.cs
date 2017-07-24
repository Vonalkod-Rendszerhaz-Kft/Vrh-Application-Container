namespace iSchedulerMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Vrh.LinqXMLProcessor.Base;
    using Vrh.Common.Serialization.Structures;

    #region iSchedulerXMLProcessor class

    public class iSchedulerXMLProcessor : LinqXMLProcessorBaseClass
    {
        #region Defination of namming rules in XML
        // A szabályok:
        //  - Mindig konstansokat használj, hogy az element és az attribútum neveket azon át hivatkozd! 
        //  - Az elnevezések feleljenek meg a konstansokra vonatkozó elnevetési szabályoknak!
        //  - Az Attribútumok neveiben mindig jelöld, mely elem alatt fordul elő.
        //  - Az elemekre ne használj, ilyet, mert az elnevezések a hierarchia mélyén túl összetetté (hosszúvá) válnának! 
        //    Ez alól kivétel, ha nem egyértelmű az elnevezés e nélkül.
        //private const string GROUP_ATTRIBUTE_IN_STRING_ELEMENT = "Group";
        //private const string PROPERTY1_ELEMENT_NAME = "Property1";
        //private const string ATTRIBUTE1_ATTRIBUTE_IN_PROPERTY1_ELEMENT = "Attribute1";
        #endregion

        #region Elements and Attributes private classes
        private class Elements
        {
            public const string DATABASECONNECTIONSTRING = "DatabaseConnectionString";
            public const string MONITORSERVICE = "MonitorService";
            public const string EXECUTEURL = "ExecuteUrl";
            public const string LOGINURL = "LoginUrl";
            public const string CHECKINTERVAL = "CheckInterval";
        }

        private class Attributes
        {
        }
        #endregion Elements and Attributes private classes

        #region Properties

        public string ErrorMessage { get; private set; } = String.Empty;

        /// <summary>
        /// Itt a model létrejöttekor egy fix érték: "en-US".
        /// </summary>
        public string LCID { get; private set; }

        /// <summary>
        /// Alkalmazott string konstasok listája.
        /// </summary>
        public List<StringElement> Strings { get; private set; }

        /// <summary>
        /// Kapcsolatok listája.
        /// Lehetséges, hogy string állandó van megadva a name attribútumban. 
        /// </summary>
        public List<StringElement> ConnectionStrings { get; private set; }

        public string DatabaseConnectionString { get; private set; }

        #region ExecuteUrl property
        private UrlElement _ExecuteUrl;
        public UrlElement ExecuteUrl
        {
            get
            {
                if (_ExecuteUrl == null)
                {
                    _ExecuteUrl = LoadUrl(Elements.MONITORSERVICE, Elements.EXECUTEURL);
                }
                return _ExecuteUrl;
            }
        }
        #endregion ExecuteUrl property

        #region LoginUrl property
        private UrlElement _LoginUrl;
        public UrlElement LoginUrl
        {
            get
            {
                if (_LoginUrl == null)
                {
                    _LoginUrl = LoadUrl(Elements.MONITORSERVICE, Elements.LOGINURL);
                }
                return _LoginUrl;
            }
        }
        #endregion LoginUrl property

        #region CheckInterval property
        /// <summary>
        /// Az időzítések figyelése ennyi időközönként (másodperc) fog megtörténni.
        /// Minimum: 60 sec (1perc), maximum: 86400 sec (1nap).
        /// Ha a tulajdonság nem létezik, vagy értelmezhetetlen, akkor a minimum lesz.
        /// </summary>
        private int _CheckInterval;
        public int CheckInterval
        {
            get
            {
                if (_CheckInterval < this.CheckIntervalMinimum)
                {
                    string chckint = GetElementValue(GetXElement(Elements.MONITORSERVICE,Elements.CHECKINTERVAL), "");
                    if (String.IsNullOrEmpty(chckint))
                    {
                        _CheckInterval = this.CheckIntervalMinimum;
                    }
                    else
                    {
                        if (!Int32.TryParse(chckint, out _CheckInterval)) _CheckInterval = this.CheckIntervalMinimum;
                    }
                    _CheckInterval = Math.Min(Math.Max(_CheckInterval, this.CheckIntervalMinimum),this.CheckIntervalMaximum);
                }
                return _CheckInterval;
            }
        }
        #endregion Properties

        public int CheckIntervalMinimum { get; private set; }

        public int CheckIntervalMaximum { get; private set; }

        public string XmlLocalPath { get; private set; }

        public string XmlRemotePath { get; private set; }

        #endregion Properties

        public iSchedulerXMLProcessor(string localPath, string remotePath = null) : base()
        {
            try
            {
                _xmlNameSpace = String.Empty;
                _xmlFileDefinition = "@" + localPath;

                this.LCID = "en-US";    //fix érték, mert nincs honnan megtudni a beállítást!
                this.CheckIntervalMinimum = 60; // 1 perc
                this.CheckIntervalMaximum = 86400; // 1 nap
                this.XmlLocalPath = localPath;
                this.XmlRemotePath = String.IsNullOrEmpty(remotePath) ? localPath : remotePath;

                _CheckInterval = -1 ;    // annak jelzésére, hogy a beállítás nem történt meg.

                #region strings load
                this.Strings = new List<StringElement>();
                foreach (XElement item in GetAllXElements(StringElement.ElementNames.STRINGS, StringElement.ElementNames.STRING))
                {
                    StringElement se = new StringElement()
                    {
                        Name = GetAttribute(item, StringElement.AttributeNames.STRING_NAME, ""),
                        LCID = GetAttribute(item, StringElement.AttributeNames.STRING_LCID, ""),
                        Value = item.Value,
                    };
                    if (String.IsNullOrEmpty(se.Name)) AddErr("The 'name' attribute is missing or empty in the <string> element!");
                    this.Strings.Add(se);
                }
                #endregion strings load

                #region connectionstrings load and parser
                this.ConnectionStrings = new List<StringElement>();
                foreach (XElement item in GetAllXElements(StringElement.ElementNames.CONNECTIONSTRINGS, StringElement.ElementNames.CONNECTIONSTRING))
                {
                    StringElement se = new StringElement()
                    {
                        Name = GetAttribute(item, StringElement.AttributeNames.STRING_NAME, ""),
                        LCID = GetAttribute(item, StringElement.AttributeNames.STRING_LCID, ""),
                        Value = item.Value,
                    };
                    if (String.IsNullOrEmpty(se.Name)) AddErr("The 'name' attribute is missing or empty in the <connectionstring> element!");
                    this.ConnectionStrings.Add(se);
                }
                foreach (StringElement se in this.ConnectionStrings) se.Value = StringElement.FindString(this.Strings, se.Value, this.LCID);
                #endregion connectionstrings load and parser

                #region DatabaseConnectionString parser

                this.DatabaseConnectionString = GetElementValue(GetXElement(Elements.DATABASECONNECTIONSTRING), "");
                if (String.IsNullOrEmpty(this.DatabaseConnectionString))
                {
                    AddErr($"The <{Elements.DATABASECONNECTIONSTRING}> element is missing or empty!");
                }
                else
                {
                    this.DatabaseConnectionString = StringElement.FindString(this.ConnectionStrings, this.DatabaseConnectionString, this.LCID);
                }

                #endregion Database parser

                if (this.ErrorMessage != String.Empty) throw new ApplicationException(this.ErrorMessage);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error occured while xml processing!", ex);
            }
        }

        #region Public methods (StringsSubstitution)

        public string StringsSubstitution(string data)
        {
            string result = data;
            if (this.Strings != null && this.Strings.Count > 0)
            {   //előbb a nylvi változatot helyettesítsük be
                if (!String.IsNullOrEmpty(this.LCID)) foreach (StringElement str in this.Strings.Where(x => x.LCID == this.LCID)) result = str.Substitution(result);
                foreach (StringElement str in this.Strings.Where(x => String.IsNullOrEmpty(x.LCID))) result = str.Substitution(result);
            }
            return result;
        }

        #endregion Public methods

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
                result = new UrlElement();
                result.Protocol = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.PROTOCOL), "");
                result.HostName = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.HOSTNAME), "");
                result.AppName = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.APPNAME), "");
                result.Area = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.AREA), "");
                result.Controller = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.CONTROLLER), "");
                result.Action = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.ACTION), "");
                result.Fragment = GetElementValue(GetXElement(one, two, UrlElement.ElementNames.FRAGMENT), "");

                result.UrlParameters = new List<UrlElement.UrlParameter>();
                foreach (XElement par in GetAllXElements(one, two, UrlElement.ElementNames.INPUTPARAMETER))
                {
                    result.UrlParameters.Add(new UrlElement.UrlParameter()
                    {
                        Name = GetAttribute(par, UrlElement.AttributeNames.PARAM_NAME, ""),
                        PassTo = GetAttribute(par, UrlElement.AttributeNames.PARAM_PASSTO, UrlElement.ParameterTypes.url),
                        Value = par.Value
                    });
                }
                foreach (UrlElement.UrlParameter par in result.UrlParameters)
                {
                    if (String.IsNullOrEmpty(par.Name)) throw new ApplicationException($"The 'name' attribute has required in the <inputparameter> element! Place = {two}");
                    int founded = 0;
                    foreach (UrlElement.UrlParameter par2 in result.UrlParameters) if (par.Name == par2.Name) founded++;
                    if (founded > 1) throw new ApplicationException($"The name='{par.Name}' attribute is not unique among the <inputparameter> elements! Place = {two}");
                }

                //ha nincs már hiba, akkor a Strings változókat keressük meg és helyettesítsük be!
                if (this.Strings != null && this.Strings.Count > 0)
                {   //előbb a nylvi változatot helyettesítsük be
                    if (!String.IsNullOrEmpty(this.LCID)) foreach (StringElement str in this.Strings.Where(x => x.LCID == this.LCID)) result.Substitution(str);
                    foreach (StringElement str in this.Strings.Where(x => String.IsNullOrEmpty(x.LCID))) result.Substitution(str);
                }
            }
            return result;
        }

        private void AddErr(string mess)
        {
            this.ErrorMessage += (this.ErrorMessage == "" ? "" : "\n") + mess;
        }

        #endregion Private methods
    }
    #endregion iSchedulerXMLProcessor class
}
