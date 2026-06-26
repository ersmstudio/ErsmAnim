using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ErsmAnim.Models;

namespace ErsmAnim
{
    public partial class MainWindow
    {
        private void AddLayerToFrame(AnimationFrame frame, string name)
        {
            var layerCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };
            StretchLayerToFrame(layerCanvas, frame.FrameCanvas);

            frame.FrameCanvas.Children.Add(layerCanvas);

            var layer = new DrawingLayer
            {
                Name = name,
                Canvas = layerCanvas
            };

            frame.Layers.Add(layer);
            activeLayer = layer;
        }

        private void StretchLayerToFrame(Canvas layerCanvas, Canvas frameCanvas)
        {
            layerCanvas.Width = frameCanvas.ActualWidth;
            layerCanvas.Height = frameCanvas.ActualHeight;
        }

        private void ResizeFrameLayers(AnimationFrame frame)
        {
            foreach (var layer in frame.Layers)
            {
                StretchLayerToFrame(layer.Canvas, frame.FrameCanvas);
            }
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (frames.Count == 0)
            {
                return;
            }

            var frame = frames[activeFrameIndex];
            AddLayerToFrame(frame, "Layer " + (frame.Layers.Count + 1));
            UpdateLayersList(frame.Layers.Count - 1);
        }

        private void RemoveLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (frames.Count == 0 || LayersList.SelectedIndex < 0)
            {
                return;
            }

            var frame = frames[activeFrameIndex];
            if (frame.Layers.Count <= 1)
            {
                return;
            }

            int removedIndex = LayersList.SelectedIndex;
            frame.FrameCanvas.Children.Remove(frame.Layers[removedIndex].Canvas);
            frame.Layers.RemoveAt(removedIndex);

            int nextIndex = System.Math.Max(0, removedIndex - 1);
            UpdateLayersList(nextIndex);
            RemoveUndoItemsForMissingCanvases();
        }

        private void UpdateLayersList(int selectedIndex = 0)
        {
            LayersList.Items.Clear();

            if (frames.Count == 0)
            {
                activeLayer = null;
                return;
            }

            var layers = frames[activeFrameIndex].Layers;
            foreach (var layer in layers)
            {
                LayersList.Items.Add(layer);
            }

            if (layers.Count == 0)
            {
                activeLayer = null;
                return;
            }

            selectedIndex = System.Math.Max(0, System.Math.Min(layers.Count - 1, selectedIndex));
            LayersList.SelectedIndex = selectedIndex;
            activeLayer = layers[selectedIndex];
        }

        private void LayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayersList.SelectedIndex >= 0 && frames.Count > 0)
            {
                activeLayer = frames[activeFrameIndex].Layers[LayersList.SelectedIndex];
            }
        }

        private void LayerVisibilityChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is DrawingLayer layer)
            {
                layer.IsVisible = checkBox.IsChecked == true;
            }
        }
    }
}
