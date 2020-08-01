using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MqttDebugger.ViewModels;
using MqttDebugger.Views.Pages;

namespace MqttDebugger.Views
{
    public class PlotWindow : Window
    {
        public PlotWindow()
        {
            this.InitializeComponent();
        }
        public PlotWindow(MainWindowViewModel viewModel)
        {
            this.InitializeComponent();
            this.Content = new TimeSeriesPage(viewModel);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
