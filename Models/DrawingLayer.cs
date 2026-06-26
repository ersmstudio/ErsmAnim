using System.Windows;
using System.Windows.Controls;

namespace ErsmAnim.Models
{
    public class DrawingLayer
    {
        public string Name { get; set; }

        public Canvas Canvas { get; set; }

        public bool IsVisible
        {
            get => Canvas?.Visibility == Visibility.Visible;
            set
            {
                if (Canvas != null)
                {
                    Canvas.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }
    }
}
