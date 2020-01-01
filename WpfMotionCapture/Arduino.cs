using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace WpfMotionCapture
{
    class Arduino
    {
        public static string debugText;

        public static string GetArduinoSerialPort()
        {
            string[] ports = SerialPort.GetPortNames();
            
            debugText = string.Join(", ", ports);

            foreach (string p in ports)
            {
                //customData.DebugText += p + ", ";
            }

            return ports[1];
        }



    }
}
