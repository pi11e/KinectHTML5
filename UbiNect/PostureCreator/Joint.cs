using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace PostureCreator
{

    public delegate void OnJointIsMovingHandler();
    public delegate void OnJointSelected();

    /// <summary>
    /// A kind of wrapper-class for the ellipses. Also contains state-flags for the states isSelected, 
    /// isBaseJoint, isLegJoint and if the user is in edit- oder adding-mode.
    /// </summary>
    public class Joint
    {
        private String name;
        private Ellipse currentEllipse;
        private Canvas canvas;
        private Point pointFromTopLeft;
        private SolidColorBrush basicBrush, onSelectLegBrush, onSelectBaseBrush;
        private double currentX, currentY;
        public OnJointIsMovingHandler Event_onJointIsMoving;
        public OnJointSelected Event_onJointSelected;
        private MainWindow win;
        private bool isSelected = false;
        private bool moveMode = true;
        private bool isBaseJoint = false;
        private bool isLegJoint = false;

        /// <summary>
        /// Constructor for a joint.
        /// </summary>
        /// <param name="name">The name of the joint. Identical to the JointIDs of the kinect-sdk.</param>
        /// <param name="c">A reference to the canvas in which the ellipse is located.</param>
        /// <param name="x">An initial x-coordinate to draw the initial skeleton.</param>
        /// <param name="y">An initial y-coordinate to draw the initial skeleton.</param>
        /// <param name="win">A reference to the main-window to allow changes in text-fields and interaction 
        /// with further display-elements.</param>
        public Joint(String name, Canvas c, double x, double y, MainWindow win)
        {
            currentX = x;
            currentY = y;
            this.name = name;
            canvas = c;
            this.win = win;
            InitData();
        }

        /// <summary>
        /// Draws the ellipse, sets the colors for the different states in edit-mode, inits the mouse-event-handler on 
        /// the ellipse.
        /// </summary>
        private void InitData()
        {
            // the basic color of a joint
            basicBrush = new SolidColorBrush();
            basicBrush.Color = Color.FromArgb(255, 100, 100, 100);

            // the color it an ellipse is selected with the right-mouse-button in edit-mode
            onSelectBaseBrush = new SolidColorBrush();
            onSelectBaseBrush.Color = Color.FromArgb(255, 255, 0, 0);

            // the color it an ellipse is selected with the left-mouse-button in edit-mode
            onSelectLegBrush = new SolidColorBrush();
            onSelectLegBrush.Color = Color.FromArgb(255, 0, 255, 0);

            currentEllipse = new Ellipse();
            currentEllipse.Fill = basicBrush;
            currentEllipse.Stroke = Brushes.Black;
            Canvas.SetLeft(currentEllipse, currentX);
            Canvas.SetTop(currentEllipse, currentY);

            currentEllipse.Height = 20;
            currentEllipse.Width = 20;

            // Create all MouseEventHandler
            currentEllipse.MouseLeftButtonDown += new MouseButtonEventHandler(currentEllipse_MouseLeftButtonDown);
            currentEllipse.MouseRightButtonDown += new MouseButtonEventHandler(currentEllipse_MouseRightButtonDown);
            currentEllipse.MouseMove += new MouseEventHandler(currentEllipse_MouseMove);
            currentEllipse.MouseUp += new MouseButtonEventHandler(currentEllipse_MouseUp);
        }

        /// <summary>
        /// The handler, that defines, what happens, if the left mousebutton is pressed on an ellipse. 
        /// If we are in dragging-mode, pressing an holding on an ellipse will drag it. 
        /// IF we are in edit-mode, pressing an ellispe will set or unset the state of the joint to be a base-joint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Handles the two modes drag the skeleton and set the constraints.
            if (moveMode)
            {
                pointFromTopLeft = e.MouseDevice.GetPosition(currentEllipse);
                e.MouseDevice.Capture(currentEllipse);
            }
            else if (!isSelected)
            {
                currentEllipse.Fill = onSelectLegBrush;
                isSelected = true;
                isBaseJoint = false;
                isLegJoint = true;
            }
            else
            {
                currentEllipse.Fill = basicBrush;
                isSelected = false;
                isLegJoint = false;
                isBaseJoint = false;
            }
        }

        /// <summary>
        /// Handler, that defines, what happens, if the right mousebutton is pressed on an ellipse. 
        /// Works only in editing-mode.
        /// IF we are in edit-mode, pressing an ellispe will set or unset the state of the joint to be a leg-joint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentEllipse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!moveMode)
            {
                if (!isSelected)
                {
                    currentEllipse.Fill = onSelectBaseBrush;
                    isSelected = true;
                    isLegJoint = false;
                    isBaseJoint = true;
                }
                else
                {
                    currentEllipse.Fill = basicBrush;
                    isSelected = false;
                    isBaseJoint = false;
                    isLegJoint = false;
                }
            }
        }

        /// <summary>
        /// Handler, that defines, what happens, if the left mousebutton is pressed on a joint while the mouse 
        /// is moving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.Captured == currentEllipse)
            {
                MoveElement(e.MouseDevice.GetPosition(canvas));
            }
        }

        /// <summary>
        /// Handler, that defines, what happens, if the left mouse-button is released.
        /// Only neccessary to handle the dropping of an ellipse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void currentEllipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.MouseDevice.Capture(null);
        }

        /// <summary>
        /// These method handles the movement of an ellipse according to the upper left corner of the canvas 
        /// in which the ellipses are lying.
        /// </summary>
        /// <param name="devicePosition"></param>
        private void MoveElement(Point devicePosition)
        {
            currentX = (devicePosition.X - pointFromTopLeft.X);
            currentY = (devicePosition.Y - pointFromTopLeft.Y);

            Canvas.SetLeft(currentEllipse, currentX);
            Canvas.SetTop(currentEllipse, currentY);
            
            this.Event_onJointIsMoving.Invoke();
        }

        /// <summary>
        /// Getter for the ellipse in the joint-class.
        /// </summary>
        /// <returns>The ellipse, the is hold in the wrapper-joint-class.</returns>
        public Ellipse getCurrentEllipse()
        {
            return currentEllipse;
        }

        /// <summary>
        /// Getter for the current x-coordinate of the ellipse.
        /// </summary>
        /// <returns>The current x-coordinate.</returns>
        public double getX()
        {
            return currentX;
        }

        /// <summary>
        /// Getter for the current y-coordinate of the ellipse.
        /// </summary>
        /// <returns>The current y-coordinate.</returns>
        public double getY()
        {
            return currentY;
        }

        /// <summary>
        /// Helper-method to reset the color and the states of the wrapper-class.
        /// </summary>
        public void resetJoint()
        {
            currentEllipse.Fill = basicBrush;
            isSelected = false;
            isBaseJoint = false;
            isLegJoint = false;
        }

        /// <summary>
        /// Setter for the operation-mode.
        /// </summary>
        /// <param name="mode">true - change to dragging-mode. False - change to edit-mode.</param>
        public void setEditMode(bool mode)
        {
            moveMode = mode;
        }

        /// <summary>
        /// Getter for the state isBaseJoint. 
        /// Hint: isBaseJoint and isLegJoint shouldn't be true at the same time.
        /// </summary>
        /// <returns>true - the joint is a base-joint</returns>
        public bool getIsBaseJoint()
        {
            return isBaseJoint;
        }

        /// <summary>
        /// Getter for the state isLegJoint. 
        /// Hint: isBaseJoint and isLegJoint shouldn't be true at the same time.
        /// </summary>
        /// <returns>true - the joint is a leg-joint</returns>
        public bool getIsLegJoint()
        {
            return isLegJoint;
        }

        /// <summary>
        /// Getter for the name. Should be a JointID according to the kinect-sdk.
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return name;
        }


    }

}
