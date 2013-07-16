using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fleck;
using Microsoft.Kinect;

using UbiNect;
using UbiNect.GesturePosture;
using UbiNect.Move;

namespace Kinect.Server
{
    // <ubi>
    // Event hook that handles the dispatching of the MoveRecognized event.
    public delegate void MoveRecognitionDispatcher(AbstractMove m);
    public delegate void GestureRecognitionDispatcher(Gesture g);
    // </ubi>
    public class MainController
    {
        static KinectSensor _nui;
        static List<IWebSocketConnection> _sockets;

        // <ubi>
        private Prototype proto;
        private MoveRecognition mr;
        private GestureRecognition gr;
        public event MoveRecognitionDispatcher MoveRecognized;
        public event GestureRecognitionDispatcher GestureRecognized;

        // </ubi>

        static bool _initialized = false;

        // <ubi>
        public MainController()
        {
            Console.Write("initializing sensor...");
            InitializeKinect();
            Console.WriteLine(_nui.IsRunning ? " done." : " error!");
            Console.Write("initializing move recognition...");
            InitializeMoveRecognizer();
            Console.WriteLine(" done.");
            Console.WriteLine("initializing sockets...");
            InitializeSockets();
            
        }

        private void InitializeMoveRecognizer()
        {
            

            proto = new Prototype(false, false, false);
            /*
            // add move recognition
            proto.AddComponent<MoveRecognition>();

            
            mr = proto.GetComponent<MoveRecognition>();
            // register moves that should be recognized
            // note: existing pause move disabled; to re-enable, add to moves-array
            //String[] moves = { "BackToMenuMove", "NewMenuSelectionMove", "CircleControlMove", "BookShelfConfirm" };
            String[] moves = { "BookShelfConfirm"};
            mr.setRecognizableMoves(moves);

            mr.MoveRecognized += new MoveRecognizedDelegate(mr_MoveRecognized);
            mr.Log += new LogDelegate(mr_Log);
            */

            // add gesture recognition
            proto.AddComponent<GestureRecognition>();

            gr = proto.GetComponent<GestureRecognition>();
            String[] gestures = { "LeftHandSwipeRightGesture", "LeftHandSwipeLeftGesture", "RightHandPullDownGesture", "RightHandPushUpGesture" };
            gr.setRecognizableGestures(gestures, false);
            gr.GestureRecognized += new GestureRecognizedDelegate(gr_GestureRecognized);
            gr.Log += new LogDelegate(mr_Log);
        }

        void gr_GestureRecognized(Gesture g)
        {
            //Console.WriteLine(g.gestureName);
            if (GestureRecognized != null)
            {
                GestureRecognized(g);
            }

            this.sendMessage(g.gestureName);
        }

        // Will get called when registered moves are performed
        void mr_MoveRecognized(AbstractMove m)
        {
            // delegating this to the MoveRecognitionDispatcher allows other components to register to this event
            // and possibly react to it
            if (MoveRecognized != null)
            {
                // raise the MoveRecognized event
                MoveRecognized(m);
            }

            
            // output move name to server console for debugging
            //Console.WriteLine(m.moveName);

            // however, we also want to handle MoveRecognition by telling the websocket 
            this.sendMessage(m.moveName);
        }

        void mr_Log(string message)
        {
            

            Console.WriteLine(message);
            // send message via sockets
            //this.sendMessage(message);
        }

        private void sendMessage(string message)
        {
            
            foreach (var socket in _sockets)
            {

                socket.Send(Server.SkeletonSerializer.toJSON(message));
            }
        }

        // </ubi>

        private static void InitializeSockets()
        {
            _sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://localhost:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Connected to " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    if(socket.ConnectionInfo != null)
                        Console.WriteLine("Disconnected from " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                };
            });

            _initialized = true;

            Console.ReadLine();
        }

        private static void InitializeKinect()
        {
            _nui = KinectSensor.KinectSensors[0];
            try
            {
                _nui.DepthStream.Enable();
                _nui.SkeletonStream.Enable();
                _nui.Start();
                //_nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);
                _nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Nui_SkeletonFrameReady);
            }
            catch (NullReferenceException nre)
            {
                Console.WriteLine("Error initializing Kinect. Make sure the sensor is plugged in and powered up.");
                Console.WriteLine(nre.StackTrace);
            }
        }

        

        static void Nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (!_initialized) return;

            List<Skeleton> users = new List<Skeleton>();

            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                if (sFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[sFrame.SkeletonArrayLength];
                    sFrame.CopySkeletonDataTo(skeletonData);

                    foreach (var user in skeletonData)
                    {
                        if (user.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            users.Add(user);
                        }
                    }

                    if (users.Count > 0)
                    {
                        string json = users.Serialize();

                        foreach (var socket in _sockets)
                        {
                            socket.Send(json);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (KinectSensor.KinectSensors.Count <= 0) return;

            MainController mc = new MainController();
            
        }
    } // end of class MainController


         
} // end of using namespace
