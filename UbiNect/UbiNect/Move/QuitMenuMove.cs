using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    public class QuitMenuMove : AbstractMove
    {

        public QuitMenuMove(String name, int id)
            : base(name, id)
        {
            // init here

        }

        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            float leftHandPositionY = dict[JointType.HandLeft].Position.Y;
            float rightHandPositionY = dict[JointType.HandRight].Position.Y;
            float headPositionY = dict[JointType.Head].Position.Y;
            return leftHandPositionY > headPositionY && rightHandPositionY > headPositionY;
        }
    }
}
