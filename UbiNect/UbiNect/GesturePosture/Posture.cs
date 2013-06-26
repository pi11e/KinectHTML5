using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Drawing;

namespace UbiNect.GesturePosture
{
    /// <summary>
    /// Represents a Posture
    /// </summary>
    abstract public class Posture
    {
        /// <summary>
        /// the name of the posture
        /// </summary>
        public String postureName { get; set; }

        /// <summary>
        /// Constructs a new posture instance from a given XML filepath.
        /// </summary>
        /// <param name="name">Specify the name of a new posture</param>
        /// <param name="XMLPath">Specify a filepath to an XML document which is used to generate a new posture</param>
        public Posture(String name, String XMLPath)
        {
            postureName = name;
        }

        /// <summary>
        /// name of an image describing the start & end posture of a gesture (200x250 png)
        /// </summary>
        public Bitmap descriptionImage { get; set; }

        /// <summary>
        /// Constructs a new posture instance.
        /// </summary>
        /// <param name="name">Specify the name of a new posture.</param>
        public Posture(String name)
        {
            postureName = name;
        }

        /// <summary>
        /// Checks if given skeleton data matches the described posture
        /// </summary>
        /// <param name="dict">skeleton data</param>
        /// <returns>true if skeleton data matches posture</returns>
        abstract public bool isPosture(Dictionary<JointType, Joint> dict);

    }
}
