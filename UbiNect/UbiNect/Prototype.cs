using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace UbiNect
{
    public delegate void VideoFrameReadyDelegate(ImageSource Image);

    public class Prototype
    {
        /// <summary>
        /// Is fired when a video frame arrives from the Kinect.
        /// </summary>
        public event VideoFrameReadyDelegate VideoFrameReady;

        /// <summary>
        /// The time, when the last skeleton frame arrived. Used to clear the set of skeletons after a while
        /// </summary>
        long LastSkeletonFrame = 0;

        public long timeSinceLastSkeletonFrame { get; set; }

        /// <summary>
        /// Holds every registered Recognition Component
        /// </summary>
        private HashSet<RecognitionComponent> Components;

        /// <summary>
        /// Kinect Runtime
        /// </summary>
        public static KinectSensor nui { get; set; }

        /// <summary>
        /// Debug output window
        /// </summary>
        private GuiNect GuiNect;

        /// <summary>
        /// Set of currently recognized skeletons (TrackingIDs)
        /// </summary>
        private HashSet<int> _Skeletons = new HashSet<int>();
        /// <summary>
        /// Set of currently recognized skeletons (TrackingIDs)
        /// </summary>
        public HashSet<int> Skeletons { get { return _Skeletons; } }

        /// <summary>
        /// FPS Counter for Depth and Skeleton frames.
        /// </summary>
        FPSCounter DepthFPS = new FPSCounter();
        /// <summary>
        /// FPS Counter for VGA frames.
        /// </summary>
        FPSCounter VGACounter = new FPSCounter();

        /// <summary>
        /// The lastly recorded VGA image
        /// </summary>
        public ImageSource VGAImage { get; set; }

        /// <summary>
        /// Creates a new Prototype instance with the specified debug flags.
        /// </summary>
        /// <param name="ShowDebug">true, if Debug window shall be shown</param>
        /// <param name="UseKinect">true, if kinect has to be plugged in</param>
        public Prototype(bool ShowDebug = true, bool IncludeRecorder = true, bool RequireKinect = true)
        {
            Components = new HashSet<RecognitionComponent>();
            //nui = new Runtime();
            nui = Microsoft.Kinect.KinectSensor.KinectSensors[0];
            bool KinectPluggedIn = true;
            try
            {
                
                //nui.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepthAndPlayerIndex);
                nui.ColorStream.Enable();
                nui.DepthStream.Enable();
                nui.SkeletonStream.Enable();
                nui.Start();
            }
            catch(Exception e)
            {
                if(!nui.IsRunning)
                {
                    if (RequireKinect)
                    {
                        //System.Windows.MessageBox.Show("Error initializing Kinect runtime", "UbiNect", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        System.Windows.MessageBox.Show(e.Message, "UbiNect", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                    KinectPluggedIn = false;
                }
                
            }

            if (ShowDebug)
            {
                GuiNect = new GuiNect(this);
                GuiNect.Show();

                GuiNect.Log("Debug output started");

                nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(GuiNect.DepthFrame);
                nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(GuiNect.VGAFrame);
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(GuiNect.SkeletonFrame);
            }

            if (KinectPluggedIn)
            {
                //nui.ColorStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.ColorStream.Enable();
                //nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
                nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                nui.SkeletonStream.Enable();

            }

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_VideoFrameReady);

            if (IncludeRecorder)
                AddComponent<Recorder.Recorder>();
        }

        /// <summary>
        /// Gets called when a depth frame arrives. Only updates the according FPS counter
        /// </summary>
        void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthFPS.Frame();
        }

        /// <summary>
        /// Gets called when a video frame arrives. Updates the FPS counter, sets VGA image and clears skeletons if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void nui_VideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            VGACounter.Frame();

            using (ColorImageFrame im = e.OpenColorImageFrame())
            {
                if (im != null)
                {

                    byte[] bits = new byte[im.PixelDataLength];
                    im.CopyPixelDataTo(bits);

                    VGAImage = BitmapSource.Create(im.Width, im.Height, 96, 96, PixelFormats.Bgr32, null, bits, im.Width * im.BytesPerPixel);
                    if (VideoFrameReady != null)
                        VideoFrameReady(VGAImage);
                }

            }

            if (Environment.TickCount - LastSkeletonFrame > 1500)
                _Skeletons.Clear();
        }

        /// <summary>
        /// Set of currently recognized Skeletons. First key is the tracking id.
        /// </summary>
        Dictionary<int, Dictionary<JointType, Joint>> Skeleton = new Dictionary<int, Dictionary<JointType, Joint>>();

        /// <summary>
        /// Gets called when a skeleton frame arrives.
        /// </summary>
        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            timeSinceLastSkeletonFrame = Environment.TickCount - LastSkeletonFrame;
            Skeleton.Clear();
            LastSkeletonFrame = Environment.TickCount;
            _Skeletons.Clear();

            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                if (sFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[sFrame.SkeletonArrayLength];
                    sFrame.CopySkeletonDataTo(skeletonData);

                    foreach (Skeleton s in skeletonData)
                    {
                        if (s.TrackingState != SkeletonTrackingState.Tracked)
                            continue;
                        Dictionary<JointType, Joint> Joints = new Dictionary<JointType, Joint>();
                        foreach (Joint j in s.Joints)
                        {
                            Joints.Add(j.JointType, j);
                        }
                        Skeleton.Add(s.TrackingId, Joints);
                        _Skeletons.Add(s.TrackingId);
                    }
                }
            }
            

            foreach (RecognitionComponent rc in Components)
                rc.SkeletonFrame(e, Skeleton);
        }

        /// <summary>
        /// Returns the position of the player's spine on screen
        /// </summary>
        /// <param name="TrackingID">Player specific Tracking ID</param>
        /// <param name="c">The canvas which to project the position on</param>
        /// <returns></returns>
        public Point GetPlayerPositionVGA(int TrackingID, Canvas c)
        {
            if (!Skeleton.ContainsKey(TrackingID))
                throw new ArgumentException("Specified Player does not exist");
            SkeletonPoint v = Skeleton[TrackingID][JointType.Spine].Position;
            return v.GetDisplayPositionVGA(nui, c);
        }

        /// <summary>
        /// Adds a new recognition component of the specified type to the Prototype. If such component already exists, nothing is added.
        /// </summary>
        /// <typeparam name="T">The type of the new recognition component</typeparam>
        public void AddComponent<T>() where T : RecognitionComponent, new()
        {
            //kurze Erläuterung zur Syntax: <T> bezeichnet einen generischen Typparameter T.
            //Die where-Klausel im Anschluss schränkt diesen Typ auf Klassen, die von RecognitionComponent erben und einen parameterlosen Konstruktor haben, ein

            foreach (var item in Components)
            {
                if (item is T)
                    return;
            }
            T NewComponent = new T();
            Components.Add(NewComponent);
            nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(NewComponent.DepthFrame);
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(NewComponent.VGAFrame);

            if (GuiNect != null)
            {
                NewComponent.Log += new LogDelegate(GuiNect.Log);
                NewComponent.Log += new LogDelegate(LogArrived);
                GuiNect.InitComponentTab<T>();

            }
        }

        void LogArrived(string message)
        {
            Recorder.Recorder rec = GetComponent<Recorder.Recorder>();
            if (rec != null)
                rec.LogArrived(message);
        }

        /// <summary>
        /// Returns the component of the specified type that is registered within the Prototype. If no such component exists, null is returned.
        /// </summary>
        /// <typeparam name="T">The type of the component to be returned</typeparam>
        /// <returns>The registered component or null</returns>
        public T GetComponent<T>() where T : RecognitionComponent
        {
            foreach (var item in Components)
            {
                if (item is T)
                    return (T)item;
            }
            return default(T);
        }

        public GuiNect getGUI()
        {
            return this.GuiNect;
        }


    }
}
