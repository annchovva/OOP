using System.Collections.Generic;
using System.Linq;

namespace VectorEditor.Core.Commands
{
    public class MoveShapeCommand : IEditorCommand
    {
        private readonly List<IShape> _shapes;
        private readonly double _dx;
        private readonly double _dy;

        public MoveShapeCommand(IEnumerable<IShape> shapes, double dx, double dy)
        {
            _shapes = shapes.ToList();
            _dx = dx;
            _dy = dy;
        }

        public void Execute()
        {
            foreach (var shape in _shapes)
                shape.Move(_dx, _dy);
        }

        public void Unexecute()
        {
            foreach (var shape in _shapes)
                shape.Move(-_dx, -_dy);
        }
    }
}
