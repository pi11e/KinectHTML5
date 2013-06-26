using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.IO;

namespace UbiNect.Recorder
{
    [Serializable]
    public class RecordedVideoFrame : RecordAction
    {
        public long Time { get; set; }
        public byte[] JpegData { get; set; }

        public RecordedVideoFrame(ColorImageFrame colorFrame)
        {
            if (colorFrame != null)
            {
                byte[] bits = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(bits);


                int BytesPerPixel = colorFrame.BytesPerPixel;
                int Width = colorFrame.Width;
                int Height = colorFrame.Height;

                var bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
                bmp.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), bits, Width * BytesPerPixel, 0);
                JpegBitmapEncoder jpeg = new JpegBitmapEncoder();
                jpeg.Frames.Add(BitmapFrame.Create(bmp));
                var SaveStream = new MemoryStream();
                jpeg.Save(SaveStream);
                SaveStream.Flush();
                JpegData = SaveStream.ToArray();
            }
            else
            {
                return;
            }
        }
    }
}
