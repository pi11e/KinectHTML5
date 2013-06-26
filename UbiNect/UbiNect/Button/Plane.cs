using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace UbiNect.Button
{
    public class Plane : Element
    {
        SkeletonPoint right_up,left_up,right_down,left_down;
        private bool coll;
        
        /// <summary>
        /// creates a plan in front of the kinect with exact values 
        /// </summary>
        /// <param name="ru_x">X - coordinate of the right upper egde </param>
        /// <param name="ru_y">Y - coordinate of the right upper egde</param>
        /// <param name="ru_z">Z - coordinate of the right upper egde</param>
        /// <param name="lu_x">X - coordinate of the left upper egde</param>
        /// <param name="lu_y">Y - coordinate of the left upper egde</param>
        /// <param name="lu_z">Z - coordinate of the left upper egde</param>
        /// <param name="rd_x">X - coordinate of the right lower egde</param>
        /// <param name="rd_y">Y - coordinate of the right lower egde</param>
        /// <param name="rd_z">Z - coordinate of the right lower egde</param>
        /// <param name="ld_x">X - coordinate of the left lower egde</param>
        /// <param name="ld_y">Y - coordinate of the left lower egde</param>
        /// <param name="ld_z">Z - coordinate of the left lower egde</param>
        public Plane(   float ru_x, float ru_y,float ru_z,
                      float lu_x, float lu_y,float lu_z,
                      float rd_x, float rd_y,float rd_z,
                      float ld_x, float ld_y,float ld_z)
        {
            right_up = new SkeletonPoint() { X = ru_x, Y = ru_y, Z = ru_z };
            left_up = new SkeletonPoint() { X = lu_x, Y = lu_y, Z = lu_z };
            right_down = new SkeletonPoint() { X = rd_x, Y = rd_y, Z = rd_z };
            left_down = new SkeletonPoint() { X = ld_x, Y = ld_y, Z = ld_z };
            coll = false;
        }
        
        /// <summary>
        /// creates an plane in front of the Kinect sensor with vector data
        /// </summary>
        /// <param name="ru">vector of the right upper corner</param>
        /// <param name="lu">vector of the left upper corner</param>
        /// <param name="rd">vector of the right lower corner</param>
        /// <param name="ld">vector of the left lower corner</param>
        public Plane(SkeletonPoint ru, SkeletonPoint lu, SkeletonPoint rd, SkeletonPoint ld) 
        {   
            right_up = ru;
            left_up = lu;
            right_down = rd;
            left_down = ld;
            coll = false;
        }
        
        /// <summary>
        /// creates an plan in front of the Kinect with center data
        /// </summary>
        /// <param name="center_x"> center coordiante in x - direction</param>
        /// <param name="center_y">center coordinate in y - direction</param>
        /// <param name="with"> with of the plan</param>
        /// <param name="height">height of the plan</param>
        /// <param name="dist">distance in z - direction</param>        
        public Plane(float center_x, float center_y, float with, float height, float dist)
        {
            with *= .5f;
            height *= .5f;
            right_up = new SkeletonPoint() { X = center_x + with, Y = center_y + height, Z = dist };
            left_up = new SkeletonPoint() { X = center_x - with, Y = center_y + height, Z = dist };
            right_down = new SkeletonPoint() { X = center_x + with, Y = center_y - height, Z = dist };
            left_down = new SkeletonPoint() { X = center_x - with, Y = center_y - height, Z = dist };
            coll = false;        
        }
                            

        public void Draw(System.Windows.Controls.Canvas c, bool ConvertFromDepthToVGA = true)
        {
            Polygon rec = new Polygon();
            if (ConvertFromDepthToVGA)
            {
                rec.Points.Add(left_up.GetDisplayPositionVGA(Prototype.nui, c));
                rec.Points.Add(right_up.GetDisplayPositionVGA(Prototype.nui, c));
                rec.Points.Add(right_down.GetDisplayPositionVGA(Prototype.nui, c));
                rec.Points.Add(left_down.GetDisplayPositionVGA(Prototype.nui, c));
            }
            else
            {
                rec.Points.Add(left_up.GetDisplayPosition(Prototype.nui, c));
                rec.Points.Add(right_up.GetDisplayPosition(Prototype.nui, c));
                rec.Points.Add(right_down.GetDisplayPosition(Prototype.nui, c));
                rec.Points.Add(left_down.GetDisplayPosition(Prototype.nui, c));
            }
            if (coll == true)
                rec.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            else rec.Fill = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
            c.Children.Add(rec);
        }

        public void DrawFront(Canvas c)
        {
            Polygon rec = new Polygon();
            rec.Points.Add(left_up.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(right_up.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(right_down.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(left_down.GetDisplayPositionVGA(Prototype.nui, c));
        }
        
        public void DrawTop(Canvas c)
        {
           Polygon rec = new Polygon();
           SkeletonPoint left_front = new SkeletonPoint(), right_front = new SkeletonPoint(), left_back = new SkeletonPoint(), right_back = new SkeletonPoint();
            right_front.X = right_up.X; right_front.Y = right_up.Z - 0.1f; right_front.Z = right_up.Y;
            right_back.X = right_up.X; right_back.Y = right_up.Z + 0.1f; right_back.Z = right_up.Y;
            left_front.X = left_up.X; left_front.Y = left_up.Z - 0.1f; left_front.Z = left_up.Y;
            left_back.X = left_up.X; left_back.Y = left_up.Z + 0.1f; left_back.Z = left_up.Y;

            rec.Points.Add(left_front.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(right_front.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(right_back.GetDisplayPositionVGA(Prototype.nui, c));
            rec.Points.Add(left_back.GetDisplayPositionVGA(Prototype.nui, c));
        }

        public bool Collision(Microsoft.Kinect.SkeletonPoint v)
        {
            if ((v.X <= right_up.X) && (v.X >= left_up.X) && (v.Y <= right_up.Y) && (v.Y >= right_down.Y) && (v.Z <= (right_up.Z + 0.1)) && (v.Z >= (right_down.Z - 0.1)))
                coll = true;
                else coll =  false;
            return coll;
        }
    }
}
