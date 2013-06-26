using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents the "ZoomOut" Gesture class
    /// </summary>
    class ZoomOutGesture : Gesture
    {

        /// <summary>
        /// holder for the last saved position
        /// </summary>
        private Dictionary<JointType, Joint> oldPosition;

        /// <summary>
        /// holder for the actual length of the gesture
        /// </summary>
        private double actualDistance;
        
        /// <summary>
        /// Constructs a new Gesture instance ZoomOutGesture.
        /// </summary>
        /// <param name="name">name of gesture</param>
        /// <param name="minimalLength">minimal Length of the gesture move</param>
        /// <param name="gestureDuration">maximal duration of the gesture</param>
        public ZoomOutGesture(String name):base(name)
        {
            oldPosition = new Dictionary<JointType, Joint>();
            actualDistance = 0.0;
            minimalLength = 1.2;
            gestureDuration = 1000;
            observationDistance = 0.03;
            descriptionImage = Properties.Resources.ZoomOut;
            description = "For correct execution: 1. stay in front of the kinect  2. hold both hands with a distance up to 20 centimeter to each other in front of your body  3. move both hands constant away from each other";
        }


        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// (distance between left and right hand smaller than 20 centimeter and higher than 10 centimeter)
        /// (distance between right hand and hip center smaller than 20 centimeter)
        /// (distance between right hand and hip center smaller than 20 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in centimeter
            double handDistance = (dict[JointType.HandLeft].Position.DistanceTo(dict[JointType.HandRight].Position));
            //distance between right hand and hip center
            double rhhc = dict[JointType.HandRight].Position.X-dict[JointType.HipCenter].Position.X;
            //distance between left hand and hip center
            double lhhc = dict[JointType.HipCenter].Position.X-dict[JointType.HandLeft].Position.X;

            if (handDistance < 0.2 && handDistance > 0.10 && rhhc < 0.2 && lhhc < 0.2)
            {
                oldPosition = dict;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override startRecognition-Method form Gesture.class
        /// Checks if distance between right and left hand higher than 22 centimeter
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is overrun</returns>
        public override bool startRecognition(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in centimeter
            double handDistance = (dict[JointType.HandLeft].Position.DistanceTo(dict[JointType.HandRight].Position));

            if (handDistance > 0.22)
            {
                actualDistance = handDistance;
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
            double rightDistance = dict[JointType.HandRight].Position.X - oldPosition[JointType.HandRight].Position.X;
            double leftDistance = oldPosition[JointType.HandLeft].Position.X - dict[JointType.HandLeft].Position.X;

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
                return true;
            }
            return false;
        }
    }
}
