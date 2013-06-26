using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Microsoft.Kinect;

namespace UbiNect.Move
{
    

    public class PauseMove : AbstractMove
    {
        Timer wait;
        bool fireMoveAllowed;

        public PauseMove(String name, int id)
            : base(name, id)
        {
            // init here
            wait = new Timer(1000);
            fireMoveAllowed = false;
            wait.Elapsed += new ElapsedEventHandler(wait_Elapsed);
            wait.Start();
        }

        void wait_Elapsed(object sender, ElapsedEventArgs e)
        {
            // every wait.interval, allow the isMove to be computed and to potentially return true
            fireMoveAllowed = true;
        }

        /// <summary>
        /// Fired whenever the right hand is raised more than 60cm in front of a player's head.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public override bool isMove(Dictionary<JointType, Joint> dict)
        {
            if (fireMoveAllowed)
            {
                /*
                fireMoveAllowed = false;
                float distance = dict[Joint.Head].Position.Z - dict[JointType.HandRight].Position.Z;
                // if right hand is more than 60cm closer to kinect sensor than a user's head
                return distance > 0.6f;
                */

                fireMoveAllowed = false;
                float distance = dict[JointType.Head].Position.Z - dict[JointType.HandRight].Position.Z;
                float rightX = dict[JointType.HandRight].Position.X;
                float rightY = dict[JointType.HandRight].Position.Y;


                return distance > 0.65f && rightX > dict[JointType.Head].Position.X && rightY > dict[JointType.HipRight].Position.Y + 0.15;


            }
            return false;
            
        }
    }
}
