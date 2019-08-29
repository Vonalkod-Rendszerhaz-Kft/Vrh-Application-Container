using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Vrh.Logger;
using Vrh.Redis.DataPoolHandler;
using Vrh.ApplicationContainer;

namespace Vrh.DataToRedisCore
{
    public class SQLToRedisControl: IDisposable
    {
        DataToRedisConfigXmlProcessor dtrc;
        Timer timer;
        public List<PoolHandler> poolHandlers = null;
        List<UpdateBatchControl> updateBatchControls = null;

        public SQLToRedisControl(XElement dtrcElement, Action stopServiceAction, PluginAncestor pluginReference)
        {
            _pluginReference = pluginReference;
            try
            {
                dtrc = new DataToRedisConfigXmlProcessor(dtrcElement);


                List<DataToRedisConfigXmlProcessor.SqlScript> InitialSqlScriptList = dtrc.GetSqlScripts();
                if (InitialSqlScriptList.Count > 0)
                {
                    foreach (DataToRedisConfigXmlProcessor.SqlScript InitialSqlScript in InitialSqlScriptList)
                    {
                        runSqlScript(InitialSqlScript.Name, InitialSqlScript.Text, InitialSqlScript.SqlConnectionString);
                    }
                }



                poolHandlers = new List<PoolHandler>();

                List<DataToRedisConfigXmlProcessor.Pool> pools = dtrc.GetPools();

                foreach (DataToRedisConfigXmlProcessor.Pool pool in pools) {   CheckPool(pool);   }

                updateBatchControls = new List<UpdateBatchControl>();
                List<DataToRedisConfigXmlProcessor.UpdateBatch> updateBatches = dtrc.GetUpdateBatches();

                if (updateBatches.Count > 0)
                {
                    foreach (DataToRedisConfigXmlProcessor.UpdateBatch updateBatch in updateBatches)
                    {
                        updateBatchControls.Add(new UpdateBatchControl(dtrc, updateBatch, poolHandlers, _pluginReference));
                    }
                }
                else
                {
                    //timer = new Timer
                    //{
                    //    Interval = dtrc.Frequency * 1000

                    //};
                    //timer.Elapsed += Timer_Elapsed;
                    //timer.Start();
                }
            }
            catch (Exception ex)
            {
                _pluginReference.LogThis("Exception in SQLToRedisControl constructor.", null, ex, LogLevel.Error, this.GetType());
                stopServiceAction.Invoke();
            }
        }

        private bool runSqlScript(string scriptname, string scripttext, string connectionString)
        {
            try
            {
                // split script on GO command
                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(scripttext, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                string cs;
                try { cs = VRH.ConnectionStringStore.VRHConnectionStringStore.GetSQLConnectionString(connectionString, false);  } 
                catch { cs = connectionString; }
                
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    connection.Open();
                    foreach (string commandString in commandStrings)
                    {
                        if (commandString.Trim() != "")
                        {
                            using (var command = new SqlCommand(commandString, connection))
                            {
                                try { command.ExecuteNonQuery(); }
                                catch (SqlException ex)
                                {
                                    string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;

                                    var logData = new Dictionary<string, string>();
                                    logData.Add("Line", $"{ex.LineNumber}");
                                    logData.Add("SQL Command", spError);
                                    _pluginReference.LogThis($"Error in SQL script text.\nScript name: {scriptname}", logData, ex , LogLevel.Error, this.GetType());
                                    return false;
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                _pluginReference.LogThis($"Error in SQL script processing.\nScript name: {scriptname}", null, ex, LogLevel.Error, this.GetType());
                return false;
            }
        }

        /// <summary>
        /// Ellenőrzi, és ha szükséges létrehozza a Redis pool-t
        /// </summary>
        public void CheckPool(DataToRedisConfigXmlProcessor.Pool pool)
        {
            // a redis connectionstringben levő poolname elemet nem használjuk itt!!!!
            PoolHandler poolHandler = new PoolHandler(pool.RedisConnectionString);
            //PoolHandler poolHandler = new PoolHandler(pool.Name,pool.Server,pool.ServerPort,pool.Serializer);

            if (poolHandler.IsPoolExists())
            {
                string poolVersion = poolHandler.GetPoolVersion();
                if (poolVersion != pool.Version)
                {
                    if (pool.Initialize)
                    {
                        poolHandler.RemovePool();
                        poolHandler.RegisterPool(pool.Version);
                    }
                    else
                    {
                        var logData = new Dictionary<string, string>();
                        logData.Add("Redis pool name", pool.Name);
                        logData.Add("Redis pool version", poolVersion);
                        logData.Add("Redis configured pool version", pool.Version);
                        logData.Add("Redis connection string", pool.RedisConnectionString.ToString());
                        _pluginReference.LogThis("$Error in registering existing Redis pool. Pool version mismatch.", logData, null, LogLevel.Error, this.GetType());
                    }
                }
                else
                {
                    if (pool.Initialize)
                    {
                        poolHandler.RemovePool();
                        poolHandler.RegisterPool(pool.Version);
                    }
                }
            }
            else
            {
                if (pool.Initialize)
                {
                    poolHandler.RegisterPool(pool.Version);
                }
                else
                {
                    var logData = new Dictionary<string, string>();
                    logData.Add("Redis pool name", pool.Name);
                    logData.Add("Redis configured pool version", pool.Version);
                    logData.Add("Redis connection string", pool.RedisConnectionString.ToString());
                    _pluginReference.LogThis("$Error in registering non-existing Redis pool.", logData, null, LogLevel.Error, this.GetType());
                }
            }

            if (poolHandler.IsPoolExists())
            {
                poolHandlers.Add(poolHandler);

                if (pool.Initialize)
                {
                    Dictionary<string, DataType> registeredKeys = poolHandler.GetRegisteredKeys();

                    foreach (DataToRedisConfigXmlProcessor.Pool.PoolVariable variable in pool.GetVariables())
                    {
                        var key = registeredKeys.SingleOrDefault(x => x.Key == variable.Name);

                        if (key.IsNull())
                        {
                            poolHandler.RegisterKey(new RedisPoolKeyDefination() { DataType = variable.Type, Name = variable.Name });
                        }
                    }

                    foreach (DataToRedisConfigXmlProcessor.Pool.Instance instance in pool.GetInstances())
                    {
                        pool.RedisConnectionString.InstanceName = instance.Name;
                        using (InstanceWriter instanceWriter = new InstanceWriter(pool.RedisConnectionString))
                        {
                            instanceWriter.StrictMode = false;

                            if (!instanceWriter.IsPoolInstanceExists() && pool.CreateInstance)
                            {
                                instanceWriter.RegisterInstance();
                            }
                        }
                    }
                }
            }
            else
            {
                var logData = new Dictionary<string, string>();
                logData.Add("Redis connection string", pool.RedisConnectionString.ToString());
                _pluginReference.LogThis($"Redis pool does not exist.", logData, null, LogLevel.Error, this.GetType());
            }
        }

        //private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        timer.Stop();

        //        List<DataToRedisConfigXmlProcessor.UpdateProcess> updateProcesses = dtrc.GetUpdateProcesses();

        //        foreach (DataToRedisConfigXmlProcessor.UpdateProcess updateProcess in updateProcesses)
        //        {
        //            DataToRedisConfigXmlProcessor.Pool pool = dtrc.GetPools().FirstOrDefault(x => x.PoolId == updateProcess.UpdateProcessPoolId);

        //            if (pool != null)
        //            {
        //                PoolHandler poolHandler = poolHandlers.FirstOrDefault(x => x.Name == pool.PoolName);

        //                string connectionString = pool.PoolSQLconnectionString;

        //                string queryString = updateProcess.UpdateProcessSQLSqlText;

        //                using (SqlConnection connection = new SqlConnection(connectionString))
        //                {
        //                    SqlCommand command = null;
        //                    SqlDataReader reader = null;
        //                    try
        //                    {
        //                        command = new SqlCommand(queryString, connection);
        //                        connection.Open();
        //                        reader = command.ExecuteReader();

        //                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

        //                        RegisterNotDeclaredVariables(reader, columns, updateProcess, poolHandler);

        //                        InstanceUpdate(reader, columns, updateProcess, pool);

        //                        reader.Close();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        //Console.WriteLine(ex.Message);
        //                        VrhLogger.Log(ex, this.GetType(), LogLevel.Error);
        //                        if (reader != null && !reader.IsClosed)
        //                        {
        //                            reader.Close();
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                VrhLogger.Log($"Missing pool configuration: {updateProcess.UpdateProcessPoolId}", LogLevel.Error, this.GetType());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        VrhLogger.Log(ex, this.GetType(), LogLevel.Error);
        //    }
        //    finally
        //    {
        //        timer.Start();
        //    }
        //}

        //private void InstanceUpdate(SqlDataReader reader, 
        //                            List<string> columns, 
        //                            DataToRedisConfigXmlProcessor.UpdateProcess updateProcess,
        //                            DataToRedisConfigXmlProcessor.Pool pool)
        //{
        //    while (reader.Read())
        //    {
        //        InstanceWriter instanceWriter = new InstanceWriter(pool.PoolName, reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString());
        //        instanceWriter.StrictMode = false;

        //        if (!instanceWriter.IsPoolInstanceExists() && pool.PoolInstanceExtensible)
        //        {
        //            instanceWriter.RegisterInstance();
        //        }

        //        if (instanceWriter.IsPoolInstanceExists())
        //        {
        //            List<DataToRedisConfigXmlProcessor.UpdateProcess.UpdateProcessVariable> variables = updateProcess.GetUpdateProcessVariables();
        //            if (variables.Count > 0)
        //            {
        //                foreach (DataToRedisConfigXmlProcessor.UpdateProcess.UpdateProcessVariable variable in variables)
        //                {
        //                    string name = !string.IsNullOrEmpty(variable.Name) ? variable.Name : variable.Column;
        //                    string column = !string.IsNullOrEmpty(variable.Column) ? variable.Column : variable.Name;

        //                    instanceWriter.WriteKeyValue(name, reader[column]);
        //                    VrhLogger.Log($"{name} instance key has been set to {reader[column]} value in {reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance in {pool.PoolName} pool!", LogLevel.Debug, this.GetType());
        //                }
        //            }
        //            else
        //            {
        //                //Nincs megadva Variables az UpdateProcessen belül ezért az összes oszlop beírásra kerül
        //                columns.ForEach(x =>
        //                {
        //                    instanceWriter.WriteKeyValue(x, reader[x]);
        //                    VrhLogger.Log($"{x} instance key has been set to {reader[x]} value in {reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance in {pool.PoolName} pool!", LogLevel.Debug, this.GetType());
        //                });
        //            }
        //        }
        //        else
        //        {
        //            VrhLogger.Log(new ApplicationException($"{reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance not exist in {pool.PoolName} pool!"), this.GetType(), LogLevel.Error);
        //        }
        //    }
        //}

        //private void RegisterNotDeclaredVariables(SqlDataReader reader, 
        //                                          List<string> columns, 
        //                                          DataToRedisConfigXmlProcessor.UpdateProcess updateProcess,
        //                                          PoolHandler poolHandler)
        //{
        //    columns.ForEach(x =>
        //    {
        //        if (!poolHandler.IsKeyExists(x))
        //        {
        //            Type sqlType = reader.GetFieldType(reader.GetOrdinal(x));
        //            DataType type;

        //            switch (sqlType.ToString().Replace("System.", string.Empty).ToUpper())
        //            {
        //                case "BOOLEAN":
        //                    type = DataType.Boolean;
        //                    break;
        //                case "DATETIME":
        //                    type = DataType.DateTime;
        //                    break;
        //                case "DOUBLE":
        //                    type = DataType.Double;
        //                    break;
        //                case "INT32":
        //                    type = DataType.Int32;
        //                    break;
        //                case "STRING":
        //                    type = DataType.String;
        //                    break;
        //                case "TIMECOUNTER":
        //                    type = DataType.TimeCounter;
        //                    break;
        //                default:
        //                    type = DataType.String;
        //                    break;
        //            }

        //            poolHandler.RegisterKey(new RedisPoolKeyDefination() { DataType = type, Name = x });
        //        }
        //    });
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private PluginAncestor _pluginReference = null;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    foreach (UpdateBatchControl updateBatchControl in updateBatchControls)
                    {
                        updateBatchControl.Dispose();
                    }

                    foreach (PoolHandler poolHandler in poolHandlers)
                    {
                        poolHandler.Dispose();
                    }

                    if (timer != null)
                    {
                        if (timer.Enabled)
                        {
                            timer.Stop();
                        }

                        timer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SQLToRedis() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
