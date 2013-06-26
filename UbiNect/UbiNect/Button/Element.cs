using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using NUIVector = Microsoft.Kinect.SkeletonPoint;

namespace UbiNect.Button
{
    /// <summary>
    /// Interface for a button element
    /// </summary>
    public interface Element
    {
        /// <summary>
        /// Draws a button on a canvas
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        /// <param name="ConvertFromDepthToVGA">If this parameter is set to true, positions are converted to VGA image space. Otherwise they remain in depth image space.</param>
        void Draw(Canvas c, bool ConvertFromDepthToVGA = true);

        /// <summary>
        /// Draws a button on a canvas in XY space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        void DrawFront(Canvas c);

        /// <summary>
        /// Draws a button on a canvas in XZ space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        void DrawTop(Canvas c);

        /// <summary>
        /// Determines if the button collides with a specified point
        /// </summary>
        /// <param name="v">The vector to be checked</param>
        /// <returns>true, if there is a collision</returns>
        bool Collision(NUIVector v);
    }
}
