using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vrh.Web.Common.Lib;
using Vrh.Redis.DataPoolHandler;

namespace Vrh.DataToRedisCore
{
    public class DataToRedisConfigXmlProcessor
    { 
        #region Const
        private const string GENERAL_ELEMENT = "General";
        private const string FREQUENCY_ELEMENT = "Frequency";
        private const string POOLS_ELEMENT = "Pools";
        private const string POOL_ELEMENT = "Pool";
        private const string NAME_ATTRIBUTE = "Name";
        private const string ID_ATTRIBUTE = "Id";
        private const string VERSION_ATTRIBUTE = "Version";
        private const string TYPE_ATTRIBUTE = "Type";
        private const string COLUMN_ATTRIBUTE = "Column";
        private const string FREQUENCY_ATTRIBUTE = "Frequency";
        private const string SQLCONNECTIONSTRING_ATTRIBUTE = "SQLconnectionString";
        private const string REDISCONNECTIONSTRING_ATTRIBUTE = "RedisConnectionString";
        private const string INITIALIZE_ATTRIBUTE = "Initialize";
        private const string CREATEINSTANCE_ATTRIBUTE = "CreateInstance";
        private const string CREATEVARIABLE_ATTRIBUTE = "CreateVariable";
        private const string VARIABLES_ELEMENT = "Variables";
        private const string VARIABLE_ELEMENT = "Variable";
        private const string INSTANCES_ELEMENT = "Instances";
        private const string INSTANCE_ELEMENT = "Instance";
        private const string UPDATEPROCESSES_ELEMENT = "UpdateProcesses";
        private const string UPDATEPROCESS_ELEMENT = "UpdateProcess";
        private const string POOLID_ATTRIBUTE = "PoolId";
        private const string SQL_ELEMENT = "SQL";
        private const string INSTANCEKEYCOLUMN_ATTRIBUTE = "InstanceKeyColumn";
        private const string SQLTEXT_ELEMENT = "SqlText"; 
        private const string UPDATEBATCHES_ELEMENT = "UpdateBatches";
        private const string UPDATEBATCH_ELEMENT = "UpdateBatch";
        private const string FIRSTRUNTRESHOLD_ATTRIBUTE = "FirstRunTreshold";
        private const string REPEATTRESHOLD_ATTRIBUTE = "RepeatTreshold";
        private const string SYNCHRONIZEDUPDATE_ATTRIBUTE = "SynchronizedUpdate";

        private const string SUBSTITUTIONS_ELEMENT = "Substitutions";
        private const string SUBSTITUTION_ELEMENT = "Substitution";
        private const string INITIALSQLSCRIPTS_ELEMENT = "InitialSQLScripts";
        private const string SQLSCRIPT_ELEMENT = "SQLScript";
        #endregion

        private XElement _rootXml = null;

        #region Constructor
        /// <summary>
        /// XML paraméterező fájl feldolgozásának konstruktora.
        /// </summary>
        /// <param name="fileName">XmlParser.xml fájl a teljes elérési útvonalával együtt.</param>
        /// <param name="configName">Példányosításhoz meg kell adni az xmlFile fizikai helyét (teljes útvonallal együtt).</param>
        /// <param name="id">Ezt a definíció kell feldolgozni. Ha nem létezik az XML hiba.</param>
        /// <param name="lcid">Alapértelmezett nyelvi beállítás. Ha null vagy üres, akkor</param>
        public DataToRedisConfigXmlProcessor(XElement rootxml)
        {
            _rootXml = rootxml;
            string rootxmlString = rootxml.ToString();
            List<VarSubstitution> substitutionList = this.GetVarSubstitutions();
            foreach (VarSubstitution varsubst in substitutionList)
            {
                if (!string.IsNullOrEmpty(varsubst.Name)) rootxmlString = rootxmlString.Replace($"@{varsubst.Name}@", varsubst.Value);
            }
            _rootXml = XElement.Parse(rootxmlString);
            _rootXml = this.RemoveSubstitutionsElement();
        }
        #endregion Constructor

        public int Frequency
        {
            get
            {
                XElement generalelement = _rootXml.Element(XName.Get(GENERAL_ELEMENT));
                if (generalelement == null) return 10;
                XElement frequencyelement = generalelement.Element(XName.Get(FREQUENCY_ELEMENT));
                if (frequencyelement == null) return 10;
                string resulttxt = frequencyelement.Value;
                if (!int.TryParse(resulttxt, out int result)) result = 10;
                return result;
            }
        }

        public class SqlScript
        {
            private XElement _sqlscriptElement = null;
            #region Constructor
            public SqlScript(XElement sqlscriptElement)
            {
                _sqlscriptElement = sqlscriptElement;
                var sqlscriptnameattribute = _sqlscriptElement.Attribute(XName.Get(NAME_ATTRIBUTE));
                var sqlconnectionstringattribute = _sqlscriptElement.Attribute(XName.Get(SQLCONNECTIONSTRING_ATTRIBUTE));
                this.Name = sqlscriptnameattribute == null ? string.Empty : sqlscriptnameattribute.Value;
                this.SqlConnectionString = sqlconnectionstringattribute == null ? string.Empty : sqlconnectionstringattribute.Value;
                this.Text = _sqlscriptElement.Value;
            }
            #endregion Constructor
            public string Name { get; }
            public string Text { get; }
            public string SqlConnectionString { get; }
        }
        public List<SqlScript> GetSqlScripts()
        {
            List<SqlScript> result = new List<SqlScript>();
            IEnumerable<XElement> sqlscriptelementList = _rootXml.Element(XName.Get(INITIALSQLSCRIPTS_ELEMENT)).Elements(XName.Get(SQLSCRIPT_ELEMENT));
            foreach (XElement sqlscriptelement in sqlscriptelementList) { result.Add(new SqlScript(sqlscriptelement)); }
            return result;
        }

        public class VarSubstitution
        {
            private XElement _varsubstitutionElement = null;
            #region Constructor
            public VarSubstitution(XElement varsubstitutionElement)
            {
                _varsubstitutionElement = varsubstitutionElement;
                var nameattribute = _varsubstitutionElement.Attribute(XName.Get(NAME_ATTRIBUTE));
                this.Name = nameattribute == null ? string.Empty : nameattribute.Value;
                this.Value = _varsubstitutionElement.Value;
            }
            #endregion Constructor
            public string Name { get; }
            public string Value { get; }
        }
        public List<VarSubstitution> GetVarSubstitutions()
        {
            List<VarSubstitution> result = new List<VarSubstitution>();
            IEnumerable<XElement> varsubstitutionelementList = _rootXml.Element(XName.Get(SUBSTITUTIONS_ELEMENT)).Elements(XName.Get(SUBSTITUTION_ELEMENT));
            foreach (XElement varsubstitutionelement in varsubstitutionelementList) { result.Add(new VarSubstitution(varsubstitutionelement)); }
            return result;
        }
        public XElement RemoveSubstitutionsElement()
        {
            XElement varsubstitutionselement = _rootXml.Element(XName.Get(SUBSTITUTIONS_ELEMENT));
            if (varsubstitutionselement != null) varsubstitutionselement.Remove();
            return _rootXml;
        }


        public class Pool
        {

            private XElement _poolElement = null;

            #region Constructor
            /// <summary>
            /// XML paraméterező fájl feldolgozásának konstruktora.
            /// </summary>
            /// <param name="fileName">XmlParser.xml fájl a teljes elérési útvonalával együtt.</param>
            /// <param name="configName">Példányosításhoz meg kell adni az xmlFile fizikai helyét (teljes útvonallal együtt).</param>
            /// <param name="id">Ezt a definíció kell feldolgozni. Ha nem létezik az XML hiba.</param>
            /// <param name="lcid">Alapértelmezett nyelvi beállítás. Ha null vagy üres, akkor</param>
            public Pool(XElement poolElement)
            {
                _poolElement = poolElement;
                var poolnameattribute = poolElement.Attribute(XName.Get(NAME_ATTRIBUTE));
                this.Name = poolnameattribute == null ? string.Empty : poolnameattribute.Value;

                var rediscsattribute = _poolElement.Attribute(XName.Get(REDISCONNECTIONSTRING_ATTRIBUTE));
                this.RedisConnectionStringTxt = rediscsattribute == null ? string.Empty : rediscsattribute.Value;

                var sqlcsattribute = _poolElement.Attribute(XName.Get(SQLCONNECTIONSTRING_ATTRIBUTE));
                this.SQLconnectionString = sqlcsattribute == null ? string.Empty : sqlcsattribute.Value;

                this.RedisConnectionString = new RedisConnectionString(this.RedisConnectionStringTxt, this.Name);
            }
            #endregion Constructor

            public string Name { get; }

            public string Id
            {
                get
                {
                    var idattribute = _poolElement.Attribute(XName.Get(ID_ATTRIBUTE));
                    string result = idattribute == null ? string.Empty : idattribute.Value;
                    return result;
                }
            }

            public string Version
            {
                get
                {
                    var versionattribute = _poolElement.Attribute(XName.Get(VERSION_ATTRIBUTE));
                    string result = versionattribute == null ? string.Empty : versionattribute.Value;
                    return $"V{result}";
                }
            }

            public string SQLconnectionString {get; }
            public string RedisConnectionStringTxt { get;  }
            public RedisConnectionString RedisConnectionString { get; }

            public bool Initialize
            {
                get
                {
                    var initializeattribute = _poolElement.Attribute(XName.Get(INITIALIZE_ATTRIBUTE));
                    bool result = initializeattribute == null ? false : (initializeattribute.Value.ToLower()==bool.TrueString.ToLower());
                    return result;
                }
            }

            public bool CreateInstance
            {
                get
                {
                    var createinstanceattribute = _poolElement.Attribute(XName.Get(CREATEINSTANCE_ATTRIBUTE));
                    bool result = createinstanceattribute == null ? false : (createinstanceattribute.Value.ToLower() == bool.TrueString.ToLower());
                    return result;
                }
            }

            public bool CreateVariable
            {
                get
                {
                    var createvariableattribute = _poolElement.Attribute(XName.Get(CREATEVARIABLE_ATTRIBUTE));
                    bool result = createvariableattribute == null ? false : (createvariableattribute.Value.ToLower() == bool.TrueString.ToLower());
                    return result;
                }
            }

            public List<PoolVariable> GetVariables()
            {
                List<PoolVariable> result = new List<PoolVariable>();

                if (_poolElement.Element(VARIABLES_ELEMENT) != null)
                {
                    IEnumerable<XElement> variables = _poolElement.Element(VARIABLES_ELEMENT).Elements(XName.Get(VARIABLE_ELEMENT));

                    foreach (XElement variable in variables)
                    {
                        XAttribute nameattribute = variable.Attribute(XName.Get(NAME_ATTRIBUTE));
                        XAttribute typeattribute = variable.Attribute(XName.Get(TYPE_ATTRIBUTE));
                        string name = nameattribute == null ? string.Empty : nameattribute.Value;
                        string type = typeattribute == null ? string.Empty : typeattribute.Value;
                        result.Add(new PoolVariable(name, type));
                    }
                }

                return result;
            }

            public class PoolVariable
            {
                public PoolVariable(string name, string type)
                {
                    Name = name;

                    switch (type.ToUpper())
                    {
                        case "BOOLEAN":
                            Type = DataType.Boolean;
                            break;
                        case "DATETIME":
                            Type = DataType.DateTime;
                            break;
                        case "DOUBLE":
                            Type = DataType.Double;
                            break;
                        case "INT32":
                            Type = DataType.Int32;
                            break;
                        case "STRING":
                            Type = DataType.String;
                            break;
                        case "TIMECOUNTER":
                            Type = DataType.TimeCounter;
                            break;
                        default:
                            Type = DataType.String;
                            break;
                    }
                }

                public string Name { get; set; }
                public DataType Type { get; set; }

            }

            public List<Instance> GetInstances()
            {
                List<Instance> result = new List<Instance>();

                if (_poolElement.Element(VARIABLES_ELEMENT) != null)
                {
                    IEnumerable<XElement> instances = _poolElement.Element(INSTANCES_ELEMENT).Elements(XName.Get(INSTANCE_ELEMENT));

                    foreach (XElement instance in instances)
                    {
                        XAttribute nameattribute = instance.Attribute(XName.Get(NAME_ATTRIBUTE));
                        string name = nameattribute == null ? string.Empty : nameattribute.Value;
                        result.Add(new Instance(name));
                    }
                }

                return result;
            }

            public class Instance
            {
                public Instance(string name)
                {
                    Name = name;
                }

                public string Name { get; set; }
            }

            /// <summary>
            /// Redis connectionstring feldolgozása
            /// </summary>
            //private void ParseRedisConnectionString(string rcs, out string poolname, out string redisserver, out int redisport, out Serializers redisser)
            //{

            //    redisserver = null;
            //    poolname = null;
            //    redisport = 6379;
            //    redisser = Serializers.XML;
            //    const string cstringerrmsg = "Error in Redis connection string!";
            //    const char ELEMENTSEPARATOR = ';';
            //    const char NAMEVALUESEPARATOR = '=';
            //    if (string.IsNullOrWhiteSpace(rcs)) throw new Exception($"{cstringerrmsg} (connectionstring is empty)");
            //    string[] rcssplit = rcs.Trim().Split(ELEMENTSEPARATOR);
            //    foreach (string par in rcssplit)
            //    {
            //        if (string.IsNullOrWhiteSpace(par)) continue;
            //        string[] parsplit = par.Trim().Split(NAMEVALUESEPARATOR);
            //        if (parsplit.Length != 2) throw new Exception($"{cstringerrmsg} (parameter format is not NAME=VALUE)");
            //        string pn = parsplit[0].Trim().ToLower();
            //        string pv = parsplit[1].Trim().ToLower();
            //        if (string.IsNullOrWhiteSpace(pn) || string.IsNullOrWhiteSpace(pv)) { throw new Exception($"{cstringerrmsg} (parameter name empty)"); }
            //        switch (pn)
            //        {
            //            case "server": redisserver = pv; break;
            //            case "port": if (!int.TryParse(pv, out redisport)) { throw new Exception(); } break;
            //            case "pool": poolname = pv; break;
            //            case "serialization": if (!Serializers.TryParse(pv.ToUpper(), out redisser)) { throw new Exception($"{cstringerrmsg} (value of Serialization is not 'XML' or 'JSON')"); } break;
            //            default: throw new Exception($"{cstringerrmsg} (unknown parameter name: {pn})");
            //        }
            //    }
            //    if (string.IsNullOrWhiteSpace(redisserver)) throw new Exception($"{cstringerrmsg} (redis server name is empty)");
            //    return;
            //}
        }
        public List<Pool> GetPools()
        {
            List<Pool> result = new List<Pool>();
            IEnumerable<XElement> poolelementList = _rootXml.Element(XName.Get(POOLS_ELEMENT)).Elements(XName.Get(POOL_ELEMENT));
            foreach (XElement poolelement in poolelementList) {    result.Add(new Pool(poolelement));    }
            return result;
        }

        public class UpdateProcess
        {
            private XElement _updateProcessElement = null;

            #region Constructor
            public UpdateProcess(XElement updateProcessElement)
            {
                _updateProcessElement = updateProcessElement;
            }
            #endregion Constructor

            public string UpdateProcessName
            {
                get
                {
                    var nameattribute = _updateProcessElement.Attribute(XName.Get(NAME_ATTRIBUTE));
                    string result = nameattribute == null ? string.Empty : nameattribute.Value;
                    return result;
                }
            }

            public string UpdateProcessType
            {
                get
                {
                    var typeattribute = _updateProcessElement.Attribute(XName.Get(TYPE_ATTRIBUTE));
                    string result = typeattribute == null ? string.Empty : typeattribute.Value;
                    return result;
                }
            }

            public string UpdateProcessPoolId
            {
                get
                {
                    var poolidattribute = _updateProcessElement.Attribute(XName.Get(POOLID_ATTRIBUTE));
                    string result = poolidattribute == null ? string.Empty : poolidattribute.Value;
                    return result;
                }
            }

            public string UpdateProcessSQLInstanceKeyColumn
            {
                get
                {
                    var sqlelement = _updateProcessElement.Element(XName.Get(SQL_ELEMENT));
                    if (sqlelement == null) return string.Empty;
                    var instancekeycolumnattribute = sqlelement.Attribute(XName.Get(INSTANCEKEYCOLUMN_ATTRIBUTE));
                    if (instancekeycolumnattribute == null) return string.Empty;
                    return instancekeycolumnattribute.Value;
                }
            }

            public string UpdateProcessSQLSqlText
            {
                get
                {
                    var sqlelement = _updateProcessElement.Element(XName.Get(SQL_ELEMENT)); if (sqlelement == null) return string.Empty;
                    var sqltextelement = sqlelement.Element(XName.Get(SQLTEXT_ELEMENT)); if (sqltextelement == null) return string.Empty;
                    string sqlText = sqltextelement.Value;
                    sqlText = sqlText.Replace("&amp;gt;", ">").Replace("&amp;lt;", "<").Replace("&amp;apos;", "'").Replace("&amp;quot;", "\"");
                    return (new Regex(@"\s\s+")).Replace(sqlText, " ");
                }
            }

            public List<UpdateProcessVariable> GetUpdateProcessVariables()
            {
                List<UpdateProcessVariable> result = new List<UpdateProcessVariable>();

                if (_updateProcessElement.Element(SQL_ELEMENT) != null &&
                    _updateProcessElement.Element(SQL_ELEMENT).Element(VARIABLES_ELEMENT) != null)
                {
                    IEnumerable<XElement> variables = _updateProcessElement.Element(SQL_ELEMENT).Element(VARIABLES_ELEMENT).Elements(XName.Get(VARIABLE_ELEMENT));

                    foreach (XElement variable in variables)
                    {
                        XAttribute nameattribute = variable.Attribute(XName.Get(NAME_ATTRIBUTE));
                        string name = nameattribute == null ? string.Empty : nameattribute.Value;
                        XAttribute columnattribute = variable.Attribute(XName.Get(COLUMN_ATTRIBUTE));
                        string column = columnattribute == null ? string.Empty : columnattribute.Value;
                        result.Add(new UpdateProcessVariable(name, column));
                    }
                }

                return result;
            }

            public class UpdateProcessVariable
            {
                public UpdateProcessVariable(string name, string column)
                {
                    Name = name;
                    Column = column;
                }

                public string Name { get; set; }
                public string Column { get; set; }
            }

        }
        public List<UpdateProcess> GetUpdateProcesses()
        {
            List<UpdateProcess> result = new List<UpdateProcess>();
            var updateprocesseselement = _rootXml.Element(XName.Get(UPDATEPROCESSES_ELEMENT));
            if (updateprocesseselement != null)
            {
                IEnumerable<XElement> updateProcessElementList = updateprocesseselement.Elements(XName.Get(UPDATEPROCESS_ELEMENT));
                foreach (XElement updateProcess in updateProcessElementList) {   result.Add(new UpdateProcess(updateProcess));   }
            }
            return result;
        }

        public int UpdateBatchFirsRunTreshold
        {
            get
            {
                XAttribute updatebatchfirstruntreshold = _rootXml.Element(XName.Get(UPDATEBATCHES_ELEMENT)).Attribute(XName.Get(FIRSTRUNTRESHOLD_ATTRIBUTE));
                if (updatebatchfirstruntreshold == null) return 10;
                if (!int.TryParse(updatebatchfirstruntreshold.Value, out int result)) return 10;
                return result;
            }
        }

        public bool UpdateBatchSynchronizedUpdateCommon
        {
            get
            {
                XAttribute synchronizedupdateCommon = _rootXml.Element(XName.Get(UPDATEBATCHES_ELEMENT)).Attribute(XName.Get(SYNCHRONIZEDUPDATE_ATTRIBUTE));
                return synchronizedupdateCommon == null ? false : synchronizedupdateCommon.Value.ToLower() == bool.TrueString.ToLower();
            }
        }

        public int UpdateBatchRepeatTreshold
        {
            get
            {
                XAttribute updatebatchrepeattreshold = _rootXml.Element(XName.Get(UPDATEBATCHES_ELEMENT)).Attribute(XName.Get(REPEATTRESHOLD_ATTRIBUTE));
                if (updatebatchrepeattreshold == null) return 10;
                if (!int.TryParse(updatebatchrepeattreshold.Value, out int result)) return 10;
                return result;
            }
        }

        public class UpdateBatch
        {
            private XElement _updateBatchElement = null;
            private bool _updatebatchsynchronizedupdateCommon;

            #region Constructor
            /// <summary>
            /// XML paraméterező fájl feldolgozásának konstruktora.
            /// </summary>
            /// <param name="fileName">XmlParser.xml fájl a teljes elérési útvonalával együtt.</param>
            /// <param name="configName">Példányosításhoz meg kell adni az xmlFile fizikai helyét (teljes útvonallal együtt).</param>
            /// <param name="id">Ezt a definíció kell feldolgozni. Ha nem létezik az XML hiba.</param>
            /// <param name="lcid">Alapértelmezett nyelvi beállítás. Ha null vagy üres, akkor</param>
            public UpdateBatch(XElement updateBatchElement, bool updatebatchsynchronizedupdateCommon)
            {
                _updateBatchElement = updateBatchElement;
                _updatebatchsynchronizedupdateCommon = updatebatchsynchronizedupdateCommon;

            }
            #endregion Constructor

            public string Name
            {
                get
                {
                    XAttribute nameattribute = _updateBatchElement.Attribute(XName.Get(NAME_ATTRIBUTE));
                    string result = nameattribute == null ? string.Empty : nameattribute.Value;
                    return result;
                }
            }

            public int Frequency
            {
                get
                {
                    XAttribute frequencyattribute = _updateBatchElement.Attribute(XName.Get(FREQUENCY_ATTRIBUTE));
                    if (frequencyattribute == null) return 15;
                    if (!int.TryParse(frequencyattribute.Value, out int resultint)) return 15;
                    return resultint;
                }
            }

            public bool SynchronizedUpdate
            {
                get
                {
                    XAttribute syncupdateattribute = _updateBatchElement.Attribute(XName.Get(SYNCHRONIZEDUPDATE_ATTRIBUTE));
                    return syncupdateattribute == null ? _updatebatchsynchronizedupdateCommon : syncupdateattribute.Value.ToLower() == bool.TrueString.ToLower();
                }
            }

            public List<UpdateProcess> GetUpdateProcesses()
            {
                List<UpdateProcess> result = new List<UpdateProcess>();

                if (_updateBatchElement.Element(UPDATEPROCESSES_ELEMENT) != null)
                {
                    IEnumerable<XElement> updateProcesses = _updateBatchElement.Element(UPDATEPROCESSES_ELEMENT).Elements(XName.Get(UPDATEPROCESS_ELEMENT));

                    foreach (XElement updateProcess in updateProcesses)
                    {
                        XAttribute nameattribute = updateProcess.Attribute(XName.Get(NAME_ATTRIBUTE));
                        string name = nameattribute == null ? string.Empty : nameattribute.Value;
                        result.Add(new UpdateProcess(name));
                    }
                }

                return result;
            }

            public class UpdateProcess
            {
                public UpdateProcess(string name)
                {
                    Name = name;
                }

                public string Name { get; set; }
            }
        }
        public List<UpdateBatch> GetUpdateBatches()
        {
            List<UpdateBatch> result = new List<UpdateBatch>();
            XElement updatebatcheselement = _rootXml.Element(XName.Get(UPDATEBATCHES_ELEMENT));
            if (updatebatcheselement != null)
            {
                IEnumerable<XElement> updateBatchElementList = updatebatcheselement.Elements(XName.Get(UPDATEBATCH_ELEMENT));
                foreach (XElement updateBatch in updateBatchElementList) {   result.Add(new UpdateBatch(updateBatch, this.UpdateBatchSynchronizedUpdateCommon));   }
            }
            return result;
        }
    }
    public static class KeyValuePairExtensions
    {
        public static bool IsNull<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(new KeyValuePair<T, TU>());
        }
    }

}