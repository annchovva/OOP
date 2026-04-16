using System.Collections.Generic;

namespace MyPaint.WPF.Models
{
    public class Layer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Layer";
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        public List<IFigure> Figures { get; set; } = new();

        public Layer Clone()
        {
            var clone = new Layer
            {
                Id = Id,
                Name = Name,
                IsVisible = IsVisible,
                IsLocked = IsLocked
            };

            foreach (var fig in Figures)
                clone.Figures.Add(fig.Clone());

            return clone;
        }
    }
}
