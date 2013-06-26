using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UbiNect;
using UbiNect.GesturePosture;
using UbiNect.Button;

namespace Demo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image VideoImage = new Image();

        Prototype proto;

        public MainWindow()
        {
            InitializeComponent();

            //zum Beispiel:
            proto = new Prototype();
            proto.VideoFrameReady += new VideoFrameReadyDelegate(proto_VideoFrameReady);
            proto.AddComponent<PostureRecognition>();
            proto.AddComponent<GestureRecognition>();
            proto.AddComponent<ButtonRecognition>();

            ButtonRecognition br = proto.GetComponent<ButtonRecognition>();
            /*br.AddButton(new Triangle(0.5f, 0, 2, 0, 0.5f, 2, -0.5f, 0, 2));
            br.AddButton(new Triangle(0.5f, 1, 2, 0.5f, 1, 3, 0.5f, -1, 3));
            br.AddButton(new Triangle(0.5f, 1, 2, 0.5f, -1, 2, 0.5f, -1, 3));
            br.AddButton(new Triangle(-0.5f, 1, 2, -0.5f, 1, 3, -0.5f, -1, 3));
            br.AddButton(new Triangle(-0.5f, 1, 2, -0.5f, -1, 2, -0.5f, -1, 3));*/
            //br.AddSavedButtons("TestButtons.but");

            // Get notification if gesture was recognized by posture recognition component:
            // 1. Get posture recognition component
            PostureRecognition pr = proto.GetComponent<PostureRecognition>();
            // 2. Define which postures we want to listen for
            //some postures are hard coded...
            String[] postures = {"BothArmsUpPosture","BothHandsTogetherPosture","XPosture"};
            pr.setRecognizablePostures(postures, false);
            //...some are not
            String[] xmlPostures = { "TPosture", "LouderVolumePosture", "LowerVolumePosture" };
            pr.setRecognizablePostures(xmlPostures, true);
            // 3. Add new delegate with link to own method to the "postureRecognized" event
            // NOTE: By specifying the recognizable postures in the above step, we tell the system 
            // to raise the "postureRecognized" Event only if any of the specified postures are recognized.
            pr.PostureRecognized += new PostureRecognizedDelegate(pr_postureRecognized);
            
            GestureRecognition gr = proto.GetComponent<GestureRecognition>();
            String[] gestures = { "ZoomOutGesture", "LeftHandSwipeRightGesture", "ZoomInGesture", "RightHandSwipeLeftGesture", "PushGesture" };
            gr.setRecognizableGestures(gestures, false);
            gr.GestureRecognized += new GestureRecognizedDelegate(gr_gestureRecognized);
            
            VideoImage.Width = 640;
            VideoImage.Height = 480;

            this.WindowState = WindowState.Minimized;
        }

        
        void pr_postureRecognized(int PlayerIndex, Posture p)
        {
            Console.WriteLine(p.postureName);
        }

        void gr_gestureRecognized(Gesture g)
        {
            Console.WriteLine(g.gestureName);
        }

        void proto_VideoFrameReady(ImageSource Image)
        {
            
            MainCanvas.Children.Clear();
            VideoImage.Source = Image;
            MainCanvas.Children.Add(VideoImage);

            proto.GetComponent<ButtonRecognition>().DrawButtons(MainCanvas);
        }


        public PostureRecognizedDelegate pr_PostureRecognized { get; set; }
    }
}
