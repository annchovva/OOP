using System.Collections.Generic;

namespace MyPaint.WPF.Core.Serialization
{
    public class DocumentDto
    {
        public List<LayerDto> Layers { get; set; } = new();
        public int ActiveLayerIndex { get; set; }
    }
}
