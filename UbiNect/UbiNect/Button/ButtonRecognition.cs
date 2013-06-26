using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows;
using Vector = Microsoft.Kinect.SkeletonPoint;
using System.Collections.ObjectModel;


namespace UbiNect.Button
{
    public delegate void CollisionDelegate(AbstractElement Button, int PlayerIndex);

    /// <summary>
    /// A recognition component that is able to recognize if buttons in 3D space are touched.
    /// </summary>
    public class ButtonRecognition : RecognitionComponent
    {
        public event CollisionDelegate Collision;
        /// <summary>
        /// Is fired when the component wants to log a message to the GuiNect
        /// </summary>
        public event LogDelegate Log;

        /// <summary>
        /// Set of currently registered Buttons
        /// </summary>
        private ObservableCollection<AbstractElement> Buttons = new ObservableCollection<AbstractElement>();

        /// <summary>
        /// returns the name of the recognition component
        /// </summary>
        /// <returns>string with name of the type</returns>
        public String GetRecognitionType()
        {
            return "Button Recognition Component";
        }
        
        /// <summary>
        /// A set of all <see cref="AbstractElement"/>s that are registered. Changes to this set do not affect the actual button data.
        /// </summary>
        public Dictionary<string, HashSet<AbstractElement>> RegisteredButtons
        {
            get
            {
                Dictionary<string, HashSet<AbstractElement>> temp = new Dictionary<string, HashSet<AbstractElement>>();
                foreach (var item in Buttons)
                {
                    if(! (item is AbstractElement))
                        continue;
                    AbstractElement bt = item as AbstractElement;
                    if(!temp.ContainsKey(bt.ID))
                        temp.Add(bt.ID,new HashSet<AbstractElement>());
                    temp[bt.ID].Add(bt);
                }
                return temp;
            }
        }

        /// <summary>
        /// A List of all <see cref="AbstractElement"/>s that are registered. Changes to this set do not affect the actual button data.
        /// </summary>
        public ObservableCollection<AbstractElement> RegisteredButtonsList
        {
            get {
                return Buttons;
            }
        }

        /// <summary>
        /// VGA frame is not used by this component
        /// </summary>
        public void VGAFrame(object sender, ColorImageFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// Depth frame is not used by this component
        /// </summary>
        public void DepthFrame(object sender, DepthImageFrameReadyEventArgs e)
        {
        }

        /// <summary>
        /// Checks whether a player touches one of the registered buttons (with his hands) and fires the appropriate events.
        /// </summary>
        public void SkeletonFrame(SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<JointType, Joint>> Skeletons)
        {
            foreach (var Button in Buttons)
            {
                foreach (var Skeleton in Skeletons)
                {
                    Dictionary<JointType, Joint> Joints = Skeleton.Value;
                    if (Button.Collision(Joints[JointType.HandLeft].Position) ||
                        Button.Collision(Joints[JointType.HandRight].Position) ||
                        Button.Collision(Joints[JointType.WristLeft].Position) ||
                        Button.Collision(Joints[JointType.WristRight].Position))
                    {
                        if (Collision != null)
                            Collision(Button, Skeleton.Key);
                        if(Button is AbstractElement)
                            Log("Collision with Button " + (Button as AbstractElement).ID + " detected");
                        else
                            Log("Collision with Button detected");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a button to the set of registered buttons
        /// </summary>
        /// <param name="b">The button to add</param>
        public void AddButton(AbstractElement b)
        {
            Buttons.Add(b);

        }

        /// <summary>
        /// Adds a button to the set of registered buttons
        /// </summary>
        /// <param name="b">The button to add</param>
        public void RemoveButton(AbstractElement b)
        {
            if (Buttons.Contains(b))
                Buttons.Remove(b);

        }

        /// <summary>
        /// Draws all registered buttons on the specified canvas in VGA space
        /// </summary>
        /// <param name="c">The canvas which to paint on</param>
        public void DrawButtons(Canvas c)
        {
            foreach (var item in Buttons)
            {
                item.Draw(c);
            }
        }

        /// <summary>
        /// Adds buttons that are saved in a binary file (default .but)
        /// </summary>
        /// <param name="Path">Path to the binary file which to load the data from</param>
        public void AddSavedButtons(String Path)
        {
            if (!File.Exists(Path))
            {
                MessageBox.Show("File path does not exist");
                return;
            }
            FileStream fs = File.OpenRead(Path);
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector sursel = new SurrogateSelector();
            //Vector and SolidColorBrush cannot be serialized directly; use a surrogate instead
            sursel.AddSurrogate(typeof(Vector), new StreamingContext(StreamingContextStates.All), new VectorSurrogate());
            sursel.AddSurrogate(typeof(SolidColorBrush), new StreamingContext(StreamingContextStates.All), new SolidColorBrushSurrogate());
            bf.SurrogateSelector = sursel;
            ObservableCollection<UbiNect.Button.AbstractElement> Buttons = bf.Deserialize(fs) as ObservableCollection<UbiNect.Button.AbstractElement>;
            if (Buttons == null) return;
            foreach (var item in Buttons)
            {
                this.AddButton(item);
            }
        }
    }
}
