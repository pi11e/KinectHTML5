using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents the "ZoomIn" Gesture class
    /// </summary>
    class ZoomInGesture : Gesture
    {
        /// <summary>
        /// holder for the last saved position
        /// </summary>
        private Dictionary<JointType, Joint> oldPosition;

        /// <summary>
        /// holder for the actual length of the gesture
        /// </summary>
        private double actualDistance;

        private bool firstRequest = true;
        
        /// <summary>
        /// Constructs a new Gesture instance ZoomOutGesture.
        /// </summary>
        /// <param name="name">name of gesture</param>
        /// <param name="minimalLength">minimal Length of the gesture move</param>
        /// <param name="gestureDuration">maximal duration of the gesture</param>
        public ZoomInGesture(String name) : base(name)
        {
            oldPosition = new Dictionary<JointType, Joint>();
            actualDistance = 0.0;
            minimalLength = 1.4;
            gestureDuration = 1000;
            observationDistance = 0.03;
            descriptionImage = Properties.Resources.ZoomIn;
            description = "For correct execution: 1. stay in front of the kinect  2. hold both hands with a distance of about 1 meter to each other in front of your body  3. bring both hands constant togehter";
        }

        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// (distance between left and right hand higher than 100 centimeter)
        /// (distance between right hand and hip center higher than 40 centimeter)
        /// (distance between right hand and hip center higher than 40 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in centimeter
            double handDistance = (dict[JointType.HandRight].Position.DistanceTo(dict[JointType.HandLeft].Position));
            //distance between right hand and hip center
            double rhhc = dict[JointType.HandRight].Position.X-dict[JointType.HipCenter].Position.X;
            //distance between left hand and hip center
            double lhhc = dict[JointType.HipCenter].Position.X-dict[JointType.HandLeft].Position.X;

            if (handDistance > 1.0 && rhhc > 0.4 && lhhc > 0.4)
            {
                oldPosition = dict;
                firstRequest = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override startRecognition-Method form Gesture.class
        /// Checks if distance between right and left hand actual smaller than 91 centimeter 
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is overrun</returns>
        public override bool startRecognition(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in centimeter
            double handDistance = (dict[JointType.HandRight].Position.DistanceTo(dict[JointType.HandLeft].Position));

            if (handDistance < 0.91)
            {
                if (firstRequest)
                {
                    actualDistance = handDistance;
                    firstRequest = false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if distance between given Skeleton and oldPosition is greater than observationDistance
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal distance is overrun</returns>
        public override bool saveObservation(Dictionary<JointType, Joint> dict)
        {
            double rightDistance = oldPosition[JointType.HandRight].Position.X-dict[JointType.HandRight].Position.X;
            double leftDistance = dict[JointType.HandLeft].Position.X-oldPosition[JointType.HandLeft].Position.X;

            if (rightDistance >= observationDistance && leftDistance >= observationDistance)
            {
                this.oldPosition = dict;
                actualDistance += rightDistance + leftDistance;
                return true;
            }
            return false;
        }

        /// <summary>
        /// get the actual length of the special move
        /// </summary>
        /// <returns>actual distance between right and left hand</returns>
        public override double getActualMoveLength()
        {
            return actualDistance;
        }

        /// <summary>
        /// Checks if terms for gesture matches given Skeleton
        /// </summary>
        /// <param name="pos">List of a map of Joints and actual joint data</param>
        /// <returns>true, if Gesture is identified</returns>
        public override bool isGesture(List<Dictionary<JointType, Joint>> pos)
        {
            if (pos.Count > 3)
            {
                firstRequest = true;
                return true;
            }
            return false;
        }
    }
}
