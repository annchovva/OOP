namespace VectorEditor.Core.Commands
{
    public class AddShapeCommand : IEditorCommand
    {
        private readonly Layer _layer;
        private readonly IShape _shape;

        public AddShapeCommand(Layer layer, IShape shape)
        {
            _layer = layer;
            _shape = shape;
        }

        public void Execute()
        {
            if (!_layer.Shapes.Contains(_shape))
                _layer.Shapes.Add(_shape);
        }

        public void Unexecute()
        {
            _layer.Shapes.Remove(_shape);
        }
    }
}
