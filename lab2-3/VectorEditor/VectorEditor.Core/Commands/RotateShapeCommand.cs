using System;
using System.Collections.Generic;
using System.Linq;
using VectorEditor.Core;

namespace VectorEditor.Core.Commands
{
    public class RotateShapeCommand : IEditorCommand
    {
        private readonly List<IShape> _shapes;
        private readonly List<double> _oldAngles;
        private readonly List<double> _newAngles;

        public RotateShapeCommand(
            IEnumerable<IShape> shapes,
            IEnumerable<double> oldAngles,
            IEnumerable<double> newAngles)
        {
            _shapes = shapes.ToList();
            _oldAngles = oldAngles.ToList();
            _newAngles = newAngles.ToList();

            if (_shapes.Count != _oldAngles.Count || _shapes.Count != _newAngles.Count)
                throw new ArgumentException("Shapes and angle collections must have the same size.");
        }

        public void Execute()
        {
            for (int i = 0; i < _shapes.Count; i++)
                _shapes[i].RotationAngle = _newAngles[i];
        }

        public void Unexecute()
        {
            for (int i = 0; i < _shapes.Count; i++)
                _shapes[i].RotationAngle = _oldAngles[i];
        }
    }
}
