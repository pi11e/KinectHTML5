using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PostureCreator
{
    /// <summary>
    /// A kind of wrapper class for a line between two ellipses. And because the ellipses are joints, the lines are bones :)
    /// </summary>
    class CustomBone
    {
        private CustomJoint jOne;
        private CustomJoint jTwo;
        private Line boneLine;

        /// <summary>
        /// Constructor for a bone-class.
        /// </summary>
        /// <param name="jOne">One joint.</param>
        /// <param name="jTwo">The other joint.</param>
        public CustomBone(CustomJoint jOne, CustomJoint jTwo)
        {
            this.jOne = jOne;
            this.jTwo = jTwo;

            boneLine = new Line();
            boneLine.Stroke = System.Windows.Media.Brushes.Black;
            // the +10 is neccessary to draw the line between the centers of two ellipses (joints)
            boneLine.X1 = jOne.getX() + 10;
            boneLine.X2 = jTwo.getX() + 10;
            boneLine.Y1 = jOne.getY() + 10;
            boneLine.Y2 = jTwo.getY() + 10;
            boneLine.StrokeThickness = 3;

            // handler to react on the movement of the joints in dragging-mode
            jOne.Event_onJointIsMoving += new OnJointIsMovingHandler(OnJointOneIsMoving);
            jTwo.Event_onJointIsMoving += new OnJointIsMovingHandler(OnJointTwoIsMoving);
        }

        /// <summary>
        /// Handler, the changes the position of an endpoint of the line.
        /// </summary>
        private void OnJointOneIsMoving()
        {
            boneLine.X1 = jOne.getX() + 10;
            boneLine.Y1 = jOne.getY() + 10;
        }

        /// <summary>
        /// Handler, the changes the position of an endpoint of the line.
        /// </summary>
        private void OnJointTwoIsMoving()
        {
            boneLine.X2 = jTwo.getX() + 10;
            boneLine.Y2 = jTwo.getY() + 10;
        }

        /// <summary>
        /// Returns the <see cref="Line"/>-Objekt.
        /// </summary>
        /// <returns></returns>
        public Line getLine()
        {
            return boneLine;
        }
    }
}
