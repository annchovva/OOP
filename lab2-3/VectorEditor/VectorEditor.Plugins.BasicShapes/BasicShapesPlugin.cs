using System.Collections.Generic;
using VectorEditor.Core;
using VectorEditor.Plugins.BasicShapes.Tools;

namespace VectorEditor.Plugins.BasicShapes
{
    public class BasicShapesPlugin : IPlugin
    {
        public string Name => "Базовые фигуры";

        public string Description => "Плагин, содержащий линию, прямоугольник, эллипс, многоугольник, ломаную и трапецию";

        public IEnumerable<ITool> GetTools()
        {
            yield return new LineTool();
            yield return new RectangleTool();
            yield return new EllipseTool();
            yield return new PolylineTool();
            yield return new PolygonTool();
            yield return new TrapezoidTool();
        }
    }
}
