using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    public class BackToMenuMove : AbstractMove
    {
        
        public BackToMenuMove(String name, int id)
            : base(name, id)
        {
            // init here

        }

        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            /*
            float leftHandPositionY = dict[JointType.HandLeft].Position.Y;
            float headPositionY = dict[Joint.Head].Position.Y;
            return leftHandPositionY > (headPositionY +  0.1F);
            */
            float rightHandPositionY = dict[JointType.HandRight].Position.Y;
            float headPositionY = dict[JointType.Head].Position.Y;
            return rightHandPositionY > (headPositionY + 0.1F);
        }
    }
}
