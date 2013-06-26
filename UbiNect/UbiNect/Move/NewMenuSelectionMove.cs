using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    public class NewMenuSelectionMove : AbstractMove
    {
        public SkeletonPoint handPosition { get; set; }
        public SkeletonPoint shoulderPosition { get; set; }

        public NewMenuSelectionMove(String name, int id) : base(name, id)
        {
            // init here
            
        }

        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            handPosition = dict[JointType.HandRight].Position;
            shoulderPosition = dict[JointType.ShoulderRight].Position;

            /*
            bool cond1 = handPosition.X < shoulderPosition.X + 0.3F && handPosition.X > shoulderPosition.X - 0.3F;
            bool cond2 = handPosition.Y < shoulderPosition.Y + 0.2F && handPosition.Y > shoulderPosition.Y - 0.4F;
            */

            float direction_x = handPosition.X - shoulderPosition.X;
            float direction_y = handPosition.Y - shoulderPosition.Y;
            bool cond1 = direction_x < 0.3F && direction_x > -0.3F;
            bool cond2 = direction_y < 0.2F && direction_y > -0.4F;

            return cond1 && cond2;
        }
    }
}
