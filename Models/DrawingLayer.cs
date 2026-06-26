using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ErsmAnim.Models
{
    public class DrawingLayer : INotifyPropertyChanged
    {
        public string Name { get; set; }

        // Per-frame canvases owned by the layer.
        public List<Canvas> Canvases { get; } = new List<Canvas>();

        // Compatibility pointer to the currently active frame canvas (kept in sync by MainWindow).
        private Canvas _canvas;
        public Canvas Canvas
        {
            get => _canvas;
            set
            {
                if (_canvas == value) return;
                _canvas = value;
                OnPropertyChanged(nameof(Canvas));
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool IsVisible
        {
            get
            {
                // Prefer the explicit current Canvas if set
                if (Canvas != null)
                    return Canvas.Visibility == Visibility.Visible;

                // Otherwise consider per-frame canvases: visible if any frame canvas is visible
                return Canvases.Any(c => c != null && c.Visibility == Visibility.Visible);
            }
            set
            {
                // Apply visibility to explicit pointer and all per-frame canvases
                if (Canvas != null)
                {
                    Canvas.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                }

                foreach (var c in Canvases)
                {
                    if (c != null)
                        c.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                }

                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public Canvas CreateCanvasForFrame(Canvas frameContainer)
        {
            var layerCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };

            Canvases.Add(layerCanvas);
            frameContainer.Children.Add(layerCanvas);
            OnPropertyChanged(nameof(Canvases));
            return layerCanvas;
        }

        public Canvas GetCanvasForFrameIndex(int frameIndex)
        {
            if (frameIndex >= 0 && frameIndex < Canvases.Count)
                return Canvases[frameIndex];
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
