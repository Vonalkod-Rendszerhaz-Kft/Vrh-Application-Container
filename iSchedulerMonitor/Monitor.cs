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
        #region Enums
        /// <summary>
        /// Ütemezések lehetséges állapotai.
        /// </summary>
        public enum ScheduleStates : byte
        {
            /// <summary>
            /// Az ütemezés végrehajtás előtt van, végrehajtásra várakozik.
            /// </summary>
            Active = 0,

            /// <summary>
            /// Az ütemezés a végrehajtás után, ha a végrehajtás sikeresen ért véget.
            /// </summary>
            Success = 1,

            /// <summary>
            /// Az ütemezés végrehajtás után, ha a végrehajtás hibával ért véget.
            /// </summary>
            Failed = 2,
        }
        #endregion Enums

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
                string thisfn = "EXAMINATION: ";
                log($"{thisfn}START. Signal time = {signalTime:HH:mm:ss}", LogLevel.Verbose);
                log($"{thisfn}DatabaseConnectionString={m_xmlp.DatabaseConnectionString}");

                using (SqlConnection cnn = new SqlConnection(m_xmlp.DatabaseConnectionString))
                {
                    cnn.Open();
                    log($"{thisfn}Connection opened.");

                    string scmd = String.Concat(
                        " select *",
                        " from iScheduler.Schedules s",
                        "    inner join iScheduler.ScheduleObjects so on s.ScheduleObjectId = so.Id",
                        " where s.[State] = 0 and s.OperationTime < @signalTime",
                        "   and so.ObjectType = @objectType and so.ObjectGroupId = @groupId");
                    using (SqlCommand cmd = new SqlCommand(scmd, cnn))
                    {
                        cmd.Parameters.Add(new SqlParameter("signalTime", signalTime));
                        cmd.Parameters.Add(new SqlParameter("objectType", m_xmlp.ObjectType));
                        cmd.Parameters.Add(new SqlParameter("groupId", m_xmlp.GroupId));

                        SqlDataReader rdr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                        if (rdr.HasRows)
                        {
                            log($"{thisfn} Scheduled job is found!", LogLevel.Verbose);
                            string xmlLoginUrl = m_xmlp.LoginUrl.GetUrl();
                            log($"{thisfn}xmlLoginUrl= {xmlLoginUrl}");

                            Uri loginUri = new Uri(xmlLoginUrl);
                            //log($"{thisfn}loginUri= {loginUri.AbsoluteUri}");
                            using (CookieWebClient wc = new CookieWebClient(loginUri, "Developer", "Dev123"))
                            {
                                if (m_xmlp.ResponseTimeout > 0) wc.Timeout = m_xmlp.ResponseTimeout * 1000; // itt millisecundumban kell
                                log($"{thisfn}Login success. WebClient.Timeout={wc.Timeout}ms");

                                int ixID = rdr.GetOrdinal("Id");

                                while (rdr.Read())
                                {
                                    int id = rdr.GetInt32(ixID);
                                    ScheduleStates state = ScheduleStates.Failed; //pessszimistán hibát feltételezünk
                                    ReturnInfoJSON ri;

                                    log($"{thisfn}Scheduled job execute started. id = {id}", LogLevel.Verbose);
                                    string execurl = m_xmlp.ExecuteUrl.GetUrl();
                                    execurl = execurl.Replace("@PATH@", m_xmlp.XmlRemotePath);
                                    execurl = execurl.Replace("@ID@", id.ToString());
                                    log($"{thisfn}ScheduleExecute url = {execurl}");

                                    #region Scheduled job is executing
                                    string supd = String.Concat(
                                        " update iScheduler.Schedules",
                                        " set State = @state,",
                                        "     ReturnValue = @rvalue,",
                                        "     ReturnMessage = @rmessage",
                                        " where Id = @id");
                                    try
                                    {
                                        string resp = Encoding.UTF8.GetString(wc.DownloadData(execurl));
                                        log($"{thisfn}ScheduleExecute OK response = {resp}");
                                        ri = JsonConvert.DeserializeObject<ReturnInfoJSON>(resp);
                                        log($"{thisfn}JsonConvert.DeserializeObject OK. ReturnValue={ri.ReturnValue}");

                                        if (ri.ReturnValue == 0)
                                        {
                                            state = ScheduleStates.Success;    //0 sikeres, akkor success(1), ha nem 0, akkor failed(2), azaz marad 
                                            supd = " update iScheduler.Schedules set State = @state where Id = @id";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Exception rex = wc.LastException == null ? ex : wc.LastException;
                                        VrhLogger.Log(rex, this.GetType());
                                        ri = new ReturnInfoJSON()
                                        {
                                            ReturnValue = -5,
                                            ReturnMessage = $"{rex.Message}<br />ScheduleExecute url = {execurl}",
                                        };
                                    }
                                    #endregion Scheduled job is executing

                                    #region Eredmény bejegyzése az adatbázisba
                                    using (SqlCommand upd = new SqlCommand(supd, cnn))
                                    {
                                        upd.Parameters.Add(new SqlParameter("id", id));
                                        upd.Parameters.Add(new SqlParameter("state", state));
                                        if (state == ScheduleStates.Failed)
                                        {
                                            upd.Parameters.Add(new SqlParameter("rvalue", ri.ReturnValue));
                                            upd.Parameters.Add(new SqlParameter("rmessage", ri.ReturnMessage));
                                        }
                                        int affect = upd.ExecuteNonQuery();

                                        LogLevel ll = ri.ReturnValue == 0 ? LogLevel.Information : LogLevel.Warning;
                                        log($"{thisfn}Scheduled job (id={id}) has executed. ReturnValue = {ri.ReturnValue} ReturnMessage = {ri.ReturnMessage}", ll);
                                    }
                                    #endregion Eredmény bejegyzése az adatbázisba

                                }
                            }
                        }
                        else
                        {
                            log($"{thisfn}No scheduling job", LogLevel.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VrhLogger.Log(ex, this.GetType());
            }
        }
        #endregion Examination

        private void log(string message, LogLevel level = LogLevel.Debug)
        {
            VrhLogger.Log(message, level, this.GetType());
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
