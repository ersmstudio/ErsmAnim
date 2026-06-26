using System.Windows;
using System.Windows.Controls;

namespace ErsmAnim.Models
{
    public class DrawingAction
    {
        public DrawingAction(Canvas layerCanvas, UIElement element)
        {
            LayerCanvas = layerCanvas;
            Element = element;
        }

        public Canvas LayerCanvas { get; }

        public UIElement Element { get; }
    }
}
