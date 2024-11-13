using System.ComponentModel;
using System.IO;

namespace FIGLet.VisualStudioExtension.UI
{
    public class FontInfo : INotifyPropertyChanged
    {
        private string _name;
        private int _height;
        private int _baseline;
        private int _maxLength;
        private SmushingRules _smushingRules;
        private string _filePath;
        private FIGFont _font;

        public FontInfo(string filePath)
        {
            _filePath = filePath;
            _font = FIGFont.FromFile(filePath);
            _name = Path.GetFileNameWithoutExtension(filePath);
            _height = _font.Height;
            _baseline = _font.Baseline;
            _maxLength = _font.MaxLength;
            _smushingRules = _font.SmushingRules;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height == value) return;
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        public int Baseline
        {
            get => _baseline;
            set
            {
                if (_baseline == value) return;
                _baseline = value;
                OnPropertyChanged(nameof(Baseline));
            }
        }

        public int MaxLength
        {
            get => _maxLength;
            set
            {
                if (_maxLength == value) return;
                _maxLength = value;
                OnPropertyChanged(nameof(MaxLength));
            }
        }

        public SmushingRules SmushingRules
        {
            get => _smushingRules;
            set
            {
                if (_smushingRules == value) return;
                _smushingRules = value;
                OnPropertyChanged(nameof(SmushingRules));
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value) return;
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public FIGFont Font => _font;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}