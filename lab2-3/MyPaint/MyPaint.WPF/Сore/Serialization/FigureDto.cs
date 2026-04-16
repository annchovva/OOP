using System.Collections.Generic;

namespace MyPaint.WPF.Core.Serialization
{
    public class FigureDto
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }

        public Dictionary<string, object> Data { get; set; } = new();
    }
}
