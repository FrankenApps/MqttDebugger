using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MqttDebugger.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace MqttDebugger.Views
{
    public class MainWindow : Window
    {
        private WindowNotificationManager _notificationArea;
        private ScrollViewer messageLogScrollViewer;

        private TextBlock linkTextBlock;
        private CheckBox saveToFileCheckBox;
        private TextBox topicFilterTextBox;

        private string topicBefore = "#";

        public MainWindow()
        {
            InitializeComponent();

            _notificationArea = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 2
            };

            DataContext = new MainWindowViewModel(this, _notificationArea);

            messageLogScrollViewer = this.FindControl<ScrollViewer>("MessageLogScrollViewer");
            ScrollTextToEnd();

            linkTextBlock = this.FindControl<TextBlock>("LinkText");
            linkTextBlock.Tapped += OpenLink;

            saveToFileCheckBox = this.FindControl<CheckBox>("SaveToFileCheckBox");
            saveToFileCheckBox.Checked += SaveToFileCheckBox_Checked;

            topicFilterTextBox = this.FindControl<TextBox>("TopicFilterTextBox");
            topicFilterTextBox.GotFocus += TopicFilterTextBox_GotFocus;
            topicFilterTextBox.LostFocus += TopicFilterTextBox_LostFocus;
        }

        private void TopicFilterTextBox_GotFocus(object sender, GotFocusEventArgs e)
        {
            topicBefore = topicFilterTextBox.Text;
        }

        private void TopicFilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((MainWindowViewModel)DataContext).IsConnectedToServer)
            {
                if (topicFilterTextBox.Text != topicBefore)
                {
                    _notificationArea.Show(new Notification("Reload required.", "You will need to reconnect to the server, for that setting to become active.", NotificationType.Information));
                }
            }
        }

        private async void SaveToFileCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select a folder to output the payload to.";

            string result = await dialog.ShowAsync(this);

            if (result.Length > 0)
            {
                ((MainWindowViewModel)DataContext).FileOutputFolder = result;
            }
            else
            {
                _notificationArea.Show(new Notification("Error", "An output folder is required to write the payload to files.", NotificationType.Error));
                saveToFileCheckBox.IsChecked = false;
            }
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            TextBlock urlTextBlock = (TextBlock)sender;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = $"http://{urlTextBlock.Text}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public void ScrollTextToEnd()
        {
            messageLogScrollViewer.ScrollToEnd();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
