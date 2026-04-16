using System.Drawing;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Figures
{
    public class LineFigure : FigureBase
    {
        public override string FigureType => "Line";

        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public override void Draw(Graphics g)
        {
            using var pen = new Pen(Style.StrokeColor, Style.StrokeWidth);
            g.DrawLine(pen, X1, Y1, X2, Y2);
        }

        public override bool HitTest(PointF point)
        {
            var rect = RectangleF.FromLTRB(
                Math.Min(X1, X2),
                Math.Min(Y1, Y2),
                Math.Max(X1, X2),
                Math.Max(Y1, Y2));

            return rect.Contains(point);
        }

        public override void Move(float dx, float dy)
        {
            X1 += (int)dx;
            Y1 += (int)dy;
            X2 += (int)dx;
            Y2 += (int)dy;
        }

        public override void Resize(RectangleF bounds)
        {
            X1 = (int)bounds.Left;
            Y1 = (int)bounds.Top;
            X2 = (int)bounds.Right;
            Y2 = (int)bounds.Bottom;
        }

        public override IFigure Clone()
        {
            return new LineFigure
            {
                X1 = X1,
                Y1 = Y1,
                X2 = X2,
                Y2 = Y2,
                Rotation = Rotation,
                IsSelected = IsSelected,
                Style = new DrawingStyle
                {
                    FillColor = Style.FillColor,
                    StrokeColor = Style.StrokeColor,
                    StrokeWidth = Style.StrokeWidth
                }
            };
        }

        public override FigureDto ToDto() => new FigureDto { Type = FigureType, Id = Id };

        public override void FromDto(FigureDto dto) { }
    }
}
