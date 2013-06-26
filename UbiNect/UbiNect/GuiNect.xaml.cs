using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using NUIVector = Microsoft.Kinect.SkeletonPoint;
using System.Windows.Media.Animation;
using UbiNect.Button;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ComponentModel;
using UbiNect.GesturePosture;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PostureCreator;
using System.Xml;
using System.Resources;
namespace UbiNect
{
    /// <summary>
    /// Interaktionslogik für GuiNect.xaml
    /// </summary>
    public partial class GuiNect : Window
    {
        private Prototype proto;
        private KinectSensor nui;
        ColorAnimation LogArrivedAnimation;
        


        //HashSet<UbiNect.Button.AbstractElement> Buttons = new HashSet<Button.AbstractElement>();

        ObservableCollection<UbiNect.Button.AbstractElement> Buttons = new ObservableCollection<AbstractElement>();

        List<DualPoint> CurrentPoints = new List<DualPoint>();
        Polyline CurrentPolyline;


        // +++ Posture-Creator attributes
        private CustomJoint Head, ShoulderCenter, ElbowLeft, ElbowRight, HandLeft, HandRight,
            HipCenter, KneeLeft, KneeRight, FootLeft, FootRight;

        private CustomBone left_upper_arm, right_upper_arm, neck, left_lower_arm, right_lower_arm, body,
            right_upper_leg, rigth_lower_leg, left_upper_leg, left_lower_leg;

        // set for all joints of the skeleton
        private HashSet<CustomJoint> joints;

        // contextdata for the list in the tab "2. Select Constraints"
        private List<string> list_selected_joints;
        // list of all constraints, the positions in list_selected_joints and list_of_constraints are equal
        private List<CustomConstraint> list_of_constraints;

        // helper counters
        private int legJointCounter, baseJointCounter;

        private string hint_text = Properties.Resources.posture_generate_hint_text;
        private string hint_text_in_edit = Properties.Resources.posture_generate_hint_text_in_edit;
           


        public GuiNect(Prototype proto)
        {
            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
            //{
                InitializeComponent();

                this.proto = proto;
                this.nui = Prototype.nui;
                Buttons.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(UpdateSurfaceViews);

            //}
            //));


        }

        /// <summary>
        /// checks if component is available in prototype
        /// adds Tab for component
        /// </summary>
        /// <typeparam name="T">The type of the component to be returned</typeparam>
        public void InitComponentTab<T>() where T : RecognitionComponent
        {
            T component = proto.GetComponent<T>();

            if (component == null)
                return;

            ComponentList.Items.Add(new ListBoxItem() { Content = component.GetRecognitionType() });

            Type listType = typeof(T);

            if (listType == typeof(GestureRecognition))
            {
                tabGestures.Visibility = Visibility.Visible;
                GestureData.ItemsSource = (component as GestureRecognition).getRecognizableGestures();
               (component as GestureRecognition).GestureRecognized += new GestureRecognizedDelegate(gestureRecognized);
            }
            if (listType == typeof(PostureRecognition))
            {
                tabPostures.Visibility = Visibility.Visible;
                PostureData.ItemsSource = (component as PostureRecognition).getRecognizablePostures();
                (component as PostureRecognition).PostureRecognized += new PostureRecognizedDelegate(postureRecognized);

                InitPostureCreatorCustomComponents();
                InitPostureCreatorSkeleton();
            }
            if (listType == typeof(ButtonRecognition))
            {
                tabSurfaces.Visibility = Visibility.Visible;
                surfaceData.ItemsSource = (component as ButtonRecognition).RegisteredButtonsList;
                proto.GetComponent<ButtonRecognition>().RegisteredButtonsList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(UpdateSurfaceViews);
            }
            if (listType == typeof(Recorder.Recorder))
            {
                tabRecorder.Visibility = Visibility.Visible;
            }
        }



        short[,] DepthValues = new short[320, 240];
        WriteableBitmap outputBitmap = null;
        /// <summary>
        /// Gets called when a new depth frame arrives and GuiNect is designed to display depth data
        /// </summary>
        public void DepthFrame(object sender, DepthImageFrameReadyEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>{
            if (!this.nui.DepthStream.IsEnabled)
                return;

            short[] pixelData = null;
            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {
                    if (pixelData == null)
                    {
                        pixelData = new short[depthImageFrame.PixelDataLength];
                    }

                    depthImageFrame.CopyPixelDataTo(pixelData);

                    if (outputBitmap == null)
                    {
                        outputBitmap = new WriteableBitmap(
                                    depthImageFrame.Width,
                                    depthImageFrame.Height,
                                    96, // DpiX
                                    96, // DpiY
                                    PixelFormats.Bgr32,
                                    null);

                        Depth.Source = outputBitmap;
                    }

                    if (this.nui.IsRunning && this.outputBitmap != null)
                    {

                        this.outputBitmap.WritePixels(new Int32Rect(0, 0, depthImageFrame.Width, depthImageFrame.Height),
                                    new byte[depthImageFrame.Width * depthImageFrame.Height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8],
                                    depthImageFrame.Width * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8,
                                    0);
                    }
                }

                
            }

/* legacy (crashes)
        using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
        {
            if (depthImageFrame != null)
                {
                    

                    short[] im = new short[depthImageFrame.PixelDataLength];
                    depthImageFrame.CopyPixelDataTo(im);
              
                    byte[] Bytes = new byte[im.Length * 4 / 2]; // von 2 auf 4 bit erweitern
                    for (int i16 = 0, i32 = 0; i16 < im.Length; i16 += 2, i32 += 4)
                    {
                        int player = im[i16] & 0x07;
                        int depth = (im[i16 + 1] << 5) | (im[i16] >> 3); //depth in mm

                        int index = i16 / 2;
                        int x = (int)(index % depthImageFrame.Width);
                        int y = (int)(index / depthImageFrame.Width);
                        DepthValues[x, y] = (short)(im[i16] | (im[i16 + 1] << 8));//(short)(depth * 10); 

                        byte intensity = (byte)(255 - (255 * depth / 0x0fff));
                        Bytes[i32 + 2] = intensity;
                        Bytes[i32 + 1] = intensity;
                        Bytes[i32 + 0] = intensity;
                        Bytes[i32 + 3] = 255; //Alpha
                    }
             
                    Depth.Source = BitmapSource.Create(depthImageFrame.Width, depthImageFrame.Height, 96, 96, PixelFormats.Bgra32, null, im, depthImageFrame.Width * 4);
                }
            }
            */

                
            //}));
        }

        /// <summary>
        /// Gets called when a new vga frame arrives and GuiNect is designed to display vga data
        /// </summary>
        public void VGAFrame(object sender, ColorImageFrameReadyEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
            //{
            if (!this.nui.ColorStream.IsEnabled)
                return;



            using (ColorImageFrame im = e.OpenColorImageFrame())
            {
                if (im != null)
                {
                    byte[] pixelData = new byte[im.PixelDataLength];
                    im.CopyPixelDataTo(pixelData);
                    VGA.Source = BitmapSource.Create(im.Width, im.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, im.Width * im.BytesPerPixel);
                }
            }

        }

        /// <summary>
        /// Gets called when a new skeleton frame arrives and GuiNect is designed to display skeleton data
        /// </summary>
        public void SkeletonFrame(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            SkeletonCanvas.Children.Clear();

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }


            UpdateSurfaceViews(null, null);

            foreach (Skeleton data in skeletons)
            {
                if (data.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                Dictionary<JointType, NUIVector> BonePositions = new Dictionary<JointType, NUIVector>();
                Dictionary<JointType, JointTrackingState> TrackingStates = new Dictionary<JointType, JointTrackingState>();

                //Gather bone information
                foreach (Joint j in data.Joints)
                {
                    BonePositions[j.JointType] = j.Position;
                    TrackingStates[j.JointType] = j.TrackingState;
                }

                //Draw the skeleton
                Polyline[] SkeletonLines = new Polyline[3];
                Ellipse[] SkeletonCircles = new Ellipse[20];
                for (int i = 0; i < 3; i++)
                {
                    SkeletonLines[i] = new Polyline { Stroke = Brushes.Red, StrokeThickness = 4 };
                    SkeletonCanvas.Children.Add(SkeletonLines[i]);
                    for (int j = 0; j < (i == 1 ? 4 : 9); j++)
                    {
                        SkeletonLines[i].Points.Add(new Point());
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    SkeletonCircles[i] = new Ellipse() { Width = 10, Height = 10 };
                    SkeletonCanvas.Children.Add(SkeletonCircles[i]);
                }

                
                SkeletonLines[0].Points[0] = BonePositions[JointType.HandLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[1] = BonePositions[JointType.WristLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[2] = BonePositions[JointType.ElbowLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[3] = BonePositions[JointType.ShoulderLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[4] = BonePositions[JointType.ShoulderCenter].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[5] = BonePositions[JointType.ShoulderRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[6] = BonePositions[JointType.ElbowRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[7] = BonePositions[JointType.WristRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[0].Points[8] = BonePositions[JointType.HandRight].GetDisplayPosition(nui, SkeletonCanvas);

                SkeletonLines[1].Points[0] = BonePositions[JointType.Head].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[1].Points[1] = BonePositions[JointType.ShoulderCenter].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[1].Points[2] = BonePositions[JointType.Spine].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[1].Points[3] = BonePositions[JointType.HipCenter].GetDisplayPosition(nui, SkeletonCanvas);

                SkeletonLines[2].Points[0] = BonePositions[JointType.FootLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[1] = BonePositions[JointType.AnkleLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[2] = BonePositions[JointType.KneeLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[3] = BonePositions[JointType.HipLeft].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[4] = BonePositions[JointType.HipCenter].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[5] = BonePositions[JointType.HipRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[6] = BonePositions[JointType.KneeRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[7] = BonePositions[JointType.AnkleRight].GetDisplayPosition(nui, SkeletonCanvas);
                SkeletonLines[2].Points[8] = BonePositions[JointType.FootRight].GetDisplayPosition(nui, SkeletonCanvas);

                 
                int CircleIndex = 0;
                foreach (var item in TrackingStates)
                {
                    Point Position = BonePositions[item.Key].GetDisplayPosition(nui, SkeletonCanvas);
                    SkeletonCircles[CircleIndex].RenderTransform = new TranslateTransform(Position.X - 5, Position.Y - 5);
                    switch (item.Value)
                    {
                        case JointTrackingState.Inferred: SkeletonCircles[CircleIndex].Fill = Brushes.Yellow; break;
                        case JointTrackingState.NotTracked: SkeletonCircles[CircleIndex].Fill = Brushes.Red; break;
                        case JointTrackingState.Tracked: SkeletonCircles[CircleIndex].Fill = Brushes.Green; break;
                    }
                    CircleIndex++;
                }


                //Draw handposition in ButtonComponent Tab
                if (proto.GetComponent<ButtonRecognition>() != null)
                {
                    if (headCheck.IsChecked == true)
                    {
                        drawSurfaceJoints(BonePositions[JointType.Head], 8, Brushes.Navy);
                    }
                    if (handsCheck.IsChecked == true)
                    {
                        drawSurfaceJoints(BonePositions[JointType.HandLeft], 5, Brushes.BlueViolet);
                        drawSurfaceJoints(BonePositions[JointType.HandRight], 5, Brushes.BlueViolet);
                    }
                }
            }

            

        }


        /// <summary>
        /// for drawing skelettonparts in surfaceTab
        /// </summary>
        private void drawSurfaceJoints(NUIVector nuiv, double size, SolidColorBrush col)
        {

            Point p = new Point(nuiv.X, nuiv.Y);
            Ellipse symbol = new Ellipse() { Width = size, Height = size, Stroke = col };
            System.Windows.Controls.Canvas.SetTop(symbol, cFrontView.Height - ScaleToKinectSpace.scaleToKinectSpaceXY(cFrontView, p).Y - (size*0.5));
            System.Windows.Controls.Canvas.SetLeft(symbol, ScaleToKinectSpace.scaleToKinectSpaceXY(cFrontView, p).X - (size * 0.5));
            cFrontView.Children.Add(symbol);


            p = new Point(nuiv.X, nuiv.Z);
            symbol = new Ellipse() { Width = size, Height = size, Stroke = col };
            System.Windows.Controls.Canvas.SetTop(symbol, cTopView.Height - ScaleToKinectSpace.scaleToKinectSpaceXZ(cFrontView, p).Y - (size * 0.5));
            System.Windows.Controls.Canvas.SetLeft(symbol, ScaleToKinectSpace.scaleToKinectSpaceXZ(cFrontView, p).X - (size * 0.5));
            cTopView.Children.Add(symbol);
        }

        /// <summary>
        /// KeyEventHandler 
        /// </summary>
        /// <param name="keyEvent">any Keyevent</param>
        protected override void OnKeyDown(KeyEventArgs keyEvent)
        {
            if (keyEvent.Key.Equals(Key.Up))
            {
                moveSensor(true);
            }

            if (keyEvent.Key.Equals(Key.Down))
            {
                moveSensor(false);
            }
            if (keyEvent.Key.Equals(Key.S))
            {

            }
        }

        /// <summary>
        /// Moves the Sensorbar manually
        /// </summary>
        /// <param name="direction">true == moving up, false == moving down</param>
        public void moveSensor(Boolean direction)
        {
            if (direction)
            {
                try
                {
                    this.nui.ElevationAngle += 5;
                    Log("sensor raised");
                }
                catch (Exception e)
                {
                    Log("Error occured while trying to raise sensor. Stacktrace: " + e.ToString());
                }
            }
            else
            {
                try
                {
                    this.nui.ElevationAngle -= 5;
                    Log("sensor lowered");
                }
                catch (Exception e)
                {
                    Log("Error occured while trying to lower sensor Stacktrace: " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Adds a log message to the output window
        /// </summary>
        /// <param name="Message">The message to be added</param>
        public void Log(String Message)
        {
            txtLog.Text = Message + Environment.NewLine + txtLog.Text;
            txtLog.Background.BeginAnimation(SolidColorBrush.ColorProperty, LogArrivedAnimation);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLog.Background = Brushes.White;
            RegisterName("LogBackground", txtLog.Background as SolidColorBrush);

            txtLog.Background = new SolidColorBrush(Colors.White);
            LogArrivedAnimation = new ColorAnimation(Colors.Red, Colors.White, new Duration(TimeSpan.FromMilliseconds(500)));
        }

        string CurrentButtonName = "";
        private void btnNewSurface_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPolyline != null)
                return;

            DepthCanvasBorder.BorderBrush = Brushes.DarkRed;
            CurrentButtonName = Microsoft.VisualBasic.Interaction.InputBox("Bitte den Namen für die zu erstellenden Buttons eingeben");
            CurrentPolyline = new Polyline();
            CurrentPolyline.Stroke = Brushes.Red;
            CurrentPolyline.StrokeThickness = 4;
            CurrentPoints.Add(new DualPoint() { DisplayPosition = new Point(160, 120) });
            CurrentPolyline.Points.Add(CurrentPoints.Last().DisplayPosition);
            DepthCanvas.Children.Add(CurrentPolyline);
        }

        /// <summary>
        /// Removes a Button from ButtonRecognition
        /// </summary>
        /// <param name="sender">surfaceData (Datagrid)</param>
        /// <param name="e"></param>
        private void btnDeleteSurface_Click(object sender, RoutedEventArgs e)
        {
                proto.GetComponent<ButtonRecognition>().RemoveButton((AbstractElement)surfaceData.SelectedItem);
        }

        private void DepthCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentPolyline == null)
                return;
            Point p = Mouse.GetPosition(sender as IInputElement);
            if (p.X >= 320)
                p.X = 319;
            if (p.Y >= 240)
                p.Y = 239;
            CurrentPoints.Last().DisplayPosition = p;
            CurrentPoints.Last().SkeletonSpacePosition =
                p.GetSkeletonSpacePosition(DepthValues[(int)p.X, (int)p.Y], Prototype.nui, DepthCanvas);
            CurrentPolyline.Points[CurrentPolyline.Points.Count - 1] = CurrentPoints.Last().DisplayPosition;
        }

        private void DepthCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentPolyline == null)
                return;
            if (e.ChangedButton == MouseButton.Right)
            {
                DepthCanvasBorder.BorderBrush = Brushes.DarkGray;
                DepthCanvas.Children.Remove(CurrentPolyline);
                CurrentPoints.Clear();
                CurrentPolyline = null;
                AddButtonsToComponent();
                return;
            }

            Point p = Mouse.GetPosition(sender as IInputElement);
            NUIVector sp = p.GetSkeletonSpacePosition(DepthValues[(int)p.X, (int)p.Y], Prototype.nui, DepthCanvas);
            if (chkTriangle.IsSelected)
            {
                if (CurrentPoints.Count == 3)
                {
                    Buttons.Add(new Triangle(CurrentPoints[0].SkeletonSpacePosition, CurrentPoints[1].SkeletonSpacePosition, CurrentPoints[2].SkeletonSpacePosition) { ID = CurrentButtonName });
                    Buttons.Last().Draw(DepthCanvas, false);

                    CurrentPoints.RemoveAt(1);
                    CurrentPolyline.Points.RemoveAt(1);
                }

                CurrentPoints.Add(new DualPoint() { DisplayPosition = p, SkeletonSpacePosition = sp });
                CurrentPolyline.Points.Add(CurrentPoints.Last().DisplayPosition);
            }
            else if (chkSphere.IsSelected)
            {
                float rad;
                while (!Single.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Bitte Radius eingeben"), out rad)) ;
                Buttons.Add(new Sphere(sp.X, sp.Y, sp.Z, rad) { ID = CurrentButtonName });
                Buttons.Last().Draw(DepthCanvas, false);
                CurrentPolyline = null;
                CurrentPoints.Clear();
                AddButtonsToComponent();
                DepthCanvasBorder.BorderBrush = Brushes.DarkGray;
            }
        }

        private void AddButtonsToComponent()
        {

            foreach (var item in Buttons)
            {
                proto.GetComponent<ButtonRecognition>().AddButton(item);
            }
            Buttons.Clear();


            for (int i = DepthCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (!(DepthCanvas.Children[i] is Image))
                    DepthCanvas.Children.RemoveAt(i);
            }
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in ((sender as MenuItem).Parent as ContextMenu).Items)
            {
                if (item != sender)
                    item.IsChecked = false;
            }
        }

        private void btnImportSurface_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDig = new OpenFileDialog();
            OpenDig.Filter = "Gespeicherte Buttons(*.but)|*.but";
            Nullable<bool> result = OpenDig.ShowDialog();
            if (result == true) {
                proto.GetComponent<ButtonRecognition>().AddSavedButtons(OpenDig.FileName);
            }
        }

        private void btnExportSurface_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SaveDiag = new SaveFileDialog();
            SaveDiag.CheckPathExists = true;
            SaveDiag.Filter = "Gespeicherte Buttons|*.but";
            if(SaveDiag.ShowDialog(this) == false)
                return;

            FileStream fs = File.OpenWrite(SaveDiag.FileName);
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector sursel = new SurrogateSelector();
            sursel.AddSurrogate(typeof(NUIVector), new StreamingContext(StreamingContextStates.All), new VectorSurrogate());
            sursel.AddSurrogate(typeof(SolidColorBrush), new StreamingContext(StreamingContextStates.All), new SolidColorBrushSurrogate());
            bf.SurrogateSelector = sursel;

            bf.Serialize(fs, proto.GetComponent<ButtonRecognition>().RegisteredButtonsList);
            fs.Close();
        }

        /// <summary>
        /// Redraws all of the elements/buttons of the frontview and topview canvases (Surfaces TabItem)
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">event params</param>
        public void UpdateSurfaceViews(object sender, EventArgs args)
        {
            cFrontView.Children.Clear();
            cTopView.Children.Clear();

            if (proto != null && proto.GetComponent<ButtonRecognition>() != null)
                foreach (AbstractElement ae in proto.GetComponent<ButtonRecognition>().RegisteredButtonsList)
                {
                    ae.DrawFront(cFrontView);
                    ae.DrawTop(cTopView);
                }

            foreach (AbstractElement ae in Buttons)
            {
                ae.DrawFront(cFrontView);
                ae.DrawTop(cTopView);
            }
        }

        private void surfaceData_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            MessageBox.Show("cell edit ending");

        }

        private void gestureRecognized(Gesture g)
        {
            regGesture.Content = g.gestureName;
        }

        private void postureRecognized(int PlayerID, Posture g)
        {
            regPosture.Content = g.postureName;
        }

        ///
        /// +++++ Posture-Creator +++++
        /// 

        /// <summary>
        /// Resets the counters, creates new lists, sets the hint-texts.
        /// </summary>
        private void InitPostureCreatorCustomComponents()
        {
            tabPostureCreator.Visibility = Visibility.Visible;
            resetCounters();
            list_selected_joints = new List<string>();
            listbox_constraints.DataContext = list_selected_joints;
            list_of_constraints = new List<CustomConstraint>();
            hint_textBlock.Text = hint_text;
            hint_text_edti.Text = hint_text_in_edit;
        }

        /// <summary>
        /// Creates the joints and bones and draws the initial skeleton.
        /// </summary>
        private void InitPostureCreatorSkeleton()
        {
            joints = new HashSet<CustomJoint>();

            Head = new CustomJoint("Head", skeleton_canvas, 130, 75, this);
            joints.Add(Head);
            ShoulderCenter = new CustomJoint("ShoulderCenter", skeleton_canvas, 130, 110, this);
            joints.Add(ShoulderCenter);
            ElbowLeft = new CustomJoint("ElbowLeft", skeleton_canvas, 80, 140, this);
            joints.Add(ElbowLeft);
            ElbowRight = new CustomJoint("ElbowRight", skeleton_canvas, 180, 140, this);
            joints.Add(ElbowRight);
            HandLeft = new CustomJoint("HandLeft", skeleton_canvas, 30, 160, this);
            joints.Add(HandLeft);
            HandRight = new CustomJoint("HandRight", skeleton_canvas, 230, 160, this);
            joints.Add(HandRight);
            HipCenter = new CustomJoint("HipCenter", skeleton_canvas, 130, 180, this);
            joints.Add(HipCenter);
            KneeLeft = new CustomJoint("KneeLeft", skeleton_canvas, 100, 220, this);
            joints.Add(KneeLeft);
            KneeRight = new CustomJoint("KneeRight", skeleton_canvas, 160, 220, this);
            joints.Add(KneeRight);
            FootLeft = new CustomJoint("FootLeft", skeleton_canvas, 80, 270, this);
            joints.Add(FootLeft);
            FootRight = new CustomJoint("FootRight", skeleton_canvas, 180, 270, this);
            joints.Add(FootRight);

            left_upper_arm = new CustomBone(ShoulderCenter, ElbowLeft);
            right_upper_arm = new CustomBone(ShoulderCenter, ElbowRight);
            neck = new CustomBone(Head, ShoulderCenter);
            left_lower_arm = new CustomBone(ElbowLeft, HandLeft);
            right_lower_arm = new CustomBone(ElbowRight, HandRight);
            body = new CustomBone(ShoulderCenter, HipCenter);
            right_upper_leg = new CustomBone(HipCenter, KneeRight);
            rigth_lower_leg = new CustomBone(KneeRight, FootRight);
            left_upper_leg = new CustomBone(HipCenter, KneeLeft);
            left_lower_leg = new CustomBone(KneeLeft, FootLeft);

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
            InitPostureCreatorSkeleton();
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
            foreach (CustomJoint joint in joints)
            {
                if (joint.getIsBaseJoint())
                {
                    baseJointCounter++;
                }
            }

            // search the list, if there are at least two joints marked as leg-joints
            foreach (CustomJoint joint in joints)
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
                List<CustomJoint> tempSet = new List<CustomJoint>();
                foreach (CustomJoint joint in joints)
                {
                    if (joint.getIsBaseJoint())
                    {
                        // So, the base-joint will be the first entry in tempSet-
                        tempSet.Add(joint);
                    }
                }
                foreach (CustomJoint joint in joints)
                {
                    if (joint.getIsLegJoint())
                    {
                        tempSet.Add(joint);
                    }
                }
                if (!isConstraintAlreadyInList(tempSet[0], tempSet[1], tempSet[2]))
                {
                    CustomConstraint constraint = new CustomConstraint(tempSet[0], tempSet[1], tempSet[2], slider_tolerance.Value);
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
            foreach (CustomJoint joint in joints)
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
                list_of_constraints.Clear();
                list_selected_joints.Clear();
                listbox_constraints.DataContext = null;
                listbox_constraints.DataContext = list_selected_joints;
                foreach (CustomJoint joint in joints)
                {
                    joint.setEditMode(true);
                    joint.resetJoint();
                }
            }
            else if (tab_mode_control.SelectedIndex == 1)
            {
                foreach (CustomJoint joint in joints)
                {
                    joint.setEditMode(false);
                    joint.resetJoint();
                }
            }
        }

        /// <summary>
        /// Helper-method to reset all joints. See also the reset-method in <see cref="CustomJoint"/>
        /// </summary>
        private void resetConstraintsOfJoints()
        {
            foreach (CustomJoint joint in joints)
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
        private bool isConstraintAlreadyInList(CustomJoint baseJoint, CustomJoint jOne, CustomJoint jTwo)
        {
            foreach (CustomConstraint constraint in list_of_constraints)
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

                    foreach (CustomConstraint constraint in list_of_constraints)
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
                MessageBox.Show("The posture " + nameOfXML + ".xml was successfully created. \n\nIts located in:\n\n " + AppDomain.CurrentDomain.BaseDirectory + "" + nameOfXML + ".xml");
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

        private void GestureData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Gesture cg = ((Gesture)GestureData.SelectedItem);
            selectedGesture.Content = cg.gestureName;
            if (cg.description != null) 
                selectedGestureDesc.Text = cg.description;
            if (cg.descriptionImage != null) 
                selectedGestureImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(cg.descriptionImage.GetHbitmap(),IntPtr.Zero, Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());

        }

        private void StartRecord(object sender, RoutedEventArgs e)
        {
            proto.GetComponent<Recorder.Recorder>().StartRecord(Microsoft.VisualBasic.Interaction.InputBox("Please type name of record"));
        }

        private void EndRecord(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            proto.GetComponent<Recorder.Recorder>().StopRecord();
            this.Cursor = Cursors.Arrow;
        }

        private void surfaceData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (AbstractElement item in surfaceData.Items)
                if (surfaceData.SelectedItems.Contains(item))
                    item.IsSelected = true;
                else
                    item.IsSelected = false;

            this.UpdateSurfaceViews(null, null);
        }

        private void PostureData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Posture cg = ((Posture)PostureData.SelectedItem);
            selectedPosture.Content = cg.postureName;
            if( cg.descriptionImage != null) 
                selectedPostureImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(cg.descriptionImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        }

        
    }
}
