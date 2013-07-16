using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents the "Move right hand from right to left side" Gesture class
    /// </summary>
    class RightHandPushUpGesture : Gesture
    {
        /// <summary>
        /// holder for the startPosture of the hand
        /// </summary>
        private SkeletonPoint starthandPosture;
        
        /// <summary>
        /// holder for the last saved position
        /// </summary>
        private SkeletonPoint oldPosition;

        /// <summary>
        /// holder for the actual length of the gesture
        /// </summary>
        private double actuallength;

        /// <summary>
        /// Constructs a new Gesture instance MoveHandRightToLeft.
        /// </summary>
        /// <param name="name">name of gesture</param>
        /// <param name="minimalLength">minimal Length of the gesture move</param>
        /// <param name="gestureDuration">maximal duration of the gesture</param>
         public RightHandPushUpGesture(String name): base(name)
        {
            starthandPosture = new SkeletonPoint();
            oldPosition = new SkeletonPoint();
            actuallength = 0.0;
            minimalLength = 0.3;
            gestureDuration = 1000;
            observationDistance = 0.03;
            descriptionImage = Properties.Resources.RightHandSwipeLeft;
            description = "";
        }
        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            // for the push up gesture, the right hand should be
            // - not too far off to the left or right (same x plane basically as the shoulder itself, with some tolerance margin)
            // - below shoulder height
            // - actually pretty far below shoulder height
            
            //distance between right hand and right shoulder
            double rightHandToRightShoulderDistanceX = Math.Abs(dict[JointType.HandRight].Position.X-dict[JointType.ShoulderRight].Position.X);
            double rightHandToRightShoulderDistanceY = Math.Abs(dict[JointType.HandRight].Position.Y - dict[JointType.ShoulderRight].Position.Y);
            double rightHandToShoulderDistanceZ = Math.Abs(dict[JointType.HandRight].Position.Z - dict[JointType.ShoulderCenter].Position.Z);

            // all of these are in terms of right hand
            bool stretchedOutFromShoulder = rightHandToShoulderDistanceZ > 0.2;
            bool inXPlaneOfShoulder = rightHandToRightShoulderDistanceX < 0.15;
            bool belowShoulder = dict[JointType.HandRight].Position.Y < dict[JointType.ShoulderCenter].Position.Y;
            bool farBelowShoulder = rightHandToRightShoulderDistanceY > 0.3;


            // former if-clause from right hand swipe left gesture for comparison
            //if (dict[JointType.HandRight].Position.X > 0 && rightHandToRightHipDistance >= distanceThresholdForStartPosture && leftHandToLeftHipDistance <= 0.3)

            if (stretchedOutFromShoulder && inXPlaneOfShoulder && belowShoulder && farBelowShoulder)
            {
                starthandPosture = dict[JointType.HandRight].Position;
                oldPosition = starthandPosture;
                actuallength = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override startRecognition-Method form Gesture.class
        /// Checks if distance between startPosture and actual Posture is overrun to start Recognition 
        /// (threshold is 5 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is overrun</returns>
        public override bool startRecognition(Dictionary<JointType, Joint> dict)
        {
            double threshold = 0.05;

            // this gesture describes an upward motion in front of the right shoulder, so we check if the y value increases
            if (dict[JointType.HandRight].Position.Y > starthandPosture.Y - threshold)
            {
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
            double Ydistance = oldPosition.Y-dict[JointType.HandRight].Position.Y;

            if (Ydistance >= observationDistance)
            {
                this.oldPosition = dict[JointType.HandRight].Position;
                actuallength += Ydistance;
                return true;
            }
            return false;
        }

        /// <summary>
        /// get the actual length of the special move
        /// </summary>
        /// <returns>actual length of the move</returns>
        public override double getActualMoveLength()
        {
             return actuallength;
        }

        /// <summary>
        /// Checks if terms for gesture matches given Skeleton
        /// </summary>
        /// <param name="pos">List of a map of Joints and actual joint data</param>
        /// <returns>true, if Gesture is identified</returns>
        public override bool isGesture(List<Dictionary<JointType, Joint>> pos)
        {
            // the observed movement's x-coordinates must not exceed more than +/- the value in xTolerance
            double xTolerance = 0.15;

            // if a sufficient number of values has been recorded
            if (pos.Count > 3)
            {
                double upperthreshold = starthandPosture.X + xTolerance;
                double lowerthreshold = starthandPosture.X - xTolerance;

                for (int i = 0; i < pos.Count; i++)
                {
                    double x = pos[i][JointType.HandRight].Position.X;
                    if (x < upperthreshold && x > lowerthreshold) // x value remains within the bounds of the x tolerance threshold
                        continue;
                    else
                    {
                         return false;
                    }
                }
                return true;
            }
            return false;
        }

    }
}
