using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace WpfMotionCapture
{

    public class CustomData
    {
        
        private string debugText;
        private string arduinoRaw;

        private float offsetN;
        private float offset;
        
        private float forearmLength;
        private float armLength;

        private int frameCount;

        public int FrameCount
        {
            get { return frameCount; }

            set
            {
                if (value != frameCount)
                {
                    frameCount = value;
                }
            }
        }

        public float ForearmLength
        {
            get { return forearmLength; }

            set
            {
                if (value != forearmLength)
                {
                    forearmLength = value;
                }
            }
        }

        public float ArmLength
        {
            get { return armLength; }

            set
            {
                if (value != armLength)
                {
                    armLength = value;
                }
            }
        }

        public float OffsetN
        {
            get { return offsetN; }

            set
            {
                if (value != offsetN)
                {
                    offsetN = value;
                }
            }
        }

        public float Offset
        {
            get { return offset; }

            set
            {
                if (value != offset)
                {
                    offset = value;
                }
            }
        }

        public string DebugText
        {
            get { return debugText; }

            set
            {
                if (value != debugText)
                {
                    debugText = value;
                }
            }
        }

        public string ArduinoRaw
        {
            get { return arduinoRaw; }

            set
            {
                if (value != arduinoRaw)
                {
                    arduinoRaw = value;
                }
            }
        }

    }
}
