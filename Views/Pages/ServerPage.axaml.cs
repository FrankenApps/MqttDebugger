using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace MqttDebugger.Views.Pages
{
    public class ServerPage : UserControl
    {
        private TextBlock linkTextBlock;
        public ServerPage()
        {
            this.InitializeComponent();

            linkTextBlock = this.FindControl<TextBlock>("LinkText");
            linkTextBlock.Tapped += OpenLink;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
    }
}
