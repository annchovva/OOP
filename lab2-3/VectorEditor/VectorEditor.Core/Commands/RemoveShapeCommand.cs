using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorEditor.Core.Commands
{
    public class RemoveShapeCommand : IEditorCommand
    {
        private readonly Layer _layer;
        private readonly List<(IShape Shape, int Index)> _removedShapes = new List<(IShape Shape, int Index)>();

        public RemoveShapeCommand(Layer layer, IEnumerable<IShape> shapes)
        {
            _layer = layer;

            var sortedWithIndices = shapes
                .Select(s => new { Shape = s, Index = _layer.Shapes.IndexOf(s) })
                .Where(x => x.Index >= 0)
                .OrderBy(x => x.Index)
                .ToList();

            foreach (var item in sortedWithIndices)
                _removedShapes.Add((item.Shape, item.Index));
        }

        public void Execute()
        {
            foreach (var item in _removedShapes.AsEnumerable().Reverse())
                _layer.Shapes.Remove(item.Shape);
        }

        public void Unexecute()
        {
            foreach (var item in _removedShapes.OrderBy(x => x.Index))
            {
                int index = Math.Min(item.Index, _layer.Shapes.Count);
                _layer.Shapes.Insert(index, item.Shape);
            }
        }
    }
}
