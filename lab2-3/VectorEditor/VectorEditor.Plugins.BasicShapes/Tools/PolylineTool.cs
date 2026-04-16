using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;
using VectorEditor.Plugins.BasicShapes.Shapes;

namespace VectorEditor.Plugins.BasicShapes.Tools
{
    public class PolylineTool : ITool
    {
        public string Name => "Ломаная";
        public EditorDocument? Document { get; set; }

        private readonly List<Point> _points = new();
        private Point _currentMousePos;
        private bool _isDrawing;

        public void OnMouseDown(Point position, MouseButtonEventArgs e)
        {
            if (Document?.ActiveLayer == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ClickCount == 2)
            {
                FinishDrawing();
                return;
            }

            if (!_isDrawing)
                _isDrawing = true;

            _points.Add(position);
            _currentMousePos = position;
        }

        public void OnMouseMove(Point position, MouseEventArgs e)
        {
            if (_isDrawing)
                _currentMousePos = position;
        }

        public void OnMouseUp(Point position, MouseButtonEventArgs e)
        {
        }

        private void FinishDrawing()
        {
            if (Document?.ActiveLayer == null)
                return;

            if (_points.Count >= 2)
            {
                var polyline = new PolylineShape
                {
                    Points = new List<Point>(_points),
                    StrokeColor = Document.CurrentStrokeColor,
                    StrokeThickness = Document.CurrentStrokeThickness
                };

                Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, polyline));
            }

            _points.Clear();
            _isDrawing = false;
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing || _points.Count == 0)
                return;

            var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dash };

            if (_points.Count == 1)
            {
                context.DrawEllipse(null, pen, _points[0], 2, 2);
                context.DrawLine(pen, _points[0], _currentMousePos);
                return;
            }

            for (int i = 0; i < _points.Count - 1; i++)
                context.DrawLine(pen, _points[i], _points[i + 1]);

            context.DrawLine(pen, _points.Last(), _currentMousePos);
        }
    }
}
