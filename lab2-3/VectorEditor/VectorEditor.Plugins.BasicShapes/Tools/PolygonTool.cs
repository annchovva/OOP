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
    public class PolygonTool : ITool
    {
        public string Name => "Полигон";
        public EditorDocument? Document { get; set; }

        private readonly List<Point> _points = new();
        private bool _isDrawing;
        private Point _currentPoint;

        public void OnMouseDown(Point position, MouseButtonEventArgs e)
        {
            if (Document?.ActiveLayer == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            if (!_isDrawing)
            {
                _isDrawing = true;
                _points.Clear();
                _points.Add(position);
                _currentPoint = position;
                return;
            }

            if (e.ClickCount == 2)
            {
                FinishPolygon();
                return;
            }

            _points.Add(position);
            _currentPoint = position;
        }

        public void OnMouseMove(Point position, MouseEventArgs e)
        {
            if (_isDrawing)
                _currentPoint = position;
        }

        public void OnMouseUp(Point position, MouseButtonEventArgs e)
        {
        }

        public void OnDrawPreview(DrawingContext context)
        {
            if (!_isDrawing || _points.Count == 0)
                return;

            var pen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyles.Dash };

            if (_points.Count == 1)
            {
                context.DrawEllipse(null, pen, _points[0], 2, 2);
                context.DrawLine(pen, _points[0], _currentPoint);
                return;
            }

            var tempPoints = new List<Point>(_points) { _currentPoint };

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(tempPoints[0], true, true);
                ctx.PolyLineTo(tempPoints.Skip(1).ToList(), true, true);
            }

            context.DrawGeometry(null, pen, geometry);
        }

        private void FinishPolygon()
        {
            if (Document?.ActiveLayer == null)
                return;

            if (_points.Count < 3)
            {
                _points.Clear();
                _isDrawing = false;
                return;
            }

            var polygon = new PolygonShape
            {
                Points = new List<Point>(_points),
                StrokeColor = Document.CurrentStrokeColor,
                FillColor = Document.CurrentFillColor,
                StrokeThickness = Document.CurrentStrokeThickness
            };

            Document.History.Execute(new AddShapeCommand(Document.ActiveLayer, polygon));

            _points.Clear();
            _isDrawing = false;
        }
    }
}
