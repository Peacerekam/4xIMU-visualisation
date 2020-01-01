using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
//using System.Numerics;

namespace WpfMotionCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SerialPort usb;

        CustomData customData = new CustomData();

        //private int frameCount = 0;
        //public string date = "";
        
        public MainWindow()
        {
            string arduinoPort = Arduino.GetArduinoSerialPort();
            customData.DebugText = Arduino.debugText;

            usb = new SerialPort {
                PortName = arduinoPort,
                BaudRate = 115200
            };

            usb.Open();


            InitializeComponent();
            CompositionTarget.Rendering += Update;
            //this.DataContext = customData;
        }


        private void Update(object sender, EventArgs e)
        {
            string a = usb.ReadExisting();
            OnSerialDataReceived(a);

            customData = new CustomData
            {
                DebugText = customData.DebugText,
                Offset = customData.Offset,
                OffsetN = customData.OffsetN,
                FrameCount = Utility.CalculateFrameRate(),
                ForearmLength = (float)this.ForearmLength.Value/2,
                ArmLength = (float)this.ArmLength.Value/2
            };

            this.DataContext = customData;

            //date = DateTime.Now.ToString();
        }


        private float q0, q1, q2, q3;
        private int mpu_i;
        private char bulkSplitChar = '/';
        private char splitChar = ';';

        private void SetIMUQuaternions()
        {
            // podział na części indywidualnych czujników (znak końca pakietu '\')
            string[] dataBulk = customData.ArduinoRaw.Split(bulkSplitChar);

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
                q1 = float.Parse(tempData[2]);
                q2 = float.Parse(tempData[4]);
                q3 = float.Parse(tempData[3]);

                Quaternion combined = new Quaternion(q0, -q1, -q2, q3);

                if (int.TryParse(tempData[0], out mpu_i))
                {
                    // Nadanie odpowiedniej rotacji obiektowi o indeksie mpu_i
                    //IMUGameObjects[mpu_i].transform.rotation =
                    //System.Numerics.Quaternion qq = System.Numerics.Quaternion.Inverse(new System.Numerics.Quaternion(-q0, q1, -q2, -q3));
                    //this.quaternion.Quaternion = new Quaternion(-qq.W, qq.X, -qq.Y, -qq.Z);
                    //this.quaternion2.Quaternion = new Quaternion(-qq.W, qq.X, -qq.Y, -qq.Z);

                    Quaternion parentQ;
                    double x, y, z;

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

                            this.quaternion1.Quaternion = combined;
                            break;

                        case 1:
                            this.quaternion2.Quaternion = combined;

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

                            if (this.SimulateArm.IsChecked == true)
                            {
                                parentQ = this.quaternion1.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                this.translate2.OffsetX = -x * jointLength;
                                this.translate2.OffsetY = -y * jointLength;
                                this.translate2.OffsetZ = -z * jointLength;
                                
                                customData.OffsetN = -1f;
                                customData.Offset = 1f;
                            }
                            else
                            {
                                this.translate2.OffsetX = -2.5;
                                this.translate2.OffsetY = 0;
                                this.translate2.OffsetZ = 0;

                                customData.OffsetN = 0f;
                                customData.Offset = 0f;
                            }
                            
                            break;

                        case 2:
                            this.quaternion3.Quaternion = combined;

                            if (this.SimulateArm.IsChecked == true)
                            {
                                parentQ = this.quaternion2.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                this.translate3.OffsetX = (-x * this.ArmLength.Value) + this.translate2.OffsetX;
                                this.translate3.OffsetY = (-y * this.ArmLength.Value) + this.translate2.OffsetY;
                                this.translate3.OffsetZ = (-z * this.ArmLength.Value) + this.translate2.OffsetZ;

                                customData.OffsetN = -1f;
                                customData.Offset = 1f;
                            }
                            else
                            {
                                this.translate3.OffsetX = -5;
                                this.translate3.OffsetY = 0;
                                this.translate3.OffsetZ = 0;

                                customData.OffsetN = 0f;
                                customData.Offset = 0f;
                            }

                            break;

                        case 3:
                            this.quaternion4.Quaternion = combined;

                            if (this.SimulateArm.IsChecked == true)
                            {
                                parentQ = this.quaternion3.Quaternion;

                                // left
                                x = 1 - 2 * (parentQ.Y * parentQ.Y + parentQ.Z * parentQ.Z);
                                y = 2 * (parentQ.X * parentQ.Y + parentQ.W * parentQ.Z);
                                z = 2 * (parentQ.X * parentQ.Z - parentQ.W * parentQ.Y);

                                this.translate4.OffsetX = (-x * this.ForearmLength.Value) + this.translate3.OffsetX;
                                this.translate4.OffsetY = (-y * this.ForearmLength.Value) + this.translate3.OffsetY;
                                this.translate4.OffsetZ = (-z * this.ForearmLength.Value) + this.translate3.OffsetZ;

                                customData.OffsetN = -1f;
                                customData.Offset = 1f;
                            }
                            else
                            {
                                this.translate4.OffsetX = -7.5;
                                this.translate4.OffsetY = 0;
                                this.translate4.OffsetZ = 0;

                                customData.OffsetN = 0f;
                                customData.Offset = 0f;
                            }

                            break;

                        default:
                            break;
                    }
                    
                }
            }
            
        }

        // tail to znak końca pojedynczego pakietu z urządzenia zgodnie z kodem Arduino
        char tail = '/';
        protected string receivedMessage;
        int maxBufferCharCount = 100;
        Queue<char> bufferQueue = new Queue<char>();
        List<char> tempBuffer = new List<char>();
        int bulkSize = 0;

        // public by móc wykorzystać zmienną w innych miejscach w Unity
        //public string IMUSensorData;

        private void OnSerialDataReceived(string data)
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
                if (c == tail) bulkSize++;
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
            customData.ArduinoRaw = receivedMessage;
            customData.DebugText = receivedMessage;

            SetIMUQuaternions();
        }


        private void Calibrate(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Próba kalibracji...");
            usb.Write("#");
        }
    }

}
