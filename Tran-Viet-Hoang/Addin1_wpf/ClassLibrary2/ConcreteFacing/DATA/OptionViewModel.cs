using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ConcreteFacing.DATA
{
    public class OptionViewModel : INotifyPropertyChanged
    {
        public enum BeamFaces
        {
            Top,
            Bottom,
            Front,
            Back
        }

        public enum ColumnFaces
        {
            Front,
            Back,
            Left,
            Right
        }

        public string CoverFaceContent { get; set; }
        public bool _coverFaceIsCheck { get; set; }

        public bool CoverFaceIsCheck
        {
            get => _coverFaceIsCheck;
            set
            {
                if (_coverFaceIsCheck != value)
                {
                    _coverFaceIsCheck = value;
                    OnPropertyChanged(nameof(CoverFaceIsCheck));
                }
            }
        }

        public BitmapImage CoverFaceImgSource { get; set; }
        public double imgheight { get; set; }
        public double imgwidth { get; set; }

        public OptionViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}