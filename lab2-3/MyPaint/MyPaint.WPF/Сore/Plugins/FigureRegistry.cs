using System.Collections.Generic;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core.Plugins
{
    public class FigureRegistry
    {
        private readonly Dictionary<string, Type> _types = new();

        public void Register<T>(string figureType) where T : IFigure, new()
        {
            _types[figureType] = typeof(T);
        }

        public IFigure Create(string figureType)
        {
            if (!_types.TryGetValue(figureType, out var type))
                throw new InvalidOperationException($"Unknown figure type: {figureType}");

            return (IFigure)Activator.CreateInstance(type)!;
        }

        public IEnumerable<string> GetRegisteredTypes() => _types.Keys;
    }
}
