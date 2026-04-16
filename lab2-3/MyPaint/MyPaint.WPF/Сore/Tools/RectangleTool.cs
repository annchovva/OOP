using System.Drawing;
using MyPaint.WPF.Figures;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core.Tools
{
    public class RectangleTool : IShapeTool
    {
        private PointF _start;
        private PointF _current;
        private EditorDocument? _document;

        public string Name => "Rectangle";

        public void Begin(PointF start, EditorDocument document)
        {
            _start = start;
            _current = start;
            _document = document;
        }

        public void Update(PointF current)
        {
            _current = current;
        }

        public IFigure? GetPreview()
        {
            if (_document == null)
                return null;

            int x = (int)Math.Min(_start.X, _current.X);
            int y = (int)Math.Min(_start.Y, _current.Y);
            int w = (int)Math.Abs(_current.X - _start.X);
            int h = (int)Math.Abs(_current.Y - _start.Y);

            if (w <= 0 || h <= 0)
                return null;

            return new RectangleFigure
            {
                X = x,
                Y = y,
                Width = w,
                Height = h,
                Style = new DrawingStyle
                {
                    FillColor = _document.CurrentStyle.FillColor,
                    StrokeColor = _document.CurrentStyle.StrokeColor,
                    StrokeWidth = _document.CurrentStyle.StrokeWidth
                }
            };
        }

        public IFigure? Finish(PointF end)
        {
            Update(end);
            return GetPreview();
        }

        public void Cancel()
        {
        }
    }
}
