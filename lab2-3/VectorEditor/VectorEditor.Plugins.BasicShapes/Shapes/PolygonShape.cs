using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.Plugins.BasicShapes.Shapes
{
    public class PolygonShape : IShape
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
            if (Points.Count < 3)
                return;

            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(Points[0], true, true);
                gc.PolyLineTo(Points.Skip(1).ToList(), true, true);
            }
            geometry.Freeze();

            var pen = new Pen(new SolidColorBrush(StrokeColor), StrokeThickness);
            var brush = new SolidColorBrush(FillColor);

            context.PushTransform(new RotateTransform(RotationAngle, center.X, center.Y));
            context.DrawGeometry(brush, pen, geometry);
            context.Pop();
        }

        public bool HitTest(Point point)
        {
            if (Points.Count < 3)
                return false;

            Rect bounds = GetBounds();
            Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

            var transform = new RotateTransform(-RotationAngle, center.X, center.Y);
            Point localPoint = transform.Transform(point);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(Points[0], true, true);
                gc.PolyLineTo(Points.Skip(1).ToList(), true, true);
            }
            geometry.Freeze();

            bool hitFill = FillColor.A > 0 && geometry.FillContains(localPoint);
            bool hitStroke = geometry.StrokeContains(
                new Pen(Brushes.Black, StrokeThickness + 6),
                localPoint);

            return hitFill || hitStroke;
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
            {
                Points = new List<Point>
                {
                    new Point(newBounds.Left, newBounds.Top),
                    new Point(newBounds.Right, newBounds.Top),
                    new Point(newBounds.Right, newBounds.Bottom)
                };
                return;
            }

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
            return new PolygonShape
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
