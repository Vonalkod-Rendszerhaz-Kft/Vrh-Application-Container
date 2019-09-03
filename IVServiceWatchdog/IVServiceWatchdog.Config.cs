using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.LinqXMLProcessor.Base;
using VRH.ConnectionStringStore;

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
                if (!TimeSpan.TryParse(strValue, out TimeSpan interval)) { interval = new TimeSpan(0, 2, 0); };
                return (int)interval.TotalMilliseconds;
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
                if (!TimeSpan.TryParse(strValue, out TimeSpan Interval)) { Interval = new TimeSpan(0, 3, 0); };
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
                if (!TimeSpan.TryParse(strValue, out TimeSpan Interval)) { Interval = new TimeSpan(0, 7, 0); };
                return Interval;
            }
        }

        /// <summary>
        /// Jelzi, hogy dump fájltr kell készíteni, amikor hibát detektál
        /// </summary>
        public bool CreateDump
        {
            get
            {
                return GetExtendedBoolElementValue(GetXElement(CREATEDUMP_ELEMENT_NAME), false, "true", "yes", "1");
            }
        }

        /// <summary>
        /// Jeltzi, hogy ha hiba lép fell, akkor ki kell-e lőni a processt
        /// </summary>
        public bool KillProcess
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
                string rcs, rcs2;
                rcs = GetElementValue<string>(GetXElement(REDISCONNECTION_ELEMENT_NAME), String.Empty);
                try
                {
                    rcs2 = VRH.ConnectionStringStore.VRHConnectionStringStore.GetRedisConnectionString(rcs);
                    int ix = rcs2.IndexOf('=');
                    if (ix >= 0) rcs2 = rcs2.Substring(ix + 1, rcs2.Length - ix - 1);
                }
                catch { rcs2 = rcs; }
                return rcs2;
            }
        }
        public int RedisconnectRetries
        {
            get
            {
                var e = GetXElement(REDISCONNECTION_ELEMENT_NAME); if (e == null) { return 1; }
                var a = e.Attribute(RETRIES_ATTRIBUTE_NAME_IN_REDISCONNECTION_ELEMENT); if (a == null) { return 1; }
                if (!int.TryParse(a.Value, out int result)) { result = 1; };
                if (result < 1) { result = 1; }
                return result;
            }
        }
        /// <summary>
        /// Ennyi lehet a szolgáltatást hostoló procesben maximum a szálak száma
        /// </summary>
        public int MaximumAllowedThreadNumber
        {
            get
            {
                var e = GetXElement(CHECKTHREADUSAGE_ELEMENT_NAME); if (e == null) { return 0; }
                var a = e.Attribute(MAXIMUM_ATTRIBUTE_NAME); if (a == null) { return 0; }
                if (!int.TryParse(a.Value, out int maxthreadusage)) { maxthreadusage = 0; };
                if (maxthreadusage < 0) { maxthreadusage=0; }
                return maxthreadusage;
            }
        }
        public bool CheckThreadNumberEnabled
        {
            get
            {
                var e = GetXElement(CHECKTHREADUSAGE_ELEMENT_NAME); if (e == null) { return false; }
                var a = e.Attribute(ENABLED_ATTRIBUTE_NAME); if (a == null) { return true; }
                return a.Value.ToUpper() != "FALSE";
            }
        }

        /// <summary>
        /// Ennyi lehet a szolgáltatást hostoló procesben maximum a szálak száma
        /// </summary>
        public int MaximumAllowedMemory
        {
            get
            {
                var e = GetXElement(CHECKMEMORYUSAGE_ELEMENT_NAME); if (e == null) { return 0; }
                var a = e.Attribute(MAXIMUM_ATTRIBUTE_NAME); if (a == null) { return 0; }
                if (!int.TryParse(a.Value, out int maxmemoryusage)) { maxmemoryusage = 0; };
                if (maxmemoryusage < 0) { maxmemoryusage = 0; }
                return maxmemoryusage;
            }
        }
        public int MemoryUsageSamples
        {
            get
            {
                var e = GetXElement(CHECKMEMORYUSAGE_ELEMENT_NAME); if (e == null) { return 1; }
                var a = e.Attribute(SAMPLES_ATTRIBUTE_NAME); if (a == null) { return 1; }
                if (!int.TryParse(a.Value, out int samples)) { samples = 1; };
                if (samples < 1) { samples = 1; }
                return samples;
            }
        }
        public bool CheckMemoryUsageEnabled
        {
            get
            {
                var e = GetXElement(CHECKMEMORYUSAGE_ELEMENT_NAME); if (e == null) { return false; }
                var a = e.Attribute(ENABLED_ATTRIBUTE_NAME); if (a == null) { return true; }
                return a.Value.ToUpper() != "FALSE";
            }
        }

        /// <summary>
        /// Ennyi lehet a szolgáltatást hostoló procesben maximum a szálak száma
        /// </summary>
        public int MaximumAllowedCPUusage
        {
            get
            {
                var e = GetXElement(CHECKCPUUSAGE_ELEMENT_NAME); if (e == null) { return 0; }
                var a = e.Attribute(MAXIMUM_ATTRIBUTE_NAME); if (a == null) { return 0; }
                if (!int.TryParse(a.Value, out int maxcpuusage)) { maxcpuusage = 0; };
                if (maxcpuusage < 0) { maxcpuusage = 0; }
                return maxcpuusage;
            }
        }
        public int CPUusageSamples
        {
            get
            {
                var e = GetXElement(CHECKCPUUSAGE_ELEMENT_NAME); if (e == null) { return 1; }
                var a = e.Attribute(SAMPLES_ATTRIBUTE_NAME); if (a == null) { return 1; }
                if (!int.TryParse(a.Value, out int samples)) { samples = 1; };
                if (samples < 1) { samples = 1; }
                return samples;
            }
        }
        public bool CheckCPUusageEnabled
        {
            get
            {
                var e = GetXElement(CHECKCPUUSAGE_ELEMENT_NAME); if (e == null) { return false; }
                var a = e.Attribute(ENABLED_ATTRIBUTE_NAME); if (a == null) { return true; }
                return a.Value.ToUpper() != "FALSE";
            }
        }

        /// <summary>
        /// A szolgáltatástól megköveztelt minimális válaszidő
        /// </summary>
        public TimeSpan MaxWCFResponseTime
        {
            get
            {
                var e = GetXElement(CHECKWCFRESPONSETIME_ELEMENT_NAME); if (e == null) { return new TimeSpan(0, 0, 0); }
                var a = GetXElement(CHECKWCFRESPONSETIME_ELEMENT_NAME).Attribute(MAXIMUM_ATTRIBUTE_NAME); if (a == null) { return new TimeSpan(0, 0, 0); }
                if (!TimeSpan.TryParse(a.Value, out TimeSpan maximumResponseTime)) { maximumResponseTime = new TimeSpan(0, 0, 0); };
                return maximumResponseTime;
            }
        }
        public bool CheckWCFResponseTimeEnabled
        {
            get
            {
                var e = GetXElement(CHECKWCFRESPONSETIME_ELEMENT_NAME); if (e == null) { return false; }
                var a = e.Attribute(ENABLED_ATTRIBUTE_NAME); if (a == null) { return true; }
                return a.Value.ToUpper() != "FALSE";
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
        private const string MAXERRORCOUNT_ELEMENT_NAME = "MaxErrorCount";
        private const string LAPSES_ELEMENT_NAME = "Lapses";
        private const string STARTDELAYTIME_ELEMENT_NAME = "StartDelayTime";
        private const string CREATEDUMP_ELEMENT_NAME = "CreateDump";
        private const string KILLPROCESS_ELEMENT_NAME = "KillProcess";
        private const string WINDOWSSERVICENAME_ELEMENT_NAME = "WindowsServiceName";
        private const string PROCDUMPATH_ELEMENT_NAME = "ProcdumpPath";
        private const string DUMPTARGETPATH_ELEMENT_NAME = "DumpTargetPath";
        private const string REDISCONNECTION_ELEMENT_NAME = "RedisConnection";
        private const string RETRIES_ATTRIBUTE_NAME_IN_REDISCONNECTION_ELEMENT = "Retries";
        private const string CHECKWCFRESPONSETIME_ELEMENT_NAME = "CheckWCFResponseTime";
        private const string CHECKTHREADUSAGE_ELEMENT_NAME = "CheckThreadUsage";
        private const string CHECKMEMORYUSAGE_ELEMENT_NAME = "CheckMemoryUsage";
        private const string CHECKCPUUSAGE_ELEMENT_NAME = "CheckCPUusage";
        private const string ENABLED_ATTRIBUTE_NAME = "Enable";
        private const string MAXIMUM_ATTRIBUTE_NAME = "Maximum";
        private const string SAMPLES_ATTRIBUTE_NAME = "Samples";
        #endregion Private Members
    }
}
