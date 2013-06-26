using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using NUIVector = Microsoft.Kinect.SkeletonPoint;

namespace UbiNect
{

    /// <summary>
    /// Saves the positions of a vector in VGA space and in skeleton space
    /// </summary>
    class DualPoint
    {
        public Point DisplayPosition { get; set; }
        public NUIVector SkeletonSpacePosition { get; set; }
    }
}
