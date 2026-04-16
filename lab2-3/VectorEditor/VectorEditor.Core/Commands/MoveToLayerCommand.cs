using System.Collections.Generic;
using System.Linq;

namespace VectorEditor.Core.Commands
{
    public class MoveToLayerCommand : IEditorCommand
    {
        private readonly Layer _fromLayer;
        private readonly Layer _toLayer;
        private readonly List<IShape> _shapes;
        private readonly Dictionary<IShape, int> _originalIndices = new Dictionary<IShape, int>();

        public MoveToLayerCommand(Layer from, Layer to, IEnumerable<IShape> shapes)
        {
            _fromLayer = from;
            _toLayer = to;
            _shapes = shapes.ToList();

            foreach (var shape in _shapes)
            {
                _originalIndices[shape] = _fromLayer.Shapes.IndexOf(shape);
            }
        }

        public void Execute()
        {
            foreach (var shape in _shapes)
            {
                _fromLayer.Shapes.Remove(shape);
                _toLayer.Shapes.Add(shape);
            }
        }

        public void Unexecute()
        {
            foreach (var shape in _shapes)
            {
                _toLayer.Shapes.Remove(shape);
            }

            foreach (var shape in _shapes.OrderBy(s => _originalIndices[s]))
            {
                int index = _originalIndices[shape];
                if (index >= 0 && index <= _fromLayer.Shapes.Count)
                    _fromLayer.Shapes.Insert(index, shape);
                else
                    _fromLayer.Shapes.Add(shape);
            }
        }
    }
}
