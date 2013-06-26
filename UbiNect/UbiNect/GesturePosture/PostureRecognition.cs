using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Drawing;

namespace UbiNect.GesturePosture
{
    // the posture recognition delegate; reveals a posture & player id
    public delegate void PostureRecognizedDelegate(int PlayerID, Posture p);

    public class PostureRecognition : RecognitionComponent
    {
        // the event raised whenever one of the registered postures is recognized (see recognizablePostures)
        public event PostureRecognizedDelegate PostureRecognized;

        public event LogDelegate Log;

        // holds a number of postures that are registered to be recognized; only recognition of registered postures will raise the recognition event
        private List<Posture> recognizablePostures = new List<Posture>();

        public String GetRecognitionType()
        {
            return "Posture Recognition Component";
        }

        /// <summary>
        /// gets all recognizable Postures
        /// </summary>
        /// <returns>List</returns>
        public List<Posture> getRecognizablePostures()
        {
            return recognizablePostures;
        }

        /// <summary>
        /// Called whenever a new VGAFrame is ready. Not used in posture recognition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VGAFrame(object sender, ColorImageFrameReadyEventArgs e)
        {
            // do nothing.
        }

        /// <summary>
        /// Called whenever a new DepthFrame is ready. Not used in posture recognition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DepthFrame(object sender, DepthImageFrameReadyEventArgs e)
        {
            // do nothing.
        }

        /// <summary>
        /// Use to register postures that need to be recognized.
        /// </summary>
        /// <param name="postures">A string array containing the names of all postures to be recognized.</param>
        /// <param name="useXML">Specify whether the posture names are class names (i.e. use hard-coded postures) or names of XML files in project space (i.e. load postures from individual XML files)</param>
        public void setRecognizablePostures(String[] postures, bool useXML)
        {
            if (useXML)
            {
                String baseFolderName = @".\GesturePosture\Postures\";
                // strings in "postures" will be interpreted as filenames (without .xml extension) in the base XML folder
                foreach (String xmlPath in postures)
                {
                    // Create a new posture instance for every XML name
                    GeneratedFromXMLPosture p = new GeneratedFromXMLPosture(xmlPath, baseFolderName + xmlPath + ".xml") { descriptionImage =  ((Bitmap)Properties.Resources.ResourceManager.GetObject(xmlPath)) };
                    // Add posture to set of recognizable postures
                    this.recognizablePostures.Add(p);
                }
            }
            else
            {
                // strings in "postures" will be interpreted as class names
                foreach (String postureName in postures)
                {
                    try
                    {
                        // get type by given name
                        Type postureType = Type.GetType("UbiNect.GesturePosture." + postureName);
                        // handle errors
                        if (postureType == null)
                        {
                            Console.WriteLine("ERROR in PostureRecognition.setRecognizablePostures() : Posture type " + postureName + " not found in current assembly.");
                            return;
                        }

                        // 1. Get constructor w/ 1 String argument (i.e. Posture(String postureName)
                        // 2. Invoke constructor with given postureName
                        Posture p = (Posture) postureType.GetConstructor(new Type[] { typeof(String) }).Invoke(new String[] { postureName });
                        // 3. Add posture instance to recognizable postures
                        this.recognizablePostures.Add(p);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while trying to create an instance of type " + postureName + "; strack trace following.");
                        Console.WriteLine(e.StackTrace);
                    }
                }


            }
        }

        /// <summary>
        /// Called whenever a new skeleton frame is ready.
        /// </summary>
        /// <param name="e">The skeleton frame event that holds all frame information</param>
        /// <param name="Skeletons">Dictionary w/ player skeletons; each skeleton consists of a map of Joints and actual joint data</param>
        public void SkeletonFrame(SkeletonFrameReadyEventArgs e, Dictionary<int, Dictionary<JointType, Joint>> Skeletons)
        {
            try
            {
                // 1. For every skeleton...
                foreach (var s in Skeletons)
                {
                    // (get the skeleton data dictionary)
                    Dictionary<JointType, Joint> dict = s.Value;
                    // 2. ... and every posture that is supposed to be recognized...
                    foreach (Posture p in this.recognizablePostures)
                    {
                        // 3. ... check if the current skeleton data matches the posture ...
                        if (p.isPosture(dict))
                        {
                            // 4. ... and if this is the case, fire the appropriate event.
                            PostureRecognized(s.Key, p);
                            // (also, log the recognized posture)
                            Log("Posture of player " + s.Key + " recognized: " + p.postureName);
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
