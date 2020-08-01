using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MqttDebugger.ViewModels;

namespace MqttDebugger.Views.Pages
{
    public class ClientPage : UserControl
    {
        private ScrollViewer messageLogScrollViewer;
        private Image openPlotWindowImage;
        public ClientPage()
        {
            this.InitializeComponent();

            messageLogScrollViewer = this.FindControl<ScrollViewer>("MessageLogScrollViewer");
            openPlotWindowImage = this.FindControl<Image>("OpenPlotWindow");

            openPlotWindowImage.Tapped += OpenPlotWindow;
        }

        private void OpenPlotWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var viewModel = (MainWindowViewModel)((Window)this.VisualRoot).DataContext;
            var window = new PlotWindow(viewModel);

            window.Show((Window)this.VisualRoot);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ScrollTextToEnd()
        {
            messageLogScrollViewer.ScrollToEnd();
        }
    }
}
