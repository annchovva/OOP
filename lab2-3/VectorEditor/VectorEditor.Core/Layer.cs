using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VectorEditor.Core
{
    public class Layer : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private bool _isVisible = true;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IShape> Shapes { get; set; } = new ObservableCollection<IShape>();

        public Layer()
        {
        }

        public Layer(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
