using System;

namespace VectorEditor.Core.Commands
{
    public class AddLayerCommand : IEditorCommand
    {
        private readonly EditorDocument _document;
        private readonly Layer _layer;
        private readonly int _index;
        private readonly Layer? _oldActiveLayer;

        public AddLayerCommand(EditorDocument document, Layer layer, int index)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _layer = layer ?? throw new ArgumentNullException(nameof(layer));
            _index = index;
            _oldActiveLayer = _document.ActiveLayer;
        }

        public void Execute()
        {
            if (_index >= 0 && _index <= _document.Layers.Count)
                _document.Layers.Insert(_index, _layer);
            else
                _document.Layers.Add(_layer);

            _document.ActiveLayer = _layer;
        }

        public void Unexecute()
        {
            _document.Layers.Remove(_layer);
            _document.ActiveLayer = _oldActiveLayer;
        }
    }
}
