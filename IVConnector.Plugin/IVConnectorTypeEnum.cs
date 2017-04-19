using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVConnector.Plugin
{
    /// <summary>
    /// Az IV connector típusa
    /// </summary>
    public enum IVConnectorType
    {
        /// <summary>
        /// TCP socketten át csatlakoztat
        /// </summary>
        TCP = 1,
        /// <summary>
        /// MS Message Queue-n át csatlakoztat
        /// </summary>
        MSMQ = 2,
    }
}
