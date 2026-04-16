using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.Plugins.BasicShapes.Shapes
{
    public class PolylineShape : IShape
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public double StrokeThickness { get; set; } = 2.0;

        public double RotationAngle { get; set; }
        public bool IsSelected { get; set; }

        public List<Point> Points { get; set; } = new();

        public void Draw(DrawingContext context)
        {
            if (Points.Count < 2)
                return;

            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(Points[0], false, false);
                gc.PolyLineTo(Points.Skip(1).ToList(), true, true);
            }
            geometry.Freeze();

            var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);

            context.PushTransform(new RotateTransform(RotationAngle, center.X, center.Y));
            context.DrawGeometry(null, pen, geometry);
            context.Pop();
        }

        public bool HitTest(Point point)
        {
            if (Points.Count < 2)
                return false;

            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var transform = new RotateTransform(-RotationAngle, center.X, center.Y);
            Point localPoint = transform.Transform(point);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(Points[0], false, false);
                gc.PolyLineTo(Points.Skip(1).ToList(), true, true);
            }
            geometry.Freeze();

            return geometry.StrokeContains(
                new Pen(Brushes.Black, StrokeThickness + 6),
                localPoint);
        }

        public void Move(double dx, double dy)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] = new Point(Points[i].X + dx, Points[i].Y + dy);
        }

        public Rect GetBounds()
        {
            if (Points.Count == 0)
                return Rect.Empty;

            double minX = Points.Min(p => p.X);
            double minY = Points.Min(p => p.Y);
            double maxX = Points.Max(p => p.X);
            double maxY = Points.Max(p => p.Y);

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        public void SetBounds(Rect bounds)
        {
            if (Points.Count == 0)
                return;

            Rect oldBounds = GetBounds();
            Resize(oldBounds, bounds);
        }

        public void Resize(Rect oldBounds, Rect newBounds)
        {
            if (Points.Count == 0)
                return;

            if (oldBounds.Width == 0 || oldBounds.Height == 0)
                return;

            double sx = newBounds.Width / oldBounds.Width;
            double sy = newBounds.Height / oldBounds.Height;

            for (int i = 0; i < Points.Count; i++)
            {
                var p = Points[i];
                Points[i] = new Point(
                    newBounds.Left + (p.X - oldBounds.Left) * sx,
                    newBounds.Top + (p.Y - oldBounds.Top) * sy);
            }
        }

        public IShape Clone()
        {
            return new PolylineShape
            {
                Points = new List<Point>(Points),
                StrokeColor = StrokeColor,
                FillColor = FillColor,
                StrokeThickness = StrokeThickness,
                RotationAngle = RotationAngle,
                IsSelected = IsSelected
            };
        }
    }
}
