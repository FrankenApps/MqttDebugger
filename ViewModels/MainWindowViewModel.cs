using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using MqttDebugger.Models;
using MqttDebugger.Views;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions;
using MQTTnet.Protocol;
using MQTTnet.Server;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttDebugger.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Create a NotificationManager in order to display messages.
        /// </summary>
        public IManagedNotificationManager NotificationManager
        {
            get { return _notificationManager; }
            set { this.RaiseAndSetIfChanged(ref _notificationManager, value); }
        }

        /// <summary>
        /// Let the user choose which key should trigger the send action.
        /// </summary>
        private bool sendMessageShortcut = true;
        public bool SendMessageShortcut
        {
            get => sendMessageShortcut;
            set => this.RaiseAndSetIfChanged(ref sendMessageShortcut, value);
        }

        /// <summary>
        /// Let the user select choose dark or light appearance.
        /// </summary>
        private bool darkMode = false;
        public bool DarkMode
        {
            get => darkMode;
            set => this.RaiseAndSetIfChanged(ref darkMode, value);
        }

        /// <summary>
        /// Indicates if the included MQTT-Server is running.
        /// </summary>
        private bool isServerRunning = false;
        public bool IsServerRunning
        {
            get => isServerRunning;
            set => this.RaiseAndSetIfChanged(ref isServerRunning, value);
        }

        /// <summary>
        /// Indicates if the application is connected to a broker (e.g. MQTT-Server).
        /// </summary>
        private bool isConnectedToServer = false;
        public bool IsConnectedToServer
        {
            get => isConnectedToServer;
            set
            {
                this.RaiseAndSetIfChanged(ref isConnectedToServer, value);
                mqttClient.IsConnected = IsConnectedToServer;
            }
        }

        /// <summary>
        /// The username of the included MQTT-Server.
        /// </summary>
        private string serverUsernames = string.Empty;
        public string ServerUsernames
        {
            get => serverUsernames;
            set => this.RaiseAndSetIfChanged(ref serverUsernames, value);
        }

        /// <summary>
        /// The password of the included MQTT-Server.
        /// </summary>
        private string serverPasswords = string.Empty;
        public string ServerPasswords
        {
            get => serverPasswords;
            set => this.RaiseAndSetIfChanged(ref serverPasswords, value);
        }

        /// <summary>
        /// The host, to which the client should connect.
        /// </summary>
        private string clientHostname = string.Empty;
        public string ClientHostname
        {
            get => clientHostname;
            set
            {
                this.RaiseAndSetIfChanged(ref clientHostname, value);
                mqttClient.Host = clientHostname;
            }
        }

        /// <summary>
        /// The username used to connect to the specified host.
        /// </summary>
        private string clientUsername = string.Empty;
        public string ClientUsername
        {
            get => clientUsername;
            set => this.RaiseAndSetIfChanged(ref clientUsername, value);
        }

        /// <summary>
        /// The password used to connect to the specified host.
        /// </summary>
        private string clientPassword;
        public string ClientPassword
        {
            get => clientPassword;
            set => this.RaiseAndSetIfChanged(ref clientPassword, value);
        }

        /// <summary>
        /// The topic used to send messages to.
        /// </summary>
        private string clientTopic = "myTopic";
        public string ClientTopic
        {
            get => clientTopic;
            set
            {
                this.RaiseAndSetIfChanged(ref clientTopic, value);
                mqttClient.Topic = ClientTopic;
            }
        }

        /// <summary>
        /// Shows if the user wants to connect to the included server.
        /// In that case hostname, username and password are not necessary.
        /// </summary>
        private bool connectToInternalServer = true;
        public bool ConnectToInternalServer
        {
            get => connectToInternalServer;
            set => this.RaiseAndSetIfChanged(ref connectToInternalServer, value);
        }

        /// <summary>
        /// The text to display the client connection status.
        /// </summary>
        private string clientStatusText = "Status: Not connected.";
        public string ClientStatusText
        {
            get => clientStatusText;
            set => this.RaiseAndSetIfChanged(ref clientStatusText, value);
        }

        /// <summary>
        /// The text to indicate which option the button will trigger.
        /// </summary>
        private string clientConnectionButtonText = "Connect";
        public string ClientConnectionButtonText
        {
            get => clientConnectionButtonText;
            set => this.RaiseAndSetIfChanged(ref clientConnectionButtonText, value);
        }

        /// <summary>
        /// The status display text for the server area.
        /// </summary>
        private string serverStatusText = "Stopped.";
        public string ServerStatusText
        {
            get => serverStatusText;
            set => this.RaiseAndSetIfChanged(ref serverStatusText, value);
        }

        /// <summary>
        /// Adjust text color according to status.
        /// </summary>
        private IBrush serverStatusTextColor = Brushes.Red;
        public IBrush ServerStatusTextColor
        {
            get => serverStatusTextColor;
            set => this.RaiseAndSetIfChanged(ref serverStatusTextColor, value);
        }

        /// <summary>
        /// The message currently staged to be sent.
        /// </summary>
        private string mqttMessageText = string.Empty;
        public string MqttMessageText
        {
            get => mqttMessageText;
            set => this.RaiseAndSetIfChanged(ref mqttMessageText, value);
        }

        /// <summary>
        /// The received messages of the current topic.
        /// </summary>
        private string receivedMessages = string.Empty;
        public string ReceivedMessages
        {
            get => receivedMessages;
            set => this.RaiseAndSetIfChanged(ref receivedMessages, value);
        }

        /// <summary>
        /// The IP-Adress of the MQTT-Server in the local network.
        /// </summary>
        private string localIp = "127.0.0.1:1883";
        public string LocalIp
        {
            get => localIp;
            set => this.RaiseAndSetIfChanged(ref localIp, value);
        }

        /// <summary>
        /// Shows if the user wants to connect to the included server.
        /// In that case hostname, username and password are not necessary.
        /// </summary>
        private bool showTopicSelector = false;
        public bool ShowTopicSelector
        {
            get => showTopicSelector;
            set => this.RaiseAndSetIfChanged(ref showTopicSelector, value);
        }

        /// <summary>
        /// Wether the Topic should be shown in the log window.
        /// </summary>
        private bool messageOptionShowTopic = false;
        public bool MessageOptionShowTopic
        {
            get => messageOptionShowTopic;
            set
            {
                this.RaiseAndSetIfChanged(ref messageOptionShowTopic, value);
                mqttMessageOptions.DisplayTopic = MessageOptionShowTopic;
            }
        }

        /// <summary>
        /// The topic used to listen for messages.
        /// Listen to all topics except topics that start with '$'.
        /// </summary>
        private string filterByTopic = "#";
        public string FilterByTopic
        {
            get => filterByTopic;
            set
            {
                this.RaiseAndSetIfChanged(ref filterByTopic, value);
                mqttMessageOptions.FilterByTopic = filterByTopic;
            }
        }

        /// <summary>
        /// Wether the Payload should be logged to the output window as a UTF-8 endcoded string.
        /// </summary>
        private bool messageOptionDisplayPayloadAsString = true;
        public bool MessageOptionDisplayPayloadAsString
        {
            get => messageOptionDisplayPayloadAsString;
            set
            {
                this.RaiseAndSetIfChanged(ref messageOptionDisplayPayloadAsString, value);
                mqttMessageOptions.DisplayPayloadAsString = MessageOptionDisplayPayloadAsString;
            }
        }

        /// <summary>
        /// Wether the Payload should be written to a file.
        /// </summary>
        private bool messageOptionWritePayloadToFile = false;
        public bool MessageOptionWritePayloadToFile
        {
            get => messageOptionWritePayloadToFile;
            set
            {
                this.RaiseAndSetIfChanged(ref messageOptionWritePayloadToFile, value);
                mqttMessageOptions.WritePayloadToFile = MessageOptionWritePayloadToFile;
            }
        }

        /// <summary>
        /// The path to the folder where the payload will be saved if WritePayloadToFile is true.
        /// </summary>
        private string fileOutputFolder;
        public string FileOutputFolder
        {
            get => fileOutputFolder;
            set
            {
                this.RaiseAndSetIfChanged(ref fileOutputFolder, value);
                mqttMessageOptions.FolderOutPath = fileOutputFolder;
            }
        }

        /// <summary>
        /// Wether the Log window should autoscroll.
        /// </summary>
        private bool autoscrollMessageLog = true;
        public bool AutoscrollMessageLog
        {
            get => autoscrollMessageLog;
            set
            {
                this.RaiseAndSetIfChanged(ref autoscrollMessageLog, value);
            }
        }

        /// <summary>
        /// Indicates if the message log should be preserved when disconnecting from the server.
        /// </summary>
        private bool preserveMessageLog = false;
        public bool PreserveMessageLog
        {
            get => preserveMessageLog;
            set
            {
                this.RaiseAndSetIfChanged(ref preserveMessageLog, value);
            }
        }

        // Create reactive commands.
        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public ReactiveCommand<Unit, Unit> StopServerCommand { get; }
        public ReactiveCommand<Unit, Unit> RestartServerCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectToServerCommand { get; }
        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearMessageLogCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAutoscrollCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePreserveLogCommand { get; }


        // For notification handling.
        private MainWindow _window;
        private IManagedNotificationManager _notificationManager;

        // Create default instances of client and server.
        private Server mqttServer = new Server();
        private Client mqttClient = new Client();
        private MqttMessageOptions mqttMessageOptions = new MqttMessageOptions();

        // The actual client and server isnstances as provided in MQTTnet.
        private IMqttServer server;
        private IMqttClient client;
        private Thread listenForMessagesThread;

        public MainWindowViewModel(MainWindow window, IManagedNotificationManager notificationManager)
        {
            // Initialize reactive commands.
            StartServerCommand = ReactiveCommand.Create(StartServer);
            StopServerCommand = ReactiveCommand.Create(StopServer);
            RestartServerCommand = ReactiveCommand.Create(RestartServer);
            ResetSettingsCommand = ReactiveCommand.Create(ResetServerSettings);
            ConnectToServerCommand = ReactiveCommand.Create(ConnectToServer);
            SendMessageCommand = ReactiveCommand.Create(SendMessage);
            ClearMessageLogCommand = ReactiveCommand.Create(ClearMessageLog);
            ToggleAutoscrollCommand = ReactiveCommand.Create(ToggleAutoscroll);
            TogglePreserveLogCommand = ReactiveCommand.Create(TogglePreserveLog);

            // Copy references for window and notificationManager
            _notificationManager = notificationManager;
            _window = window;
        }

        /// <summary>
        /// Creates a new MQTT-Server instance and starts it.
        /// </summary>
        private async void StartServer()
        {
            IMqttServerOptions serverOptions;
            // Get Users and Passwords from user input.
            string[] usernames = ServerUsernames.Replace(" ", "").Split(';');
            string[] passwords = ServerPasswords.Replace(" ", "").Split(';');

            if (usernames.Length != passwords.Length)
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", $"Could not start the server, because usernames and passwords do not match.", NotificationType.Error));
                return;
            }

            mqttServer.Users = new List<MqttUser>();
            for (int i = 0; i < usernames.Length; i++)
            {
                mqttServer.Users.Add(new MqttUser(usernames[i], passwords[i]));
            }

            var factory = new MqttFactory();
            server = factory.CreateMqttServer();

            if (ServerUsernames.Length > 0)
            {
                serverOptions = new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .WithDefaultEndpointPort(mqttServer.Port)
                    .WithConnectionValidator(c =>
                    {
                        foreach (MqttUser user in mqttServer.Users)
                        {
                            if (user.Username == c.Username && user.Password == c.Password)
                            {
                                c.ReasonCode = MqttConnectReasonCode.Success;
                                return;
                            }
                        }
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    })
                    .WithSubscriptionInterceptor(c =>
                    {
                        c.AcceptSubscription = true;
                    })
                    .WithApplicationMessageInterceptor(c =>
                    {
                        c.AcceptPublish = true;
                    })
                    .Build();
            }
            else
            {
                serverOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(mqttServer.Port)
                .WithSubscriptionInterceptor(c =>
                {
                    c.AcceptSubscription = true;
                })
                .WithApplicationMessageInterceptor(c =>
                {
                    c.AcceptPublish = true;
                })
                .Build();
            }

            try
            {
                await server.StartAsync(serverOptions);
                ServerStatusText = "Running";
                ServerStatusTextColor = Brushes.Green;
                IsServerRunning = true;
            }
            catch (Exception e)
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", $"Could not start the server: {e.Message}", NotificationType.Error));
            }

            try
            {
                try
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        LocalIp = $"{endPoint.Address}:{mqttServer.Port}";
                    }
                }
                catch (SocketException)
                {
                    LocalIp = $"127.0.0.1:{mqttServer.Port}";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Could not read local ip adress: {e}");
            }

        }

        /// <summary>
        /// Stops a running MQTT-Server if available.
        /// </summary>
        private async void StopServer()
        {
            try
            {
                await server.StopAsync();
                ServerStatusText = "Stopped";
                ServerStatusTextColor = Brushes.Red;
                IsServerRunning = false;
            }
            catch (Exception e)
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", $"Could not stop the server: {e.Message}", NotificationType.Error));
            }
        }

        /// <summary>
        /// Restarts or Starts the MQTT-Server (depends on current state).
        /// </summary>
        private async void RestartServer()
        {
            if (server.IsStarted)
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Information", $"Restarting the server...", NotificationType.Information));
                StopServer();
                // Should be improved by making StopServer() awaitable.
                await Task.Delay(300);
                StartServer();
            }
            else
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Information", $"Starting the server...", NotificationType.Information));
                StartServer();
            }
        }

        /// <summary>
        /// Connect to a server via the provided credentials.
        /// </summary>
        private async void ConnectToServer()
        {
            if (mqttClient.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync();
                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Information", $"Disconnected from server.", NotificationType.Information));
                    ClientStatusText = "Status: Not connected.";
                    ClientConnectionButtonText = "Connect";
                    IsConnectedToServer = false;
                    MqttMessageText = string.Empty;

                    if (PreserveMessageLog == false)
                    {
                        ReceivedMessages = string.Empty;
                    }
                }
                catch (Exception e)
                {
                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", $"Could not disconnect: {e.Message}", NotificationType.Error));
                }

            }
            else
            {
                mqttClient.User = new MqttUser(ClientUsername, ClientPassword);

                if (ClientHostname.Length > 0)
                {
                    if (ClientHostname.Contains(':'))
                    {
                        int port = 1883;

                        mqttClient.Host = ClientHostname.Split(':')[0];
                        if (int.TryParse(ClientHostname.Split(':')[1], out port))
                        {
                            mqttClient.Port = port;
                        }
                    }
                    else
                    {
                        mqttClient.Host = ClientHostname;
                    }
                }

                var factory = new MqttFactory();
                client = factory.CreateMqttClient();
                var clientOptions = new MqttClientOptionsBuilder()
                                        .WithTcpServer(mqttClient.Host, mqttClient.Port)
                                        .WithCredentials((ConnectToInternalServer ? mqttServer.Users?.FirstOrDefault().Username : mqttClient.User.Username) ?? "",
                                                         (ConnectToInternalServer ? mqttServer.Users?.FirstOrDefault().Password : mqttClient.User.Password) ?? "")
                                        .Build();
                try
                {
                    await client.ConnectAsync(clientOptions, new CancellationToken());
                    listenForMessagesThread = new Thread(ListenForMessages);
                    listenForMessagesThread.Start();
                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Success", $"Connected to Server.", NotificationType.Success));
                    ClientStatusText = "Status: Connected to Server.";
                    ClientConnectionButtonText = "Disconnect";
                    IsConnectedToServer = true;
                }
                catch (Exception e)
                {
                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", $"Could not connect: {e.Message}", NotificationType.Error));
                }
            }
        }

        /// <summary>
        /// Send the message from the current queue.
        /// </summary>
        private async void SendMessage()
        {
            if (client.IsConnected)
            {
                if (string.IsNullOrEmpty(mqttClient.Topic))
                {
                    NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "The Topic can not be empty.", NotificationType.Error));
                }
                else
                {
                    if (string.IsNullOrEmpty(MqttMessageText))
                    {
                        NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "The Payload can not be empty.", NotificationType.Error));
                    }
                    else
                    {
                        await client.PublishAsync(mqttClient.Topic, MqttMessageText);
                        MqttMessageText = string.Empty;
                    }
                }
            }
            else
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "The server can not be reached anymore...", NotificationType.Error));
            }
        }

        /// <summary>
        /// Listen to incoming messages on the subscribed topics.
        /// </summary>
        private async void ListenForMessages()
        {
            List<MqttTopicFilter> topicFilters = new List<MqttTopicFilter>();
            string [] topicsAsString = mqttMessageOptions.FilterByTopic.Replace(" ", string.Empty).Split(';');

            if (topicsAsString.Length > 0)
            {
                foreach (string topicAsString in topicsAsString)
                {
                    MqttTopicFilter mqttTopicFilter = new MqttTopicFilter();
                    mqttTopicFilter.Topic = topicAsString;
                    topicFilters.Add(mqttTopicFilter);
                }

                await client.SubscribeAsync(topicFilters.ToArray());
            }
            else
            {
                NotificationManager.Show(new Avalonia.Controls.Notifications.Notification("Error", "Can not subscribe to chosen topic. Please change the topic in the general tab.", NotificationType.Error));
            }

            
            while (client.IsConnected == true)
            {
                client.UseApplicationMessageReceivedHandler(e =>
                {
                    if (e.ProcessingFailed == false)
                    {
                        string ReceivedMessage = string.Empty;
                        if (mqttMessageOptions.DisplayTopic)
                        {
                            ReceivedMessage += $"Topic: {e.ApplicationMessage.Topic} ";
                        }
                        if (mqttMessageOptions.DisplayPayloadAsString)
                        {
                            if (mqttMessageOptions.DisplayTopic || mqttMessageOptions.WritePayloadToFile)
                            {
                                ReceivedMessage += $"Payload: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)} ";
                            }
                            else
                            {
                                // If no other information is outputted, write out the message only.
                                ReceivedMessage += Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            }
                        }
                        ReceivedMessage += "\n";
                        ReceivedMessages += ReceivedMessage;
                        if (AutoscrollMessageLog)
                        {
                            // A short timeout is needed, so that the scroll viewer will scroll to the new end of its content.
                            Thread.Sleep(10);
                            Dispatcher.UIThread.InvokeAsync(_window.ScrollTextToEnd);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Replaces the user configuration of the development server with the default values.
        /// </summary>
        private void ResetServerSettings()
        {
            ServerPasswords = string.Empty;
            ServerUsernames = string.Empty;
            StopServer();
        }

        /// <summary>
        /// Remove all past messages from the log window.
        /// </summary>
        private void ClearMessageLog()
        {
            ReceivedMessages = string.Empty;
        }

        /// <summary>
        /// Toggles autoscroll property of the message log.
        /// </summary>
        private void ToggleAutoscroll()
        {
            AutoscrollMessageLog = !AutoscrollMessageLog;
        }

        /// <summary>
        /// Toggles the option to preserve the log (e.g. prevent the log window from being emptied when disconnecting).
        /// </summary>
        private void TogglePreserveLog()
        {
            PreserveMessageLog = !PreserveMessageLog;
        }
    }
}
