using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PostureCreator
{
    class Constraint
    {
        private Joint baseJoint, legJointOne, legJointTwo;
        private double minAngle, maxAngle, tolerance;

        /// <summary>
        /// Contructor of a constraint.
        /// </summary>
        /// <param name="baseJoint"></param>
        /// <param name="legJointOne"></param>
        /// <param name="legJointTwo"></param>
        public Constraint(Joint baseJoint, Joint legJointOne, Joint legJointTwo, double tolerance)
        {
            this.baseJoint = baseJoint;
            this.legJointOne = legJointOne;
            this.legJointTwo = legJointTwo;
            this.tolerance = tolerance;
            calcAngles();
        }

        /// <summary>
        /// Calculates the angle between the two leg-joints and the base-joint. Is called, when an instance of constraint is generated.
        /// </summary>
        private void calcAngles()
        {
            double V1x = legJointOne.getX() - baseJoint.getX();
            double V2x = legJointTwo.getX() - baseJoint.getX();
            double V1y = legJointOne.getY() - baseJoint.getY();
            double V2y = legJointTwo.getY() - baseJoint.getY();

            Vector V1 = new Vector(V1x, V1y);
            Vector V2 = new Vector(V2x, V2y);
            double d = (V1 * V2) / (V1.Length * V2.Length);
            double angle = (Math.Floor(180 * Math.Acos(d) / Math.PI));
            if (angle > 180) angle -= 180;
            double min = Math.Floor(angle - tolerance);
            double max = Math.Floor(angle + tolerance);
            minAngle = Math.Min(min, max);
            maxAngle = Math.Max(min, max);
            if (minAngle < 0) minAngle = 0;
            if (maxAngle > 180) maxAngle = 180;
            Console.WriteLine("minAngle: " + minAngle + "  maxAngle: "+ maxAngle);
        }

        /// <summary>
        /// Getter for the min-angle.
        /// </summary>
        /// <returns>min-angle</returns>
        public double getMinAngle()
        {
            return minAngle;
        }

        /// <summary>
        /// Getter for the max-angle.
        /// </summary>
        /// <returns>max-angle</returns>
        public double getMaxAngle()
        {
            return maxAngle;
        }

        /// <summary>
        /// Getter for one of the two leg-<see cref="Joint"/>-objects.
        /// </summary>
        /// <returns>legJointOne</returns>
        public Joint getLegJointOne()
        {
            return legJointOne;
        }

        /// <summary>
        /// Getter for one of the two leg-<see cref="Joint"/>-objects.
        /// </summary>
        /// <returns>legJointTwo</returns>
        public Joint getLegJointTwo()
        {
            return legJointTwo;
        }

        /// <summary>
        /// Getter for the base-<see cref="Joint"/>-object.
        /// </summary>
        /// <returns>legJointTwo</returns>
        public Joint getBaseJoint()
        {
            return baseJoint;
        }
    }
}
