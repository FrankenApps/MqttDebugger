using System;
using System.Collections.Generic;
using System.Text;

namespace MqttDebugger.Models
{
    public class MqttMessageOptions
    {
        public bool DisplayTopic { get; set; } = false;
        public bool DisplayPayloadAsString { get; set; } = true;
        public string FilterByTopic { get; set; } = "#";
        public bool WritePayloadToFile { get; set; } = false;
        public string FolderOutPath { get; set; }
    }
}
