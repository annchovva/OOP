using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core
{
    public class EditorDocument
    {
        public List<Layer> Layers { get; } = new();

        public int ActiveLayerIndex { get; set; } = 0;

        public Layer ActiveLayer => Layers[ActiveLayerIndex];

        public DrawingStyle CurrentStyle { get; set; } = new DrawingStyle();

        public EditorDocument()
        {
            Layers.Add(new Layer { Name = "Layer 1" });
        }
    }
}
