using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfMotionCapture
{
    public partial class Arduino
    {
        private MainWindow _mainWindow = Application.Current.Windows[0] as MainWindow;
        public SerialPort usb;

        protected string receivedMessage;
        private int maxBufferCharCount = 100;
        private Queue<char> bufferQueue = new Queue<char>();
        private List<char> tempBuffer = new List<char>();
        private int bulkSize = 0;

        public float q0, q1, q2, q3;
        private int mpu_i;
        private char bulkSplitChar = '/';
        private char splitChar = ';';


        public Arduino()
        {
            string arduinoPort = GetArduinoSerialPort();

            usb = new SerialPort
            {
                PortName = arduinoPort,
                BaudRate = 115200
            };

            usb.Open();

        }

        public string GetArduinoSerialPort()
        {
            string[] ports = SerialPort.GetPortNames();

            _mainWindow.customData.DebugText = string.Join(", ", ports);

            foreach (string p in ports)
            {
                //_mainWindow.customData.DebugText += p + ", ";
            }

            //return ports[1];
            return "COM3";
        }

        public void OnSerialDataReceived(string data)
        {
            var tempChars = data.ToCharArray();

            if (tempChars.Length == 0) return;

            // wypełnienie kolejki tablicą znaków z serialu USB
            for (int i = 0; i < tempChars.Length; i++)
            {
                bufferQueue.Enqueue(tempChars[i]);
            }

            // pętla sprawdzająca kolejkę aż do znalezienia 
            // czterech znaków końca pakietu (od razu dla 4 czujników)
            while (true)
            {
                char c = bufferQueue.Dequeue();
                if (c == bulkSplitChar) bulkSize++;
                if (bulkSize >= 4)
                {
                    bulkSize = 0;
                    break;
                }
                // dodanie pojedynczego znaku z kolejki do bufora
                tempBuffer.Add(c);
                if (bufferQueue.Count == 0) return;
            }

            // utworzenie wartości typu string z danych w buforze
            receivedMessage = new string(tempBuffer.ToArray()).Substring(2);  // odciecie newline
            tempBuffer.Clear();

            // sprawdzenie, czy kolejka nie przekroczyła maksymalnej liczby znaków
            // np. w przypadku gdyby skrypt nie znalazł żadnego znaku końca pakietu
            if (bufferQueue.Count > maxBufferCharCount) bufferQueue.Clear();

            // tej zmiennej będziemy używać do odczytu orientacji czujników
            _mainWindow.customData.ArduinoRaw = receivedMessage;
            _mainWindow.customData.DebugText = receivedMessage;

            SetIMUQuaternions();
        }

        public void SetIMUQuaternions()
        {
            // podział na części indywidualnych czujników (znak końca pakietu '\')
            string[] dataBulk = _mainWindow.customData.ArduinoRaw.Split(bulkSplitChar);

            if (dataBulk.Length != 4) return;

            for (int i = 0; i < 4; i++)
            {
                // utworzenie tabeli z id + kwaternionami (znak separatora ';')
                string[] tempData = dataBulk[i].Split(splitChar);

                //customData.DebugText = "" + tempData.Length + " " + tempData[1];

                // upewniamy się, że dostaliśmy poprawna ilość danych
                if (tempData.Length != 6) break;

                tempData = tempData.Select(d => d.Replace('.', ',')).ToArray();

                // parsujemy odebrane wartości kwaternionów z Arduino na liczby float
                q0 = float.Parse(tempData[1]);
                q1 = float.Parse(tempData[4]);
                q2 = float.Parse(tempData[2]);
                q3 = float.Parse(tempData[3]);

                //Quaternion combined = new Quaternion(q0, -q1, -q2, q3);
                Quaternion combined = new Quaternion(q0, q1, q2, -q3);

                if (int.TryParse(tempData[0], out mpu_i))
                {
                    // Nadanie odpowiedniej rotacji obiektowi o indeksie mpu_i
                    //IMUGameObjects[mpu_i].transform.rotation =
                    //System.Numerics.Quaternion qq = System.Numerics.Quaternion.Inverse(new System.Numerics.Quaternion(-q0, q1, -q2, -q3));
                    //this.quaternion.Quaternion = new Quaternion(-qq.W, qq.X, -qq.Y, -qq.Z);
                    //this.quaternion2.Quaternion = new Quaternion(-qq.W, qq.X, -qq.Y, -qq.Z);

                    Quaternion parentQ;
                    double x = 0, y = 0, z = 0;

                    float jointLength = 2f;

                    switch (mpu_i)
                    {
                        case 0:

                            // Czujnik na klatce piersiowej (ID: 0) znajduje się
                            // do góry nogami, więc trzeba zrobić korektę
                            //customData.Q = new Quaternion(-q0, q1, -q2, -q3);
                            //mirroredPos.x = -IMUGameObjects[mpu_i].transform.eulerAngles.x;
                            //mirroredPos.y = IMUGameObjects[mpu_i].transform.eulerAngles.y;
                            //mirroredPos.z = -IMUGameObjects[mpu_i].transform.eulerAngles.z;
                            //IMUGameObjects[mpu_i].transform.eulerAngles = mirroredPos;

                            _mainWindow.quaternion1.Quaternion = combined;
                            break;

                        case 1:
                            _mainWindow.quaternion2.Quaternion = combined;

                            /*
                            forward vector:
                            x = 2 * (x * z + w * y)
                            y = 2 * (y * z - w * x)
                            z = 1 - 2 * (x * x + y * y)

                            up vector
                            x = 2 * (x * y - w * z)
                            y = 1 - 2 * (x * x + z * z)
                            z = 2 * (y * z + w * x)

                            left vector
                            x = 1 - 2 * (y * y + z * z)
                            y = 2 * (x * y + w * z)
                            z = 2 * (x * z - w * y)
                            */

                            // fwd
                            //double x = 2 * (parentQ.X * parentQ.Z + parentQ.W * parentQ.Y);
                            //double y = 2 * (parentQ.Y * parentQ.Z - parentQ.W * parentQ.X);
                            //double z = 1 - 2 * (parentQ.X * parentQ.X + parentQ.Y * parentQ.Y);

                            if (_mainWindow.SimulateArm.IsChecked == true)
                            {
                                parentQ = _mainWindow.quaternion1.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                _mainWindow.translate2.OffsetX = x * jointLength;
                                _mainWindow.translate2.OffsetY = y * jointLength;
                                _mainWindow.translate2.OffsetZ = z * jointLength;

                                _mainWindow.customData.OffsetN = 1f;
                                _mainWindow.customData.Offset = -1f;
                            }
                            else
                            {
                                _mainWindow.translate2.OffsetX = 2.5;
                                _mainWindow.translate2.OffsetY = 0;
                                _mainWindow.translate2.OffsetZ = 0;

                                _mainWindow.customData.OffsetN = 0f;
                                _mainWindow.customData.Offset = 0f;
                            }

                            break;

                        case 2:
                            _mainWindow.quaternion3.Quaternion = combined;

                            if (_mainWindow.SimulateArm.IsChecked == true)
                            {
                                parentQ = _mainWindow.quaternion2.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                _mainWindow.translate3.OffsetX = (x * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetX;
                                _mainWindow.translate3.OffsetY = (y * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetY;
                                _mainWindow.translate3.OffsetZ = (z * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetZ;

                                _mainWindow.customData.OffsetN = 1f;
                                _mainWindow.customData.Offset = -1f;
                            }
                            else
                            {
                                _mainWindow.translate3.OffsetX = 5;
                                _mainWindow.translate3.OffsetY = 0;
                                _mainWindow.translate3.OffsetZ = 0;

                                _mainWindow.customData.OffsetN = 0f;
                                _mainWindow.customData.Offset = 0f;
                            }



                            Transform3DGroup myTransformGroup = new Transform3DGroup();

                            RotateTransform3D myInitialRotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0,0,1), 90));
                            RotateTransform3D myRotateTransform = new RotateTransform3D(new QuaternionRotation3D(combined));

                            //                                                                   |
                            //                                                     change this:  |
                            //                                                                   V
                            TranslateTransform3D myInitialTranslate = new TranslateTransform3D(20000, 0, 0);
                            TranslateTransform3D myLastTranslate = new TranslateTransform3D(-2, 0, 0);

                            ScaleTransform3D myScaleTransform = new ScaleTransform3D(new Vector3D(_mainWindow.ForearmLength.Value/8, 0.2, 0.2));

                            myRotateTransform.CenterX = (x * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetX;
                            myRotateTransform.CenterY = (y * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetY;
                            myRotateTransform.CenterZ = (z * _mainWindow.ArmLength.Value) + _mainWindow.translate2.OffsetZ;
                            
                            myTransformGroup.Children.Add(myInitialRotate);
                            myTransformGroup.Children.Add(myInitialTranslate);
                            myTransformGroup.Children.Add(myScaleTransform);
                            myTransformGroup.Children.Add(myRotateTransform);
                            myTransformGroup.Children.Add(myLastTranslate);

                            _mainWindow._model1.Transform = myTransformGroup;



                            break;

                        case 3:
                            _mainWindow.quaternion4.Quaternion = combined;

                            if (_mainWindow.SimulateArm.IsChecked == true)
                            {
                                parentQ = _mainWindow.quaternion3.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                _mainWindow.translate4.OffsetX = (x * _mainWindow.ForearmLength.Value) + _mainWindow.translate3.OffsetX;
                                _mainWindow.translate4.OffsetY = (y * _mainWindow.ForearmLength.Value) + _mainWindow.translate3.OffsetY;
                                _mainWindow.translate4.OffsetZ = (z * _mainWindow.ForearmLength.Value) + _mainWindow.translate3.OffsetZ;

                                _mainWindow.customData.OffsetN = 1f;
                                _mainWindow.customData.Offset = -1f;
                            }
                            else
                            {
                                _mainWindow.translate4.OffsetX = 7.5;
                                _mainWindow.translate4.OffsetY = 0;
                                _mainWindow.translate4.OffsetZ = 0;

                                _mainWindow.customData.OffsetN = 0f;
                                _mainWindow.customData.Offset = 0f;
                            }

                            break;

                        default:
                            break;
                    }

                }
            }

        }

    }
}
