using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfMotionCapture
{
    public partial class MainWindow : Window
    {
        private SerialPort usb;
        private Arduino uno;

        public CustomData customData = new CustomData();

        //public Model3D TheSkeleton { get; set; }
        //ModelImporter importer;
        //Model3DGroup skeleton;
        //Model3D rest;

        public MainWindow()
        {

            uno = new Arduino();

            InitializeComponent();
            CompositionTarget.Rendering += Update;

            viewport3D1.LookAt(new Point3D(3.5f, 0, 1), 20, 0);

            this.DataContext = customData;

            //importer = new ModelImporter();
            //skeleton = new Model3DGroup();

            //load the files
            //rest = importer.Load(@"Bones/Group4.obj");

            //skeleton.Children.Add(rest);
            //this.TheSkeleton = skeleton;
        }


        private void Update(object sender, EventArgs e)
        {
            string a = uno.usb.ReadExisting();
            uno.OnSerialDataReceived(a);

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
        }



        private void Calibrate(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Próba kalibracji...");
            usb.Write("#");
        }
    }

}
