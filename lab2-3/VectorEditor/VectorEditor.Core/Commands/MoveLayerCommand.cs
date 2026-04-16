using System;
using System.Collections.Generic;

namespace VectorEditor.Core.Commands
{
    public class MoveLayerCommand : IEditorCommand
    {
        private readonly IList<Layer> _layers;
        private readonly int _fromIndex;
        private readonly int _toIndex;

        public MoveLayerCommand(IList<Layer> layers, int fromIndex, int toIndex)
        {
            _layers = layers ?? throw new ArgumentNullException(nameof(layers));
            _fromIndex = fromIndex;
            _toIndex = toIndex;
        }

        public void Execute()
        {
            if (_fromIndex < 0 || _fromIndex >= _layers.Count)
                return;

            var item = _layers[_fromIndex];
            _layers.RemoveAt(_fromIndex);

            int insertIndex = _toIndex;

            if (insertIndex < 0)
                insertIndex = 0;

            if (insertIndex > _layers.Count)
                insertIndex = _layers.Count;

            _layers.Insert(insertIndex, item);
        }

        public void Unexecute()
        {
            int currentIndex = _layers.Count == 0 ? -1 : _layers.IndexOf(GetLayerAtTargetPosition());

            if (currentIndex < 0 || currentIndex >= _layers.Count)
                return;

            var item = _layers[currentIndex];
            _layers.RemoveAt(currentIndex);

            int insertIndex = _fromIndex;

            if (insertIndex < 0)
                insertIndex = 0;

            if (insertIndex > _layers.Count)
                insertIndex = _layers.Count;

            _layers.Insert(insertIndex, item);
        }

        private Layer GetLayerAtTargetPosition()
        {
            int index = _toIndex;

            if (_layers.Count == 0)
                throw new InvalidOperationException("No layers available.");

            if (index < 0)
                index = 0;

            if (index >= _layers.Count)
                index = _layers.Count - 1;

            return _layers[index];
        }
    }
}
