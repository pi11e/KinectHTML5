using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiNect.Recorder
{
    public interface RecordAction
    {
        /// <summary>
        /// Time, when the action occured
        /// </summary>
        long Time { get; set; }
    }
}
