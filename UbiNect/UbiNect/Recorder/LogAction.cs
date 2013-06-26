using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiNect.Recorder
{
    [Serializable]
    public class LogAction : RecordAction
    {
        public String Message { get; set; }

        public long Time { get; set; }
    }
}
