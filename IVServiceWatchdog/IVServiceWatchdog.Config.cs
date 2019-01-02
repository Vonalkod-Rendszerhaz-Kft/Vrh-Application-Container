using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.LinqXMLProcessor.Base;

namespace IVServiceWatchdog
{
    internal class IVServiceWatchdogParameterFileProcessor : LinqXMLProcessorBaseClass
    {
        #region Public Members

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="parameterFile">paraméter fájl és fájlon belüli útvonal a root TAG-ig</param>
        public IVServiceWatchdogParameterFileProcessor(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        /// <summary>
        /// Ennyi időközönként ellenőriz
        /// </summary>
        public int CheckInterval
        {
            get
            {
                string strValue = GetElementValue<string>(GetXElement(CHECKINTERVAL_ELEMENT_NAME), "00:02:00");
                TimeSpan interval = new TimeSpan(0, 2, 0);
                TimeSpan.TryParse(strValue, out interval);
                return (int)interval.TotalMilliseconds;
            }
        }

        /// <summary>
        /// A szolgáltatástól megköveztelt minimális válaszidő
        /// </summary>
        public TimeSpan MinimalResponseTime
        {
            get
            {
                string strValue = GetElementValue<string>(GetXElement(MINIMALRESPONSETIME_ELEMENT_NAME), "00:00:00");
                TimeSpan minimalResponseTime = new TimeSpan(0, 0, 0);
                TimeSpan.TryParse(strValue, out minimalResponseTime);
                return minimalResponseTime;
            }
        }

        /// <summary>
        /// Maximálisan megengedett hibaszám
        /// </summary>
        public int MaxErrorCount
        {
            get
            {
                return GetElementValue<int>(GetXElement(MAXERRORCOUNT_ELEMENT_NAME), 1);
            }
        }

        /// <summary>
        /// Indulás után ennyi ideig nem végez ellenőrzést
        /// </summary>
        public TimeSpan StartDelayTime
        {
            get
            {
                string strValue = GetElementValue<string>(GetXElement(STARTDELAYTIME_ELEMENT_NAME), "00:03:00");
                TimeSpan Interval = new TimeSpan(0, 3, 0);
                TimeSpan.TryParse(strValue, out Interval);
                return Interval;
            }
        }

        /// <summary>
        /// Ha ennyi ideig nem lép fel hiba, akkor nullázódik a hibaszámláló (elévülnek a korábbi hibák)
        /// </summary>
        public TimeSpan LapsesInterval
        {
            get
            {
                string strValue = GetElementValue<string>(GetXElement(LAPSES_ELEMENT_NAME), "00:07:00");
                TimeSpan Interval = new TimeSpan(0, 7, 0);
                TimeSpan.TryParse(strValue, out Interval);
                return Interval;
            }
        }

        /// <summary>
        /// Jelzi, hogy dump fájltr kell készíteni, amikor hibát detektál
        /// </summary>
        public bool CreateDumpNeed
        {
            get
            {
                return GetExtendedBoolElementValue(GetXElement(CREATEDUMP_ELEMENT_NAME), false, "true", "yes", "1");
            }
        }

        /// <summary>
        /// Jeltzi, hogy ha hiba lép fell, akkor ki kell-e lőni a processt
        /// </summary>
        public bool KillProcessNeed
        {
            get
            {
                return GetExtendedBoolElementValue(GetXElement(KILLPROCESS_ELEMENT_NAME), true, "true", "yes", "1");
            }
        }

        /// <summary>
        /// A windowsservice neve, amelyik a vizsgált WCF szolgáltatást hostolja
        /// </summary>
        public string WindowsServiceName
        {
            get
            {
                return GetElementValue<string>(GetXElement(WINDOWSSERVICENAME_ELEMENT_NAME), "notsetted");
            }
        }

        /// <summary>
        /// A pocdump.exe helye, amivel a dumpot csináljuk
        /// </summary>
        public string ProcdumpPath
        {
            get
            {
                return GetElementValue<string>(GetXElement(PROCDUMPATH_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Ide kell tenni a dump fájlokat
        /// </summary>
        public string DumpTargetPath
        {
            get
            {
                return GetElementValue<string>(GetXElement(DUMPTARGETPATH_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Visszadja  aredis connection stringet
        /// </summary>
        public string RedisConnection
        {
            get
            {
                return GetElementValue<string>(GetXElement(REDISCONNECTION_ELEMENT_NAME), String.Empty);
            }
        }

        /// <summary>
        /// Ennyi lehet a szolgáltatást hostoló procesben maximum a szálak száma
        /// </summary>
        public int MaximumAlloewedThreadNumber
        {
            get
            {
                return GetElementValue<int>(GetXElement(MAXIMUMALLOWEDTHREADNUMBER_ELEMENT_NAME), 0);
            }
        }

        /// <summary>
        /// A használt konfiguráció
        /// </summary>
        public string ConfigurationFileDefinition
        {
            get
            {
                return _xmlFileDefinition;
            }
        }

        #endregion Public Members

        #region Private Members

        //COMMON Config
        private const string CHECKINTERVAL_ELEMENT_NAME = "CheckInterval";
        private const string MINIMALRESPONSETIME_ELEMENT_NAME = "MinimalResponseTime";
        private const string MAXERRORCOUNT_ELEMENT_NAME = "MaxErrorCount";
        private const string LAPSES_ELEMENT_NAME = "Lapses";
        private const string STARTDELAYTIME_ELEMENT_NAME = "StartDelayTime";
        private const string CREATEDUMP_ELEMENT_NAME = "CreateDump";
        private const string KILLPROCESS_ELEMENT_NAME = "KillProcess";
        private const string WINDOWSSERVICENAME_ELEMENT_NAME = "WindowsServiceName";
        private const string PROCDUMPATH_ELEMENT_NAME = "ProcdumpPath";
        private const string DUMPTARGETPATH_ELEMENT_NAME = "DumpTargetPath";
        private const string REDISCONNECTION_ELEMENT_NAME = "RedisConnection";
        private const string MAXIMUMALLOWEDTHREADNUMBER_ELEMENT_NAME = "MaximumAllowedThreadNumber";

        #endregion Private Members
    }
}
