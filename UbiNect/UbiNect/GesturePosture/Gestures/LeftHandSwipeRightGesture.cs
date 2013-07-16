﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents the "Move left hand from left to right side" Gesture class
    /// </summary>
    class LeftHandSwipeRightGesture : Gesture
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
        /// Constructs a new Gesture instance LeftHandSwipeRightGesture.
        /// </summary>
        /// <param name="name">name of gesture</param>
        /// <param name="minimalLength">minimal Length of the gesture move</param>
        /// <param name="gestureDuration">maximal duration of the gesture</param>
        public LeftHandSwipeRightGesture(String name) : base(name)
        {
            starthandPosture = new SkeletonPoint();
            oldPosition = new SkeletonPoint();
            actuallength = 0.0;
            minimalLength = 0.3;
            gestureDuration = 1000;
            observationDistance = 0.03;
            descriptionImage = Properties.Resources.LeftHandSwipeRight;
            description = "For correct execution: 1. stay in front of the kinect 2. let your right arm hang loose  3. keep your left hand about 30 centimeters (in x direction) to the left away from your body  4. swipe your left hand in a fluid motion to the right";
        }

        /// <summary>
        /// Override isStartPostureMethod form Gesture.class
        /// Checks if Posture is Startposture of Gesture  
        /// (left hand 40 centimeter away from hip,x coordinate under 0 and right hand distance to hip smaller than 25 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if startPosture is recognized</returns>
        public override bool isStartPosture(Dictionary<JointType, Joint> dict)
        {
            // minimum distance between left hand and left hip joint required to register starting posture
            double distanceThresholdForStartPosture = 0.3;

            //distance between left hand and hip left (in x plane)
            double leftHandToLeftHipDistance = dict[JointType.HipLeft].Position.X-dict[JointType.HandLeft].Position.X;
            //distance between right hand and right hip
            double rightHandToRightHipDistance = dict[JointType.HandRight].Position.X-dict[JointType.HipRight].Position.X;

            if (dict[JointType.HandLeft].Position.X < 0 && leftHandToLeftHipDistance >= distanceThresholdForStartPosture && rightHandToRightHipDistance <= 0.3)
            {
                starthandPosture = dict[JointType.HandLeft].Position;
                oldPosition = starthandPosture;
                actuallength = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override startRecognition-Method form Gesture.class
        /// Checks if distance between startPosture and actual Posture is exceeded to start Recognition 
        /// (threshold is 5 centimeter)
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal Distance is exceeded</returns>
        public override bool startRecognition(Dictionary<JointType, Joint> dict)
        {
            double threshold = 0.05;
            // starthandPosture.X is the left hand position from starting posture
            if (dict[JointType.HandLeft].Position.X > starthandPosture.X + threshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if distance between given Skeleton and oldPosition is greater than observationDistance
        /// </summary>
        /// <param name="dict">a map of Joints and actual joint data</param>
        /// <returns>true, if minimal distance is exceeded</returns>
        public override bool saveObservation(Dictionary<JointType, Joint> dict)
        {
            double Xdistance = dict[JointType.HandLeft].Position.X - oldPosition.X;
            
            if (Xdistance >= observationDistance)
            {
                this.oldPosition = dict[JointType.HandLeft].Position;
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
                    double y = pos[i][JointType.HandLeft].Position.Y;
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
