using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyPaint.WPF.Core;
using MyPaint.WPF.Core.Tools;
using MyPaint.WPF.Models;

namespace MyPaint.WPF
{
    public partial class MainWindow : Window
    {
        private readonly EditorDocument _document = new();
        private IShapeTool? _currentTool;
        private bool _isDrawing;

        public MainWindow()
        {
            InitializeComponent();

            Surface.Document = _document;
            _currentTool = new RectangleTool();

            StrokeWidthBox.SelectedIndex = 1; // 2 px
            UpdateStyleButtons();
        }

        private PointF GetPoint(MouseEventArgs e)
        {
            var p = e.GetPosition(Surface);
            return new PointF((float)p.X, (float)p.Y);
        }

        private void Surface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentTool == null)
                return;

            var p = GetPoint(e);
            _currentTool.Begin(p, _document);
            _isDrawing = true;
            Surface.CaptureMouse();
        }

        private void Surface_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _currentTool == null)
                return;

            var p = GetPoint(e);
            _currentTool.Update(p);
            Surface.PreviewFigure = _currentTool.GetPreview();
            Surface.InvalidateVisual();
        }

        private void Surface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing || _currentTool == null)
                return;

            var p = GetPoint(e);
            var figure = _currentTool.Finish(p);

            if (figure != null)
                _document.ActiveLayer.Figures.Add(figure);

            _isDrawing = false;
            Surface.PreviewFigure = null;
            Surface.ReleaseMouseCapture();
            Surface.InvalidateVisual();
        }

        private void BtnLine_Click(object sender, RoutedEventArgs e)
        {
            _currentTool = new LineTool();
        }

        private void BtnRect_Click(object sender, RoutedEventArgs e)
        {
            _currentTool = new RectangleTool();
        }

        private void BtnEllipse_Click(object sender, RoutedEventArgs e)
        {
            _currentTool = new EllipseTool();
        }

        private void FillColorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.ColorDialog
            {
                Color = _document.CurrentStyle.FillColor
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _document.CurrentStyle.FillColor = dialog.Color;
                UpdateStyleButtons();
            }
        }

        private void StrokeColorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.ColorDialog
            {
                Color = _document.CurrentStyle.StrokeColor
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _document.CurrentStyle.StrokeColor = dialog.Color;
                UpdateStyleButtons();
            }
        }

        private void StrokeWidthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StrokeWidthBox.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out int width))
            {
                _document.CurrentStyle.StrokeWidth = width;
            }
        }

        private void UpdateStyleButtons()
        {
            FillColorButton.Content = ColorToText(_document.CurrentStyle.FillColor);
            FillColorButton.Background = new SolidColorBrush(ToMediaColor(_document.CurrentStyle.FillColor));

            StrokeColorButton.Content = ColorToText(_document.CurrentStyle.StrokeColor);
            StrokeColorButton.Background = new SolidColorBrush(ToMediaColor(_document.CurrentStyle.StrokeColor));
        }

        private static string ColorToText(System.Drawing.Color c)
        {
            return $"{c.R}, {c.G}, {c.B}";
        }

        private static System.Windows.Media.Color ToMediaColor(System.Drawing.Color c)
        {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
