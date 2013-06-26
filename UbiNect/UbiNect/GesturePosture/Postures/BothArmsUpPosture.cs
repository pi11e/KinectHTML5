using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents a posture where the player has both arms up
    /// </summary>
    class BothArmsUpPosture : Posture
    {
        
        /// <summary>
        /// Cosntructor of BothArmsUpPosture
        /// </summary>
        /// <param name="name">Name of the posture</param>
        public BothArmsUpPosture(String name): base(name){
            descriptionImage = Properties.Resources.BothArmsUpPosture;
        }

        /// <summary>
        /// Overrides isPosture from Posture.class
        /// Checks if given Skeleton matchs BothArmsUP-Skeleton
        /// (angle of arm and body between 140 and 180 degree)
        /// (angle arm higher 150 degree /straight-line)
        /// </summary>
        /// <param name="dict">Skeleton</param>
        /// <returns></returns>
        public override bool isPosture(Dictionary<JointType, Joint> dict)
        {
            //angle between Left Arm and Body
            double aLAB = dict[JointType.ShoulderLeft].Position.AngleToDeg(dict[JointType.ElbowLeft].Position, dict[JointType.ShoulderCenter].Position, dict[JointType.Spine].Position);
            //angle of Left Arm
            double aLA = dict[JointType.ElbowLeft].Position.AngleToDeg(dict[JointType.ShoulderLeft].Position, dict[JointType.WristLeft].Position);
            //angle between Right Arm and Body
            double aRAB = dict[JointType.ShoulderRight].Position.AngleToDeg(dict[JointType.ElbowRight].Position, dict[JointType.ShoulderCenter].Position, dict[JointType.Spine].Position);
            //angle of Right Arm
            double aRA = dict[JointType.ElbowRight].Position.AngleToDeg(dict[JointType.ShoulderRight].Position, dict[JointType.WristRight].Position);

            if (aLAB > 140 && aLAB <= 180 && aRAB > 140 && aRAB <= 180)
            {
                if (aLA > 150 && aRA > 150)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
