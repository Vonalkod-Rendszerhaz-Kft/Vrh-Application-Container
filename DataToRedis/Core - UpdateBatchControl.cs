using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Timers;
using Vrh.Logger;
using Vrh.Redis.DataPoolHandler;
using Vrh.ApplicationContainer;

namespace Vrh.DataToRedisCore
{
    public class UpdateBatchControl: IDisposable
    {
        Timer _timer;
        DataToRedisConfigXmlProcessor _dtrc;
        DataToRedisConfigXmlProcessor.UpdateBatch _updateBatch;
        List<PoolHandler> _poolHandlers;

        public string Name { get; set; }

        public UpdateBatchControl(DataToRedisConfigXmlProcessor dtrc, DataToRedisConfigXmlProcessor.UpdateBatch updateBatch, List<PoolHandler> poolHandlers, PluginAncestor pluginReference)
        {
            _pluginReference = pluginReference;
            _dtrc = dtrc;
            _updateBatch = updateBatch;
            _poolHandlers = poolHandlers;

            Name = _updateBatch.Name;

            Random rnd = new Random();
            int highestValue = (int)Math.Round(((100 - (double)_dtrc.UpdateBatchFirsRunTreshold) / 100) * ((double)_updateBatch.Frequency * 1000));
            int interval = rnd.Next(1, highestValue);
            _timer = new Timer
            {
                Interval = interval
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private PluginAncestor _pluginReference = null;

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                try
                {
                    _timer.Stop();
                    //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}###  {Name} batch process start");
                    List<DataToRedisConfigXmlProcessor.UpdateBatch.UpdateProcess> batchUpdateProcesseses = _updateBatch.GetUpdateProcesses();

                    if (!_updateBatch.SynchronizedUpdate)
                    {
                        foreach (DataToRedisConfigXmlProcessor.UpdateBatch.UpdateProcess batchUpdateProcess in batchUpdateProcesseses)
                        {
                            Task.Run(() => UpdateProcessRunAsync(batchUpdateProcess.Name));
                        }
                    }
                    else
                    {
                        UpdateProcessRunSync(batchUpdateProcesseses);
                    }
                }
                catch (Exception ex)
                {
                    _pluginReference.LogThis($"Exception in execution of update process.", null, ex, LogLevel.Error, this.GetType());
                }
                finally
                {
                    Random rnd = new Random();
                    int lowestValue = (int)Math.Round(((100 - (double)_dtrc.UpdateBatchRepeatTreshold) / 100) * ((double)_updateBatch.Frequency * 1000));
                    int highestValue = (int)Math.Round(((100 + (double)_dtrc.UpdateBatchRepeatTreshold) / 100) * ((double)_updateBatch.Frequency * 1000));
                    int interval = rnd.Next(lowestValue, highestValue);

                    _timer.Interval = interval != 0 ? interval : 100;
                    //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}###  {Name} batch process stop");
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateProcessRunAsync(string name)
        {
            DataToRedisConfigXmlProcessor.UpdateProcess updateProcess = _dtrc.GetUpdateProcesses().FirstOrDefault(x => x.UpdateProcessName == name);

            if (updateProcess != null)
            {
                DataToRedisConfigXmlProcessor.Pool pool = _dtrc.GetPools().FirstOrDefault(x => x.Id == updateProcess.UpdateProcessPoolId);

                if (pool != null)
                {
                    PoolHandler poolHandler = _poolHandlers.FirstOrDefault(x => x.Name == pool.Name);

                    string connectionString = pool.SQLconnectionString;

                    string queryString = updateProcess.UpdateProcessSQLSqlText;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = null;
                        SqlDataReader reader = null;
                        try
                        {
                            command = new SqlCommand(queryString, connection);
                            connection.Open();
                            reader = command.ExecuteReader();

                            var logData = new Dictionary<string, string>();
                            logData.Add("Update process Name", updateProcess.UpdateProcessName);
                            logData.Add("SQL query text", queryString);
                            _pluginReference.LogThis($"Executing SQL query in update process.", logData, null, LogLevel.Information, this.GetType());

                            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

                            if (pool.CreateVariable && updateProcess.GetUpdateProcessVariables().Count == 0)
                            {
                                RegisterNotDeclaredVariables(reader, columns, updateProcess, poolHandler);
                            }

                            InstanceUpdate(reader, columns, updateProcess, pool, poolHandler);

                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.Message);
                            var logData = new Dictionary<string, string>();
                            logData.Add("Update process Name", updateProcess.UpdateProcessName);
                            logData.Add("SQL query text", queryString);
                            _pluginReference.LogThis($"Exception in execution of update process.", logData, ex, LogLevel.Error, this.GetType());
                            if (reader != null && !reader.IsClosed)
                            {
                                reader.Close();
                            }
                        }
                    }
                }
                else
                {
                    var logData = new Dictionary<string, string>();
                    logData.Add("Update process pool Id", updateProcess.UpdateProcessPoolId);
                    _pluginReference.LogThis($"Missing pool configuration.", logData, null, LogLevel.Error, this.GetType());
                }
            }
            else
            {
                var logData = new Dictionary<string, string>();
                logData.Add("update process name", updateProcess.UpdateProcessName);
                _pluginReference.LogThis($"Missing update process configuration.", logData, null, LogLevel.Error, this.GetType());
            }
        }

        private void UpdateProcessRunSync(List<DataToRedisConfigXmlProcessor.UpdateBatch.UpdateProcess> batchUpdateProcesses)
        {
            List<Process> processes = new List<Process>();

            foreach (DataToRedisConfigXmlProcessor.UpdateBatch.UpdateProcess batchUpdateProcess in batchUpdateProcesses)
            {
                Process process = new Process();

                DataToRedisConfigXmlProcessor.UpdateProcess updateProcess = _dtrc.GetUpdateProcesses().FirstOrDefault(x => x.UpdateProcessName == batchUpdateProcess.Name);                

                if (updateProcess != null)
                {
                    process.UpdateProcess = updateProcess;

                    DataToRedisConfigXmlProcessor.Pool pool = _dtrc.GetPools().FirstOrDefault(x => x.Id == updateProcess.UpdateProcessPoolId);

                    if (pool != null)
                    {
                        process.Pool = pool;

                        PoolHandler poolHandler = _poolHandlers.FirstOrDefault(x => x.Name == pool.Name);

                        if (poolHandler != null)
                        {
                            process.PoolHandler = poolHandler;

                            string connectionString = pool.SQLconnectionString;

                            string queryString = updateProcess.UpdateProcessSQLSqlText;

                            process.Connection = new SqlConnection(connectionString);
                            
                            try
                            {
                                process.Command = new SqlCommand(queryString, process.Connection);
                                process.Connection.Open();
                                process.Reader = process.Command.ExecuteReader();

                                var logData = new Dictionary<string, string>();
                                logData.Add("Update process Name", process.UpdateProcess.UpdateProcessName);
                                logData.Add("SQL query text", queryString);
                                _pluginReference.LogThis($"Executing SQL query in update process.", logData, null, LogLevel.Information, this.GetType());

                                process.Columns = Enumerable.Range(0, process.Reader.FieldCount).Select(process.Reader.GetName).ToList();

                                processes.Add(process);
                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine(ex.Message);
                                var logData = new Dictionary<string, string>();
                                logData.Add("Update process Name", process.UpdateProcess.UpdateProcessName);
                                logData.Add("SQL query text", queryString);
                                _pluginReference.LogThis("$Exception in execution of update process.", logData, ex, LogLevel.Error, this.GetType());
                                process.Dispose();
                            }
                            
                        }
                    }
                    else
                    {
                        var logData = new Dictionary<string, string>();
                        logData.Add("Update process pool Id", updateProcess.UpdateProcessPoolId);
                        _pluginReference.LogThis($"Missing pool configuration.", logData, null, LogLevel.Error, this.GetType());
                    }
                }
                else
                {
                    var logData = new Dictionary<string, string>();
                    logData.Add("update process name", updateProcess.UpdateProcessName);
                    _pluginReference.LogThis($"Missing update process configuration.", logData, null, LogLevel.Error, this.GetType());
                }
            }

            //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}###### All reader finished");

            foreach (Process process in processes)
            {
                try
                {
                    if (process.Pool.CreateVariable && process.UpdateProcess.GetUpdateProcessVariables().Count == 0)
                    {
                        RegisterNotDeclaredVariables(process.Reader, process.Columns, process.UpdateProcess, process.PoolHandler);
                    }
                    //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}######### Register not declared variables finished");

                    InstanceUpdate(process.Reader, process.Columns, process.UpdateProcess, process.Pool, process.PoolHandler);

                    //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}######### Instance update finished");
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    _pluginReference.LogThis("$Exception in execution of update process.", null, ex, LogLevel.Error, this.GetType());
                }
                finally
                {
                    process.Dispose();
                }
            }

            //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}###### Redis update finished");
        }

        private void InstanceUpdate(SqlDataReader reader,
                            List<string> columns,
                            DataToRedisConfigXmlProcessor.UpdateProcess updateProcess,
                            DataToRedisConfigXmlProcessor.Pool pool,
                            PoolHandler poolHandler)
        {
            List<string> instanceNameList = new List<string>();
            List<ExtendedOneData> extendedOneDataList = new List<ExtendedOneData>();

            while (reader.Read())
            {
                try
                {
                    string instanceName = reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString();
                    if (!instanceNameList.Any(x => x == instanceName))
                    {
                        instanceNameList.Add(instanceName);
                    }                    
                    List<DataToRedisConfigXmlProcessor.UpdateProcess.UpdateProcessVariable> variables = updateProcess.GetUpdateProcessVariables();
                    if (variables.Count > 0)
                    {
                        foreach (DataToRedisConfigXmlProcessor.UpdateProcess.UpdateProcessVariable variable in variables)
                        {
                            ExtendedOneData extendedOneData = new ExtendedOneData();
                            extendedOneData.InstanceName = instanceName;

                            string name = string.Empty;
                            string column = string.Empty;

                            name = !string.IsNullOrEmpty(variable.Name) ? variable.Name : variable.Column;
                            column = !string.IsNullOrEmpty(variable.Column) ? variable.Column : variable.Name;

                            if (!string.IsNullOrEmpty(name) &&
                                !string.IsNullOrEmpty(column) &&
                                columns.Any(x => x == column))
                            {

                                if (reader[column].GetType().Name != DBNull.Value.GetType().Name)
                                {
                                    extendedOneData.OneData.DataKey = name;

                                    switch (reader[column].GetType().Name)
                                    {
                                        case "Boolean":
                                            extendedOneData.OneData.FieldType = DataType.Boolean;
                                            break;
                                        case "DateTime":
                                            extendedOneData.OneData.FieldType = DataType.DateTime;
                                            break;
                                        case "Double":
                                            extendedOneData.OneData.FieldType = DataType.Double;
                                            break;
                                        case "Int32":
                                            extendedOneData.OneData.FieldType = DataType.Int32;
                                            break;
                                        case "ASETimeCounter":
                                            //Ilyen sose lesz...
                                            extendedOneData.OneData.FieldType = DataType.TimeCounter;
                                            break;
                                        default:
                                            extendedOneData.OneData.FieldType = DataType.String;
                                            break;
                                    }

                                    extendedOneData.OneData.Value = reader[column];

                                    if (pool.CreateVariable)
                                    {
                                        if (!poolHandler.IsKeyExists(name))
                                        {
                                            poolHandler.RegisterKey(new RedisPoolKeyDefination()
                                            {
                                                DataType = extendedOneData.OneData.FieldType,
                                                Name = extendedOneData.OneData.DataKey
                                            });
                                        }
                                    }

                                    if (poolHandler.IsKeyExists(extendedOneData.OneData.DataKey))
                                    {
                                        extendedOneDataList.Add(extendedOneData);
                                    }

                                    //VrhLogger.Log($"{name} instance key has been set to {reader[column]} value in {reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance in {pool.PoolName} pool!", LogLevel.Debug, this.GetType());
                                }
                            }
                        }
                    }
                    else
                    {
                        //Nincs megadva Variables az UpdateProcessen belül ezért az összes oszlop beírásra kerül
                        columns.ForEach(x =>
                        {
                            ExtendedOneData extendedOneData = new ExtendedOneData();
                            extendedOneData.InstanceName = instanceName;

                            if (reader[x].GetType().Name != DBNull.Value.GetType().Name)
                            {
                                //System.Diagnostics.Debug.WriteLine($"{x}: {reader[x]}");

                                extendedOneData.OneData.DataKey = x;

                                switch (reader[x].GetType().Name)
                                {
                                    case "Boolean":
                                        extendedOneData.OneData.FieldType = DataType.Boolean;
                                        break;
                                    case "DateTime":
                                        extendedOneData.OneData.FieldType = DataType.DateTime;
                                        break;
                                    case "Double":
                                        extendedOneData.OneData.FieldType = DataType.Double;
                                        break;
                                    case "Int32":
                                        extendedOneData.OneData.FieldType = DataType.Int32;
                                        break;
                                    case "ASETimeCounter":
                                        //Ilyen sose lesz...
                                        extendedOneData.OneData.FieldType = DataType.TimeCounter;
                                        break;
                                    default:
                                        extendedOneData.OneData.FieldType = DataType.String;
                                        break;
                                }

                                extendedOneData.OneData.Value = reader[x];

                                if (poolHandler.IsKeyExists(extendedOneData.OneData.DataKey))
                                {
                                    extendedOneDataList.Add(extendedOneData);
                                }

                                //VrhLogger.Log($"{x} instance key has been set to {reader[x]} value in {reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance in {pool.PoolName} pool!", LogLevel.Debug, this.GetType());
                            }
                            //else
                            //{
                            //    VrhLogger.Log($"{x} instance key not set because has DBNull value in {reader[updateProcess.UpdateProcessSQLInstanceKeyColumn].ToString()} instance in {pool.PoolName} pool!", LogLevel.Warning, this.GetType());
                            //}
                        });
                    }
                }
                catch (Exception ex)
                {
                    //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}#########>>>>>> {ex.Message}");
                }
            }

            //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}#########>>>>>> Fill OneData List finished");

            foreach (string instanceName in instanceNameList)
            {
                try
                {
                    pool.RedisConnectionString.InstanceName = instanceName;
                    using (InstanceWriter instanceWriter = new InstanceWriter(pool.RedisConnectionString))
                    {
                        if (!instanceWriter.IsPoolInstanceExists() && pool.CreateInstance)
                        {
                            instanceWriter.RegisterInstance();
                        }

                        if (instanceWriter.IsPoolInstanceExists())
                        {
                            List<OneData> oneDatas = extendedOneDataList.Where(x => x.InstanceName == instanceName).Select(x => x.OneData).ToList();
                            instanceWriter.WriteKeyValue(oneDatas);
                            //System.Diagnostics.Debug.WriteLine($"###{DateTime.Now.ToShortTimeString()}###  In {instanceName} instance: {oneDatas.Count} key refreshed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logData = new Dictionary<string, string>();
                    logData.Add("InstanceName", instanceName);
                    logData.Add("Pool name", "????");
                    _pluginReference.LogThis("$Exception in writing to redis.", logData, ex, LogLevel.Error, this.GetType());
                }
            }

            //System.Console.WriteLine($"###{DateTime.Now.ToString("HH:mm:ss:fff")}#########>>>>>> Instances write value finished");
        }

        private void RegisterNotDeclaredVariables(SqlDataReader reader,
                                                  List<string> columns,
                                                  DataToRedisConfigXmlProcessor.UpdateProcess updateProcess,
                                                  PoolHandler poolHandler)
        {
            columns.ForEach(x =>
            {
                if (!poolHandler.IsKeyExists(x))
                {
                    Type sqlType = reader.GetFieldType(reader.GetOrdinal(x));
                    DataType type;

                    switch (sqlType.ToString().Replace("System.", string.Empty).ToUpper())
                    {
                        case "BOOLEAN":
                            type = DataType.Boolean;
                            break;
                        case "DATETIME":
                            type = DataType.DateTime;
                            break;
                        case "DOUBLE":
                            type = DataType.Double;
                            break;
                        case "INT32":
                            type = DataType.Int32;
                            break;
                        case "STRING":
                            type = DataType.String;
                            break;
                        case "TIMECOUNTER":
                            type = DataType.TimeCounter;
                            break;
                        default:
                            type = DataType.String;
                            break;
                    }

                    poolHandler.RegisterKey(new RedisPoolKeyDefination() { DataType = type, Name = x });
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    if (_timer != null)
                    {
                        if (_timer.Enabled)
                        {
                            _timer.Stop();
                        }

                        _timer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UpdateBatch() {
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
