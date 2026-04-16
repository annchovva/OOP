using System.Collections.Generic;

namespace VectorEditor.Core
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }

        IEnumerable<ITool> GetTools();
    }
}
