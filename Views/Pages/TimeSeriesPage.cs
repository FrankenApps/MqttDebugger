using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using MqttDebugger.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MqttDebugger.Views.Pages
{
    public class TimeSeriesPage : Control
    {
        private MainWindowViewModel viewModel;
        public TimeSeriesPage(MainWindowViewModel _viewModel)
        {
            ClipToBounds = true;
            viewModel = _viewModel;
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            private readonly FormattedText _noSkia;
            private MainWindowViewModel _viewModel;

            public CustomDrawOp(Rect bounds, FormattedText noSkia, MainWindowViewModel viewModel)
            {
                _noSkia = noSkia;
                _viewModel = viewModel;
                Bounds = bounds;
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;
            public void Render(IDrawingContextImpl context)
            {
                bool isLightTheme = Application.Current.Styles[1] == App.FluentLight;

                var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
                if (canvas == null)
                    context.DrawText(Brushes.Black, new Point(), _noSkia.PlatformImpl);
                else
                {
                    canvas.Save();

                    SKPaint solidForeground = new SKPaint
                    {
                        Color =  isLightTheme ? new SKColor(0, 0, 0) : new SKColor(255, 255, 255),
                        TextSize = 12.0f
                    };

                    SKPaint strokeBlue = new SKPaint
                    {
                        Color = new SKColor(0, 0, 255),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2.0f
                    };

                    SKPaint solidBlue = new SKPaint
                    {
                        Color = new SKColor(0, 0, 255)
                    };

                    float canvasWidth = canvas.LocalClipBounds.Width;
                    float canvasHeight = canvas.LocalClipBounds.Height;

                    /*canvas.DrawLine(0, 
                                    canvasHeight / 2.0f,
                                    canvasWidth,
                                    canvasHeight / 2.0f, 
                                    solidBlack);*/

                    canvas.DrawLine(5,
                                    0,
                                    5,
                                    canvasHeight,
                                    solidForeground);

                    var stringData = _viewModel.ReceivedMessages.Split('\n');
                    List<float> numericalData = new List<float>();

                    foreach (string item in stringData)
                    {
                        float numericData;
                        if (float.TryParse(item, out numericData))
                        {
                            numericalData.Add(numericData);
                        }
                    }

                    float maxValue, minValue;
                    try
                    {
                        maxValue = numericalData.Max();
                        minValue = numericalData.Min();
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.WriteLine(e.Message);
                        return;
                    }


                    float range = Math.Abs(maxValue - minValue);
                    float offset = range * 0.05f;
                    //float upperBound = maxValue + offset;
                    float lowerBound = minValue - offset;
                    float plotRange = range + offset*2;

                    //Prevent errors by zero division.
                    if (plotRange == 0)
                    {
                        plotRange = 0.1f;
                    }

                    canvas.DrawLine(2.5f, canvasHeight - (maxValue - lowerBound) / plotRange * canvasHeight, 7.5f, canvasHeight - (maxValue - lowerBound) / plotRange * canvasHeight, solidForeground);
                    canvas.DrawLine(2.5f, canvasHeight - (minValue - lowerBound) / plotRange * canvasHeight, 7.5f, canvasHeight - (minValue - lowerBound) / plotRange * canvasHeight, solidForeground);

                    canvas.DrawText(maxValue.ToString(), 7.5f, canvasHeight - (maxValue - lowerBound) / plotRange * canvasHeight + 6.0f, solidForeground);
                    canvas.DrawText(minValue.ToString(), 7.5f, canvasHeight - (minValue - lowerBound) / plotRange * canvasHeight + 6.0f, solidForeground);

                    int dataPoints = numericalData.Count;
                    float pointDistance =  (canvasWidth - 5.0f) / (dataPoints-1);

                    if (dataPoints > 1)
                    {
                        SKPath plotLine = new SKPath();
                        plotLine.MoveTo(5.0f, canvasHeight - (numericalData[0] - lowerBound) / plotRange * canvasHeight);
                        for (int i = 1; i < dataPoints; i++)
                        {
                            plotLine.LineTo(i * pointDistance + 5.0f, canvasHeight - (numericalData[i] - lowerBound) / plotRange * canvasHeight);
                        }
                        canvas.DrawPath(plotLine, strokeBlue);
                    }
                    else if (dataPoints == 1)
                    {
                        canvas.DrawCircle(5.0f , canvasHeight - (numericalData[0] - lowerBound) / plotRange * canvasHeight, 5.0f, solidBlue);
                    }

                    canvas.Restore();
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            var noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), noSkia, viewModel));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
