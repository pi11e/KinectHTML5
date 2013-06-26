using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    class PushGesture : Gesture
    {
        /// <summary>
        /// holder for the startPosture of the hand
        /// </summary>
        private SkeletonPoint starthandPosture;

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
        public PushGesture(String name) : base(name)
        {
            oldPosition = new Dictionary<JointType, Joint>();
            actualDistance = 0.0;
            minimalLength = 0.3;
            gestureDuration = 800;
            observationDistance = 0.02;
            descriptionImage = Properties.Resources.Push;
            description = "For correct execution: 1. stay in front of the kinect  2. hold the right hand in front of you, up to 20 centimeters (in z direction) and 10 centimeter (+/- in y direction) away from right shoulder 3. push your right arm constant in a straight line forward";
        }

        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// (y distance of right hand have to be between 10 centimeter upper and lower form right shoulder)
        /// (z distance have to be smaller than 20 centimeter away from shoulder)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            //actual y position of the shoulder
            float shoulderRight = dict[JointType.ShoulderRight].Position.Y;
            //actual y position of the hand
            float handRight = dict[JointType.HandRight].Position.Y;
            // actual distance between shoulder and hand
            double handDistance = dict[JointType.ShoulderRight].Position.Z - dict[JointType.HandRight].Position.Z;

            if (handRight > shoulderRight-0.1 && handRight < shoulderRight+0.1 && handDistance < 0.2)
            {
                starthandPosture = dict[JointType.HandRight].Position;
                oldPosition = dict;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override startRecognition-Method form Gesture.class
        /// Checks if z distance of actual and lastPosition is higher than 5 centimeter to start recognition
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is overrun</returns>
        public override bool startRecognition(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in centimeter
            double handRight_z = dict[JointType.HandRight].Position.Z;
            // compute z distance to starting posture
            double handDistance = starthandPosture.Z - dict[JointType.HandRight].Position.Z;

            if (handRight_z < oldPosition[JointType.HandRight].Position.Z-0.1)
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
            double handDistance = oldPosition[JointType.HandRight].Position.Z-dict[JointType.HandRight].Position.Z;
            
            if (handDistance >= observationDistance)
            {
                this.oldPosition = dict;
                actualDistance += handDistance;
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
            Debug.WriteLine("Anzahl elemente: "+pos.Count);
            if (pos.Count > 1)
            {
                return true;
            }
            return false;
        }
    }
}
