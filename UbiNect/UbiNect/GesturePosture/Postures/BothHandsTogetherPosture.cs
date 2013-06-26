using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents a posture where the player has both hands together, i.e. hands are closer than 6cm
    /// </summary>
    class BothHandsTogetherPosture : Posture
    {

        /// <summary>
        /// Constructor of BothHandsTogetherPosture
        /// </summary>
        /// <param name="name">Name of the posture</param>
        public BothHandsTogetherPosture(String name) : base(name){
            descriptionImage = Properties.Resources.BothHandsTogetherPosture;
        }
        
        /// <summary>
        /// Override isPostureMethode form Posture.class
        /// Checks if given Skeleton matchs SkeltonData
        /// (distance between right and left hand smaller than 6 centimeter)
        /// </summary>
        /// <param name="dict">Skeleton</param>
        /// <returns></returns>
        public override bool isPosture(Dictionary<JointType, Joint> dict)
        {
            //Distance between Both Hands in meters
            double handDistance = (dict[JointType.HandLeft].Position.DistanceTo(dict[JointType.HandRight].Position));

            if (handDistance < 0.06)
            {
                return true;
            }
            return false;
        }
    }
}
