using System.Collections.Generic;
using MyPaint.WPF.Models;

namespace MyPaint.WPF.Core
{
    public class SelectionState
    {
        public List<IFigure> SelectedFigures { get; } = new();

        public void Clear() => SelectedFigures.Clear();

        public void Select(IFigure figure, bool additive = false)
        {
            if (!additive)
                SelectedFigures.Clear();

            if (!SelectedFigures.Contains(figure))
                SelectedFigures.Add(figure);
        }

        public bool Contains(IFigure figure) => SelectedFigures.Contains(figure);
    }
}
