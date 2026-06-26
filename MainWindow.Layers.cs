using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ErsmAnim.Models;

namespace ErsmAnim
{
    public partial class MainWindow
    {
        // New global list of layers — layers now drive per-frame canvases.
        private readonly List<DrawingLayer> layers = new List<DrawingLayer>();

        private void AddLayer(string name)
        {
            var layer = new DrawingLayer
            {
                Name = name
            };

            // For every existing frame, create and attach a canvas for this layer.
            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                var layerCanvas = new Canvas
                {
                    Background = Brushes.Transparent,
                    ClipToBounds = true
                };

                StretchLayerToFrame(layerCanvas, frame.FrameCanvas);

                frame.FrameCanvas.Children.Add(layerCanvas);

                // Keep the per-frame list inside DrawingLayer in sync.
                layer.Canvases.Add(layerCanvas);

                // Keep existing AnimationFrame.Layers for compatibility: add reference to this layer.
                frame.Layers.Add(layer);
            }

            // If there are no frames yet, layer.Canvases will be empty — canvases should be created when frames are added.
            layers.Add(layer);
            activeLayer = layer;

            // Select the new layer in UI
            UpdateLayersList(layers.Count - 1);
        }

        private void StretchLayerToFrame(Canvas layerCanvas, Canvas frameCanvas)
        {
            layerCanvas.Width = frameCanvas.ActualWidth;
            layerCanvas.Height = frameCanvas.ActualHeight;
        }

        private void ResizeFrameLayers(AnimationFrame frame, int frameIndex)
        {
            // Resize all layer canvases that correspond to this frame index.
            foreach (var layer in layers)
            {
                var c = layer.GetCanvasForFrameIndex(frameIndex);
                if (c != null)
                {
                    StretchLayerToFrame(c, frame.FrameCanvas);
                }
            }
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            AddLayer("Layer " + (layers.Count + 1));
        }

        private void RemoveLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (layers.Count == 0 || LayersList.SelectedIndex < 0)
            {
                return;
            }

            int removedIndex = LayersList.SelectedIndex;
            var layer = layers[removedIndex];

            // Remove this layer's canvases from every frame and remove the layer reference from each frame's Layers list
            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                // Remove canvas if it exists in the frame's visual tree
                var c = layer.GetCanvasForFrameIndex(i);
                if (c != null && frame.FrameCanvas.Children.Contains(c))
                {
                    frame.FrameCanvas.Children.Remove(c);
                }

                // Remove the layer reference from the frame's Layers list if present
                if (frame.Layers.Contains(layer))
                {
                    frame.Layers.Remove(layer);
                }
            }

            layers.RemoveAt(removedIndex);

            int nextIndex = System.Math.Max(0, removedIndex - 1);
            UpdateLayersList(nextIndex);
        }

        private void UpdateLayersList(int selectedIndex = 0)
        {
            LayersList.Items.Clear();

            if (layers.Count == 0)
            {
                activeLayer = null;
                return;
            }

            foreach (var layer in layers)
            {
                LayersList.Items.Add(layer);
            }

            selectedIndex = System.Math.Max(0, System.Math.Min(layers.Count - 1, selectedIndex));
            LayersList.SelectedIndex = selectedIndex;
            activeLayer = layers[selectedIndex];
        }

        private void LayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayersList.SelectedIndex >= 0)
            {
                activeLayer = layers[LayersList.SelectedIndex];
            }
        }

        private void LayerVisibilityChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is DrawingLayer layer)
            {
                // Toggle visibility for the whole layer (all frames)
                layer.IsVisible = checkBox.IsChecked == true;
            }
        }
    }
}
