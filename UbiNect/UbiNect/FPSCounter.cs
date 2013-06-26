using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiNect
{

    /// <summary>
    /// Counts frequency of frames
    /// </summary>
    class FPSCounter
    {
        int Frames = 0;
        long LastReset = System.Environment.TickCount;
        int fps;
        long lastFrame; 
        int framemillis;

        /// <summary>
        /// Call this method when a new frame starts
        /// </summary>
        public void Frame()
        {
            Frames++;
            framemillis = (int)(System.Environment.TickCount - lastFrame);
            lastFrame = System.Environment.TickCount;
        }

        /// <summary>
        /// The duration of the last frame in milliseconds
        /// </summary>
        public int FrameMillis
        {
            get { return framemillis; }
        }

        /// <summary>
        /// The frequency of occured frames in frames per second
        /// </summary>
        public int FPS
        {
            get
            {
                if (Environment.TickCount - LastReset > 1000)
                {
                    fps = Frames;
                    Frames = 0;
                    LastReset = Environment.TickCount;
                }
                return fps;
            }
        }
    }
}
