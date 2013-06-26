using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UbiNect;
using UbiNect.Button;
using System.IO;
using System.Windows.Media.Animation;
using UbiNect.GesturePosture;
using System.Media;

namespace _3D_Twister
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Prototype proto;
        ButtonRecognition br;
        PostureRecognition pr;
        GestureRecognition gr;
        Random rnd = new Random();

        /// <summary>
        /// Is played when the game is over
        /// </summary>
        ColorAnimation EndAnim;

        string[] postures = { "BothArmsUpPosture", "BothHandsTogetherPosture", "XPosture" };
        List<string> Buttons;
        List<string> PossibleTargets;

        /// <summary>
        /// Tracking IDs for Player 1 and 2 (0 and 1 respectively)
        /// </summary>
        int?[] PlayerIndex = new int?[2] { null, null };
        /// <summary>
        /// The name of the players' targets
        /// </summary>
        string[] PlayerTarget = new string[2] { null, null };

        //References to controls
        Label[] lblPlayer = new Label[2];
        Label[] lblPlayerPoints = new Label[2];
        Label[] lblPlayerPos = new Label[2];
        Image[] imgPlayer = new Image[2];

        /// <summary>
        /// Associates the name of a posture with the according Icon
        /// </summary>
        Dictionary<string, BitmapSource> PostureIcons = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Is the game running
        /// </summary>
        bool Running = false;
        /// <summary>
        /// When did the game start
        /// </summary>
        long StartTime;

        //Pling sound from: http://soundbible.com/1645-Pling.html
        /// <summary>
        /// Sound that is played when a target is reached
        /// </summary>
        SoundPlayer TargetReachedSound = new SoundPlayer("Pling.wav");
        SoundPlayer StartEndSound = new SoundPlayer("beep.wav");

        public MainWindow()
        {
            InitializeComponent();

            //Create new Prototype object
            proto = new Prototype();
            //Listen for video frames
            proto.VideoFrameReady += new VideoFrameReadyDelegate(proto_VideoFrameReady);

            //Add ButtonRecognition component
            proto.AddComponent<ButtonRecognition>();
            br = proto.GetComponent<ButtonRecognition>();
            //Listen for button collisions
            br.Collision += new CollisionDelegate(br_Collision);

            //Load buttons from file if there is one
            if (File.Exists("TwisterButtons.but"))
                br.AddSavedButtons("TwisterButtons.but");
            else
                MessageBox.Show("No Buttons defined. Please create TwisterButtons.but", "3D-Twister", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //set all buttons' color to red
            foreach (var item in br.RegisteredButtons.Values)
            {
                foreach (var item2 in item)
                {
                    //Set default color to red
                    item2.ElementColor = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));
                }
            }

            //Add a PostureRecognition component
            proto.AddComponent<PostureRecognition>();
            pr = proto.GetComponent<PostureRecognition>();
            //listen for the specified postures
            pr.setRecognizablePostures(postures, false);
            pr.PostureRecognized += new PostureRecognizedDelegate(pr_PostureRecognized);

            //Add a GestureRecognition component
            proto.AddComponent<GestureRecognition>();
            gr = proto.GetComponent<GestureRecognition>();
            //listen for start gesture
            gr.setRecognizableGestures(new string[] { "LeftHandSwipeRightGesture" }, false);
            gr.GestureRecognized += new GestureRecognizedDelegate(gr_GestureRecognized);

            //get the recognizable buttons
            string[] PossibleButtons = br.RegisteredButtons.Keys.ToArray();
            //create a set of all recognizable targets by combining buttons and postures
            PossibleTargets = postures.Concat(PossibleButtons).ToList();
            Buttons = br.RegisteredButtons.Keys.ToList();

            //Initialize end animation
            EndAnim = new ColorAnimation(Colors.Red, Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(1000)));
            DynamicCanvas.Background = new SolidColorBrush(Colors.Transparent);

            //set references to controls
            lblPlayer[0] = lblPlayer1;
            lblPlayer[1] = lblPlayer2;
            lblPlayerPoints[0] = lblPlayer1Points;
            lblPlayerPoints[1] = lblPlayer2Points;
            lblPlayerPos[0] = lblPlayer1Pos;
            lblPlayerPos[1] = lblPlayer2Pos;
            imgPlayer[0] = imgPlayer1;
            imgPlayer[1] = imgPlayer2;

            //Load and set posture icons
            PostureIcons.Add("BothArmsUpPosture", BitmapFrame.Create(File.OpenRead("HandsUp.png")));
            PostureIcons.Add("BothHandsTogetherPosture", BitmapFrame.Create(File.OpenRead("HandsTogether.png")));
            PostureIcons.Add("XPosture", BitmapFrame.Create(File.OpenRead("X.png")));

            //Load Sounds
            TargetReachedSound.Load();
            StartEndSound.Load();
        }

        void gr_GestureRecognized(Gesture g)
        {
            if (!Running)
                StartGame();
        }

        void pr_PostureRecognized(int PlayerID, Posture p)
        {
            TargetReached(PlayerID, p.postureName);
        }

        void br_Collision(AbstractElement Button, int PlayerIndex)
        {
            TargetReached(PlayerIndex, Button.ID);
        }

        /// <summary>
        /// Gets called when a player reaches a target (posture or button)
        /// </summary>
        /// <param name="PlayerID">ID of player who reached the target</param>
        /// <param name="Name">Name of reached target</param>
        void TargetReached(int PlayerID, string Name)
        {
            if (!Running)
                return;
            for (int Player = 0; Player < 2; ++Player)
            {
                //Check if player id is player 1 or 2 and if player's target is the reached one
                if (PlayerID == PlayerIndex[Player] && PlayerTarget[Player] == Name)
                {
                    TargetReachedSound.Play();
                    //Get a new target for the player
                    GetTargetForPlayer(Player);
                    //Increment player's points
                    lblPlayerPoints[Player].Content = (int.Parse(lblPlayerPoints[Player].Content.ToString()) + 1).ToString();
                }
            }
        }

        /// <summary>
        /// Rearrange the visualized buttons
        /// </summary>
        void ButtonsChanged()
        {
            this.Show();
            if (!this.IsMeasureValid || br == null)
                return;
            DynamicCanvas.Children.Clear();
            br.DrawButtons(DynamicCanvas);
        }

        void proto_VideoFrameReady(ImageSource Image)
        {
            //Set the new image on screen
            VGAImage.Source = Image;
            if (Running)
            {
                //Decrement the remaining time
                int TimeLeft = (int)(StartTime + 60000 - Environment.TickCount);
                lblTimeLeft.Content = String.Format("{0:0.00}", TimeLeft / 1000.0);
                if (TimeLeft <= 0)
                {
                    //End the game if time is up
                    StartEndSound.Play();
                    Running = false;
                    lblTimeLeft.Content = "Time's up";
                    (DynamicCanvas.Background as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, EndAnim);

                    for(int i = 0; i < 2; ++i)
                            UnsetPlayerTarget(i);
                }
            }
            for (int Player = 0; Player < 2; ++Player)
            {
                if (PlayerIndex[Player] != null)
                {
                    if (!proto.Skeletons.Contains((int)PlayerIndex[Player]))
                    {
                        //remove player if he is not recognized by kinect anymore
                        lblPlayer[Player].Content = "not recognized";
                        UnsetPlayerTarget(Player);
                        PlayerIndex[Player] = null;
                        lblPlayerPos[Player].Visibility = System.Windows.Visibility.Hidden;
                        imgPlayer[Player].Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else
                {

                    //Check if one of the recognized skeletons is not bound to a player yet...
                    foreach (var item in proto.Skeletons)
                    {
                        if (item != PlayerIndex[Player == 0 ? 1 : 0])
                        {
                            //... and bind it
                            PlayerIndex[Player] = item;
                            break;
                        }
                    }
                    //update controls for all recognized players if player just has become visible
                    if (PlayerIndex[Player] != null)
                    {
                        lblPlayer[Player].Content = "recognized";
                        lblPlayerPos[Player].Visibility = System.Windows.Visibility.Visible;
                        if (Running)
                            GetTargetForPlayer(Player);
                    }
                }

                //update controls for all recognized players
                if (PlayerIndex[Player] != null)
                {
                    Point p = proto.GetPlayerPositionVGA((int)PlayerIndex[Player], VGACanvas);
                    lblPlayerPos[Player].RenderTransform = new TranslateTransform(p.X - lblPlayerPos[Player].ActualWidth / 2, p.Y - lblPlayerPos[Player].ActualHeight / 2);
                    imgPlayer[Player].RenderTransform = new TranslateTransform(p.X - imgPlayer[Player].ActualWidth / 2, p.Y - lblPlayerPos[Player].ActualHeight / 2 - imgPlayer[Player].ActualHeight);
                }
            }
        }

        /// <summary>
        /// Gets a new target for a player
        /// </summary>
        /// <param name="Player">The player index (0 or 1)</param>
        private void GetTargetForPlayer(int Player)
        {
            if (PlayerIndex[Player] == null)
                return;
            if (PossibleTargets.Count == 0)
                return;
            string NewTarget = null;
            //Try to avoid same targets by generating random target 10 times if necessary
            for (int i = 0; i < 10; i++)
            {
                int j = 0;
                int index = rnd.Next(PossibleTargets.Count);
                foreach (var item in PossibleTargets)
                {
                    NewTarget = item;
                    if (j == index)
                        break;
                    j++;
                }
                if (PlayerTarget[0] != NewTarget && PlayerTarget[1] != NewTarget)
                    break;
            }
            //Unset old target
            if (PlayerTarget[Player] != null)
                UnsetPlayerTarget(Player);
            //set new target
            PlayerTarget[Player] = NewTarget;
            if (PlayerTarget[Player] != null)
                SetPlayerTarget(Player);
            ButtonsChanged();
        }

        /// <summary>
        /// Removes the highlight of a player's target
        /// </summary>
        /// <param name="Player">The player index (0 or 1)</param>
        void UnsetPlayerTarget(int Player)
        {
            if (PlayerTarget[Player] == null)
                return;
            //If target is a button
            if (Buttons.Contains(PlayerTarget[Player]))
            {
                var _Buttons = br.RegisteredButtons;
                foreach (var item in _Buttons[PlayerTarget[Player]])
                {
                    Color CurrentColor = item.ElementColor.Color;
                    if (Player == 0)
                        CurrentColor.G = 0;
                    if (Player == 1)
                        CurrentColor.B = 0;
                    if (CurrentColor.G == 0 && CurrentColor.B == 0)
                        CurrentColor.R = 255;

                    item.ElementColor = new SolidColorBrush(CurrentColor);
                }
                ButtonsChanged();
            }
            //If target is a posture
            if (postures.Contains(PlayerTarget[Player]))
            {
                imgPlayer[Player].Visibility = System.Windows.Visibility.Hidden;
            }
            PlayerTarget[Player] = null;
        }

        /// <summary>
        /// Adds highlight to a player's target
        /// </summary>
        /// <param name="Player">The player index (0 or 1)</param>
        void SetPlayerTarget(int Player)
        {
            if (PlayerTarget[Player] == null)
                return;
            //if target is a button
            if (Buttons.Contains(PlayerTarget[Player]))
            {
                var _Buttons = br.RegisteredButtons;
                foreach (var item in _Buttons[PlayerTarget[Player]])
                {
                    Color CurrentColor = item.ElementColor.Color;
                    if (Player == 0)
                        CurrentColor.G = 255;
                    if (Player == 1)
                        CurrentColor.B = 255;
                    CurrentColor.R = 0;

                    item.ElementColor = new SolidColorBrush(CurrentColor);
                }
                ButtonsChanged();
            }
            //if target is a posture
            if (postures.Contains(PlayerTarget[Player]))
            {
                imgPlayer[Player].Visibility = System.Windows.Visibility.Visible;
                imgPlayer[Player].Source = PostureIcons[PlayerTarget[Player]];
            }
        }
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ButtonsChanged();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width;
            double h = e.NewSize.Height;
            if (w / h >= 4.0 / 3) //Too broad
            {
                VGACanvas.Height = h;
                VGACanvas.Width = h * 4 / 3;
                DynamicCanvas.Height = h;
                DynamicCanvas.Width = h * 4 / 3;
            }
            else //Too high
            {
                VGACanvas.Height = w * 3 / 4;
                VGACanvas.Width = w;
                DynamicCanvas.Height = w * 3 / 4;
                DynamicCanvas.Width = w;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        void StartGame()
        {
            StartEndSound.Play();
            Running = true;
            StartTime = Environment.TickCount;

            for (int Player = 0; Player < 2; ++Player)
            {
                lblPlayerPoints[Player].Content = "0";
                UnsetPlayerTarget(Player);
                GetTargetForPlayer(Player);
            }
        }
    }
}
