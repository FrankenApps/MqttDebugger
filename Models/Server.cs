using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace MqttDebugger.Models
{
    public class Server
    {
        public bool IsRunning { get; set; }
        public List<MqttUser> Users { get; set; }
        public int Port { get; set; } = 1883;
    }
}
