using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyPaint.WPF.Core;
using MyPaint.WPF.Models;

namespace MyPaint.WPF
{
    public class DrawingSurface : Control
    {
        public EditorDocument? Document { get; set; }
        public IFigure? PreviewFigure { get; set; }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (Background != null)
                dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (Document == null)
                return;

            foreach (var layer in Document.Layers)
            {
                if (!layer.IsVisible)
                    continue;

                foreach (var figure in layer.Figures)
                    DrawFigure(dc, figure, false);
            }

            if (PreviewFigure != null)
                DrawFigure(dc, PreviewFigure, true);
        }

        private void DrawFigure(DrawingContext dc, IFigure figure, bool preview)
        {
            switch (figure)
            {
                case MyPaint.WPF.Figures.RectangleFigure rect:
                    DrawRectangle(dc, rect, preview);
                    break;
                case MyPaint.WPF.Figures.EllipseFigure ellipse:
                    DrawEllipse(dc, ellipse, preview);
                    break;
                case MyPaint.WPF.Figures.LineFigure line:
                    DrawLine(dc, line, preview);
                    break;
            }
        }

        private void DrawRectangle(DrawingContext dc, MyPaint.WPF.Figures.RectangleFigure r, bool preview)
        {
            var rect = new Rect(r.X, r.Y, r.Width, r.Height);

            if (preview)
            {
                var dashPen = new Pen(Brushes.Gray, 1);
                dashPen.DashStyle = DashStyles.Dash;
                dc.DrawRectangle(null, dashPen, rect);
                return;
            }

            var fill = new SolidColorBrush(Color.FromArgb(r.Style.FillColor.A, r.Style.FillColor.R, r.Style.FillColor.G, r.Style.FillColor.B));
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(r.Style.StrokeColor.A, r.Style.StrokeColor.R, r.Style.StrokeColor.G, r.Style.StrokeColor.B)), r.Style.StrokeWidth);

            fill.Freeze();
            pen.Freeze();

            dc.DrawRectangle(fill, pen, rect);
        }

        private void DrawEllipse(DrawingContext dc, MyPaint.WPF.Figures.EllipseFigure e, bool preview)
        {
            var rect = new Rect(e.X, e.Y, e.Width, e.Height);

            if (preview)
            {
                var dashPen = new Pen(Brushes.Gray, 1);
                dashPen.DashStyle = DashStyles.Dash;
                dc.DrawEllipse(null, dashPen,
                    new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2),
                    rect.Width / 2, rect.Height / 2);
                return;
            }

            var fill = new SolidColorBrush(Color.FromArgb(e.Style.FillColor.A, e.Style.FillColor.R, e.Style.FillColor.G, e.Style.FillColor.B));
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(e.Style.StrokeColor.A, e.Style.StrokeColor.R, e.Style.StrokeColor.G, e.Style.StrokeColor.B)), e.Style.StrokeWidth);

            fill.Freeze();
            pen.Freeze();

            dc.DrawEllipse(fill, pen,
                new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2),
                rect.Width / 2, rect.Height / 2);
        }

        private void DrawLine(DrawingContext dc, MyPaint.WPF.Figures.LineFigure l, bool preview)
        {
            var pen = preview
                ? new Pen(Brushes.Gray, 1)
                : new Pen(new SolidColorBrush(Color.FromArgb(l.Style.StrokeColor.A, l.Style.StrokeColor.R, l.Style.StrokeColor.G, l.Style.StrokeColor.B)), l.Style.StrokeWidth);

            if (preview)
                pen.DashStyle = DashStyles.Dash;

            pen.Freeze();

            dc.DrawLine(pen, new Point(l.X1, l.Y1), new Point(l.X2, l.Y2));
        }
    }
}
