using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;
using VectorEditor.Plugins.BasicShapes.Shapes;

namespace VectorEditor.Plugins.BasicShapes.Tools
{
    public class EllipseTool : ITool
    {
        public string Name => "Эллипс";
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

            double left = Math.Min(_startPoint.X, _currentPoint.X);
            double top = Math.Min(_startPoint.Y, _currentPoint.Y);
            double width = Math.Abs(_currentPoint.X - _startPoint.X);
            double height = Math.Abs(_currentPoint.Y - _startPoint.Y);

            if (width < 1 || height < 1)
                return;

            var ellipse = new EllipseShape
            {
                Center = new Point(left + width / 2, top + height / 2),
                RadiusX = width / 2,
                RadiusY = height / 2,
                StrokeColor = Document.CurrentStrokeColor,
                FillColor = Document.CurrentFillColor,
                StrokeThickness = Document.CurrentStrokeThickness
            };

            Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, ellipse));
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing)
                return;

            double left = Math.Min(_startPoint.X, _currentPoint.X);
            double top = Math.Min(_startPoint.Y, _currentPoint.Y);
            double width = Math.Abs(_currentPoint.X - _startPoint.X);
            double height = Math.Abs(_currentPoint.Y - _startPoint.Y);

            var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dash };
            Point center = new Point(left + width / 2, top + height / 2);

            context.DrawEllipse(null, pen, center, width / 2, height / 2);
        }
    }
}
