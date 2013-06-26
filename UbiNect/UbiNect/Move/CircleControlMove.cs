using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    public class CircleControlMove : AbstractMove
    {
        public float rightHandX { get; set; }
        public float rightShoulderX { get; set; }

        public CircleControlMove(String name, int id)
            : base(name, id)
        {
            // init here

        }

        /// <summary>
        /// Fired whenever the right hand is 
        /// situated between 50cm left & right of shoulder
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            SkeletonPoint rightHandPos = dict[JointType.HandRight].Position;
            SkeletonPoint rightShoulderPos = dict[JointType.ShoulderRight].Position;
            bool cond1 = rightHandPos.Y < rightShoulderPos.Y;
            bool cond2 = rightHandPos.X > rightShoulderPos.X - 0.42F && rightHandPos.X < rightShoulderPos.X + 0.42F;
            rightHandX = rightHandPos.X;
            rightShoulderX = rightShoulderPos.X;
            return cond1 && cond2;
        }
    }
}
