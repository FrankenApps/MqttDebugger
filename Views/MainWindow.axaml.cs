using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MqttDebugger.ViewModels;
using MqttDebugger.Views.Pages;
using System.Diagnostics;
using System.Windows.Input;

namespace MqttDebugger.Views
{
    public class MainWindow : Window
    {
        public WindowNotificationManager _notificationArea;

        private ClientPage clientPage;

        public MainWindow()
        {
            InitializeComponent();

            _notificationArea = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 2
            };

            DataContext = new MainWindowViewModel(this, _notificationArea);

            clientPage = this.FindControl<ClientPage>("ClientPage");
        }

        /// <summary>
        /// Routes the ScrollToEnd call from the ViewModel to the corresponding page (UserControl).
        /// </summary>
        public void ScrollTextToEnd()
        {
            clientPage.ScrollTextToEnd();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
