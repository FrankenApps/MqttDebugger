using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace MqttDebugger.Models
{
    public class Client
    {
        public bool IsConnected { get; set; } = false;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public MqttUser User { get; set; }
        public string Topic { get; set; } = "myTopic";
        public bool ConnectToInternalServer { get; set; } = false;
    }
}
