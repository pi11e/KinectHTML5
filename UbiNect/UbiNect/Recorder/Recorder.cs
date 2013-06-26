using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using System.IO.Compression;


namespace UbiNect.Recorder
{
    /// <summary>
    /// Component that records some data and saves them in a file
    /// </summary>
    public class Recorder : RecognitionComponent
    {
        public string GetRecognitionType()
        {
            return "Recorder Component";
        }

        public event LogDelegate Log;

        private List<RecordAction> Actions;
        private String CurrentName = null;
        private long StartTime;
        

        /// <summary>
        /// Starts a new record if there is no record running
        /// </summary>
        /// <param name="name">The name of the new record</param>
        public void StartRecord(String name)
        {
            if (CurrentName != null)
            {
                Log("A record is running already.");
                return;
            }
            CurrentName = name;
            Actions = new List<RecordAction>();
            StartTime = Environment.TickCount;
        }

        /// <summary>
        /// Stops the record and saves it to a file
        /// </summary>
        public void StopRecord()
        {
            if(!Directory.Exists("Records"))
                Directory.CreateDirectory("Records");
            FileStream fs = File.OpenWrite("Records/" + CurrentName + ".rec");
            //Gzip compression enlarges file size (already binary)
            //DeflateStream gs = new DeflateStream(fs, CompressionMode.Compress);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, Actions);
            //gs.Close();
            fs.Close();
        }

        long LastRecord = 0;
        /// <summary>
        /// Records frames
        /// </summary>
        public void VGAFrame(object sender, Microsoft.Kinect.ColorImageFrameReadyEventArgs e)
        {

            if (CurrentName == null)
                return;
            //Max 10 fps
            if (Environment.TickCount - LastRecord < 200)
                return;


            using (ColorImageFrame cImageFrame = e.OpenColorImageFrame())
            {
                if (cImageFrame != null)
                {
                    Actions.Add(new RecordedVideoFrame(cImageFrame) { Time = Environment.TickCount - StartTime });
                    LastRecord = Environment.TickCount;
                }
            }

            
        }

        /// <summary>
        /// Depth frame is not used by this component
        /// </summary>
        public void DepthFrame(object sender, Microsoft.Kinect.DepthImageFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// Skeleton frame is not used by this component
        /// </summary>
        public void SkeletonFrame(Microsoft.Kinect.SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<Microsoft.Kinect.JointType, Microsoft.Kinect.Joint>> Skeletons)
        {
        }

        /// <summary>
        /// Gets called whenever a registered component in the underlying prototype logs a message
        /// </summary>
        /// <param name="Message">The message to be logged</param>
        public void LogArrived(String Message)
        {
            if (CurrentName == null)
                return;
            Actions.Add(new LogAction { Time = Environment.TickCount - StartTime, Message = Message });
        }
    }
}
