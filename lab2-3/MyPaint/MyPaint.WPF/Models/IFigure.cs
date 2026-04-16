using System.Drawing;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Models
{
    public interface IFigure
    {
        Guid Id { get; }
        string FigureType { get; }

        bool IsSelected { get; set; }

        DrawingStyle Style { get; set; }

        float Rotation { get; set; }

        void Draw(Graphics g);
        bool HitTest(PointF point);
        void Move(float dx, float dy);
        void Resize(RectangleF bounds);
        IFigure Clone();

        FigureDto ToDto();
        void FromDto(FigureDto dto);
    }
}
