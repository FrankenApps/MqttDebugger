using System;
using System.Collections.Generic;
using System.Text;

namespace MqttDebugger.Models
{
    public class MqttUser
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public MqttUser(string _username, string _password)
        {
            Username = _username;
            Password = _password;
        }
    }
}
