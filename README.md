# Mqtt Debugger
This program is designed to help you quickly test any project using MQTT. It includes a broker (server) and a client, that you can use to subscribe and send messages to certain topics. The goal is to provide a flexible and easy interface for interacting with MQTT.

Internally this project uses [MQTTnet](https://github.com/chkr1011/MQTTnet) and [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)

## How to use
1. Clone the repository `git clone https://github.com/FrankenApps/MqttDebugger`
2. Go to directory `cd MqttDebugger`
3. Run the project `dotnet run`

There are also prebuilt (but unsigned) binaries available for Windows, Linux and Mac.

## Quick overview
Here is a quick overview of what it can currently do.

### Broker
It is possibly to set up a local server, that can be reached from your local network, if you grant access. The broker can use several usernames (and password) for authentication, but don't expect real security (this is only meant for testing).

### Cient
You can subscribe to all (or some) topics and send and receive messages. In the future it will also be possible to write messages to files and plot the received data. I also hope to include tools that allow for more sophisticated analysis of the messages in the future. 
