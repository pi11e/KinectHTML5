using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    abstract public class AbstractMove
    {
        public int playerID { get; set; }
        public String moveName { get; set; }
        public float depthIndex { get; set; }

        public AbstractMove(String name, int player)
        {
            moveName = name;
            playerID = player;
        }

        abstract public bool isMove(Dictionary<JointType, Joint> dict);
    }
}
