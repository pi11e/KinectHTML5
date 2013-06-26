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
using UbiNect;
using UbiNect.Recorder;
using UbiNect.GesturePosture;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace RecordDemo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer PlayTimer = new DispatcherTimer();
        ColorAnimation LogArrivedAnimation; 

        public MainWindow()
        {
            InitializeComponent();
            PlayTimer.Tick += new EventHandler(PlayTimer_Tick);
        }

        void PlayTimer_Tick(object sender, EventArgs e)
        {
            LastEvent++;
            RecordAction CurrentEvent = Actions[LastEvent];
            if (CurrentEvent is RecordedVideoFrame)
            {
                RecordedVideoFrame vf = CurrentEvent as RecordedVideoFrame;
                var OpenStream = new MemoryStream(vf.JpegData);
                var jpeg = new JpegBitmapDecoder(OpenStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                VideoImage.Source = jpeg.Frames[0];
            }
            if (CurrentEvent is LogAction)
            {
                LogAction la = CurrentEvent as LogAction;
                Log(la.Message);
            }
            WaitForNextEvent();
        }

        Prototype proto;
        UbiNect.Recorder.Recorder rec;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            proto = new Prototype(RequireKinect:false);
            proto.AddComponent<Recorder>();
            rec = proto.GetComponent<Recorder>();

            LogArrivedAnimation = new ColorAnimation(Colors.Red, Colors.White, new Duration(TimeSpan.FromMilliseconds(500)));
            txtLog.Background = new SolidColorBrush(Colors.Transparent);
        }

        private List<RecordAction> Actions;
        long StartTime;
        int LastEvent;
        private void LoadRec(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Record Files|*.rec";
            ofd.Multiselect=false;
            if(ofd.ShowDialog(this) == false)
                return;
            FileStream fs = File.OpenRead(ofd.FileName);
            //Gzip compression enlarges file size (is already binary)
            //DeflateStream gs = new DeflateStream(fs, CompressionMode.Decompress);
            BinaryFormatter bf = new BinaryFormatter();
            Actions = bf.Deserialize(fs) as List<RecordAction>;
            fs.Close();
            this.Cursor = Cursors.Arrow;
            StartTime = Environment.TickCount;
            LastEvent = -1;
            WaitForNextEvent();
        }

        void WaitForNextEvent()
        {
            int NextEvent = LastEvent + 1;
            if (NextEvent >= Actions.Count)
            {
                PlayTimer.Stop();
                return;
            }
            long LastAction = LastEvent < 0 ? StartTime : Actions[LastEvent].Time;
            RecordAction rac = Actions[NextEvent];
            long millis = rac.Time - LastAction;
            if (millis <= 0)
                millis = 1;
            PlayTimer.Interval = TimeSpan.FromMilliseconds(millis);
            PlayTimer.Start();
        }

        private void StartRecord(object sender, RoutedEventArgs e)
        {
            rec.StartRecord(Microsoft.VisualBasic.Interaction.InputBox("Please type name of record"));
        }

        private void StopRecord(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            rec.StopRecord();
            this.Cursor = Cursors.Arrow;
        }

        public void Log(String Message)
        {
            txtLog.Text = Message + Environment.NewLine + txtLog.Text;
            txtLog.Background.BeginAnimation(SolidColorBrush.ColorProperty, LogArrivedAnimation);
        }
    }
}
