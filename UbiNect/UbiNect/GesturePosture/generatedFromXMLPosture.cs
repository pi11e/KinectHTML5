using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;

namespace UbiNect.GesturePosture
{
    class GeneratedFromXMLPosture : Posture 
    {
        //data-structure for three joints and minimal and maximal angle
        private struct Constraint
        {
            public String JointA;
            public String JointB;
            public String JointC;
            public int min;
            public int max;
        }

        //constraints contains each rule for describe a posture from the XML-File
        private List<Constraint> constraints = new List<Constraint>();


        /// <summary>
        /// Constructs a Posture Recognizer from a XML-File
        /// </summary>
        /// <param name="name">name of the given Posture (same as the XML-Source-File)</param>
        /// <param name="XMLPath">full path of the XML-Source-File</param>
        public GeneratedFromXMLPosture(String name, String xmlPath) : base(name)
        {
            try
            {
                //Load the XML-Document in a new XmlDocument-Object
                XmlDocument constraintsXML = new XmlDocument();
                constraintsXML.Load(xmlPath);

                //put Angle-Nodes from the XML to a XMLNodeList-Object 
                XmlNodeList xmlConstraintsList;
                XmlNode root = constraintsXML.DocumentElement;
                xmlConstraintsList = root.SelectNodes("//posture/constraints/angle");

                //xmlConstraintsList -> constraints
                foreach (XmlNode newConstraint in xmlConstraintsList)
                {
                    Constraint newOne = new Constraint();
                    
                    newOne.JointA = newConstraint.ChildNodes[0].InnerText;
                    newOne.JointB = newConstraint.ChildNodes[1].InnerText;
                    newOne.JointC = newConstraint.ChildNodes[2].InnerText;
                    newOne.min = Convert.ToInt16(newConstraint.ChildNodes[3].InnerText);
                    newOne.max = Convert.ToInt16(newConstraint.ChildNodes[4].InnerText);



                    constraints.Add(newOne);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Sorry, could not read " + xmlPath + ". StackTrace following." + e.StackTrace);
            }

        }


        override public bool isPosture(Dictionary<JointType, Joint> dict)
        {
            if (constraints.Count == 0) { return false; }

            //each constraint from constraints checked with the current skeleton
            foreach (Constraint constraint in constraints)
            {
                SkeletonPoint vecA = dict[(JointType)Enum.Parse(typeof(JointType), constraint.JointA)].Position;
                SkeletonPoint vecB = dict[(JointType)Enum.Parse(typeof(JointType), constraint.JointB)].Position;
                SkeletonPoint vecC = dict[(JointType)Enum.Parse(typeof(JointType), constraint.JointC)].Position;

                double constraintAngle = vecB.AngleToDeg(vecA, vecC);
                //Console.WriteLine("" + constraint.min + " | " + constraintAngle + " | " + constraint.max);

                if ((constraintAngle < constraint.min) || (constraintAngle > constraint.max))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
