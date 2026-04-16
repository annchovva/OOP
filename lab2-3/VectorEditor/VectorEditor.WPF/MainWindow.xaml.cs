using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;
using VectorEditor.Core;
using VectorEditor.Core.Commands;

namespace VectorEditor.WPF
{
    public partial class MainWindow : Window
    {
        private EditorDocument _document;
        private bool _isDraggingSlider = false;
        private List<object?> _sliderStartValues = new List<object?>();
        private List<IShape> _clipboard = new List<IShape>();

        private readonly List<Color> _paletteColors = new()
        {
            Colors.Black,
            Colors.DimGray,
            Colors.Gray,
            Colors.Silver,
            Colors.White,
            Colors.Red,
            Colors.OrangeRed,
            Colors.Orange,
            Colors.Gold,
            Colors.Yellow,
            Colors.LimeGreen,
            Colors.Green,
            Colors.Cyan,
            Colors.DeepSkyBlue,
            Colors.Blue,
            Colors.MediumBlue,
            Colors.Purple,
            Colors.Magenta,
            Colors.Brown,
            Colors.Pink
        };

        private Color _selectedStrokeColor = Colors.Black;
        private Color _selectedFillColor = Colors.Transparent;

        public MainWindow()
        {
            InitializeComponent();

            _document = new EditorDocument();

            _document.CurrentStrokeColor = _selectedStrokeColor;
            _document.CurrentFillColor = _selectedFillColor;
            _document.CurrentStrokeThickness = ThicknessSlider.Value;

            MainCanvas.Document = _document;
            LayersListBox.ItemsSource = _document.Layers;

            if (_document.Layers.Count > 0)
                LayersListBox.SelectedIndex = 0;

            BuildColorPalettes();
            UpdateColorPreviews();

            LoadPlugins();

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void BuildColorPalettes()
        {
            StrokePalette.Children.Clear();
            FillPalette.Children.Clear();

            foreach (var color in _paletteColors)
            {
                StrokePalette.Children.Add(CreateColorButton(color, (s, e) =>
                {
                    _selectedStrokeColor = color;
                    _document.CurrentStrokeColor = color;
                    UpdateColorPreviews();
                    ApplyColorToSelection(isStroke: true, color);
                    MainCanvas.Focus();
                }));

                FillPalette.Children.Add(CreateColorButton(color, (s, e) =>
                {
                    _selectedFillColor = color;
                    _document.CurrentFillColor = color;
                    UpdateColorPreviews();
                    ApplyColorToSelection(isStroke: false, color);
                    MainCanvas.Focus();
                }));
            }

            var transparentButton = CreateColorButton(Colors.Transparent, (s, e) =>
            {
                _selectedFillColor = Colors.Transparent;
                _document.CurrentFillColor = Colors.Transparent;
                UpdateColorPreviews();
                ApplyColorToSelection(isStroke: false, Colors.Transparent);
                MainCanvas.Focus();
            });

            FillPalette.Children.Insert(0, transparentButton);
        }

        private Button CreateColorButton(Color color, RoutedEventHandler onClick)
        {
            var border = new Border
            {
                Width = 18,
                Height = 18,
                Background = new SolidColorBrush(color),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1)
            };

            var button = new Button
            {
                Content = border,
                Width = 22,
                Height = 22,
                Padding = new Thickness(0),
                Margin = new Thickness(1)
            };

            button.Click += onClick;
            return button;
        }

        private void UpdateColorPreviews()
        {
            StrokePreview.Background = new SolidColorBrush(_selectedStrokeColor);
            FillPreview.Background = new SolidColorBrush(_selectedFillColor);
        }

        private void ApplyColorToSelection(bool isStroke, Color color)
        {
            var selectedShapes = _document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();

            if (!selectedShapes.Any())
                return;

            IEditorCommand? command = null;

            if (isStroke)
            {
                command = new ChangePropertyCommand<Color>(
                    selectedShapes,
                    s => s.StrokeColor,
                    color);
            }
            else
            {
                command = new ChangePropertyCommand<Color>(
                    selectedShapes,
                    s => s.FillColor,
                    color);
            }

            _document.History.Execute(command);
            MainCanvas.InvalidateVisual();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Vector Graphics (*.vdoc)|*.vdoc" };
            if (dialog.ShowDialog() == true)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                };

                string json = JsonConvert.SerializeObject(_document, settings);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Vector Graphics (*.vdoc)|*.vdoc" };
            if (dialog.ShowDialog() == true)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                string json = File.ReadAllText(dialog.FileName);
                var loadedDoc = JsonConvert.DeserializeObject<EditorDocument>(json, settings);

                if (loadedDoc != null)
                {
                    _document = loadedDoc;

                    if (_document.Layers == null || _document.Layers.Count == 0)
                    {
                        _document.Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>
                        {
                            new Layer("Слой 1")
                        };
                    }

                    _document.ActiveLayer = _document.Layers.FirstOrDefault();
                    _document.History.Clear();

                    MainCanvas.Document = _document;
                    LayersListBox.ItemsSource = null;
                    LayersListBox.ItemsSource = _document.Layers;

                    if (_document.Layers.Count > 0)
                        LayersListBox.SelectedIndex = 0;

                    _selectedStrokeColor = _document.CurrentStrokeColor;
                    _selectedFillColor = _document.CurrentFillColor;

                    ThicknessSlider.Value = _document.CurrentStrokeThickness > 0
                        ? _document.CurrentStrokeThickness
                        : 2;

                    UpdateColorPreviews();
                    RefreshToolsDocument();

                    MainCanvas.InvalidateVisual();
                    MainCanvas.Focus();
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_document == null) return;

            var selectedShapes = _document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();

            if (e.Key == Key.Delete && selectedShapes.Any())
            {
                DeleteShapes_Click(null, null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    MenuUndo_Click(null, null);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Y)
                {
                    MenuRedo_Click(null, null);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.C && selectedShapes.Any())
                {
                    _clipboard = selectedShapes.Select(s => (IShape)s.Clone()).ToList();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.V && _clipboard.Any())
                {
                    foreach (var s in _clipboard)
                    {
                        var clone = (IShape)s.Clone();
                        clone.Move(15, 15);

                        if (_document.ActiveLayer != null)
                            _document.History.Execute(new AddShapeCommand(_document.ActiveLayer, clone));
                    }

                    MainCanvas.InvalidateVisual();
                    MainCanvas.Focus();
                    e.Handled = true;
                    return;
                }
            }

            if (selectedShapes.Any())
            {
                double dx = 0, dy = 0;

                if (e.Key == Key.Left) dx = -1;
                else if (e.Key == Key.Right) dx = 1;
                else if (e.Key == Key.Up) dy = -1;
                else if (e.Key == Key.Down) dy = 1;

                if (dx != 0 || dy != 0)
                {
                    _document.History.Execute(new MoveShapeCommand(selectedShapes, dx, dy));
                    MainCanvas.InvalidateVisual();
                    e.Handled = true;
                    return;
                }
            }
        }

        private void LayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayersListBox.SelectedItem is Layer selectedLayer)
                _document.ActiveLayer = selectedLayer;

            MainCanvas.Focus();
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            var newLayer = new Layer(_document.GenerateUniqueLayerName());
            var cmd = new AddLayerCommand(_document, newLayer, _document.Layers.Count);
            _document.History.Execute(cmd);

            LayersListBox.SelectedItem = newLayer;
            MainCanvas.Focus();
            MainCanvas.InvalidateVisual();
        }

        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem is Layer selectedLayer && _document.Layers.Count > 1)
            {
                var cmd = new RemoveLayerCommand(_document, selectedLayer);
                _document.History.Execute(cmd);

                if (_document.ActiveLayer != null)
                    LayersListBox.SelectedItem = _document.ActiveLayer;
            }

            MainCanvas.Focus();
            MainCanvas.InvalidateVisual();
        }

        private void MoveLayerUp_Click(object sender, RoutedEventArgs e)
        {
            int index = LayersListBox.SelectedIndex;
            if (index > 0)
            {
                var cmd = new MoveLayerCommand(_document.Layers, index, index - 1);
                _document.History.Execute(cmd);
                LayersListBox.SelectedIndex = index - 1;
                MainCanvas.InvalidateVisual();
            }

            MainCanvas.Focus();
        }

        private void MoveLayerDown_Click(object sender, RoutedEventArgs e)
        {
            int index = LayersListBox.SelectedIndex;
            if (index < _document.Layers.Count - 1)
            {
                var cmd = new MoveLayerCommand(_document.Layers, index, index + 1);
                _document.History.Execute(cmd);
                LayersListBox.SelectedIndex = index + 1;
                MainCanvas.InvalidateVisual();
            }

            MainCanvas.Focus();
        }

        private void MoveToLayer_Click(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem is Layer targetLayer)
            {
                var selectedShapes = _document.Layers
                    .SelectMany(l => l.Shapes)
                    .Where(s => s.IsSelected)
                    .ToList();

                if (!selectedShapes.Any())
                    return;

                var sourceLayer = _document.Layers.FirstOrDefault(l => l.Shapes.Contains(selectedShapes[0]));

                if (sourceLayer != null && sourceLayer != targetLayer)
                {
                    var command = new MoveToLayerCommand(sourceLayer, targetLayer, selectedShapes);
                    _document.History.Execute(command);
                    MainCanvas.InvalidateVisual();
                }
            }

            MainCanvas.Focus();
        }

        private void ToolsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsListBox.SelectedItem is ITool selectedTool)
            {
                selectedTool.Document = _document;
                MainCanvas.CurrentTool = selectedTool;
                MainCanvas.Focus();
            }
        }

        private void RefreshToolsDocument()
        {
            foreach (var item in ToolsListBox.Items)
            {
                if (item is ITool tool)
                    tool.Document = _document;
            }
        }

        private void LoadPlugins()
        {
            ToolsListBox.Items.Clear();
            ToolsListBox.Items.Add(new SelectionTool { Document = _document });

            string pluginPath = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(pluginPath))
                return;

            foreach (var file in Directory.GetFiles(pluginPath, "*.dll"))
            {
                string fileName = Path.GetFileName(file);
                if (fileName.StartsWith("VectorEditor.Core") || fileName.StartsWith("VectorEditor.WPF"))
                    continue;

                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    var pluginTypes = assembly
                        .GetExportedTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            foreach (var tool in plugin.GetTools())
                            {
                                tool.Document = _document;
                                ToolsListBox.Items.Add(tool);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Plugin load error in {file}: {ex.Message}");
                }
            }

            ToolsListBox.SelectedIndex = 0;
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSlider = true;

            var selectedShapes = _document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();

            _sliderStartValues.Clear();

            string propertyName = (sender == ThicknessSlider) ? "StrokeThickness" : "RotationAngle";

            foreach (var shape in selectedShapes)
            {
                _sliderStartValues.Add(shape.GetType().GetProperty(propertyName)?.GetValue(shape));
            }
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSlider = false;

            var selectedShapes = _document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();

            if (!selectedShapes.Any() || !_sliderStartValues.Any())
            {
                MainCanvas.Focus();
                return;
            }

            if (sender == ThicknessSlider)
            {
                var cmd = new ChangePropertyCommand<double>(
                    selectedShapes,
                    s => s.StrokeThickness,
                    ThicknessSlider.Value,
                    _sliderStartValues);

                _document.History.RegisterExecutedCommand(cmd);
            }
            else if (sender == RotationSlider)
            {
                var cmd = new ChangePropertyCommand<double>(
                    selectedShapes,
                    s => s.RotationAngle,
                    RotationSlider.Value,
                    _sliderStartValues);

                _document.History.RegisterExecutedCommand(cmd);
            }

            MainCanvas.Focus();
        }

        private void Property_Changed(object sender, RoutedEventArgs e)
        {
            if (_document == null) return;

            if (sender == ThicknessSlider)
            {
                _document.CurrentStrokeThickness = ThicknessSlider.Value;
            }

            var selectedShapes = _document.Layers
                .SelectMany(l => l.Shapes)
                .Where(s => s.IsSelected)
                .ToList();

            if (_isDraggingSlider)
            {
                if (!selectedShapes.Any())
                    return;

                foreach (var shape in selectedShapes)
                {
                    if (sender == ThicknessSlider)
                        shape.StrokeThickness = ThicknessSlider.Value;

                    if (sender == RotationSlider)
                        shape.RotationAngle = RotationSlider.Value;
                }

                MainCanvas.InvalidateVisual();
                return;
            }

            if (sender == RotationSlider)
            {
                if (!selectedShapes.Any())
                    return;
            }

            if (sender == ThicknessSlider)
            {
                if (selectedShapes.Any())
                {
                    var cmd = new ChangePropertyCommand<double>(
                        selectedShapes,
                        s => s.StrokeThickness,
                        ThicknessSlider.Value);

                    _document.History.Execute(cmd);
                    MainCanvas.InvalidateVisual();
                }

                MainCanvas.Focus();
                return;
            }

            if (sender == RotationSlider)
            {
                if (selectedShapes.Any())
                {
                    var cmd = new ChangePropertyCommand<double>(
                        selectedShapes,
                        s => s.RotationAngle,
                        RotationSlider.Value);

                    _document.History.Execute(cmd);
                    MainCanvas.InvalidateVisual();
                }

                MainCanvas.Focus();
                return;
            }

            MainCanvas.Focus();
        }

        private void MenuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_document.History.CanUndo)
            {
                _document.History.Undo();
                MainCanvas.InvalidateVisual();
                MainCanvas.Focus();
            }
        }

        private void MenuRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_document.History.CanRedo)
            {
                _document.History.Redo();
                MainCanvas.InvalidateVisual();
                MainCanvas.Focus();
            }
        }

        private void DeleteShapes_Click(object sender, RoutedEventArgs e)
        {
            foreach (var layer in _document.Layers)
            {
                var selectedInLayer = layer.Shapes.Where(s => s.IsSelected).ToList();
                if (selectedInLayer.Any())
                    _document.History.Execute(new RemoveShapeCommand(layer, selectedInLayer));
            }

            MainCanvas.InvalidateVisual();
            MainCanvas.Focus();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
