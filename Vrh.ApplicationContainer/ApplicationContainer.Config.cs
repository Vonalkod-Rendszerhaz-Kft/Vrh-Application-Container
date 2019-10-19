using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.XmlProcessing;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// TODO: Mindig nevezd át ezt az osztályt!!! 
    ///         Használd az osztályra vonatkozó elnevezési konvenciókat, beszédes neveket használj a DDD (Domain Driven Development) alapelveinek megfelelően!
    ///         Naming pattern: {YourClassName}XmlProcessor 
    ///         Mindig használd az XmlProcessor suffix-et!
    /// </summary>
    public class ApplicationContainerConfig : LinqXMLProcessorBase
    {
        /// <summary>
        /// Constructor 
        ///     TODO: Nevezd át az osztály nevére!
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        public ApplicationContainerConfig(string parameterFile):base(parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive all information from XML

        // TODO: Írd át vagy töröld ezeket a meglévő példa implemenmtációkat! Az alapelveket bent hagyhatod, hogy később is szem előtt legyenek!
        //  Alapelvek:
        //      - Mindig csak olvasható property-ket használj getter megvalósítással, ha az adat visszanyeréséhez nem szükséges paraméter átadása!
        //      - Csak akkor használj függvényeket, ha a paraméterek átadására van szükség az információ visszanyeréséhez!
        //      - Mindig légy típusos! Az alaposztály jelenlegi implementációja (v1.1.X) az alábbi típusokat kezeli: int, string, bool, Enumerátor (generikusan)! Ha típus bővítésre lenne szükséged, kérj fejlesztést rá (change request)! 
        //      - Bonyolultabb típusokat elemi feldolgozással építs! Soha ne használj XML alapú szérializációt, amit depcreatednek tekintünk a fejlesztési környezeteinkben!
        //      - A bonyolultabb típusok kódját ne helyezd el ebben a fájlban, hanem külső definíciókat használj!
        //      - Ismétlődő információk visszanyerésére (listák, felsorolások), generikus kollekciókat használj (Lis<T>, Dictonary<Tkey, TValue>, IEnumerable<T>, stb...) 

        /// <summary>
        /// Ebben az Assemblyben található a használandó InstanceFactory plugin
        /// </summary>
        public string InstanceFactoryAssembly
        {
            get
            {
                string value = ConfigurationManager.AppSettings[ApplicationContainer.GetApplicationConfigName(INSTANCEFACTORYASSEMBLY_ELEMENT_NAME)];
                if (String.IsNullOrEmpty(value))
                {
                    value = GetElementValue(GetXElement(CONFIG_ELEMENT_NAME, INSTANCEFACTORYASSEMBLY_ELEMENT_NAME), String.Empty);
                }
                return value;
            }
        }

        /// <summary>
        /// Az InstanceFactory típus, amit használni kell
        /// </summary>
        public string InstanceFactoryType
        {
            get
            {
                string value = ConfigurationManager.AppSettings[ApplicationContainer.GetApplicationConfigName(INSTANCEFACTORYTYPE_ELEMENT_NAME)];
                if (String.IsNullOrEmpty(value))
                {
                    value = GetElementValue(GetXElement(CONFIG_ELEMENT_NAME, INSTANCEFACTORYTYPE_ELEMENT_NAME), String.Empty);
                }
                return value;
            }
        }

        /// <summary>
        /// Az InstanceFactory verziója, amit használni kell
        /// </summary>
        public string InstanceFactoryVersion
        {
            get
            {
                string value = ConfigurationManager.AppSettings[ApplicationContainer.GetApplicationConfigName(INSTANCEFACTORYVERSION_ELEMENT_NAME)];
                if (String.IsNullOrEmpty(value))
                {
                    value = GetElementValue(GetXElement(CONFIG_ELEMENT_NAME, INSTANCEFACTORYVERSION_ELEMENT_NAME), String.Empty);
                }
                return value;
            }
        }

        /// <summary>
        /// Ekkora méretekben tárolja a működéi üzenet infókat (utolsó ennyi darab)
        /// </summary>
        public ushort MessageStackSize
        {
            get
            {
                string strValue = ConfigurationManager.AppSettings[ApplicationContainer.GetApplicationConfigName(MESSAGESTACKSIZE_ELEMENT_NAME)];
                if (!String.IsNullOrEmpty(strValue))
                {
                    int intValue = GetValue<int>(strValue, -1);
                    if (intValue > 0)
                    {
                        return intValue > ushort.MaxValue ? ushort.MaxValue : (ushort)intValue;
                    }
                }
                return GetElementValue<ushort>(GetXElement(CONFIG_ELEMENT_NAME, MESSAGESTACKSIZE_ELEMENT_NAME), 50);
            }
        }
        #endregion

        #region Defination of namming rules in XML
        // A szabályok:
        //  - Mindig konstansokat használj, hogy az element és az attribútum neveket azon át hivatkozd! 
        //  - Az elnevezések feleljenek meg a konstansokra vonatkozó elnevetési szabályoknak!
        //  - Az Attribútumok neveiben mindig jelöld, mely elem alatt fordul elő.
        //  - Az elemekre ne használj, ilyet, mert az elnevezések a hierarchia mélyén túl összetetté (hosszúvá) válnának! Ez alól kivétel, ha nem egyértelmű az elnevezés e nélkül.
        // 
        private const string CONFIG_ELEMENT_NAME = "Config";
        private const string INSTANCEFACTORYASSEMBLY_ELEMENT_NAME = "InstanceFactoryAssembly";
        private const string INSTANCEFACTORYTYPE_ELEMENT_NAME = "InstanceFactoryType";
        private const string INSTANCEFACTORYVERSION_ELEMENT_NAME = "InstanceFactoryVersion";
        private const string MESSAGESTACKSIZE_ELEMENT_NAME = "MessageStackSize";

        #endregion
    }
}
