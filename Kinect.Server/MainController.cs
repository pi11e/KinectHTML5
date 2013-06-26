﻿using System;
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
    // </ubi>
    public class MainController
    {
        static KinectSensor _nui;
        static List<IWebSocketConnection> _sockets;

        // <ubi>
        private Prototype proto;
        private MoveRecognition mr;
        public event MoveRecognitionDispatcher MoveRecognized;
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
            //proto.AddComponent<GestureRecognition>();
            proto.AddComponent<MoveRecognition>();

            // add move recognition
            mr = proto.GetComponent<MoveRecognition>();
            // register moves that should be recognized
            // note: existing pause move disabled; to re-enable, add to moves-array
            //String[] moves = { "BackToMenuMove", "NewMenuSelectionMove", "CircleControlMove", "QuitMenuMove" };
            String[] moves = { "QuitMenuMove"};
            mr.setRecognizableMoves(moves);

            mr.MoveRecognized += new MoveRecognizedDelegate(mr_MoveRecognized);
            mr.Log += new LogDelegate(mr_Log);

            
        }

        // Will get called when registered moves are performed
        void mr_MoveRecognized(AbstractMove m)
        {
            // delegating this to the MoveRecognitionDispatcher allows other components to register to this event
            // and possibly react to it
                if (MoveRecognized != null)
                    MoveRecognized(m);

            // however, we want to manage this by telling the websocket about the recognized move
                Console.WriteLine(m.moveName);
        }

        void mr_Log(string message)
        {
            Console.WriteLine(message);
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

        static void Main(string[] args)
        {
            if (KinectSensor.KinectSensors.Count <= 0) return;

            MainController mc = new MainController();
            Console.WriteLine("MainController up and running.");
        }
    } // end of class MainController


         
} // end of using namespace