using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace UbiNect.Move
{
    public class MenuSelectionMove : AbstractMove
    {
        public SkeletonPoint handPosition { get; set; }
        public SkeletonPoint shoulderPosition { get; set; }
        public SkeletonPoint leftHandPosition { get; set; }
        public SkeletonPoint leftShoulderPosition { get; set; }

        public MenuSelectionMove(String name, int id) : base(name, id)
        {
            // init here
            
        }

        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            handPosition = dict[JointType.HandRight].Position;
            shoulderPosition = dict[JointType.ShoulderRight].Position;
            leftHandPosition = dict[JointType.HandLeft].Position;
            leftShoulderPosition = dict[JointType.ShoulderLeft].Position;
            return true;
        }
    }
}
