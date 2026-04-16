using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VectorEditor.Core;

namespace VectorEditor.Core.Commands
{
    public class MoveShapesByBoundsCommand : IEditorCommand
    {
        private readonly List<IShape> _shapes;
        private readonly List<Rect> _oldBounds;
        private readonly List<Rect> _newBounds;

        public MoveShapesByBoundsCommand(
            IEnumerable<IShape> shapes,
            IEnumerable<Rect> oldBounds,
            IEnumerable<Rect> newBounds)
        {
            _shapes = shapes.ToList();
            _oldBounds = oldBounds.ToList();
            _newBounds = newBounds.ToList();

            if (_shapes.Count != _oldBounds.Count || _shapes.Count != _newBounds.Count)
                throw new ArgumentException("Shapes and bounds collections must have the same size.");
        }

        public void Execute()
        {
            for (int i = 0; i < _shapes.Count; i++)
                _shapes[i].SetBounds(_newBounds[i]);
        }

        public void Unexecute()
        {
            for (int i = 0; i < _shapes.Count; i++)
                _shapes[i].SetBounds(_oldBounds[i]);
        }
    }
}
