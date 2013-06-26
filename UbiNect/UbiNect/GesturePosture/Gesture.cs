using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Drawing;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents a Gesture
    /// </summary>
    abstract public class Gesture
    {
        /// <summary>
        /// description for the correct performance of a gesture
        /// </summary>
        public String description { get; set; }

        /// <summary>
        /// name of an image describing the start & end posture of a gesture (200x250 png)
        /// </summary>
        public Bitmap descriptionImage { get; set; }
        
        /// <summary>
        /// the gesture name
        /// </summary>
        public String gestureName { get; set; }

        /// <summary>
        /// the maximal duration that the gesture may have
        /// </summary>
        public double gestureDuration { get; set; }

        /// <summary>
        /// the minimal length that the gesture must have
        /// </summary>
        public double minimalLength { get; set; }

        /// <summary>
        /// Each gesture is evaluated by a number of movement observations. This value describes the space interval of each observation.
        /// </summary>
        protected double observationDistance;
        
        /// <summary>
        /// Constructs a new gesture instance with minimal length and maximal duration time 
        /// </summary>
        /// <param name="name">name of gesture</param>
        /// <param name="minimalLength">minimal Length of the gesture move (in meters)</param>
        /// <param name="gestureDuration">maximal duration of the gesture (in milliseconds)</param>
        public Gesture(String name)
        {
            gestureName = name;
            gestureDuration = 0.0;
            minimalLength = 0.0;
            this.observationDistance = 0.0;
        }

        /// <summary>
        /// Constructs a new posture instance described with start- and endposture and maximal duration time
        /// </summary>
        /// <param name="name">name of posture</param>
        /// <param name="gestureDuration">maximal duration of the gesture (in milliseconds)</param>
        /// <param name="startPosture">starting posture to initiate recognition</param>
        /// <param name="endPosture">end posture to complete recognition</param>
        public Gesture(String name, double gestureDuration, Posture startPosture, Posture endPosture)
        {
            gestureName = name;
        }

        /// <summary>
        /// Checks if given skeleton data matches the described gesture
        /// </summary>
        /// <param name="pos">List of a map of Joints and actual joint data</param>
        /// <returns>true, if Gesture is identified</returns>
        abstract public bool isGesture(List<Dictionary<JointType, Joint>> pos);

        /// <summary>
        /// Check if given skeleton data matches the starting posture of a gesture
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        abstract public bool isStartPosture(Dictionary<JointType, Joint> dict);

        /// <summary>
        /// Checks if distance between starting posture and current position is large enough to start recognition  
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is overrun</returns>
        abstract public bool startRecognition(Dictionary<JointType, Joint> dict);

        /// <summary>
        /// Checks if given skeleton data meets the requirements to accept it as a new position of the gesture movement
        /// </summary>
        /// <param name="dict">actual Skeleton to test</param>
        /// <returns>true, if condition is met</returns>
        abstract public bool saveObservation(Dictionary<JointType, Joint> dict);

        /// <summary>
        /// Get the actual length of the current gesture move
        /// </summary>
        /// <returns>actual length of the move</returns>
        abstract public double getActualMoveLength();

    }
}
