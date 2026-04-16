using System.Collections.Generic;

namespace MyPaint.WPF.Core.Serialization
{
    public class LayerDto
    {
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; }
        public bool IsLocked { get; set; }
        public List<FigureDto> Figures { get; set; } = new();
    }
}
