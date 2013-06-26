using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using System.Xml;

namespace PostureCreator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Joint Head, ShoulderCenter, ElbowLeft, ElbowRight, HandLeft, HandRight,
            HipCenter, KneeLeft, KneeRight, FootLeft, FootRight;

        private Bone left_upper_arm,right_upper_arm,neck,left_lower_arm,right_lower_arm,body,
            right_upper_leg,rigth_lower_leg,left_upper_leg,left_lower_leg;
           
        // set for all joints of the skeleton
        private HashSet<Joint> joints;

        // contextdata for the list in the tab "2. Select Constraints"
        private List<string> list_selected_joints;
        // list of all constraints, the positions in list_selected_joints and list_of_constraints are equal
        private List<Constraint> list_of_constraints;

        // helper counters
        private int legJointCounter, baseJointCounter;

        private string hint_text =
            "Use the left mouse-button to select two leg-joints (green) and the right mouse-button for the base-joint (red). "+
            "The base-joint should lie between two leg-joints. "+
            "You can deselect a joint by reclick it. "+
            "We recommend to set a sufficient tolerance for a better recognition. "+
            "The tolerance should not been changed during the adding process.";

        private string hint_text_in_edit =
            "Drag the joints in an approximate position. "+
            "When specifying the constraints, the position can not be changed. "+
            "If the posture has to be changed again, the set constraints are lost. "+
            "To reset the posture, use the appropriate button below.";
       
        public MainWindow()
        {
            InitializeComponent();
            InitCustomComponents();
            InitSkeleton();
        }

        /// <summary>
        /// Resets the counters, creates new lists, sets the hint-texts.
        /// </summary>
        private void InitCustomComponents()
        {
            resetCounters();
            list_selected_joints = new List<string>();
            listbox_constraints.DataContext = list_selected_joints;
            list_of_constraints = new List<Constraint>();
            hint_textBlock.Text = hint_text;
            hint_text_edti.Text = hint_text_in_edit;
        }

        /// <summary>
        /// Creates the joints and bones and draws the initial skeleton.
        /// </summary>
        private void InitSkeleton()
        {
            joints = new HashSet<Joint>();

            Head = new Joint("Head", skeleton_canvas, 130, 75, this);
            joints.Add(Head);
            ShoulderCenter = new Joint("ShoulderCenter", skeleton_canvas, 130, 110, this);
            joints.Add(ShoulderCenter);
            ElbowLeft = new Joint("ElbowLeft", skeleton_canvas, 80, 140, this);
            joints.Add(ElbowLeft);
            ElbowRight = new Joint("ElbowRight", skeleton_canvas, 180, 140, this);
            joints.Add(ElbowRight);
            HandLeft = new Joint("HandLeft", skeleton_canvas, 30, 160, this);
            joints.Add(HandLeft);
            HandRight = new Joint("HandRight", skeleton_canvas, 230, 160, this);
            joints.Add(HandRight);
            HipCenter = new Joint("HipCenter", skeleton_canvas, 130, 180, this);
            joints.Add(HipCenter);
            KneeLeft = new Joint("KneeLeft", skeleton_canvas, 100, 220, this);
            joints.Add(KneeLeft);
            KneeRight = new Joint("KneeRight", skeleton_canvas, 160, 220, this);
            joints.Add(KneeRight);
            FootLeft = new Joint("FootLeft", skeleton_canvas, 80, 270, this);
            joints.Add(FootLeft);
            FootRight = new Joint("FootRight", skeleton_canvas, 180, 270, this);
            joints.Add(FootRight);

            left_upper_arm = new Bone(ShoulderCenter, ElbowLeft);
            right_upper_arm = new Bone(ShoulderCenter, ElbowRight);
            neck = new Bone(Head, ShoulderCenter);
            left_lower_arm = new Bone(ElbowLeft, HandLeft);
            right_lower_arm = new Bone(ElbowRight, HandRight);
            body = new Bone(ShoulderCenter, HipCenter);
            right_upper_leg = new Bone(HipCenter, KneeRight);
            rigth_lower_leg = new Bone(KneeRight, FootRight);
            left_upper_leg = new Bone(HipCenter, KneeLeft);
            left_lower_leg = new Bone(KneeLeft, FootLeft);

            skeleton_canvas.Children.Add(left_upper_arm.getLine());
            skeleton_canvas.Children.Add(right_upper_arm.getLine());
            skeleton_canvas.Children.Add(neck.getLine());
            skeleton_canvas.Children.Add(left_lower_arm.getLine());
            skeleton_canvas.Children.Add(right_lower_arm.getLine());
            skeleton_canvas.Children.Add(body.getLine());
            skeleton_canvas.Children.Add(right_upper_leg.getLine());
            skeleton_canvas.Children.Add(rigth_lower_leg.getLine());
            skeleton_canvas.Children.Add(left_upper_leg.getLine());
            skeleton_canvas.Children.Add(left_lower_leg.getLine());
            skeleton_canvas.Children.Add(Head.getCurrentEllipse());
            skeleton_canvas.Children.Add(ShoulderCenter.getCurrentEllipse());
            skeleton_canvas.Children.Add(ElbowLeft.getCurrentEllipse());
            skeleton_canvas.Children.Add(ElbowRight.getCurrentEllipse());
            skeleton_canvas.Children.Add(HandLeft.getCurrentEllipse());
            skeleton_canvas.Children.Add(HandRight.getCurrentEllipse());
            skeleton_canvas.Children.Add(HipCenter.getCurrentEllipse());
            skeleton_canvas.Children.Add(KneeLeft.getCurrentEllipse());
            skeleton_canvas.Children.Add(KneeRight.getCurrentEllipse());
            skeleton_canvas.Children.Add(FootLeft.getCurrentEllipse());
            skeleton_canvas.Children.Add(FootRight.getCurrentEllipse());
        }

        /// <summary>
        /// Handler to react on a click of the reset-button in dragging-mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            skeleton_canvas.Children.Clear();
            InitSkeleton();
        }

        /// <summary>
        /// Helper-method to add a new list-entry.
        /// </summary>
        /// <param name="s"></param>
        public void addToSelectedJointsList(string s)
        {
            list_selected_joints.Add(s);
            listbox_constraints.DataContext = null;
            listbox_constraints.DataContext = list_selected_joints;
        }

        /// <summary>
        /// Helper-Method to remove a constraint from the list.
        /// </summary>
        /// <param name="s"></param>
        public void removeFromSelectedJointsList(string s)
        {
            if (list_selected_joints.Contains(s))
            {
                list_selected_joints.Remove(s);
            }
            listbox_constraints.DataContext = null;
            listbox_constraints.DataContext = list_selected_joints;
        }

        /// <summary>
        /// Handler to react on 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bn_add_constraint_Click(object sender, RoutedEventArgs e)
        {
            hint_textBlock.Text = "";

            resetCounters();
            // search the list, if there is at least one joint marked as base-joint
            foreach (Joint joint in joints)
            {
                if (joint.getIsBaseJoint())
                {
                    baseJointCounter++;
                }
            }

            // search the list, if there are at least two joints marked as leg-joints
            foreach (Joint joint in joints)
            {
                if (joint.getIsLegJoint())
                {
                    legJointCounter++;
                }
            }

            // the user have to select one base- and two leg-joints
            if (!(baseJointCounter == 1 && legJointCounter == 2))
            {
                hint_textBlock.Text = "You have to select one base-joint and two leg-joints!";
            }
            else
            {
                List<Joint> tempSet = new List<Joint>();
                foreach (Joint joint in joints)
                {
                    if (joint.getIsBaseJoint())
                    {
                        // So, the base-joint will be the first entry in tempSet-
                        tempSet.Add(joint);
                    }
                }
                foreach (Joint joint in joints)
                {
                    if (joint.getIsLegJoint())
                    {
                        tempSet.Add(joint);
                    }
                }
                if (!isConstraintAlreadyInList(tempSet[0], tempSet[1], tempSet[2]))
                {
                    Constraint constraint = new Constraint(tempSet[0], tempSet[1], tempSet[2], slider_tolerance.Value);
                    list_of_constraints.Add(constraint);
                    list_selected_joints.Add("" + tempSet[0].getName() + " : " + tempSet[1].getName() + " : " + tempSet[2].getName() + " : " + constraint.getMinAngle() + " : " + constraint.getMaxAngle());
                }
                else hint_textBlock.Text = "Constraint already exists.";
            }

            listbox_constraints.DataContext = null;
            listbox_constraints.DataContext = list_selected_joints;
            resetCounters();
            resetConstraintsOfJoints();
        }


        /// <summary>
        /// Handler to react on a click a the "Add Constraint"-Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bn_remove_constraint_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = listbox_constraints.SelectedIndex;

            try
            {
                list_of_constraints.RemoveAt(selectedIndex);
                list_selected_joints.RemoveAt(selectedIndex);
            }
            catch
            {
            }

            listbox_constraints.DataContext = null;
            listbox_constraints.DataContext = list_selected_joints;
        }

        /// <summary>
        /// Handler to react on a selection-change in the constraint-list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selected_joints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Joint joint in joints)
            {
                joint.resetJoint();
            }
        }

        /// <summary>
        /// Handler to react on a tab-change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tab_mode_control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab_mode_control.SelectedIndex == 0)
            {
                foreach (Joint joint in joints)
                {
                    joint.setEditMode(true);
                    joint.resetJoint();
                }
            }
            else if (tab_mode_control.SelectedIndex == 1)
            {
                foreach (Joint joint in joints)
                {
                    joint.setEditMode(false);
                    joint.resetJoint();
                }
            }
        }

        /// <summary>
        /// Helper-method to reset all joints. See also the reset-method in <see cref="Joint"/>
        /// </summary>
        private void resetConstraintsOfJoints()
        {
            foreach (Joint joint in joints)
            {
                joint.resetJoint();
            }
        }

        /// <summary>
        /// Helper-method to check, if a constraints already exists.
        /// </summary>
        /// <param name="baseJoint"></param>
        /// <param name="jOne"></param>
        /// <param name="jTwo"></param>
        /// <returns>true - if a constraint is found.</returns>
        private bool isConstraintAlreadyInList(Joint baseJoint, Joint jOne, Joint jTwo)
        {
            foreach (Constraint constraint in list_of_constraints)
            {
                if (constraint.getBaseJoint().getName().Equals(baseJoint.getName()))
                {
                    if ((constraint.getLegJointOne().getName().Equals(jOne.getName()) && constraint.getLegJointTwo().getName().Equals(jTwo.getName()))
                        || (constraint.getLegJointOne().getName().Equals(jTwo.getName()) && constraint.getLegJointTwo().getName().Equals(jOne.getName())))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handler to react on a click of the "Info"-Button. Only shows the initial-hint-text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bn_info_Click(object sender, RoutedEventArgs e)
        {
            hint_textBlock.Text = hint_text;
        }

        /// <summary>
        /// Helper-method to reset the counters "legJointCounter" and "baseJointCounter"
        /// </summary>
        private void resetCounters()
        {
            legJointCounter = 0;
            baseJointCounter = 0;
        }

        /// <summary>
        /// Handler, to react on a click of the "Create XML"-Button. 
        /// A XML-File is only generated, if at least one constraint is available and the name of the xml-file is not empty. 
        /// The file yet safed to the location, where the executable PostureCreator was started.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bn_create_xml_Click(object sender, RoutedEventArgs e)
        {
            String nameOfXML = nameOf.GetLineText(0);
            if (nameOfXML.Trim().Length > 0 && list_of_constraints.Count > 0)
            {
                using (XmlWriter writer = XmlWriter.Create("" + nameOfXML + ".xml"))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("posture");

                    writer.WriteElementString("name", nameOfXML);
                    writer.WriteElementString("type", "posture");

                    writer.WriteStartElement("constraints");

                    foreach (Constraint constraint in list_of_constraints)
                    {
                        writer.WriteStartElement("angle");

                        writer.WriteElementString("jointLegOne", constraint.getLegJointOne().getName());
                        writer.WriteElementString("jointBase", constraint.getBaseJoint().getName());
                        writer.WriteElementString("jointLegTwo", constraint.getLegJointTwo().getName());
                        writer.WriteElementString("minAngle", "" + constraint.getMinAngle());
                        writer.WriteElementString("maxAngle", "" + constraint.getMaxAngle());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                MessageBox.Show("The posture " + nameOfXML + ".xml was successfully created. \n\nIts located in:\n\n "+ AppDomain.CurrentDomain.BaseDirectory+ "" +nameOfXML + ".xml");
                resetConstraintsOfJoints();
                resetCounters();
            }
            else MessageBox.Show("You have to create at least one constraint and/or the posture-name should not be emtpy.");
        }

        /// <summary>
        /// Handler, to react on a value-change of the tolerance-slider. 
        /// Only changes the value of the nearby textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_tolerance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textblock_slider_tolerance.Text = "" + slider_tolerance.Value + "°";
        }
    }
}
