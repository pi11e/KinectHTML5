using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUIVector = Microsoft.Kinect.SkeletonPoint;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace UbiNect
{
    /// <summary>
    /// Implements some basic vector maths
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// Determines if a vector's components are zero
        /// </summary>
        /// <param name="v">current instance</param>
        public static bool IsZero(this NUIVector v)
        {
            return v.X == 0 && v.Y == 0 && v.Z == 0;
        }

        public static bool IsValid(this NUIVector v)
        {
            return !Single.IsInfinity(v.X) && !Single.IsInfinity(v.Y) && !Single.IsInfinity(v.Z) &&
                !Single.IsNaN(v.X) && !Single.IsNaN(v.Y) && !Single.IsNaN(v.Z);
        }

        /// <summary>
        /// Calculates the distance of a vector to another one in 3D space
        /// </summary>
        /// <param name="v1">current instance</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Distance</returns>
        public static double DistanceTo(this NUIVector v1, NUIVector v2)
        {
            return v2.Minus(v1).Length();
        }

        /// <summary>
        /// Calculates the Length of a vector in 3D space
        /// </summary>
        /// <param name="v">current instance</param>
        /// <returns>Length</returns>
        public static double Length(this NUIVector v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Subtracts a vector from the current one
        /// </summary>
        /// <param name="v1">current instance</param>
        /// <param name="v2">vector to subtract</param>
        /// <returns>v1 - v2</returns>
        public static NUIVector Minus(this NUIVector v1, NUIVector v2)
        {
            return new NUIVector() { X = v1.X - v2.X, Y = v1.Y - v2.Y, Z = v1.Z - v2.Z};
        }

        /// <summary>
        /// Adds a vector to the current one
        /// </summary>
        /// <param name="v1">current instance</param>
        /// <param name="v2">vector to add</param>
        /// <returns>v1 + v2</returns>
        public static NUIVector Plus(this NUIVector v1, NUIVector v2)
        {
            return new NUIVector() { X = v1.X + v2.X, Y = v1.Y + v2.Y, Z = v1.Z + v2.Z};
        }

        /// <summary>
        /// Multiplies a vector with a scalar value
        /// </summary>
        /// <param name="v">current instance</param>
        /// <param name="factor">factor to multiply the vector with</param>
        /// <returns>factor * v</returns>
        public static NUIVector MultiplyWith(this NUIVector v, float factor)
        {
            return new NUIVector() { X = factor * v.X, Y = factor * v.Y, Z = factor * v.Z};
        }

        /// <summary>
        /// Multiplies a vector with the inverse of a scalar value
        /// </summary>
        /// <param name="v">current instance</param>
        /// <param name="factor">divisor to divide the vector by</param>
        /// <returns>(1 / factor) * v</returns>
        public static NUIVector DivideBy(this NUIVector v, float divisor)
        {
            return new NUIVector() { X = (1 / divisor) * v.X, Y = (1 / divisor) * v.Y, Z = (1 / divisor) * v.Z};
        }

        /// <summary>
        /// Calculates the dot product of the current vector with another one in 3D space
        /// </summary>
        /// <param name="v1">current instance</param>
        /// <param name="v2">vector to be multiplied</param>
        /// <returns>v1 * v2</returns>
        public static double DotProduct(this NUIVector v1, NUIVector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Calculates the cross product of the current vector with another on in 3D space
        /// </summary>
        /// <param name="v1">current instance</param>
        /// <param name="v2">right hand argument</param>
        /// <returns>Cross product</returns>
        public static NUIVector Cross(this NUIVector v1, NUIVector v2)
        {
            NUIVector res = new NUIVector();
            res.X = v1.Y * v2.Z - v1.Z * v2.Y;
            res.Y = v1.Z * v2.X - v1.X * v2.Z;
            res.Z = v1.X * v2.Y - v1.Y * v2.X;
            
            return res;
        }

        /// <summary>
        /// Makes the vector a unit vector
        /// </summary>
        public static void Normalize(this NUIVector v)
        {
            v = v.DivideBy((float)v.Length());
        }

        /// <summary>
        /// Calculates the angle of two rays that do not necessarily share a point
        /// </summary>
        /// <param name="ray1_1">first vector on first ray</param>
        /// <param name="ray1_2">second vector on first ray</param>
        /// <param name="ray2_1">first vector on second ray</param>
        /// <param name="ray2_2">second vector on second ray</param>
        /// <returns>angle in radians</returns>
        public static double AngleTo(this NUIVector ray1_1, NUIVector ray1_2, NUIVector ray2_1, NUIVector ray2_2)
        {
            NUIVector diff1 = ray1_2.Minus(ray1_1);
            NUIVector diff2 = ray2_2.Minus(ray2_1);

            return Math.Acos(diff1.DotProduct(diff2) / diff1.Length() / diff2.Length());
        }

        /// <summary>
        /// Calculates the angle of two rays that do not necessarily share a point
        /// </summary>
        /// <param name="ray1_1">first vector on first ray</param>
        /// <param name="ray1_2">second vector on first ray</param>
        /// <param name="ray2_1">first vector on second ray</param>
        /// <param name="ray2_2">second vector on second ray</param>
        /// <returns>angle in degrees</returns>
        public static double AngleToDeg(this NUIVector ray1_1, NUIVector ray1_2, NUIVector ray2_1, NUIVector ray2_2)
        {
            return 180 * AngleTo(ray1_1, ray1_2, ray2_1, ray2_2) / Math.PI;
        }

        /// <summary>
        /// Calculates the angle between two rays sharing the current vector
        /// </summary>
        /// <param name="v">current instance</param>
        /// <param name="v1">vector on first ray</param>
        /// <param name="v2">vector on second ray</param>
        /// <returns>angle in radians</returns>
        public static double AngleTo(this NUIVector v, NUIVector v1, NUIVector v2)
        {
            NUIVector diff1 = v1.Minus(v);
            NUIVector diff2 = v2.Minus(v);

            return Math.Acos(diff1.DotProduct(diff2) / diff1.Length() / diff2.Length());
        }

        /// <summary>
        /// Calculates the angle between two rays sharing the current vector
        /// </summary>
        /// <param name="v">current instance</param>
        /// <param name="v1">vector on first ray</param>
        /// <param name="v2">vector on second ray</param>
        /// <returns>angle in degrees</returns>
        public static double AngleToDeg(this NUIVector v, NUIVector v1, NUIVector v2)
        {
            return 180 * v.AngleTo(v1, v2) / Math.PI;
        }

        /// <summary>
        /// Returns the position of a 3D vector on a 2D surface in depth space
        /// </summary>
        /// <param name="j">current instance</param>
        /// <param name="nui">NUI Runtime</param>
        /// <param name="panel">The canvas which to project the vector on</param>
        /// <returns>2D position</returns>
        public static Point GetDisplayPosition(this NUIVector j, KinectSensor nui, Canvas panel)
        {
            float depthX, depthY;
            
            //nui.SkeletonStream.SkeletonToDepthImage(j, out depthX, out depthY);

            CoordinateMapper cm = new CoordinateMapper(nui);

            DepthImagePoint dip = cm.MapSkeletonPointToDepthPoint(j, nui.DepthStream.Format);
            depthX = dip.X;
            depthY = dip.Y;


            // crop to panel? - yields 320x240 all the time o.O
            //int X = (int)Math.Max(0, Math.Min(depthX * panel.ActualWidth, panel.ActualWidth));
            //int Y = (int)Math.Max(0, Math.Min(depthY * panel.ActualHeight, panel.ActualHeight));

            return new Point(depthX, depthY);
        }


        /// <summary>
        /// Returns the position of a 3D vector on a 2D surface in VGA space
        /// </summary>
        /// <param name="j">current instance</param>
        /// <param name="nui">NUI Runtime</param>
        /// <param name="panel">The image which to project the vector on</param>
        /// <returns>2D position</returns>
        public static Point GetDisplayPositionVGA(this NUIVector j, KinectSensor nui, Canvas panel)
        {
            
            CoordinateMapper cm = new CoordinateMapper(nui);
            ColorImagePoint cip = cm.MapSkeletonPointToColorPoint(j, nui.ColorStream.Format);
            //nui.SkeletonStream.SkeletonToDepthImage(j, out depthX, out depthY, out depthValue);
            int colorX = cip.X;
            int colorY = cip.Y;

            return new Point((int)(colorX * panel.ActualWidth / 640), (int)(colorY * panel.ActualHeight / 480));
        }

        /// <summary>
        /// Calculates the position of a point on screen (depth image) in skeleton space
        /// </summary>
        /// <param name="p">point on screen (depth image)</param>
        /// <param name="DepthValue">Depth value of point</param>
        /// <param name="nui">active nui runtime</param>
        /// <param name="panel">panel on which the screen point lies</param>
        /// <returns></returns>
        public static NUIVector GetSkeletonSpacePosition(this Point p, short DepthValue, KinectSensor nui, Canvas panel)
        {
            DepthImagePoint dip = new DepthImagePoint { Depth = DepthValue, X = (int)p.X, Y = (int)p.Y };

            CoordinateMapper cm = new CoordinateMapper(nui);
            return cm.MapDepthPointToSkeletonPoint(nui.DepthStream.Format, dip);

            //return nui.SkeletonEngine.DepthImageToSkeleton((float)(p.X/panel.ActualWidth), (float)(p.Y/panel.ActualHeight), DepthValue);
        }
    }

    public class VectorSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            NUIVector v = (NUIVector)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
            
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            NUIVector v = (NUIVector)obj;
            v.X = info.GetSingle("X");
            v.Y = info.GetSingle("Y");
            v.Z = info.GetSingle("Z");
            
            return v;
        }
    }

    public class SolidColorBrushSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            SolidColorBrush b = (SolidColorBrush)obj;
            info.AddValue("A", b.Color.A);
            info.AddValue("R", b.Color.R);
            info.AddValue("G", b.Color.G);
            info.AddValue("B", b.Color.B);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Color c = new Color();
            c.A = info.GetByte("A");
            c.R = info.GetByte("R");
            c.G = info.GetByte("G");
            c.B = info.GetByte("B");
            return new SolidColorBrush(c);
        }
    }

    /// <summary>
    /// implements methods for scaling points to kinect space
    /// </summary>
    static class ScaleToKinectSpace
    {
        /// <summary>
        /// scales a point <paramref name="pntToScale"/> to Kinect space in XZ plane
        /// </summary>
        /// <param name="c">canvas where to scale</param>
        /// <param name="pntToScale">point to scale</param>
        /// <returns>scaled point</returns>
        public static System.Windows.Point scaleToKinectSpaceXZ(System.Windows.Controls.Canvas c, System.Windows.Point pntToScale)
        {

            pntToScale.Y = (c.Height * pntToScale.Y / 4) ; // Kinect depth = 4m
            pntToScale.X = (c.Width  * pntToScale.X / 3.5) + c.Width/2; // Kinect width ~ 7m

            return pntToScale;
        }

        /// <summary>
        /// scales a point <paramref name="pntToScale"/> to Kinect space in XY plane
        /// </summary>
        /// <param name="c">canvas where to scale</param>
        /// <param name="pntToScale">point to scale</param>
        /// <returns>scaled point</returns>
        public static System.Windows.Point scaleToKinectSpaceXY(System.Windows.Controls.Canvas c, System.Windows.Point pntToScale)
        {

            pntToScale.Y = (c.Height * pntToScale.Y / 2.625) + c.Height / 2; // Kinect height ~ 5.25m
            pntToScale.X = (c.Width * pntToScale.X / 3.5) + c.Width / 2; // Kinect width ~ 7m

            return pntToScale;
        }

        /// <summary>
        /// scales a distance to Kinect space in z direction
        /// </summary>
        /// <param name="c">canvas where to scale</param>
        /// <param name="lengthToScale">distance to scale</param>
        /// <returns>scaled distance</returns>
        public static double scaleToKinectSpaceZ(System.Windows.Controls.Canvas c, double lengthToScale)
        {
            lengthToScale = (c.Height * lengthToScale / 4); // Kinect depth = 4m
            return lengthToScale;
        }

        /// <summary>
        /// scales a distance to Kinect space in x direction
        /// </summary>
        /// <param name="c">canvas where to scale</param>
        /// <param name="lengthToScale">distance to scale</param>
        /// <returns>scaled distance</returns>
        public static double scaleToKinectSpaceX(System.Windows.Controls.Canvas c, double lengthToScale)
        {
            lengthToScale = (c.Height * lengthToScale / 3.5); // Kinect width ~ 7m
            return lengthToScale;
        }

    }
}
