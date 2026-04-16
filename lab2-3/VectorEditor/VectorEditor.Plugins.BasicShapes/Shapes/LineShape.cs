using System;
using System.Windows;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.Plugins.BasicShapes.Shapes
{
    public class LineShape : IShape
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public double StrokeThickness { get; set; } = 2.0;

        public double RotationAngle { get; set; }
        public bool IsSelected { get; set; }

        public void Draw(DrawingContext context)
        {
            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);

            context.PushTransform(new RotateTransform(RotationAngle, center.X, center.Y));
            context.DrawLine(pen, StartPoint, EndPoint);
            context.Pop();
        }

        public bool HitTest(Point point)
        {
            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var transform = new RotateTransform(-RotationAngle, center.X, center.Y);
            Point transformedPoint = transform.Transform(point);

            var lineGeom = new LineGeometry(StartPoint, EndPoint);

            return lineGeom.StrokeContains(
                new Pen(Brushes.Black, StrokeThickness + 6),
                transformedPoint);
        }

        public void Move(double dx, double dy)
        {
            StartPoint = new Point(StartPoint.X + dx, StartPoint.Y + dy);
            EndPoint = new Point(EndPoint.X + dx, EndPoint.Y + dy);
        }

        public IShape Clone()
        {
            return new LineShape
            {
                StartPoint = StartPoint,
                EndPoint = EndPoint,
                StrokeColor = StrokeColor,
                FillColor = FillColor,
                StrokeThickness = StrokeThickness,
                RotationAngle = RotationAngle,
                IsSelected = IsSelected
            };
        }

        public Rect GetBounds()
        {
            double x1 = Math.Min(StartPoint.X, EndPoint.X);
            double y1 = Math.Min(StartPoint.Y, EndPoint.Y);
            double x2 = Math.Max(StartPoint.X, EndPoint.X);
            double y2 = Math.Max(StartPoint.Y, EndPoint.Y);

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        public void SetBounds(Rect bounds)
        {
            StartPoint = new Point(bounds.Left, bounds.Top);
            EndPoint = new Point(bounds.Right, bounds.Bottom);
        }

        public void Resize(Rect oldBounds, Rect newBounds)
        {
            if (oldBounds.Width == 0 || oldBounds.Height == 0)
            {
                SetBounds(newBounds);
                return;
            }

            StartPoint = ScalePoint(StartPoint, oldBounds, newBounds);
            EndPoint = ScalePoint(EndPoint, oldBounds, newBounds);
        }

        private static Point ScalePoint(Point p, Rect oldBounds, Rect newBounds)
        {
            double sx = newBounds.Width / oldBounds.Width;
            double sy = newBounds.Height / oldBounds.Height;

            return new Point(
                newBounds.Left + (p.X - oldBounds.Left) * sx,
                newBounds.Top + (p.Y - oldBounds.Top) * sy);
        }
    }
}
