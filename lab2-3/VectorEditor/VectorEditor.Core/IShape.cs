using System;
using System.Windows;
using System.Windows.Media;

namespace VectorEditor.Core
{
    public interface IShape
    {
        Guid Id { get; }

        Color StrokeColor { get; set; }
        Color FillColor { get; set; }
        double StrokeThickness { get; set; }

        double RotationAngle { get; set; }
        bool IsSelected { get; set; }

        void Draw(DrawingContext context);
        bool HitTest(Point point);

        void Move(double dx, double dy);
        Rect GetBounds();
        void SetBounds(Rect bounds);
        void Resize(Rect oldBounds, Rect newBounds);

        IShape Clone();
    }
}
