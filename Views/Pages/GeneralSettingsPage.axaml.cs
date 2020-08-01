using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MqttDebugger.ViewModels;

namespace MqttDebugger.Views.Pages
{
    public class GeneralSettingsPage : UserControl
    {
        private CheckBox saveToFileCheckBox;
        private TextBox topicFilterTextBox;
        private ToggleButton darkModeToggleButton;

        private string topicBefore = "#";

        public GeneralSettingsPage()
        {
            this.InitializeComponent();

            saveToFileCheckBox = this.FindControl<CheckBox>("SaveToFileCheckBox");
            saveToFileCheckBox.Checked += SaveToFileCheckBox_Checked;

            topicFilterTextBox = this.FindControl<TextBox>("TopicFilterTextBox");
            topicFilterTextBox.GotFocus += TopicFilterTextBox_GotFocus;
            topicFilterTextBox.LostFocus += TopicFilterTextBox_LostFocus;

            darkModeToggleButton = this.FindControl<ToggleButton>("DarkModeToggleButton");
            darkModeToggleButton.Checked += SetDarkTheme;
            darkModeToggleButton.Unchecked += SetLightTheme;
        }

        /// <summary>
        /// Set apllication main theme to light fluent theme.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetLightTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = App.FluentLight;
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        /// <summary>
        /// Set apllication main theme to dark fluent theme.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDarkTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = App.FluentDark;
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        /// <summary>
        /// When the TopicFilterTextBox gets focus, override the stored filter with its current value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopicFilterTextBox_GotFocus(object sender, GotFocusEventArgs e)
        {
            topicBefore = topicFilterTextBox.Text;
        }

        /// <summary>
        /// If the TopicFilterTextBox lost focus and the user changed the value and the client is connected,
        /// notify the user, that he will need to reconnect, for the new filter to become active.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopicFilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((MainWindowViewModel)DataContext).IsConnectedToServer)
            {
                if (topicFilterTextBox.Text != topicBefore)
                {
                    ((MainWindow)this.VisualRoot)._notificationArea.Show(new Notification("Reload required.", "You will need to reconnect to the server, for that setting to become active.", NotificationType.Information));
                }
            }
        }

        /// <summary>
        /// When save to file is checked, the user will need to supply an output path for outputting the files,
        /// to which the payload is written.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveToFileCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select a folder to output the payload to.";

            string result = await dialog.ShowAsync((Window)this.VisualRoot);

            if (result.Length > 0)
            {
                ((MainWindowViewModel)DataContext).FileOutputFolder = result;
            }
            else
            {
                ((MainWindow)this.VisualRoot)._notificationArea.Show(new Notification("Error", "An output folder is required to write the payload to files.", NotificationType.Error));
                saveToFileCheckBox.IsChecked = false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
