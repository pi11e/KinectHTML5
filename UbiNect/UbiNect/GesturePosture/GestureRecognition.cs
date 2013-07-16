using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace UbiNect.GesturePosture
{
    public delegate void GestureRecognizedDelegate(Gesture g);
    
    public class GestureRecognition : RecognitionComponent
    {
        // the event raised whenever one of the registered gestures is recognized (see recognizableGestures)
        public event GestureRecognizedDelegate GestureRecognized;

        public event LogDelegate Log;
        
        /// <summary>
        /// holder for the current gesture which is about to be recognized
        /// </summary>
        private Gesture currentGesture = null;

        /// <summary>
        /// variable which allows that only one gestures can recognized at once
        /// </summary>
        private bool isGestureRecording = false;

        /// <summary>
        /// holder for the startdate of an gesture
        /// </summary>
        private DateTime startdate = DateTime.Now;

        private List<Dictionary<JointType, Joint>> positions = new List<Dictionary<JointType, Joint>>();

        /// <summary>
        /// holds a number of gestures that are registered to be recognized; only recognition of registered gestures will raise the recognition event
        /// </summary>
        private List<Gesture> recognizableGestures = new List<Gesture>();

        public String GetRecognitionType()
        {
            return "Gesture Recognition Component";
        }

        /// <summary>
        /// gets all recognizable Gestures
        /// </summary>
        /// <returns>List</returns>
        public List<Gesture> getRecognizableGestures()
        {
            return recognizableGestures;
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

        /// <summary>
        /// Use to register gestures that need to be recognized.
        /// </summary>
        /// <param name="gestures">A string array containing the names of all gestures to be recognized.</param>
        /// <param name="useXML">Specify whether the posture names are class names (i.e. use hard-coded postures) or names of XML files in project space (i.e. load postures from individual XML files)</param>
        public void setRecognizableGestures(String[] gestures, bool useXML)
        {

            if (useXML)
            {
                //String baseFolderName = @".\GesturePosture\Gestures\";

                // strings in "postures" will be interpreted as filenames (without .xml extension) in the base XML folder
                foreach (String xmlPath in gestures)
                {
                    // entry point for XMLLoader of gesture XML files from base folder
                    // for details, see PostureRecognition.setRecognizablePostures(...)
                }
            }
            else
            {
                // strings in "postures" will be interpreted as class names
                foreach (String gestureName in gestures)
                {
                    try
                    {
                        // get type by given name
                        Type gestureType = Type.GetType("UbiNect.GesturePosture." + gestureName);
                        // handle errors
                        if (gestureType == null)
                        {
                            Console.WriteLine("ERROR in GestureRecognition.setRecognizableGestures() : Gesture type " + gestureName + " not found in current assembly.");
                            return;
                        }

                        // 1. Get constructor w/ 1 String argument (i.e. Gesture(String gestureName)
                        // 2. Invoke constructor with given gestureName
                        Gesture g = (Gesture)gestureType.GetConstructor(new Type[] { typeof(String) }).Invoke(new String[] { gestureName });
                        // 3. Add posture instance to recognizable postures
                        this.recognizableGestures.Add(g);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while trying to create an instance of type " + gestureName + "; strack trace following.");
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Called whenever a new skeleton frame is ready.
        /// </summary>
        /// <param name="e">The skeleton frame event that holds any frame information</param>
        /// <param name="Skeletons">Dictionary w/ player skeletons; each skeleton consists of a map of Joints and actual joint data</param>
        public void SkeletonFrame(SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<JointType, Joint>> Skeletons)
        {
            //Console.WriteLine("SkeletonFrame.");
            try
            {
                // 1. For every skeleton...
                foreach (var s in Skeletons)
                {
                    // (get the skeleton data dictionary)
                    Dictionary<JointType, Joint> dict = s.Value;
                    
                    // 2. ... and every gesture that is supposed to be recognized...
                    foreach (Gesture g in this.recognizableGestures)
                    {
                        // 3. ...test if gesture is recognized and the start conditions are conform
                        if (g==currentGesture && g.startRecognition(dict))
                        {

                            // set variable true, so that only this gesture is proofed and all others ignored
                            isGestureRecording = true;

                            // 4. ...set the start date of the gesture
                            if (positions.Count == 0)
                            {
                                startdate = DateTime.Now;
                            }
                            // 5. ...tests if time is under maximal gestureDuration and smaller minimal gestureMove lenght
                            if ((DateTime.Now - startdate).TotalMilliseconds < g.gestureDuration && g.getActualMoveLength() <= g.minimalLength)
                            {
                                if (g.saveObservation(dict))
                                {
                                    positions.Add(dict);
                                }
                            }
                            else
                            {
                                if (g.isGesture(positions))
                                {
                                    //. ... and if this is the case, fire the appropriate event.
                                    GestureRecognized(g);
                                    Log("Gesture " + g.gestureName + " recognized ( Player " + s.Key + " )");
                                    isGestureRecording = false;
                                    currentGesture = null;
                                }
                                else
                                {
                                    Log("couldn't recognize "+g.gestureName + " (Player "+ s.Key + " )");
                                    isGestureRecording = false;
                                    currentGesture = null;
                                }
                            }
                        }
                        else
                        {
                            //...if given Skeleton is start posture not registred yet and no gesture is recording
                            if (g.isStartPosture(dict) && g!=currentGesture && !isGestureRecording)
                            {
                                if (currentGesture == null)
                                {
                                    currentGesture = g;
                                } // otherwise, currentGesture is set to a previously registered (as in start posture recognized) gesture

                                Log("start posture of Player " + s.Key + " for gesture: " + g.gestureName + " recognized. Previously registered "+ currentGesture.gestureName + " is recording: " + currentGesture.startRecognition(dict) + ", positions recorded = " + positions.Count);
                                //only current gesture which start posture is recognized is saved

                                // maybe this should be changed: it will override the previous gesture even if it is already recording. this should check if a previously registered gesture could be recording.
                                currentGesture = g;
                                positions.Clear();
                            }
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Log("No skeleton found.");
            }
        }
    }
}
