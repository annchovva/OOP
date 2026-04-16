using System;

namespace VectorEditor.Core.Commands
{
    public class RemoveLayerCommand : IEditorCommand
    {
        private readonly EditorDocument _document;
        private readonly Layer _layer;
        private readonly int _index;
        private readonly Layer? _oldActiveLayer;

        public RemoveLayerCommand(EditorDocument document, Layer layer)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _layer = layer ?? throw new ArgumentNullException(nameof(layer));

            _index = _document.Layers.IndexOf(_layer);
            if (_index < 0)
                throw new ArgumentException("Layer is not part of document.");

            _oldActiveLayer = _document.ActiveLayer;
        }

        public void Execute()
        {
            _document.Layers.RemoveAt(_index);

            if (_document.ActiveLayer == _layer)
            {
                if (_document.Layers.Count > 0)
                {
                    int newIndex = _index;
                    if (newIndex >= _document.Layers.Count)
                        newIndex = _document.Layers.Count - 1;

                    _document.ActiveLayer = _document.Layers[newIndex];
                }
                else
                {
                    _document.ActiveLayer = null;
                }
            }
        }

        public void Unexecute()
        {
            _document.Layers.Insert(_index, _layer);
            _document.ActiveLayer = _oldActiveLayer;
        }
    }
}
