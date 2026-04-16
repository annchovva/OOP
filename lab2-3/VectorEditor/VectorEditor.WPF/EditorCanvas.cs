using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VectorEditor.Core;

namespace VectorEditor.WPF
{
    public class EditorCanvas : FrameworkElement
    {
        private EditorDocument? _document;

        public EditorDocument? Document
        {
            get => _document;
            set
            {
                if (_document?.Layers != null)
                    _document.Layers.CollectionChanged -= OnLayersChanged;

                _document = value;

                if (_document?.Layers != null)
                    _document.Layers.CollectionChanged += OnLayersChanged;

                InvalidateVisual();
            }
        }

        public ITool? CurrentTool { get; set; }

        public EditorCanvas()
        {
            ClipToBounds = true;
            Focusable = true;
        }

        private void OnLayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();
            CurrentTool?.OnMouseDown(e.GetPosition(this), e);
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            CurrentTool?.OnMouseMove(e.GetPosition(this), e);
            InvalidateVisual();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            CurrentTool?.OnMouseUp(e.GetPosition(this), e);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (Document?.Layers == null)
                return;

            foreach (var layer in Document.Layers)
            {
                if (!layer.IsVisible)
                    continue;

                foreach (var shape in layer.Shapes)
                    shape.Draw(drawingContext);
            }

            CurrentTool?.OnDrawPreview(drawingContext);
        }
    }
}
