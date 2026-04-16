using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Media;
using VectorEditor.Core.Commands;

namespace VectorEditor.Core
{
    public class EditorDocument
    {
        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        [JsonIgnore]
        public CommandManager History { get; } = new CommandManager();

        [JsonIgnore]
        public Layer? ActiveLayer { get; set; }

        public Color CurrentStrokeColor { get; set; } = Colors.Black;
        public Color CurrentFillColor { get; set; } = Colors.Transparent;
        public double CurrentStrokeThickness { get; set; } = 2.0;

        public EditorDocument()
        {
            Layers.Add(new Layer("Слой 1"));
            ActiveLayer = Layers[0];
        }

        public string GenerateUniqueLayerName()
        {
            int maxNumber = 0;

            foreach (var layer in Layers)
            {
                if (layer.Name.StartsWith("Слой ", StringComparison.OrdinalIgnoreCase))
                {
                    var tail = layer.Name.Substring(5).Trim();
                    if (int.TryParse(tail, out int number))
                        maxNumber = Math.Max(maxNumber, number);
                }
            }

            return $"Слой {maxNumber + 1}";
        }

        public void Save(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filePath, json);
        }

        public static EditorDocument? Load(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string json = File.ReadAllText(filePath);
            var doc = JsonConvert.DeserializeObject<EditorDocument>(json, settings);

            if (doc == null)
                return null;

            doc.EnsureValidState();
            return doc;
        }

        private void EnsureValidState()
        {
            if (Layers == null || Layers.Count == 0)
            {
                Layers = new ObservableCollection<Layer>
                {
                    new Layer("Слой 1")
                };
            }

            ActiveLayer = Layers.FirstOrDefault();

            if (CurrentStrokeColor == default)
                CurrentStrokeColor = Colors.Black;

            if (CurrentFillColor == default)
                CurrentFillColor = Colors.Transparent;

            if (CurrentStrokeThickness <= 0)
                CurrentStrokeThickness = 2.0;
        }
    }
}
