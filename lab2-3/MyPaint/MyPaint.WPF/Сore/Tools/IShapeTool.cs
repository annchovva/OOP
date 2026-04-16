using System.Drawing;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core.Tools
{
    public interface IShapeTool
    {
        string Name { get; }
        void Begin(PointF start, EditorDocument document);
        void Update(PointF current);
        IFigure? Finish(PointF end);
        void Cancel();
        IFigure? GetPreview();
    }
}
