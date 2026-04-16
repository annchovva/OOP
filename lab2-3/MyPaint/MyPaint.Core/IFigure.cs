using System.Drawing;

namespace MyPaint.Core
{
    public interface IFigure
    {
        string Name { get; }
        Color Color { get; set; }
        Color BorderColor { get; set; }
        int Thickness { get; set; }
        float Angle { get; set; }


        public int StartX { get; set; }
        public int StartY { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        void OnMouseDown(int x, int y);
        void OnMouseUp(int x, int y, Graphics graphics);
        void OnMouseMove(int x, int y, Graphics graphics);

        void Draw(Graphics graphics);

        bool IsSelected(int x, int y);

        System.Drawing.Rectangle PaintBorderSelected();

        void Move(int delta_x, int delta_y);

        void ChangeStartPoint(int x, int y);
        void ChangeEndPoint(int x, int y);
        IFigure Clone();

    }
}
