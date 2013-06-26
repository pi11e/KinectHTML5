using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace UbiNect
{
    public delegate void LogDelegate(String message);

    public interface RecognitionComponent
    {
        /// <summary>
        /// Returns Recognitiontypes type
        /// </summary>
        /// <returns></returns>
        String GetRecognitionType();

        

        /// <summary>
        /// Event that is raised when the component wants to log something
        /// </summary>
        event LogDelegate Log;

        /// <summary>
        /// Gets called when a video frame is ready
        /// </summary>
        void VGAFrame(object sender, ColorImageFrameReadyEventArgs e);

        /// <summary>
        /// Gets called when a depth frame is ready
        /// </summary>
        void DepthFrame(object sender, DepthImageFrameReadyEventArgs e);

        /// <summary>
        /// Gets called when a skeleton frame is ready
        /// </summary>
        void SkeletonFrame(SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<JointType, Joint>> Skeletons);
    }
}
