using System;
using System.Windows;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.Plugins.BasicShapes.Shapes
{
    public class EllipseShape : IShape
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Point Center { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }

        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public double StrokeThickness { get; set; } = 2.0;

        public double RotationAngle { get; set; }
        public bool IsSelected { get; set; }

        public void Draw(DrawingContext context)
        {
            var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
            var brush = new SolidColorBrush(FillColor);

            context.PushTransform(new RotateTransform(RotationAngle, Center.X, Center.Y));
            context.DrawEllipse(brush, pen, Center, RadiusX, RadiusY);
            context.Pop();
        }

        public bool HitTest(Point point)
        {
            var transform = new RotateTransform(-RotationAngle, Center.X, Center.Y);
            Point localPoint = transform.Transform(point);

            var ellipseGeom = new EllipseGeometry(Center, RadiusX, RadiusY);

            bool hitStroke = ellipseGeom.StrokeContains(
                new Pen(Brushes.Black, StrokeThickness + 6),
                localPoint);

            bool hitFill = FillColor.A > 0 && ellipseGeom.FillContains(localPoint);

            return hitStroke || hitFill;
        }

        public IShape Clone()
        {
            return new EllipseShape
            {
                Center = Center,
                RadiusX = RadiusX,
                RadiusY = RadiusY,
                StrokeColor = StrokeColor,
                FillColor = FillColor,
                StrokeThickness = StrokeThickness,
                RotationAngle = RotationAngle,
                IsSelected = IsSelected
            };
        }

        public void Move(double dx, double dy)
        {
            Center = new Point(Center.X + dx, Center.Y + dy);
        }

        public Rect GetBounds()
        {
            return new Rect(
                Center.X - RadiusX,
                Center.Y - RadiusY,
                RadiusX * 2,
                RadiusY * 2);
        }

        public void SetBounds(Rect bounds)
        {
            Center = new Point(
                bounds.Left + bounds.Width / 2,
                bounds.Top + bounds.Height / 2);

            RadiusX = Math.Max(0, bounds.Width / 2);
            RadiusY = Math.Max(0, bounds.Height / 2);
        }

        public void Resize(Rect oldBounds, Rect newBounds)
        {
            if (oldBounds.Width == 0 || oldBounds.Height == 0)
            {
                SetBounds(newBounds);
                return;
            }

            double sx = newBounds.Width / oldBounds.Width;
            double sy = newBounds.Height / oldBounds.Height;

            Center = new Point(
                newBounds.Left + (Center.X - oldBounds.Left) * sx,
                newBounds.Top + (Center.Y - oldBounds.Top) * sy);

            RadiusX *= sx;
            RadiusY *= sy;
        }
    }
}
