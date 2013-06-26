using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UbiNect;
using Microsoft.Kinect;
using System.Reflection;

namespace UbiNect.Move
{
    public delegate void MoveRecognizedDelegate(AbstractMove m);

    public class MoveRecognition : RecognitionComponent
    {
        public event MoveRecognizedDelegate MoveRecognized;

        public event LogDelegate Log;

        public int skeletonCount { get; private set; }

        private List<Dictionary<JointType, Joint>> positions = new List<Dictionary<JointType, Joint>>();

        private List<AbstractMove> recognizableMoves = new List<AbstractMove>();

        public String GetRecognitionType()
        {
            return "Movement Recognition Component";
        }

        public List<AbstractMove> getRecognizableMoves()
        {
            return recognizableMoves;
        }

        /// <summary>
        /// VGA frame is not used by this component
        /// </summary>
        public void VGAFrame(object sender, ColorImageFrameReadyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Depth frame is not used by this component
        /// </summary>
        public void DepthFrame(object sender, DepthImageFrameReadyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void setRecognizableMoves(String[] moves)
        {
            // strings in "postures" will be interpreted as class names
            foreach (String moveName in moves)
            {
                try
                {
                    // get type by given name
                    Type moveType = Type.GetType("UbiNect.Move." + moveName);
                    // alternatives:
                    //Type moveType = Type.GetType("UbiNect.Move." + moveName + ", UbiNect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                    //Type moveType = typeof(MoveRecognition).Assembly.GetType("UbiNect.Move." + moveName);
                    // handle errors
                    if (moveType == null)
                    {
                        Console.WriteLine("ERROR in MoveRecognition.setRecognizableGestures() : Move type " + moveName + " not found in current assembly.");
                        return;
                    }

                    // 1. Get constructor w/ 1 String argument (i.e. Gesture(String moveName)
                    // 2. Invoke constructor with given moveName
                    AbstractMove m = (AbstractMove)moveType.GetConstructor(new Type[] { typeof(String), typeof(int) }).Invoke(new String[] { moveName , null});
                    // 3. Add posture instance to recognizable postures
                    this.recognizableMoves.Add(m);

                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Error while trying to create an instance of type " + moveName, "MoveRecognition", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    Console.WriteLine("Error while trying to create an instance of type " + moveName + "; strack trace following.");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void fireEvent(AbstractMove m)
        {
            MoveRecognized(m);
        }

        /// <summary>
        /// Called whenever a new skeleton frame is ready.
        /// </summary>
        /// <param name="e">The skeleton frame event that holds any frame information</param>
        /// <param name="Skeletons">Dictionary w/ player skeletons; each skeleton consists of a map of Joints and actual joint data</param>
        public void SkeletonFrame(SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<JointType, Joint>> Skeletons)
        {
            
            // get player ID for skeleton
            try
            {
                this.skeletonCount = Skeletons.Count;
                // 1. For every skeleton...
                foreach (var s in Skeletons)
                {
                    // (get the skeleton data dictionary)
                    Dictionary<JointType, Joint> dict = s.Value;
                    
                    foreach (AbstractMove move in this.recognizableMoves)
                    {

                        if (move.isMove(dict))
                        {
                            //Console.WriteLine("Skeleton Count = " + Skeletons.Count() + ", Index = " + s.Key + " | MoveRecognition.cs");
                            // tell the move which player performed it
                            move.playerID = s.Key;
                            move.depthIndex = dict[JointType.Head].Position.Z;
                            // line below is the same as "MoveRecognized(move)", only thread safe
                            this.GetType().InvokeMember("fireEvent", BindingFlags.Default | BindingFlags.InvokeMethod, null, this, new Object[]{move});
                            //MoveRecognized(move);
                            
                        }

                        // ... no need to do the following, since we're interested in all hand positions (no actual gesture)
                        // 1. if okay, save observed skeleton positions:
                        // if(... several preconditions apply ...)
                        //      then record the dict
                        //
                        // 2. hand observed positions over to the move instance & ask if conditions are fulfilled
                        // if(move.isMove(dict))
                        //      then fire event moveRecognized
                        //      like so: MoveRecognized(move);
                    }

                    // to abort after 1st skeleton found, break the loop here
                    //break;
                }
            }
            catch (KeyNotFoundException)
            {
                Log("No skeleton found.");
            }
        }
    }
}
