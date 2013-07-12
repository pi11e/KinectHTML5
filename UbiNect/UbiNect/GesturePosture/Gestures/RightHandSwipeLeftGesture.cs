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
    class RightHandSwipeLeftGesture : Gesture
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
        public RightHandSwipeLeftGesture(String name)
            : base(name)
        {
            starthandPosture = new SkeletonPoint();
            oldPosition = new SkeletonPoint();
            actuallength = 0.0;
            minimalLength = 0.4;
            gestureDuration = 1000;
            observationDistance = 0.03;
            descriptionImage = Properties.Resources.RightHandSwipeLeft;
            description = "For correct execution: 1. stand in front of the kinect 2. let your left arm hang loose  3. keep your right hand about 30 centimeters (in x direction) to the right away from your body  4. swipe your right hand in a fluid motion to the left";
        }
        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// (right hand 40 centimeter away from hip, x coordinate above 0 and left hand distance to hip smaller than 25 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            double distanceThresholdForStartPosture = 0.3;

            //distance between right hand and hip right
            double rightHandToRightHipDistance = dict[JointType.HandRight].Position.X - dict[JointType.HipRight].Position.X;
            //distance between left hand and vector hip-shoulder
            double leftHandToLeftHipDistance = dict[JointType.HipLeft].Position.X - dict[JointType.HandLeft].Position.X;

            if (dict[JointType.HandRight].Position.X > 0 && rightHandToRightHipDistance >= distanceThresholdForStartPosture && leftHandToLeftHipDistance <= 0.3)
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
            if (dict[JointType.HandRight].Position.X < starthandPosture.X - threshold)
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
            double Xdistance = oldPosition.X - dict[JointType.HandRight].Position.X;

            if (Xdistance >= observationDistance)
            {
                this.oldPosition = dict[JointType.HandRight].Position;
                actuallength += Xdistance;
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
            // the observed movement's y-coordinates must not exceed more than +/- the value in yTolerance
            double yTolerance = 0.2;

            if (pos.Count > 3)
            {
                double upperthreshold = starthandPosture.Y + yTolerance;
                double lowerthreshold = starthandPosture.Y - yTolerance;

                for (int i = 0; i < pos.Count; i++)
                {
                    double y = pos[i][JointType.HandRight].Position.Y;
                    if (y < upperthreshold && y > lowerthreshold)
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
