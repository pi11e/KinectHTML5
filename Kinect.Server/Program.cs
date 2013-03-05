using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fleck;
using Microsoft.Kinect;

namespace Kinect.Server
{
    class Program
    {
        static KinectSensor _nui;
        static List<IWebSocketConnection> _sockets;

        static bool _initialized = false;

        static void Main(string[] args)
        {
            if (KinectSensor.KinectSensors.Count <= 0) return;

            InitilizeKinect();
            InitializeSockets();
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
                };
            });

            _initialized = true;

            Console.ReadLine();
        }

        private static void InitilizeKinect()
        {
            _nui = KinectSensor.KinectSensors[0];
            _nui.DepthStream.Enable();
            _nui.SkeletonStream.Enable();
            _nui.Start();
            //_nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);
            _nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Nui_SkeletonFrameReady);
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
    }
}
