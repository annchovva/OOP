using System.Drawing;

namespace MyPaint.WPF.Models
{
    public class DrawingStyle
    {
        public Color FillColor { get; set; } = Color.White;
        public Color StrokeColor { get; set; } = Color.Black;
        public int StrokeWidth { get; set; } = 2;
    }
}
