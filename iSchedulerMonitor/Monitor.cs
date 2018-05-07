using Newtonsoft.Json;
using System;
using System.Text;
using System.Timers;
using System.Data.SqlClient;

using Vrh.Web.Common.Lib;
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
            //try
            //{
            MyLog($"PREPARATION: xmlpath={localPath}");

            m_xmlp = new iSchedulerXMLProcessor(localPath, remotePath);
            //MyLog($"PREPARATION: XMLProcesszor OK. ObjectType={m_xmlp.ObjectType}, GroupId={m_xmlp.GroupId}, ResponseTimeout={m_xmlp.ResponseTimeout}s");
            MyLog($"PREPARATION: XMLProcesszor OK. ObjectType={m_xmlp.ObjectType}, GroupId={m_xmlp.GroupId}");

            MyLog($"PREPARATION: Set timer! CheckInterval={m_xmlp.CheckInterval}s");
            m_timer = new Timer(m_xmlp.CheckInterval * 1000); // !!! Ez itt a jó sor !!!
                                                              //m_timer = new Timer(5000); // !!! Ez meg itt a debug !!!
            m_timer.Elapsed += OnExamination;

            MyLog("PREPARATION ready.", LogLevel.Verbose);

            //}
            //catch (Exception ex)
            //{
            //    var message = String.Join(",", WebCommon.ErrorListBuilder(ex));
            //    MyLog("PREPARATION ERROR. " + message);
            //    throw new ApplicationException("PREPARATION ERROR. ", ex);
            //}
        }

        #endregion Constructor

        public void Start()
        {
            MyLog("iSchedulerMonitor started.", LogLevel.Information);
            m_timer.Start();
        }
        public void Stop()
        {
            m_timer.Stop();
            MyLog("iSchedulerMonitor stopped.", LogLevel.Information);
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
                MyLog($"{thisfn}START. Signal time = {signalTime:HH:mm:ss}", LogLevel.Verbose);
                MyLog($"{thisfn}DatabaseConnectionString={m_xmlp.DatabaseConnectionString}");

                using (SqlConnection cnn = new SqlConnection(m_xmlp.DatabaseConnectionString))
                {
                    cnn.Open();
                    MyLog($"{thisfn}Connection opened.");

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
                            MyLog($"{thisfn}Scheduled job is found!", LogLevel.Verbose);

                            int ixID = rdr.GetOrdinal("Id");
                            Vrh.iScheduler.ScheduleExecute se = new Vrh.iScheduler.ScheduleExecute(m_xmlp.XmlLocalPath);

                            while (rdr.Read())
                            {
                                int id = rdr.GetInt32(ixID);
                                MyLog($"{thisfn}Scheduled job execute started. id = {id}", LogLevel.Verbose);
                                se.Run(id);
                                MyLog($"{thisfn}Scheduled job has executed. id = {id}", LogLevel.Verbose);
                            }//while (rdr.Read())
                        }//if (rdr.HasRows)
                        else
                        {
                            MyLog($"{thisfn}No scheduling job", LogLevel.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog(String.Join(System.Environment.NewLine,WebCommon.ErrorListBuilder(ex)));
            }
        }
        #endregion Examination

        private void MyLog(string message, LogLevel level = LogLevel.Debug)
        {
            System.Diagnostics.Debug.WriteLine(message);
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
