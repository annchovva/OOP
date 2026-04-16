using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;
using VectorEditor.Core.Commands;

namespace VectorEditor.WPF
{
    public enum ResizeHandle
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }

    public class SelectionTool : ITool
    {
        public string Name => "Selection";
        public EditorDocument? Document { get; set; }

        private Point _startPoint;
        private bool _isDragging;
        private bool _isResizing;
        private bool _isRotating;
        private bool _isMarqueeSelecting;
        private bool _didChange;

        private ResizeHandle _activeHandle = ResizeHandle.None;

        private readonly List<IShape> _selectedShapes = new();
        private readonly List<Rect> _startBounds = new();
        private readonly List<double> _startAngles = new();

        private Rect _selectionRect;
        private Rect _resizeStartSelectionBounds;
        private Rect _resizeCurrentSelectionBounds;

        private Point _rotationCenter;
        private double _rotationStartAngle;

        private const double RotationHandleDistance = 24.0;
        private const double RotationHandleHitRadius = 8.0;

        public void OnMouseDown(Point position, MouseButtonEventArgs e)
        {
            if (Document?.Layers == null)
                return;

            _startPoint = position;
            _didChange = false;

            _isDragging = false;
            _isResizing = false;
            _isRotating = false;
            _isMarqueeSelecting = false;
            _activeHandle = ResizeHandle.None;

            _selectedShapes.Clear();
            _startBounds.Clear();
            _startAngles.Clear();
            _resizeStartSelectionBounds = Rect.Empty;
            _resizeCurrentSelectionBounds = Rect.Empty;

            _selectedShapes.AddRange(GetSelectedShapes());

            if (_selectedShapes.Count > 0)
            {
                if (GetRotationHandleAtPoint(position))
                {
                    _isRotating = true;
                    _rotationCenter = GetSelectionCenter(_selectedShapes);
                    _rotationStartAngle = GetAngle(_rotationCenter, position);
                    _startAngles.AddRange(_selectedShapes.Select(s => s.RotationAngle));
                    return;
                }

                if (GetResizeHandleAtPoint(position, out _, out var handle))
                {
                    _activeHandle = handle;
                    _isResizing = true;

                    _resizeStartSelectionBounds = GetSelectionBounds(_selectedShapes);
                    _resizeCurrentSelectionBounds = _resizeStartSelectionBounds;

                    foreach (var shape in _selectedShapes)
                        _startBounds.Add(shape.GetBounds());

                    return;
                }
            }

            var shapeUnderMouse = HitTestShape(position);

            if (shapeUnderMouse != null)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    shapeUnderMouse.IsSelected = !shapeUnderMouse.IsSelected;
                    _selectedShapes.Clear();
                    _selectedShapes.AddRange(GetSelectedShapes());
                    return;
                }

                if (!shapeUnderMouse.IsSelected)
                {
                    ClearSelection();
                    shapeUnderMouse.IsSelected = true;
                }

                _selectedShapes.Clear();
                _selectedShapes.AddRange(GetSelectedShapes());

                _isDragging = true;
                _startBounds.AddRange(_selectedShapes.Select(s => s.GetBounds()));
            }
            else
            {
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    ClearSelection();

                _selectedShapes.Clear();
                _selectedShapes.AddRange(GetSelectedShapes());

                _isMarqueeSelecting = true;
                _selectionRect = new Rect(position, position);
            }
        }

        public void OnMouseMove(Point position, MouseEventArgs e)
        {
            if (Document?.Layers == null)
                return;

            if (_isDragging && _selectedShapes.Any())
            {
                double dx = position.X - _startPoint.X;
                double dy = position.Y - _startPoint.Y;

                if (Math.Abs(dx) > 0.0 || Math.Abs(dy) > 0.0)
                    _didChange = true;

                foreach (var shape in _selectedShapes)
                    shape.Move(dx, dy);

                _startPoint = position;
            }
            else if (_isRotating && _selectedShapes.Any())
            {
                double currentAngle = GetAngle(_rotationCenter, position);
                double totalDelta = NormalizeAngle(currentAngle - _rotationStartAngle);

                for (int i = 0; i < _selectedShapes.Count; i++)
                    _selectedShapes[i].RotationAngle = _startAngles[i] + totalDelta;

                _didChange = true;
            }
            else if (_isResizing && _selectedShapes.Any())
            {
                UpdateResize(position);
                _didChange = true;
            }
            else if (_isMarqueeSelecting)
            {
                _selectionRect = new Rect(_startPoint, position);
                _didChange = true;
                UpdateMarqueeSelection(Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
            }
        }

        public void OnMouseUp(Point position, MouseButtonEventArgs e)
        {
            if (Document?.Layers == null)
                return;

            if (_isDragging && _didChange && _selectedShapes.Any() && _startBounds.Count == _selectedShapes.Count)
            {
                var endBounds = _selectedShapes.Select(s => s.GetBounds()).ToList();
                var cmd = new MoveShapesByBoundsCommand(_selectedShapes, _startBounds, endBounds);
                Document.History.RegisterExecutedCommand(cmd);
            }

            if (_isRotating && _didChange && _selectedShapes.Any() && _startAngles.Count == _selectedShapes.Count)
            {
                var endAngles = _selectedShapes.Select(s => s.RotationAngle).ToList();
                var cmd = new RotateShapeCommand(_selectedShapes, _startAngles, endAngles);
                Document.History.RegisterExecutedCommand(cmd);
            }

            if (_isResizing && _didChange && _selectedShapes.Any() && _startBounds.Count == _selectedShapes.Count)
            {
                var newBounds = _selectedShapes.Select(s => s.GetBounds()).ToList();
                var cmd = new ResizeShapeCommand(_selectedShapes, _startBounds, newBounds);
                Document.History.RegisterExecutedCommand(cmd);
            }

            if (_isMarqueeSelecting)
                _selectionRect = Rect.Empty;

            _isDragging = false;
            _isResizing = false;
            _isRotating = false;
            _isMarqueeSelecting = false;
            _activeHandle = ResizeHandle.None;
            _resizeStartSelectionBounds = Rect.Empty;
            _resizeCurrentSelectionBounds = Rect.Empty;
            _rotationStartAngle = 0.0;
        }

        public void OnDrawPreview(DrawingContext drawingContext)
        {
            if (_isMarqueeSelecting && !_selectionRect.IsEmpty)
            {
                var marqueePen = new Pen(Brushes.DodgerBlue, 1)
                {
                    DashStyle = DashStyles.Dash
                };

                drawingContext.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(30, 30, 144, 255)),
                    marqueePen,
                    _selectionRect);
            }

            var selected = GetSelectedShapes();
            if (selected.Count == 0)
                return;

            Rect bounds = _isResizing && !_resizeCurrentSelectionBounds.IsEmpty
                ? _resizeCurrentSelectionBounds
                : GetSelectionBounds(selected);

            if (bounds.IsEmpty)
                return;

            var selectionPen = new Pen(Brushes.DodgerBlue, 1)
            {
                DashStyle = DashStyles.Dash
            };

            drawingContext.DrawRectangle(null, selectionPen, bounds);

            foreach (var p in GetSelectionHandlePoints(bounds))
            {
                drawingContext.DrawRectangle(
                    Brushes.White,
                    new Pen(Brushes.DodgerBlue, 1),
                    new Rect(p.X - 4, p.Y - 4, 8, 8));
            }

            var rotationHandle = GetRotationHandlePoint(bounds);
            drawingContext.DrawLine(
                new Pen(Brushes.DodgerBlue, 1),
                new Point(bounds.Left + bounds.Width / 2, bounds.Top),
                rotationHandle);

            drawingContext.DrawEllipse(
                Brushes.White,
                new Pen(Brushes.DodgerBlue, 1),
                rotationHandle,
                5,
                5);
        }

        private void UpdateMarqueeSelection(bool additive)
        {
            if (Document?.Layers == null)
                return;

            Rect rect = NormalizeRect(_selectionRect);

            if (!additive)
                ClearSelection();

            foreach (var shape in Document.Layers.SelectMany(l => l.Shapes))
            {
                if (shape.HitTest(rect.TopLeft) ||
                    shape.HitTest(rect.TopRight) ||
                    shape.HitTest(rect.BottomLeft) ||
                    shape.HitTest(rect.BottomRight) ||
                    rect.Contains(shape.GetBounds()))
                {
                    shape.IsSelected = true;
                }
            }

            _selectedShapes.Clear();
            _selectedShapes.AddRange(GetSelectedShapes());
        }

        private List<IShape> GetSelectedShapes()
        {
            if (Document?.Layers == null)
                return new List<IShape>();

            return Document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();
        }

        private void ClearSelection()
        {
            if (Document?.Layers == null)
                return;

            foreach (var shape in Document.Layers.SelectMany(l => l.Shapes))
                shape.IsSelected = false;
        }

        private IShape? HitTestShape(Point p)
        {
            if (Document?.Layers == null)
                return null;

            foreach (var layer in Document.Layers.Reverse())
            {
                if (!layer.IsVisible)
                    continue;

                for (int i = layer.Shapes.Count - 1; i >= 0; i--)
                {
                    var shape = layer.Shapes[i];
                    if (shape.HitTest(p))
                        return shape;
                }
            }

            return null;
        }

        private bool GetResizeHandleAtPoint(Point p, out IShape? shape, out ResizeHandle handle)
        {
            shape = null;
            handle = ResizeHandle.None;

            foreach (var s in GetSelectedShapes())
            {
                Rect b = s.GetBounds();
                Rect r = b;
                r.Inflate(6, 6);

                var tl = new Point(r.Left, r.Top);
                var tc = new Point(r.Left + r.Width / 2, r.Top);
                var tr = new Point(r.Right, r.Top);
                var mr = new Point(r.Right, r.Top + r.Height / 2);
                var br = new Point(r.Right, r.Bottom);
                var bc = new Point(r.Left + r.Width / 2, r.Bottom);
                var bl = new Point(r.Left, r.Bottom);
                var ml = new Point(r.Left, r.Top + r.Height / 2);

                if (IsNear(p, tl)) { shape = s; handle = ResizeHandle.TopLeft; return true; }
                if (IsNear(p, tc)) { shape = s; handle = ResizeHandle.Top; return true; }
                if (IsNear(p, tr)) { shape = s; handle = ResizeHandle.TopRight; return true; }
                if (IsNear(p, mr)) { shape = s; handle = ResizeHandle.Right; return true; }
                if (IsNear(p, br)) { shape = s; handle = ResizeHandle.BottomRight; return true; }
                if (IsNear(p, bc)) { shape = s; handle = ResizeHandle.Bottom; return true; }
                if (IsNear(p, bl)) { shape = s; handle = ResizeHandle.BottomLeft; return true; }
                if (IsNear(p, ml)) { shape = s; handle = ResizeHandle.Left; return true; }
            }

            return false;
        }

        private bool GetRotationHandleAtPoint(Point p)
        {
            var selected = GetSelectedShapes();
            if (selected.Count == 0)
                return false;

            var bounds = GetSelectionBounds(selected);
            var rotationHandle = GetRotationHandlePoint(bounds);

            return Distance(p, rotationHandle) <= RotationHandleHitRadius;
        }

        private void UpdateResize(Point current)
        {
            if (_selectedShapes.Count == 0 || _resizeStartSelectionBounds.IsEmpty)
                return;

            Rect newSelection = GetResizedSelectionBounds(_resizeStartSelectionBounds, current, _activeHandle);

            if (newSelection.IsEmpty)
                return;

            _resizeCurrentSelectionBounds = newSelection;

            for (int i = 0; i < _selectedShapes.Count; i++)
            {
                var shape = _selectedShapes[i];
                Rect oldShapeBounds = _startBounds[i];
                Rect newShapeBounds = MapRect(oldShapeBounds, _resizeStartSelectionBounds, _resizeCurrentSelectionBounds);

                shape.SetBounds(newShapeBounds);
            }
        }

        private static Rect GetResizedSelectionBounds(Rect oldBounds, Point current, ResizeHandle handle)
        {
            double left = oldBounds.Left;
            double top = oldBounds.Top;
            double right = oldBounds.Right;
            double bottom = oldBounds.Bottom;

            switch (handle)
            {
                case ResizeHandle.TopLeft:
                    left = current.X;
                    top = current.Y;
                    break;
                case ResizeHandle.Top:
                    top = current.Y;
                    break;
                case ResizeHandle.TopRight:
                    right = current.X;
                    top = current.Y;
                    break;
                case ResizeHandle.Right:
                    right = current.X;
                    break;
                case ResizeHandle.BottomRight:
                    right = current.X;
                    bottom = current.Y;
                    break;
                case ResizeHandle.Bottom:
                    bottom = current.Y;
                    break;
                case ResizeHandle.BottomLeft:
                    left = current.X;
                    bottom = current.Y;
                    break;
                case ResizeHandle.Left:
                    left = current.X;
                    break;
            }

            if (right < left)
                (left, right) = (right, left);

            if (bottom < top)
                (top, bottom) = (bottom, top);

            return new Rect(new Point(left, top), new Point(right, bottom));
        }

        private static Rect MapRect(Rect rect, Rect oldBounds, Rect newBounds)
        {
            if (oldBounds.Width == 0 || oldBounds.Height == 0)
                return rect;

            double scaleX = newBounds.Width / oldBounds.Width;
            double scaleY = newBounds.Height / oldBounds.Height;

            double relLeft = rect.Left - oldBounds.Left;
            double relTop = rect.Top - oldBounds.Top;

            return new Rect(
                newBounds.Left + relLeft * scaleX,
                newBounds.Top + relTop * scaleY,
                rect.Width * scaleX,
                rect.Height * scaleY);
        }

        private static Rect NormalizeRect(Rect rect)
        {
            double x1 = Math.Min(rect.Left, rect.Right);
            double y1 = Math.Min(rect.Top, rect.Bottom);
            double x2 = Math.Max(rect.Left, rect.Right);
            double y2 = Math.Max(rect.Top, rect.Bottom);

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        private static Rect GetSelectionBounds(IEnumerable<IShape> shapes)
        {
            var list = shapes.ToList();
            if (list.Count == 0)
                return Rect.Empty;

            double minX = list.Min(s => s.GetBounds().Left);
            double minY = list.Min(s => s.GetBounds().Top);
            double maxX = list.Max(s => s.GetBounds().Right);
            double maxY = list.Max(s => s.GetBounds().Bottom);

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        private static Point GetSelectionCenter(IEnumerable<IShape> shapes)
        {
            var bounds = GetSelectionBounds(shapes);
            if (bounds.IsEmpty)
                return new Point(0, 0);

            return new Point(
                bounds.Left + bounds.Width / 2,
                bounds.Top + bounds.Height / 2);
        }

        private static IEnumerable<Point> GetSelectionHandlePoints(Rect bounds)
        {
            yield return new Point(bounds.Left, bounds.Top);
            yield return new Point(bounds.Left + bounds.Width / 2, bounds.Top);
            yield return new Point(bounds.Right, bounds.Top);
            yield return new Point(bounds.Right, bounds.Top + bounds.Height / 2);
            yield return new Point(bounds.Right, bounds.Bottom);
            yield return new Point(bounds.Left + bounds.Width / 2, bounds.Bottom);
            yield return new Point(bounds.Left, bounds.Bottom);
            yield return new Point(bounds.Left, bounds.Top + bounds.Height / 2);
        }

        private static Point GetRotationHandlePoint(Rect bounds)
        {
            return new Point(
                bounds.Left + bounds.Width / 2,
                bounds.Top - RotationHandleDistance);
        }

        private static bool IsNear(Point a, Point b, double tolerance = 6.0)
        {
            return Math.Abs(a.X - b.X) <= tolerance && Math.Abs(a.Y - b.Y) <= tolerance;
        }

        private static double GetAngle(Point center, Point p)
        {
            return Math.Atan2(p.Y - center.Y, p.X - center.X) * 180.0 / Math.PI;
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle <= -180.0)
                angle += 360.0;

            while (angle > 180.0)
                angle -= 360.0;

            return angle;
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
