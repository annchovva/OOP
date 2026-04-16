using System.Drawing;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Models
{
    public abstract class FigureBase : IFigure
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public abstract string FigureType { get; }

        public bool IsSelected { get; set; }
        public DrawingStyle Style { get; set; } = new DrawingStyle();
        public float Rotation { get; set; }

        public abstract void Draw(Graphics g);
        public abstract bool HitTest(PointF point);
        public abstract void Move(float dx, float dy);
        public abstract void Resize(RectangleF bounds);
        public abstract IFigure Clone();
        public abstract FigureDto ToDto();
        public abstract void FromDto(FigureDto dto);
    }
}
