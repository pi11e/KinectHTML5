using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fleck;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;


namespace Kinect.Server
{
   
    public class InteractionController
    {
        
        static List<IWebSocketConnection> _sockets;

        private static KinectSensor _sensor;  //The Kinect Sensor the application will use
        private InteractionStream _interactionStream;

        private Skeleton[] _skeletons; //the skeletons 
        private UserInfo[] _userInfos; //the information about the interactive users
   
        

        static bool _initialized = false;


        public InteractionController()
        {
            Console.Write("initializing sensor...");
            InitializeKinect();
            Console.WriteLine(_initialized ? " done." : " error!");
            
            Console.WriteLine("initializing sockets...");
            InitializeSockets();
            
        }

        private void InitializeKinect()
        {
            bool useNearMode = false;
        
           // this is just a test, so it only works with one Kinect, and quits if that is not available.
           _sensor = KinectSensor.KinectSensors.FirstOrDefault();
           if (_sensor == null)
           {
               _initialized = false;
               return;
           }
        
           _skeletons = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
           _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
        
        

           _sensor.DepthStream.Range = useNearMode ? DepthRange.Near : DepthRange.Default;
           _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
        
            // for seated mode / near range interaction, enable these:
            
           _sensor.SkeletonStream.TrackingMode = useNearMode ? SkeletonTrackingMode.Seated : SkeletonTrackingMode.Default;
           _sensor.SkeletonStream.EnableTrackingInNearRange = useNearMode;
           _sensor.SkeletonStream.Enable();
        
           _interactionStream = new InteractionStream(_sensor, new DummyInteractionClient());
           _interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;
        
           _sensor.DepthFrameReady += SensorOnDepthFrameReady;
           _sensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;
        
           _sensor.Start();
           _initialized = true;
        
        }

        

        private bool InitializeInteractionRecognizer()
        {
            
            return true;
        }

        

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
                    if (message.StartsWith("setSensorRange("))
                    {
                        /*
                        int parameterLength = message.IndexOf(")") - message.IndexOf("(") -1; // either 1 or 2
                        string numberParameter = message.Substring("setSensorRange(".Length, parameterLength);
                        
                        try
                        {
                            int number = int.Parse(numberParameter);
                            if (-27 < number && number < 27)
                            {
                                _sensor.ElevationAngle = 5;
                            }
                            else 
                            {
                                Console.WriteLine("invalid value. must be between -27 and 27 degrees");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error converting number parameter " + numberParameter + " to int. paramLength = " + parameterLength + "; stacktrace: ");
                            Console.WriteLine(e.StackTrace);
                        }
                         */
                    }
                };
            });

            _initialized = true;
            Console.WriteLine(" done.");
            Console.WriteLine("");

            Console.ReadLine();
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            List<Skeleton> users = new List<Skeleton>();

            using (SkeletonFrame skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                try
                {
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    var accelerometerReading = _sensor.AccelerometerGetCurrentReading();
                    _interactionStream.ProcessSkeleton(_skeletons, accelerometerReading, skeletonFrame.Timestamp);

                    // <KinectHTML5>
                    // KinectHTML5: send skeleton frame over websocket
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

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
                    // </KinectHTML5>
                }
                catch (InvalidOperationException)
                {
                    // SkeletonFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }

        private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            using (DepthImageFrame depthFrame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;

                try
                {
                    _interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // DepthFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }

        private Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();
        private Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();

        private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs args)
        {
            using (var iaf = args.OpenInteractionFrame()) //dispose as soon as possible
            {
                if (iaf == null)
                    return;

                iaf.CopyInteractionDataTo(_userInfos);
            }

            StringBuilder dump = new StringBuilder();
            this.buildAndSendStringDump(dump);

            foreach (var userInfo in _userInfos)
            {
                var userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                    continue;

                var hands = userInfo.HandPointers;
                if (hands.Count == 0)
                    dump.AppendLine("    No hands");
                else
                {
                    foreach (var hand in hands)
                    {
                        if (hand.IsActive)
                        {
                            
                        }


                        var lastHandEvents = hand.HandType == InteractionHandType.Left
                                                 ? _lastLeftHandEvents
                                                 : _lastRightHandEvents;

                        if (hand.HandEventType != InteractionHandEventType.None)
                            lastHandEvents[userID] = hand.HandEventType;

                        var lastHandEvent = lastHandEvents.ContainsKey(userID)
                                                ? lastHandEvents[userID]
                                                : InteractionHandEventType.None;

                        
                    }
                }


            }
            
        }

        private void buildAndSendStringDump(StringBuilder dump)
        {
            var hasUser = false;
            foreach (var userInfo in _userInfos)
            {
                var userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                    continue;

                hasUser = true;
                dump.AppendLine("User ID = " + userID);
                dump.AppendLine(";");
                dump.AppendLine("  Hands: ");
                dump.AppendLine(";");
                var hands = userInfo.HandPointers;
                if (hands.Count == 0)
                    dump.AppendLine("    No hands");
                else
                {
                    foreach (var hand in hands)
                    {
                        
                        var lastHandEvents = hand.HandType == InteractionHandType.Left
                                                 ? _lastLeftHandEvents
                                                 : _lastRightHandEvents;

                        if (hand.HandEventType != InteractionHandEventType.None)
                            lastHandEvents[userID] = hand.HandEventType;

                        var lastHandEvent = lastHandEvents.ContainsKey(userID)
                                                ? lastHandEvents[userID]
                                                : InteractionHandEventType.None;
                            
                        // send a string that says "handconfig:[left|right]:$isActive:$isGripped"
                        var side = hand.HandType == InteractionHandType.Left ? "left:" : "right:";
                        var isActive = hand.IsActive.ToString() + ":";
                        var isGripped = lastHandEvent.ToString() + ":";

                        var message = "handconfig:" + side + isActive + isGripped;
                        if (hasUser)
                        {
                            this.sendMessage(message);
                        }

                        

                        

                        dump.AppendLine();
                        dump.AppendLine("    HandType: " + hand.HandType);
                        dump.AppendLine(";");
                        dump.AppendLine("    HandEventType: " + hand.HandEventType);
                        dump.AppendLine(";");
                        dump.AppendLine("    LastHandEventType: " + lastHandEvent);
                        dump.AppendLine(";");
                        dump.AppendLine("    IsActive: " + hand.IsActive);
                        dump.AppendLine(";");
                        dump.AppendLine("    IsPrimaryForUser: " + hand.IsPrimaryForUser);
                        dump.AppendLine(";");
                        dump.AppendLine("    IsInteractive: " + hand.IsInteractive);
                        dump.AppendLine(";");
                        dump.AppendLine("    PressExtent: " + hand.PressExtent.ToString("N3"));
                        dump.AppendLine(";");
                        dump.AppendLine("    IsPressed: " + hand.IsPressed);
                        dump.AppendLine(";");
                        dump.AppendLine("    IsTracked: " + hand.IsTracked);
                        dump.AppendLine(";");
                        dump.AppendLine("    X: " + hand.X.ToString("N3"));
                        dump.AppendLine(";");
                        dump.AppendLine("    Y: " + hand.Y.ToString("N3"));
                        dump.AppendLine(";");
                        dump.AppendLine("    RawX: " + hand.RawX.ToString("N3"));
                        dump.AppendLine(";");
                        dump.AppendLine("    RawY: " + hand.RawY.ToString("N3"));
                        dump.AppendLine(";");
                        dump.AppendLine("    RawZ: " + hand.RawZ.ToString("N3"));
                        dump.AppendLine(";");
                    }
                }


            }

            // now send the data over via websocket
            if (!hasUser)
            {
                // no user detected
                this.sendMessage("no user detected");
            }
            else
            {
                // users detected
                this.sendMessage(dump.ToString());
            }
        }

        private void sendMessage(string message)
        {
            if (_sockets != null)
            {
                // broadcast message
                foreach (var socket in _sockets)
                {
                    // throws IOException if connection was broken on client side
                    try
                    {
                        socket.Send(Server.SkeletonSerializer.toJSON(message));
                    }
                    catch (System.IO.IOException ioe)
                    {
                        Console.WriteLine("ERROR: Sending message " + message + " could not be completed, socket was terminated.");
                    }
                }
            }

            
        }
        
        static void Main(string[] args)
        {
            if (KinectSensor.KinectSensors.Count <= 0) return;

            InteractionController mc = new InteractionController();
            
        }
    } // end of class InteractionController


         
} // end of using namespace
