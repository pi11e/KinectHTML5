using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using NUIVector = Microsoft.Kinect.SkeletonPoint;
using System.ComponentModel;

namespace UbiNect.Button
{
    /// <summary>
    /// super class for button elements, implements the <see cref="Element"/> interface
    /// </summary>
    [Serializable]
    public abstract class  AbstractElement : Element
    {

        public AbstractElement()
        {
            ElementColor = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            Visibility = true;

            IsSelected = false;
        }

        /// <summary>
        /// specifies the ID of a button object as string
        /// </summary>
        public string ID { get; set; }
        
        /// <summary>
        /// specifies the type name of a button object
        /// </summary>
        public abstract String Type { get; set; }
        
        /// <summary>
        /// specifies the color of a button object
        /// </summary>
        public SolidColorBrush ElementColor { get; set; }
        
        /// <summary>
        /// specifies the visibility of a button object
        /// </summary>
        public bool Visibility { get; set; }

        /// <summary>
        /// specifies the selection of a button object (in front and topView)
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Draws a button on a canvas
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        /// <param name="ConvertFromDepthToVGA">If this parameter is set to true, positions are converted to VGA image space. Otherwise they remain in depth image space.</param>

        public abstract void Draw(Canvas c, bool ConvertFromDepthToVGA = true);

        /// <summary>
        /// Draws a button on a canvas in XY space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public abstract void DrawFront(Canvas c);

        /// <summary>
        /// Draws a button on a canvas in XZ space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public abstract void DrawTop(Canvas c);

        /// <summary>
        /// Determines if the button collides with a specified point
        /// </summary>
        /// <param name="v">The vector to be checked</param>
        /// <returns>true, if there is a collision</returns>
        public abstract bool Collision(NUIVector v);
    }
}
