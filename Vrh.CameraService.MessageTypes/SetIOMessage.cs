using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vrh.CameraService.MessageTypes
{
    /// <summary>
    /// 
    /// </summary>
    public class SetIOMessage
    {
        public SetIOMessage()
        {
        }

        public SetIOMessage(Bulbs bulb, bool bulbTurneOn)
        {
            Bulb = bulb;
            BulbTurneOn = bulbTurneOn;
        }

        public Bulbs Bulb { get; set; }

        public bool BulbTurneOn { get; set; }
    }
}
