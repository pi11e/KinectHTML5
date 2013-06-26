using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Representate the X Posture(Arms an Legs form a X)
    /// </summary>
    class XPosture : Posture
    {
        /// <summary>
        /// Constructor of XPosture
        /// </summary>
        /// <param name="name">Name of the posture</param>
        public XPosture(String name) : base(name) {
            descriptionImage = Properties.Resources.XPosture;
        }

        /// <summary>
        /// Override isPostureMethode form Posture.class
        /// Checks if given Skeleton matchs XPosture-SkeltonData
        /// (angle of arm and body between 110 and 150 degree)
        /// (angle between both legs upper than 40 degree)
        /// </summary>
        /// <param name="dict">Skeleton</param>
        /// <returns></returns>
        public override bool isPosture(Dictionary<JointType, Joint> dict)
        {
            //angle between Left Arm and Body
            double aLAB = dict[JointType.ShoulderLeft].Position.AngleToDeg(dict[JointType.HandLeft].Position, dict[JointType.ShoulderCenter].Position, dict[JointType.Spine].Position);
            //angle between Right Arm and Body
            double aRAB = dict[JointType.ShoulderRight].Position.AngleToDeg(dict[JointType.HandRight].Position, dict[JointType.ShoulderCenter].Position, dict[JointType.Spine].Position);
            // angle between left leg & right leg
            double aLLRL = dict[JointType.HipCenter].Position.AngleToDeg(dict[JointType.FootLeft].Position, dict[JointType.FootRight].Position);

            if (aLAB > 110 && aLAB <= 150 && aRAB > 110 && aRAB <= 150 && aLLRL > 40)
            {

                return true;
            }
            return false;
        }
    }
}
