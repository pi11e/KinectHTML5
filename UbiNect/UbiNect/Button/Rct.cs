using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Windows.Shapes;
using System.Windows.Media;

namespace UbiNect.Button
{
    public class Rct : Element
    {
        Vector right_up,left_up,right_down,left_down;

        public Rct(   float ru_x, float ru_y,float ru_z,
                            float lu_x, float lu_y,float lu_z,
                            float rd_x, float rd_y,float rd_z,
                            float ld_x, float ld_y,float ld_z)
        {   
            right_up = new Vector() {X = ru_x, Y = ru_y, Z = ru_z};
            left_up  = new Vector() {X = lu_x, Y = lu_y, Z = lu_z};
            right_down = new Vector() {X = rd_x, Y = rd_y, Z = rd_z};
            left_down  = new Vector() {X = ld_x, Y = ld_y, Z = ld_z};        
        }

        public Rct(Vector ru, Vector lu, Vector rd, Vector ld) 
        {   
            right_up = ru;
            left_up = lu;
            right_down = rd;
            left_down = ld;
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
            rec.Fill = new SolidColorBrush(Color.FromArgb(100,100,100,100));
            c.Children.Add(rec);
        }

        public bool Collision(Microsoft.Research.Kinect.Nui.Vector v)
        {
            return false;
        }
    }
}
