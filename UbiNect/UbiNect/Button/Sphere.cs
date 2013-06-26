using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Shapes;
using System.Windows.Media;
using Vector = Microsoft.Kinect.SkeletonPoint;

namespace UbiNect.Button
{

    [Serializable]
    public class Sphere : AbstractElement
    {
        public Vector Center { get; set; }
        private float _radius;

        public float Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                if (value < 0)
                    _radius = 0;
                else
                    _radius = value;
            }
        }

        public override String Type
        {
            get{ return "Kugel"; }
            set { }
        }

        public Sphere(float x, float y, float z, float radius)
        {
            Center = new Vector() { X = x, Y = y, Z = z};
            this.Radius = radius;
            this.ElementColor = Brushes.Magenta;
        }

        /// <summary>
        /// Draws the sphere on the specified canvas
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        /// <param name="ConvertFromDepthToVGA">If this parameter is set to true, positions are converted to VGA image space. Otherwise they remain in depth image space.</param>
        public override void Draw(System.Windows.Controls.Canvas c, bool ConvertFromDepthToVGA = true)
        {
            Ellipse _image;

            // compute projected radius with center and a point from surface
            System.Windows.Point centerProj, surfaceProj;
            double radiusProj = 0.0;

            // projected frontView
            if (ConvertFromDepthToVGA)
            {
                centerProj = Center.GetDisplayPositionVGA(Prototype.nui, c);
                surfaceProj = (new Vector() { X = Center.X, Y = Center.Y + Radius, Z = Center.Z}).GetDisplayPositionVGA(Prototype.nui, c);
            }
            else
            {
                centerProj = Center.GetDisplayPosition(Prototype.nui, c);
                surfaceProj = (new Vector() { X = Center.X, Y = Center.Y + Radius, Z = Center.Z}).GetDisplayPosition(Prototype.nui, c);
            }
            radiusProj = (centerProj - surfaceProj).Length;

            _image = new Ellipse() { Width = 2 * radiusProj, Height = 2 * radiusProj, Fill = ElementColor};
            System.Windows.Controls.Canvas.SetTop(_image, c.Height - ScaleToKinectSpace.scaleToKinectSpaceXY(c, centerProj).Y - radiusProj);
            System.Windows.Controls.Canvas.SetLeft(_image, ScaleToKinectSpace.scaleToKinectSpaceXY(c, centerProj).X - radiusProj);

            // projected frontView
            System.Windows.Controls.Canvas.SetTop(_image, centerProj.Y - radiusProj);
            System.Windows.Controls.Canvas.SetLeft(_image, centerProj.X - radiusProj);

            c.Children.Add(_image);
        }

        /// <summary>
        /// Draws the sphere on the specified canvas in XY space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public override void DrawFront(System.Windows.Controls.Canvas c)
        {
            if (Visibility)
            {
                Brush outline = Brushes.DarkRed;
                
                if (IsSelected)
                    outline = Brushes.DodgerBlue;

                Ellipse _image;
                System.Windows.Point centerProj, surfaceProj;
                double radiusProj = 0.0;

                centerProj = new System.Windows.Point(Center.X, Center.Y);
                surfaceProj = new System.Windows.Point(Center.X, Center.X + Radius);
                radiusProj = ScaleToKinectSpace.scaleToKinectSpaceX(c, Radius);

                _image = new Ellipse() { Width = 2 * radiusProj, Height = 2 * radiusProj, Opacity = 0.4, Stroke = outline};

                System.Windows.Controls.Canvas.SetTop(_image, c.Height - ScaleToKinectSpace.scaleToKinectSpaceXY(c, centerProj).Y - radiusProj);
                System.Windows.Controls.Canvas.SetLeft(_image, ScaleToKinectSpace.scaleToKinectSpaceXY(c, centerProj).X - radiusProj);

                c.Children.Add(_image);
            }
        }

        /// <summary>
        /// Draws the sphere on the specified canvas in XZ space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public override void DrawTop(System.Windows.Controls.Canvas c)
        {
            if (Visibility)
            {
                Brush outline = Brushes.DarkRed;

                if (IsSelected)
                    outline = Brushes.DodgerBlue;

                Ellipse _image;
                System.Windows.Point centerProj, surfaceProj;
                double radiusProj = 0.0;

                centerProj = new System.Windows.Point(Center.X, Center.Z);
                surfaceProj = new System.Windows.Point(Center.X, Center.Z + Radius);

                radiusProj = ScaleToKinectSpace.scaleToKinectSpaceZ(c, Radius);

                _image = new Ellipse() { Width = 2 * radiusProj, Height = 2 * radiusProj, Opacity = 0.4, Stroke = outline };

                System.Windows.Controls.Canvas.SetTop(_image, c.Height - ScaleToKinectSpace.scaleToKinectSpaceXZ(c, centerProj).Y - radiusProj);
                System.Windows.Controls.Canvas.SetLeft(_image, ScaleToKinectSpace.scaleToKinectSpaceXZ(c, centerProj).X - radiusProj);

                c.Children.Add(_image);
            }
        }

        /// <summary>
        /// Determines if the sphere collides with a specified point
        /// </summary>
        /// <param name="v">The vector to be checked</param>
        /// <returns>true, if there is a collision</returns>
        public override bool Collision(Microsoft.Kinect.SkeletonPoint v)
        {
            return (Extensions.DistanceTo(v, Center) <= Radius);
        }

    }
}
