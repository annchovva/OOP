using System;
using System.Collections.Generic;

namespace MyPaint.WPF.Models
{
    public class FigureDto
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }

        public Dictionary<string, string> Data { get; set; } = new();
    }
}
