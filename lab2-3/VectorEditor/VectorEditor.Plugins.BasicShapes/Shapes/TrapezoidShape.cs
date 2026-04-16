using System;
using System.Windows;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.Plugins.BasicShapes.Shapes
{
    public class TrapezoidShape : IShape
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public double StrokeThickness { get; set; } = 2.0;

        public double RotationAngle { get; set; }
        public bool IsSelected { get; set; }

        public Point Point1 { get; set; }
        public Point Point2 { get; set; }

        public void Draw(DrawingContext context)
        {
            Rect rect = GetBounds();
            Point center = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

            var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
            var brush = new SolidColorBrush(FillColor);

            var geometry = BuildGeometry(rect);

            context.PushTransform(new RotateTransform(RotationAngle, center.X, center.Y));
            context.DrawGeometry(brush, pen, geometry);
            context.Pop();
        }

        public bool HitTest(Point point)
        {
            Rect rect = GetBounds();
            Point center = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

            var transform = new RotateTransform(-RotationAngle, center.X, center.Y);
            Point localPoint = transform.Transform(point);

            var geometry = BuildGeometry(rect);

            bool hitFill = FillColor.A > 0 && geometry.FillContains(localPoint);
            bool hitStroke = geometry.StrokeContains(
                new Pen(Brushes.Black, StrokeThickness + 6),
                localPoint);

            return hitFill || hitStroke;
        }

        public void Move(double dx, double dy)
        {
            Point1 = new Point(Point1.X + dx, Point1.Y + dy);
            Point2 = new Point(Point2.X + dx, Point2.Y + dy);
        }

        public Rect GetBounds()
        {
            double x1 = Math.Min(Point1.X, Point2.X);
            double y1 = Math.Min(Point1.Y, Point2.Y);
            double x2 = Math.Max(Point1.X, Point2.X);
            double y2 = Math.Max(Point1.Y, Point2.Y);

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        public void SetBounds(Rect bounds)
        {
            Point1 = new Point(bounds.Left, bounds.Top);
            Point2 = new Point(bounds.Right, bounds.Bottom);
        }

        public void Resize(Rect oldBounds, Rect newBounds)
        {
            if (oldBounds.Width == 0 || oldBounds.Height == 0)
            {
                SetBounds(newBounds);
                return;
            }

            Point1 = ScalePoint(Point1, oldBounds, newBounds);
            Point2 = ScalePoint(Point2, oldBounds, newBounds);
        }

        public IShape Clone()
        {
            return new TrapezoidShape
            {
                Point1 = Point1,
                Point2 = Point2,
                StrokeColor = StrokeColor,
                FillColor = FillColor,
                StrokeThickness = StrokeThickness,
                RotationAngle = RotationAngle,
                IsSelected = IsSelected
            };
        }

        private Geometry BuildGeometry(Rect rect)
        {
            // Делает верхнее основание уже нижнего
            double topFactor = 0.65;
            double topWidth = rect.Width * topFactor;

            double topLeftX = rect.Left + (rect.Width - topWidth) / 2;
            double topRightX = topLeftX + topWidth;

            var geometry = new StreamGeometry();

            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(topLeftX, rect.Top), true, true);
                gc.LineTo(new Point(topRightX, rect.Top), true, false);
                gc.LineTo(new Point(rect.Right, rect.Bottom), true, false);
                gc.LineTo(new Point(rect.Left, rect.Bottom), true, false);
            }

            geometry.Freeze();
            return geometry;
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
