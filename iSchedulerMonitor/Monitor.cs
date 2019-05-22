using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Timers;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Collections.Generic;

using Vrh.Web.Common.Lib;
using Vrh.Logger;
using Vrh.LinqXMLProcessor.Base;
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
        private MonitorPlugin _pluginReference = null;

        #endregion Privates

        #region Constructor

        /// <summary>
        /// iSchedulerMonitor időzítés figyelő osztály.
        /// </summary>
        /// <param name="localPath">iScheduler.xml elérési helye a névvel együtt a plugin futtató környezetében.</param>
        /// <param name="remotePath">ScheduleExecute akció által használt iScheduler.xml elérési helye a névvel együtt
        /// <param name="pluginReference">Az indító plugin példányra mutató referencia
        /// a távoli gépen. Ha egy gépen fut, akkor nem kötelező.</param>
        public Monitor(string localPath, string remotePath, MonitorPlugin pluginReference)
        {
            //try
            //{

            _pluginReference = pluginReference;
            m_xmlp = new iSchedulerXMLProcessor(localPath, remotePath);
            m_timer = new Timer(m_xmlp.CheckInterval * 1000); // !!! Ez itt a jó sor !!!
                                                              // m_timer = new Timer(20000); // !!! Ez meg itt a debug !!!
            m_timer.Elapsed += OnExamination;

            var logData = new Dictionary<string, string>
            {
                { "xmlpath", localPath },
                { "Scheduled object type", m_xmlp.ObjectType },
                { "Group Id", m_xmlp.GroupId },
                { "Check interval", m_xmlp.CheckInterval.ToString() }
            };
            _pluginReference.LogThis("Preparation ready.", logData, null, LogLevel.Debug, this.GetType());

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
            _pluginReference.LogThis("iSchedulerMonitor started.", null, null, LogLevel.Information, this.GetType());
            m_timer.Start();
        }
        public void Stop()
        {
            m_timer.Stop();
            _pluginReference.LogThis("iSchedulerMonitor stopped.", null, null, LogLevel.Information, this.GetType());
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
                var logData = new Dictionary<string, string>()
                    {
                        { "Start time", $"{signalTime:HH:mm:ss}"}
                    };
                _pluginReference.LogThis($"Examination cycle started.", logData, null, LogLevel.Verbose, this.GetType());

                using (SqlConnection cnn = new SqlConnection(m_xmlp.DatabaseConnectionString))
                {
                    cnn.Open();
                    logData.Add("Database connection string", m_xmlp.DatabaseConnectionString);
                    _pluginReference.LogThis($"Database connection opened.", logData, null, LogLevel.Verbose, this.GetType());

                    string scmd = string.Concat(
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
                            _pluginReference.LogThis($"Scheduled job found!", logData, null, LogLevel.Verbose, this.GetType());

                            int ixID = rdr.GetOrdinal("Id");
                            Vrh.iScheduler.ScheduleExecute se = new Vrh.iScheduler.ScheduleExecute(m_xmlp.XmlLocalPath);

                            int ScheduledJobCounter = 0;
                            while (rdr.Read())
                            {
                                ScheduledJobCounter++;
                                int id = rdr.GetInt32(ixID);
                                DateTime jobstartedat = DateTime.Now;
                                logData.Add($"{ScheduledJobCounter}. scheduled job Id", id.ToString());
                                _pluginReference.LogThis($"Scheduled job execution started (job index in cycle: #{ScheduledJobCounter.ToString()})!", logData, null, LogLevel.Verbose, this.GetType());
                                se.Run(id);
                                DateTime jobfinishedat = DateTime.Now;
                                string jobexecutiontime = jobfinishedat.Subtract(jobstartedat).ToString(@"hh\:mm\:ss");

                                _pluginReference.LogThis($"Scheduled job execution finished (execution time: {jobexecutiontime} !", logData, null, LogLevel.Verbose, this.GetType());
                            }//while (rdr.Read())
                        }//if (rdr.HasRows)
                        else
                        {
                            _pluginReference.LogThis($"No scheduled job found!", logData, null, LogLevel.Verbose, this.GetType());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginReference.LogThis($"Exception in scheduled job execution.", null, ex, LogLevel.Error, this.GetType());
            }
        }
        #endregion Examination

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
