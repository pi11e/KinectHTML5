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

    /// <summary>
    /// A triangle in 3D space
    /// </summary>
    [Serializable]
    public class Triangle : AbstractElement
    {
        /// <summary>
        /// Corner of the triangle
        /// </summary>
        Vector v1, v2, v3;

        public override String Type
        {
            get { return "Dreieck"; }
            set { }
        }

        /// <summary>
        /// Creates a new triangle from the position of its corners
        /// </summary>
        /// <param name="x1">x coordinate of first corner</param>
        /// <param name="y1">y coordinate of first corner</param>
        /// <param name="z1">z coordinate of first corner</param>
        /// <param name="x2">x coordinate of second corner</param>
        /// <param name="y2">y coordinate of second corner</param>
        /// <param name="z2">z coordinate of second corner</param>
        /// <param name="x3">x coordinate of third corner</param>
        /// <param name="y3">y coordinate of third corner</param>
        /// <param name="z3">z coordinate of third corner</param>
        public Triangle(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3)
        {
            v1 = new Vector() { X = x1, Y = y1, Z = z1};
            v2 = new Vector() { X = x2, Y = y2, Z = z2};
            v3 = new Vector() { X = x3, Y = y3, Z = z3};
        }

        /// <summary>
        /// Creates a new triangle from the position of its corners
        /// </summary>
        /// <param name="v1">Position of first corner</param>
        /// <param name="v2">Position of second corner</param>
        /// <param name="v3">Position of third corner</param>
        public Triangle(Vector v1, Vector v2, Vector v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }        

        /// <summary>
        /// Draws the triangle on the specified canvas
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        /// <param name="ConvertFromDepthToVGA">If this parameter is set to true, positions are converted to VGA image space. Otherwise they remain in depth image space.</param>
        public override void Draw(System.Windows.Controls.Canvas c, bool ConvertFromDepthToVGA = true)
        {
            Polygon tri = new Polygon();
            if (ConvertFromDepthToVGA)
            {
                tri.Points.Add(v1.GetDisplayPositionVGA(Prototype.nui, c));
                tri.Points.Add(v2.GetDisplayPositionVGA(Prototype.nui, c));
                tri.Points.Add(v3.GetDisplayPositionVGA(Prototype.nui, c));
            }
            else
            {
                tri.Points.Add(v1.GetDisplayPosition(Prototype.nui, c));
                tri.Points.Add(v2.GetDisplayPosition(Prototype.nui, c));
                tri.Points.Add(v3.GetDisplayPosition(Prototype.nui, c));
            }

            tri.Fill = ElementColor;

            c.Children.Add(tri);
        }

        /// <summary>
        /// Draws the triangle on the specified canvas in XY space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public override void DrawFront(System.Windows.Controls.Canvas c)
        {
            if (Visibility)
            {
                Brush outline = Brushes.DarkRed;

                if (IsSelected)
                    outline = Brushes.DodgerBlue;

                Polygon tri = new Polygon();
                System.Windows.Point v1s = ScaleToKinectSpace.scaleToKinectSpaceXY(c, new System.Windows.Point(v1.X, v1.Y));
                System.Windows.Point v2s = ScaleToKinectSpace.scaleToKinectSpaceXY(c, new System.Windows.Point(v2.X, v2.Y));
                System.Windows.Point v3s = ScaleToKinectSpace.scaleToKinectSpaceXY(c, new System.Windows.Point(v3.X, v3.Y));

                v1s.Y = c.Height - v1s.Y; v2s.Y = c.Height - v2s.Y; v3s.Y = c.Height - v3s.Y;

                tri.Points.Add(v1s); tri.Points.Add(v2s); tri.Points.Add(v3s);

                tri.Stroke = outline;

                c.Children.Add(tri);
            }
               
        }

        /// <summary>
        /// Draws the triangle on the specified canvas in XZ space
        /// </summary>
        /// <param name="c">The canvas which to paint the button on</param>
        public override void DrawTop(System.Windows.Controls.Canvas c)
        {
            if (Visibility)
            {
                Brush outline = Brushes.DarkRed;

                if (IsSelected)
                    outline = Brushes.DodgerBlue;

                Polygon tri = new Polygon();
                System.Windows.Point v1s = ScaleToKinectSpace.scaleToKinectSpaceXZ(c, new System.Windows.Point(v1.X, v1.Z));
                System.Windows.Point v2s = ScaleToKinectSpace.scaleToKinectSpaceXZ(c, new System.Windows.Point(v2.X, v2.Z));
                System.Windows.Point v3s = ScaleToKinectSpace.scaleToKinectSpaceXZ(c, new System.Windows.Point(v3.X, v3.Z));

                v1s.Y = c.Height - v1s.Y; v2s.Y = c.Height - v2s.Y; v3s.Y = c.Height - v3s.Y;

                tri.Points.Add(v1s); tri.Points.Add(v2s); tri.Points.Add(v3s);

                tri.Stroke = outline;

                c.Children.Add(tri);
            }
        }

        /// <summary>
        /// Determines if the triangle collides with a specified point
        /// </summary>
        /// <param name="v">The vector to be checked</param>
        /// <returns>true, if there is a collision</returns>
        public override bool Collision(Vector v)
        {
            Vector Normal = v2.Minus(v1).Cross(v3.Minus(v1));
            Normal = Normal.DivideBy((float)Normal.Length());
            double Dist = Normal.DotProduct(v.Minus(v1));
            if (Math.Abs(Dist) < 0.2)
            {
                //Vector in plane of triangle
                //check barycentric coordinates

                //push vector on plane
                v = v.Minus(Normal.MultiplyWith((float)Dist));

                // calculate offset for a greater tolerance
                Vector v12, v13, v23;
                v12 = v2.Minus(v1); v12.Normalize();
                v13 = v3.Minus(v1); v13.Normalize();
                v23 = v3.Minus(v2); v23.Normalize();
                Vector off1, off2, off3;
                off1 = v12.MultiplyWith(-1).Minus(v13); off1.Normalize();
                off2 = v12.Minus(v23); off2.Normalize();
                off3 = v13.Plus(v23); off3.Normalize();
                Vector _v1 = v1.Plus(off1.MultiplyWith(0.2f));
                Vector _v2 = v2.Plus(off2.MultiplyWith(0.2f));
                Vector _v3 = v3.Plus(off3.MultiplyWith(0.2f));

                float s1, s2, s3;
                if (Math.Abs(Normal.X) != 1 && Math.Abs(Normal.Y) != 1) // if triangle is not parallel to YZ-plane or XZ-plane
                {
                    s1 = -(-v.Y * _v2.X + v.X * _v2.Y + v.Y * _v3.X - _v2.Y * _v3.X - v.X * _v3.Y + _v2.X * _v3.Y) / (
                                _v1.Y * _v2.X - _v1.X * _v2.Y - _v1.Y * _v3.X + _v2.Y * _v3.X + _v1.X * _v3.Y - _v2.X * _v3.Y);

                    s2 = -(v.Y * _v1.X - v.X * _v1.Y - v.Y * _v3.X + _v1.Y * _v3.X + v.X * _v3.Y - _v1.X * _v3.Y) / (
                                _v1.Y * _v2.X - _v1.X * _v2.Y - _v1.Y * _v3.X + _v2.Y * _v3.X + _v1.X * _v3.Y - _v2.X * _v3.Y);

                    s3 = -(-(-v.Y + _v1.Y) * (-_v1.X + _v2.X) + (-v.X + _v1.X) * (-_v1.Y + _v2.Y)) / ((-
                                 _v1.Y + _v2.Y) * (-_v1.X + _v3.X) - (-_v1.X + _v2.X) * (-_v1.Y + _v3.Y));
                }
                else if(Math.Abs(Normal.X) == 1) //parallel to YZ-plane => use Z instead of X coordinates
                {
                    s1 = -(-v.Y * _v2.Z + v.Z * _v2.Y + v.Y * _v3.Z - _v2.Y * _v3.Z - v.Z * _v3.Y + _v2.Z * _v3.Y) / (
                                _v1.Y * _v2.Z - _v1.Z * _v2.Y - _v1.Y * _v3.Z + _v2.Y * _v3.Z + _v1.Z * _v3.Y - _v2.Z * _v3.Y);

                    s2 = -(v.Y * _v1.Z - v.Z * _v1.Y - v.Y * _v3.Z + _v1.Y * _v3.Z + v.Z * _v3.Y - _v1.Z * _v3.Y) / (
                                _v1.Y * _v2.Z - _v1.Z * _v2.Y - _v1.Y * _v3.Z + _v2.Y * _v3.Z + _v1.Z * _v3.Y - _v2.Z * _v3.Y);

                    s3 = -(-(-v.Y + _v1.Y) * (-_v1.Z + _v2.Z) + (-v.Z + _v1.Z) * (-_v1.Y + _v2.Y)) / ((-
                                 _v1.Y + _v2.Y) * (-_v1.Z + _v3.Z) - (-_v1.Z + _v2.Z) * (-_v1.Y + _v3.Y));
                }
                else //parallel to XZ-plane => use Z instead of Y coordinates
                {
                    s1 = -(-v.Z * _v2.X + v.X * _v2.Z + v.Z * _v3.X - _v2.Z * _v3.X - v.X * _v3.Z + _v2.X * _v3.Z) / (
                                _v1.Z * _v2.X - _v1.X * _v2.Z - _v1.Z * _v3.X + _v2.Z * _v3.X + _v1.X * _v3.Z - _v2.X * _v3.Z);

                    s2 = -(v.Z * _v1.X - v.X * _v1.Z - v.Z * _v3.X + _v1.Z * _v3.X + v.X * _v3.Z - _v1.X * _v3.Z) / (
                                _v1.Z * _v2.X - _v1.X * _v2.Z - _v1.Z * _v3.X + _v2.Z * _v3.X + _v1.X * _v3.Z - _v2.X * _v3.Z);

                    s3 = -(-(-v.Z + _v1.Z) * (-_v1.X + _v2.X) + (-v.X + _v1.X) * (-_v1.Z + _v2.Z)) / ((-
                                 _v1.Z + _v2.Z) * (-_v1.X + _v3.X) - (-_v1.X + _v2.X) * (-_v1.Z + _v3.Z));
                }

                if (s1 >= 0 && s1 <= 1 && s2 >= 0 && s2 <= 1 && s3 >= 0 && s3 <= 1)
                    return true;
            }
            return false;
        }
    }
}
