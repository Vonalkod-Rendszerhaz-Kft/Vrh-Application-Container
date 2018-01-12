using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVConnector.Plugin
{
    /// <summary>
    /// A feldolgozott üzenetek fotrmátuma 
    /// </summary>
    public enum MessageFormat
    {
        /// <summary>
        /// A pozicó reprezentálja, melyik adatról van szó
        /// </summary>
        Positional = 0,
        /// <summary>
        /// Az adatok név-értékpárok (egyenlőségjellel elválasztva), melyekben a név egzaktul definiálja, hogy melyik adatról van szó, a poziciójától függetlenül
        /// </summary>
        ByName =1,
    }
}
