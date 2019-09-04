using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.LinqXMLProcessor.Base;
using VRH.ConnectionStringStore;

namespace Service.Starter
{
    internal class ServiceStarterParameterFileProcessor : LinqXMLProcessorBaseClass
    {
        #region Public Members

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="parameterFile">paraméter fájl és fájlon belüli útvonal a root TAG-ig</param>
        public ServiceStarterParameterFileProcessor(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        /// <summary>
        /// Visszadaja az összes definiált kontrollált service listáját
        /// </summary>
        public List<ServiceProperties> AllControlledServices
        {
            get
            {
                List<ServiceProperties> services = new List<ServiceProperties>();
                foreach (var service in GetAllXElements(CONTROLLEDSERVICES_ELEMENT_NAME, SERVICE_ELEMENT_NAME))
                {
                    string servicename = GetAttribute<string>(service, NAME_ATTRIBUTE_IN_SERVICE_ELEMENT, String.Empty);
                    if (!String.IsNullOrEmpty(servicename))
                    {
                        ServiceProperties sp = new ServiceProperties() {   ServiceName = servicename,   };
                        TimeSpan interval = GetAttribute<TimeSpan>(service, CHECKINTERVAL_ATTRIBUTE_IN_SERVICE_ELEMENT, DefaultCheckInterval);
                        sp.CheckInterval = TimeSpan.FromMilliseconds(Math.Abs(interval.TotalMilliseconds));

                        if (sp.CheckInterval.TotalMilliseconds > 0)
                        {
                            sp.DependenciesSemafor = GetExtendedBoolAttribute(service, DEPENDENCIESSEMAFOR_ATTRIBUTE_IN_SERVICE_ELEMENT, DefaultDependenciesSemafor, "yes", "1", "true");

                            TimeSpan maxWaitTime = new TimeSpan(0);
                            TimeSpan.TryParse(GetAttribute<string>(service, MAXSTARTINGTIME_ATTRIBUTE_IN_SERVICE_ELEMENT, DefaultMaxStartingTime.ToString()), out maxWaitTime);
                            sp.MaxStartingWait = TimeSpan.FromMilliseconds(Math.Abs(maxWaitTime.TotalMilliseconds));

                            TimeSpan redisSemaforTime = new TimeSpan(0);
                            TimeSpan.TryParse(GetAttribute<string>(service, CREATEREDISSEMAFOR_ATTRIBUTE_IN_SERVICE_ELEMENT, DefaultRedisSemaforTime.ToString()), out redisSemaforTime);
                            sp.CreateRedisSemaforTime = TimeSpan.FromMilliseconds(Math.Abs(redisSemaforTime.TotalMilliseconds));

                            services.Add(sp);
                        }
                    }
                } 
                return services;
            }
        }

        /// <summary>
        /// Az adott nevű service definicióját adja
        /// </summary>
        /// <param name="serviceName">a service neve</param>
        /// <returns></returns>
        public ServiceProperties GetControlledService(string serviceName)
        {
            return AllControlledServices.FirstOrDefault(x => x.ServiceName == serviceName);
        }

        /// <summary>
        /// Visszadja az alapértelmezett ellenőrzési intervallumot
        /// </summary>
        public TimeSpan DefaultCheckInterval
        {
            get
            {                
                TimeSpan interval = new TimeSpan(0);
                TimeSpan.TryParse(GetAttribute<string>(GetXElement(CONTROLLEDSERVICES_ELEMENT_NAME), CHECKINTERVAL_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT, "00:01:00"), out interval);
                return interval;
            }
        } 

        /// <summary>
        /// Alapértelmezettben kell-e semafort rakni azokra a servicejkre, amiktől a service fut, amikor elindítja ezt a service-t
        /// </summary>
        public bool DefaultDependenciesSemafor
        {
            get
            {
                return GetExtendedBoolAttribute(GetXElement(CONTROLLEDSERVICES_ELEMENT_NAME), DEPENDENCIES_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT, false, "yes", "1", "true");
            }
        }

        /// <summary>
        /// Allapértelmezet idő, ameddig az újraindítás után él a szolgáltatás szemafora a redisen
        /// </summary>
        public TimeSpan DefaultRedisSemaforTime
        {
            get
            {
                TimeSpan redissemafortime = new TimeSpan(0);
                TimeSpan.TryParse(GetAttribute<string>(GetXElement(CONTROLLEDSERVICES_ELEMENT_NAME), CREATEREDISSEMAFOR_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT, "00:00:00"), out redissemafortime);
                return redissemafortime;
            }

        }

        /// <summary>
        /// Visszadja az alapértelmezett maximum várakozási uidőt a service indítás megtörténtére
        /// </summary>
        public TimeSpan DefaultMaxStartingTime
        {
            get
            {
                TimeSpan maxWaitTime = new TimeSpan(0);
                TimeSpan.TryParse(GetAttribute<string>(GetXElement(CONTROLLEDSERVICES_ELEMENT_NAME), MAXSTARTINGTIME_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT, "00:00:00"), out maxWaitTime);
                return maxWaitTime;
            }
        }

        /// <summary>
        /// A redis connection, ahol a semaforokat tároljuk
        /// </summary>
        /// <returns></returns>
        public string RedisConnection
        {
            get
            {
                string rcs,rcs2;
                rcs = GetElementValue<string>(GetXElement(REDISCONNECTION_ELEMENT_NAME), String.Empty);
                try
                {
                    rcs2 = VRH.ConnectionStringStore.VRHConnectionStringStore.GetRedisConnectionString(rcs);
                    int ix = rcs2.IndexOf('=');
                    if (ix >= 0) rcs2 = rcs2.Substring(ix+1, rcs2.Length- ix - 1);
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



        #endregion Public Members

        #region Private Members

        // ControlledServices element
        private const string CONTROLLEDSERVICES_ELEMENT_NAME = "ControlledServices";
        private const string CHECKINTERVAL_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT = "CheckInterval";
        private const string DEPENDENCIES_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT = "DependenciesSemafor";
        private const string MAXSTARTINGTIME_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT = "MaxStartingTime";
        private const string CREATEREDISSEMAFOR_ATTRIBUTE_IN_CONTROLLEDSERVICES_ELEMENT = "CreateRedisSemafor";
        // Service element
        private const string SERVICE_ELEMENT_NAME = "Service";
        private const string NAME_ATTRIBUTE_IN_SERVICE_ELEMENT = "Name";
        private const string CHECKINTERVAL_ATTRIBUTE_IN_SERVICE_ELEMENT = "CheckInterval";
        private const string DEPENDENCIESSEMAFOR_ATTRIBUTE_IN_SERVICE_ELEMENT = "DependenciesSemafor";
        private const string MAXSTARTINGTIME_ATTRIBUTE_IN_SERVICE_ELEMENT = "MaxStartingTime";
        private const string CREATEREDISSEMAFOR_ATTRIBUTE_IN_SERVICE_ELEMENT = "CreateRedisSemafor";
        // RedisConnection
        private const string REDISCONNECTION_ELEMENT_NAME = "RedisConnection";
        private const string RETRIES_ATTRIBUTE_NAME_IN_REDISCONNECTION_ELEMENT = "Retries";

        #endregion Private Members

    }
}
