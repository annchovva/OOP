using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VectorEditor.Core
{
    public interface ITool
    {
        string Name { get; }
        EditorDocument? Document { get; set; }

        void OnMouseDown(Point position, MouseButtonEventArgs e);
        void OnMouseMove(Point position, MouseEventArgs e);
        void OnMouseUp(Point position, MouseButtonEventArgs e);

        void OnDrawPreview(DrawingContext context);
    }
}
