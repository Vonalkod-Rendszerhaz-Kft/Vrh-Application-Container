using Newtonsoft.Json;
using System;
using System.Text;
using System.Timers;
using System.Data.SqlClient;

using Vrh.Common.Serialization.Structures;
using Vrh.Logger;
using System.Collections.Specialized;

namespace iSchedulerMonitor
{
    internal class Monitor : IDisposable
    {
        #region Privates

        private Timer m_timer;
        private iSchedulerXMLProcessor m_xmlp;

        #endregion Privates

        #region Constructor

        /// <summary>
        /// iSchedulerMonitor időzítés figyelő osztály.
        /// </summary>
        /// <param name="localPath">iScheduler.xml elérési helye a névvel együtt a plugin futtató környezetében.</param>
        /// <param name="remotePath">ScheduleExecute akció által használt iScheduler.xml elérési helye a névvel együtt
        /// a távoli gépen. Ha egy gépen fut, akkor nem kötelező.</param>
        public Monitor(string localPath, string remotePath = null)
        {
            log($"PREPARATION: xmlpath={localPath}");

            m_xmlp = new iSchedulerXMLProcessor(localPath, remotePath);
            log($"PREPARATION: XMLProcesszor OK. ObjectType={m_xmlp.ObjectType}, GroupId={m_xmlp.GroupId}, ResponseTimeout={m_xmlp.ResponseTimeout}s");

            log($"PREPARATION: Set timer! CheckInterval={m_xmlp.CheckInterval}s");
            m_timer = new Timer(m_xmlp.CheckInterval * 1000); // !!! Ez itt a jó sor !!!
            //m_timer = new Timer(5000); // !!! Ez meg itt a debug !!!
            m_timer.Elapsed += OnExamination;

            log("PREPARATION ready.", LogLevel.Verbose);
        }

        #endregion Constructor

        public void Start()
        {
            log("iSchedulerMonitor started.", LogLevel.Information);
            m_timer.Start();
        }
        public void Stop()
        {
            m_timer.Stop();
            log("iSchedulerMonitor stopped.", LogLevel.Information);
        }

        #region Private methods

        private void OnExamination(object sender, ElapsedEventArgs e)
        {
            m_timer.Stop();
            Examination(e.SignalTime);
            m_timer.Start();
        }

        #region Examination
        /// <summary>
        /// Időzített események megkeresése, és végrehajtása.
        /// </summary>
        /// <param name="signalTime"></param>
        private void Examination(DateTime signalTime)
        {
            try
            {
                log($"Examination START. Signal time = {signalTime:HH:mm:ss}", LogLevel.Verbose);
                log($"EXAMINATION: DatabaseConnectionString={m_xmlp.DatabaseConnectionString}");

                using (SqlConnection cnn = new SqlConnection(m_xmlp.DatabaseConnectionString))
                {
                    string scmd = $"select * from iScheduler.Schedules where State = 0 and OperationTime < @now order by OperationTime";
                    using (SqlCommand cmd = new SqlCommand(scmd, cnn))
                    {
                        cmd.Parameters.Add(new SqlParameter("now", signalTime));
                        cnn.Open();
                        log($"EXAMINATION: Connection opened.");

                        SqlDataReader rdr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                        if (rdr.HasRows)
                        {
                            log("The examination found scheduled jobs.", LogLevel.Verbose);

                            Uri loginUri = new Uri(m_xmlp.LoginUrl.GetUrl());
                            using (CookieWebClient wc = new CookieWebClient(loginUri, "Developer", "Dev123"))
                            {
                                if (m_xmlp.ResponseTimeout > 0) wc.Timeout = m_xmlp.ResponseTimeout * 1000; // itt millisecundumban kell
                                log($"EXAMINATION: Login success. WebClient.Timeout={wc.Timeout}");

                                int ixID = rdr.GetOrdinal("Id");

                                while (rdr.Read())
                                {
                                    int id = rdr.GetInt32(ixID);

                                    #region Scheduled job is executing
                                    try
                                    {
                                        log($"Scheduled job execute started. id = {id}", LogLevel.Verbose);
                                        string execurl = m_xmlp.ExecuteUrl.GetUrl();
                                        execurl = execurl.Replace("@PATH@", m_xmlp.XmlRemotePath);
                                        execurl = execurl.Replace("@ID@", id.ToString());
                                        log($"EXAMINATION: ScheduleExecute url = {execurl}");

                                        string resp = Encoding.UTF8.GetString(wc.DownloadData(execurl));
                                        log($"EXAMINATION: ScheduleExecute response = {resp}");
                                        ReturnInfoJSON ri = JsonConvert.DeserializeObject<ReturnInfoJSON>(resp);
                                        log("EXAMINATION: JsonConvert.DeserializeObject OK.");

                                        using (SqlCommand upd = new SqlCommand($"update iScheduler.Schedules set State = 1 where Id = {id}", cnn))
                                        {
                                            log($"EXAMINATION: updatesql={upd.CommandText}");
                                            int affect = upd.ExecuteNonQuery();
                                            LogLevel ll = ri.ReturnValue == 0 ? LogLevel.Information : LogLevel.Warning;
                                            log($"Scheduled job (id={id} has executed.\nReturnValue = {ri.ReturnValue}\nReturnMessage = {ri.ReturnMessage}", ll);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (wc.LastException == null) Logger.Log(ex, this.GetType());
                                        else Logger.Log(wc.LastException, this.GetType());
                                    }
                                    #endregion Scheduled job is executing
                                }
                            }
                        }
                        else
                        {
                            log("The examination did not find a scheduled job.", LogLevel.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, this.GetType());
            }
        }
        #endregion Examination

        private void log(string message, LogLevel level = LogLevel.Debug)
        {
            Logger.Log(message, level, this.GetType());
        }

        #endregion Private methods

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~iScheduler() {
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
