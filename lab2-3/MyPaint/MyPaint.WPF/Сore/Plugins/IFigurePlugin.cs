using MyPaint.WPF.Models;
using MyPaint.WPF.Сore.Plugins;

namespace MyPaint.WPF.Core.Plugins
{
    public interface IFigurePlugin
    {
        string PluginName { get; }
        IEnumerable<string> SupportedFigureTypes { get; }

        void Register(FigureRegistry registry);
    }
}
