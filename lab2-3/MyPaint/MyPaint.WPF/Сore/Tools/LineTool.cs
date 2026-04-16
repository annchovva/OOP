using System.Drawing;
using MyPaint.WPF.Figures;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core.Tools
{
    public class LineTool : IShapeTool
    {
        private PointF _start;
        private PointF _current;
        private EditorDocument? _document;

        public string Name => "Line";

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

            if (_start == _current)
                return null;

            return new LineFigure
            {
                X1 = (int)_start.X,
                Y1 = (int)_start.Y,
                X2 = (int)_current.X,
                Y2 = (int)_current.Y,
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
