using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;
using VectorEditor.Plugins.BasicShapes.Shapes;

namespace VectorEditor.Plugins.BasicShapes.Tools
{
    public class RectangleTool : ITool
    {
        public string Name => "Прямоугольник";
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

            var rect = new RectangleShape
            {
                Point1 = _startPoint,
                Point2 = _currentPoint,
                StrokeColor = Document.CurrentStrokeColor,
                FillColor = Document.CurrentFillColor,
                StrokeThickness = Document.CurrentStrokeThickness
            };

            Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, rect));
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing)
                return;

            var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dash };
            Rect previewRect = new Rect(_startPoint, _currentPoint);
            context.DrawRectangle(null, pen, previewRect);
        }
    }
}
