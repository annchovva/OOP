using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;
using VectorEditor.Plugins.BasicShapes.Shapes;

namespace VectorEditor.Plugins.BasicShapes.Tools
{
    public class LineTool : ITool
    {
        public string Name => "Линия";
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

            if ((_currentPoint - _startPoint).Length < 1)
                return;

            var line = new LineShape
            {
                StartPoint = _startPoint,
                EndPoint = _currentPoint,
                StrokeColor = Document.CurrentStrokeColor,
                StrokeThickness = Document.CurrentStrokeThickness
            };

            Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, line));
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing)
                return;

            var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dash };
            context.DrawLine(pen, _startPoint, _currentPoint);
        }
    }
}
