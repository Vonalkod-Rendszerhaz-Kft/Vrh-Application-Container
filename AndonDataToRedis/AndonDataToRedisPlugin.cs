using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using Vrh.Logger;
using Vrh.DataToRedisCore;
using Vrh.Web.Common.Lib;


namespace Vrh.AndonDataToRedis
{
    public class AndonDataToRedisPlugin : PluginAncestor
    {
        SQLToRedisControl sqlToRedisControl;

        /// <summary>
        /// Constructor
        /// </summary>
        private AndonDataToRedisPlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Static Factory (Ha nincs megadva, akkor egy egy paraméteres konstruktort kell implementálni, amely konstruktor paraméterben fogja megkapni a )
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">A példánynak átadott adat(ok)</param>
        /// <returns></returns>
        public static AndonDataToRedisPlugin AndonDataToRedisPluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new AndonDataToRedisPlugin();
            instance._myData = instanceDefinition;
            return instance;
        }

        /// <summary>
        /// IPlugin.Start
        /// </summary>
        public override void Start()
        {
            if (MyStatus == PluginStateEnum.Starting || MyStatus == PluginStateEnum.Running)
            {
                return;
            }
            BeginStart();
            try
            {
                // Implement Start logic here 
                string puginConfig = _myData.InstanceConfig;
                if (String.IsNullOrEmpty(puginConfig)) { puginConfig = _myData.Type.PluginConfig; }
                int separatorIndex = puginConfig.IndexOf(":") > -1 ? puginConfig.IndexOf(":") : puginConfig.Length;
                string configFile = puginConfig;
                string xmlfilepath = string.Empty;
                if (configFile.Substring(0, 1) == @"\")
                {
                    string configParameterFile = configFile;
                    string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    xmlfilepath = assemblyFolder + configParameterFile;
                }
                else { xmlfilepath = configFile; }
                var adtrcElement = XElement.Load(xmlfilepath);
                var adtrXmlProcessor = new AndonDataToRedisConfigXmlProcessor(adtrcElement);
                
                ResourceReader rr = new ResourceReader();
                string resourcecontent = rr.GetFileContentAsString("DataToRedisCore_EmbeddedConfig.xml");

                resourcecontent = resourcecontent.Replace($"@{AndonDataToRedisConfigXmlProcessor.FREQUENCY_ELEMENT}@", adtrXmlProcessor.Frequency.ToString().Trim());
                resourcecontent = resourcecontent.Replace($"@{AndonDataToRedisConfigXmlProcessor.ANDONVIEWFREQ_ELEMENT}@", adtrXmlProcessor.AndonViewFreqvency.ToString().Trim());
                resourcecontent = resourcecontent.Replace($"@{AndonDataToRedisConfigXmlProcessor.LCID_ELEMENT}@", adtrXmlProcessor.Lcid);
                resourcecontent = resourcecontent.Replace($"@{AndonDataToRedisConfigXmlProcessor.REDISCS_ELEMENT}@", adtrXmlProcessor.RedisConnectionString);
                resourcecontent = resourcecontent.Replace($"@{AndonDataToRedisConfigXmlProcessor.SQLDBCS_ELEMENT}@", adtrXmlProcessor.SqlConnectionString);

                XElement dtrcElement = XElement.Parse(resourcecontent);

                sqlToRedisControl = new SQLToRedisControl(dtrcElement, this.Stop, this);
                var logData = new Dictionary<string, string>();
                logData.Add("Config file", configFile);
                LogThis("AndonDataToRedisPlugin started.", logData, null, LogLevel.Debug, this.GetType());

                base.Start();
            }
            catch (Exception ex) {   SetErrorState(ex);   }
        }

        /// <summary>
        /// IPlugin.Stop
        /// </summary>
        public override void Stop()
        {
            if (MyStatus == PluginStateEnum.Stopping || MyStatus == PluginStateEnum.Loaded)
            {
                return;
            }
            BeginStop();
            try
            {
                if (sqlToRedisControl != null)
                {
                    sqlToRedisControl.Dispose();
                }
                // Implement stop logic here
                LogThis("AndonDataToRedisPlugin stopped.", null, null, LogLevel.Information, this.GetType());
                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        BeginDispose();
                        // TODO: dispose managed state (managed objects).

                        Stop();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TestPlugin() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class AndonDataToRedisConfigXmlProcessor
    {
        #region Const
        public const string SUBSTITUTIONS_ELEMENT = "Substitutions";
        public const string FREQUENCY_ELEMENT = "FREQUENCY";
        public const string ANDONVIEWFREQ_ELEMENT= "ANDONVIEWFREQ";
        public const string LCID_ELEMENT = "LCID";
        public const string SQLDBCS_ELEMENT = "SQLDBCS";
        public const string REDISCS_ELEMENT = "REDISCS";
        #endregion

        private XElement _rootElement = null;

        #region Constructor
        public AndonDataToRedisConfigXmlProcessor(XElement rootxml)
        {
            _rootElement = rootxml;
        }
        #endregion Constructor

        public int Frequency
        {
            get
            {
                XElement substitutionselement = _rootElement.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
                if (substitutionselement == null) return 10;
                XElement frequencyelement = substitutionselement.Element(XName.Get(FREQUENCY_ELEMENT));
                if (frequencyelement == null) return 10;
                if (!int.TryParse(frequencyelement.Value, out int result)) return 10;
                return result;
            }
        }
        public int AndonViewFreqvency
        {
            get
            {
                XElement substitutionselement = _rootElement.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
                if (substitutionselement == null) return 10;
                XElement andonviewfrequencyelement = substitutionselement.Element(XName.Get(ANDONVIEWFREQ_ELEMENT));
                if (andonviewfrequencyelement == null) return 10;
                if (!int.TryParse(andonviewfrequencyelement.Value, out int result)) return 10;
                return result;
            }
        }
        public string Lcid
        {
            get
            {
                XElement substitutionselement = _rootElement.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
                if (substitutionselement == null) return "en-US";
                XElement lcidelement = substitutionselement.Element(XName.Get(LCID_ELEMENT));
                if (lcidelement == null) return "en-US";
                return lcidelement.Value;
            }
        }
        public string RedisConnectionString
        {
            get
            {
                XElement rediscselement = _rootElement.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
                if (rediscselement == null) return null;
                XElement lcidelement = rediscselement.Element(XName.Get(REDISCS_ELEMENT));
                if (lcidelement == null) return null;
                return lcidelement.Value;
            }
        }
        public string SqlConnectionString
        {
            get
            {
                XElement sqlconnectionstringelement = _rootElement.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
                if (sqlconnectionstringelement == null) return null;
                XElement lcidelement = sqlconnectionstringelement.Element(XName.Get(SQLDBCS_ELEMENT));
                if (lcidelement == null) return null;
                return lcidelement.Value;
            }
        }
    }
}

