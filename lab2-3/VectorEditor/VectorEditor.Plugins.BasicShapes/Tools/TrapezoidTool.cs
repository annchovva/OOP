using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;
using VectorEditor.Plugins.BasicShapes.Shapes;

namespace VectorEditor.Plugins.BasicShapes.Tools
{
    public class TrapezoidTool : ITool
    {
        public string Name => "Трапеция";
        public EditorDocument? Document { get; set; }

        private Point _startPoint;
        private Point _currentPoint;
        private bool _isDrawing;

        public void OnMouseDown(Point position, MouseButtonEventArgs e)
        {
            if (Document?.ActiveLayer == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            _isDrawing = true;
            _startPoint = position;
            _currentPoint = position;
        }

        public void OnMouseMove(Point position, MouseEventArgs e)
        {
            if (_isDrawing)
                _currentPoint = position;
        }

        public void OnMouseUp(Point position, MouseButtonEventArgs e)
        {
            if (!_isDrawing || Document?.ActiveLayer == null)
                return;

            _isDrawing = false;
            _currentPoint = position;

            if (Math.Abs(_currentPoint.X - _startPoint.X) < 1 ||
                Math.Abs(_currentPoint.Y - _startPoint.Y) < 1)
            {
                return;
            }

            var shape = new TrapezoidShape
            {
                Point1 = _startPoint,
                Point2 = _currentPoint,
                StrokeColor = Document.CurrentStrokeColor,
                FillColor = Document.CurrentFillColor,
                StrokeThickness = Document.CurrentStrokeThickness
            };

            Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, shape));
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing)
                return;

            Rect previewRect = new Rect(_startPoint, _currentPoint);
            if (previewRect.Width <= 0 || previewRect.Height <= 0)
                return;

            var pen = new Pen(Brushes.Gray, 1)
            {
                DashStyle = DashStyles.Dash
            };

            DrawPreviewTrapezoid(context, previewRect, pen);
        }

        private void DrawPreviewTrapezoid(DrawingContext context, Rect rect, Pen pen)
        {
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

            context.DrawGeometry(null, pen, geometry);
        }
    }
}
