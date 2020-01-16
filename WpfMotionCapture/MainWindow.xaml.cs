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
        private Arduino uno;

        public CustomData customData = new CustomData();

        public MainWindow()
        {

            uno = new Arduino();

            InitializeComponent();
            CompositionTarget.Rendering += Update;

            viewport3D1.LookAt(new Point3D(3.5f, 0, 1), 20, 0);

            this.DataContext = customData;
        }

        private void Test3DModel()
        {

            // test 3d stuff
            Transform3DGroup myTransformGroup = new Transform3DGroup();

            RotateTransform3D myRotateTransform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(uno.q0, uno.q1, uno.q2, -uno.q3)));
            ScaleTransform3D myScaleTransform = new ScaleTransform3D(new Vector3D(customData.ForearmLength, 1, 1));

            myRotateTransform.CenterX = 3;
            myRotateTransform.CenterY = 0;
            myRotateTransform.CenterZ = 0;

            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);

            _model1.Transform = myTransformGroup;


        }

        private void Update(object sender, EventArgs e)
        {
            string a = uno.usb.ReadExisting();
            uno.OnSerialDataReceived(a);


            //Test3DModel();


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
            uno.usb.Write("#");
        }
    }

}
