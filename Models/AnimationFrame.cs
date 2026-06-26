using System.Collections.Generic;
using System.Windows.Controls;

namespace ErsmAnim.Models
{
    public class AnimationFrame
    {
        public Canvas FrameCanvas { get; set; }

        public List<DrawingLayer> Layers { get; } = new List<DrawingLayer>();
    }
}
