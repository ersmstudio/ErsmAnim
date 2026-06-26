using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ErsmAnim.Models;

namespace ErsmAnim
{
    public partial class MainWindow
    {
        private void Timeline_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            timelineZoom += e.Delta > 0 ? 0.1 : -0.1;
            timelineZoom = Math.Max(0.5, Math.Min(2.0, timelineZoom));

            TimelinePanel.LayoutTransform = new ScaleTransform(timelineZoom, 1);
            KeyframeLayer.LayoutTransform = new ScaleTransform(timelineZoom, 1);

            DrawTimelineGrid();
            DrawKeyframes();
            MoveCursorToFrame(activeFrameIndex);
        }

        private void DrawTimelineGrid()
        {
            TimelineGrid.Children.Clear();

            double width = Math.Max(TimelinePanel.ActualWidth, frames.Count * FrameSlot + 40);
            for (double x = 0; x < width; x += FrameSlot)
            {
                TimelineGrid.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = TimelineHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                    StrokeThickness = 1
                });
            }
        }

        private void DrawKeyframes()
        {
            KeyframeLayer.Children.Clear();

            for (int i = 0; i < frames.Count; i++)
            {
                var keyframe = new Rectangle
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Yellow,
                    RadiusX = 2,
                    RadiusY = 2
                };

                Canvas.SetLeft(keyframe, i * FrameSlot + 16);
                Canvas.SetTop(keyframe, 3);
                KeyframeLayer.Children.Add(keyframe);
            }
        }

        private void TimelineCursor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursorDragging = true;
            Mouse.Capture(TimelineCursor, CaptureMode.Element);
        }

        private void TimelineCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (!cursorDragging || frames.Count == 0)
            {
                return;
            }

            Point position = e.GetPosition(TimelinePanel);
            double x = position.X + TimelineScroll.HorizontalOffset;
            int index = ClampFrameIndex((int)Math.Round(x / FrameSlot));

            MoveCursorToFrame(index);
        }

        private void TimelineCursor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            cursorDragging = false;
            Mouse.Capture(null);

            if (frames.Count == 0)
            {
                return;
            }

            int index = ClampFrameIndex((int)Math.Round(TimelineCursor.Margin.Left / FrameSlot));
            SwitchToFrame(index);
        }

        private void MoveCursorToFrame(int index)
        {
            double targetX = index * FrameSlot;
            var animation = new ThicknessAnimation(
                TimelineCursor.Margin,
                new Thickness(targetX, TimelineCursor.Margin.Top, 0, 0),
                TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            TimelineCursor.BeginAnimation(MarginProperty, animation);
        }

        private void AddFrame()
        {
            int newIndex = frames.Count;

            var button = new Button
            {
                Style = (Style)FindResource("FrameButtonStyle"),
                Content = (newIndex + 1).ToString(),
                Tag = newIndex
            };
            button.Click += TimelineFrame_Click;
            TimelinePanel.Children.Add(button);

            var frameCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true,
                Visibility = Visibility.Hidden
            };
            var frame = new AnimationFrame { FrameCanvas = frameCanvas };
            frameCanvas.SizeChanged += (sender, e) => ResizeFrameLayers(frame);

            CanvasContainer.Children.Add(frameCanvas);
            frames.Add(frame);

            if (frames.Count == 1)
            {
                frameCanvas.Visibility = Visibility.Visible;
                AddLayerToFrame(frame, "Layer 1");
                UpdateLayersList(0);
            }

            ReindexFrames();
            DrawTimelineGrid();
            DrawKeyframes();
            MoveCursorToFrame(activeFrameIndex);
        }

        private void ReindexFrames()
        {
            for (int i = 0; i < TimelinePanel.Children.Count; i++)
            {
                if (TimelinePanel.Children[i] is Button button)
                {
                    button.Tag = i;
                    button.Content = (i + 1).ToString();
                }
            }
        }

        private void TimelineFrame_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int index)
            {
                SwitchToFrame(index);
            }
        }

        private void SwitchToFrame(int index)
        {
            if (index < 0 || index >= frames.Count)
            {
                return;
            }

            frames[activeFrameIndex].FrameCanvas.Visibility = Visibility.Hidden;
            frames[index].FrameCanvas.Visibility = Visibility.Visible;
            activeFrameIndex = index;

            UpdateLayersList();
            MoveCursorToFrame(index);

            if (TimelinePanel.Children[index] is Button button)
            {
                button.BringIntoView();
            }

            DrawTimelineGrid();
            DrawKeyframes();
        }

        private void AddFrameBtn_Click(object sender, RoutedEventArgs e)
        {
            AddFrame();
            SwitchToFrame(frames.Count - 1);
        }

        private void RemoveFrameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (frames.Count <= 1)
            {
                return;
            }

            frames[activeFrameIndex].FrameCanvas.Visibility = Visibility.Hidden;
            frames.RemoveAt(activeFrameIndex);
            TimelinePanel.Children.RemoveAt(activeFrameIndex);
            CanvasContainer.Children.RemoveAt(activeFrameIndex);

            activeFrameIndex = ClampFrameIndex(activeFrameIndex);
            ReindexFrames();
            SwitchToFrame(activeFrameIndex);
            RemoveUndoItemsForMissingCanvases();
        }

        private int ClampFrameIndex(int index)
        {
            if (frames.Count == 0)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(frames.Count - 1, index));
        }
    }
}
