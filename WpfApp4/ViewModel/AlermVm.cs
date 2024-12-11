using System.ComponentModel;

namespace WpfApp4.ViewModel
{

    public class AlermVm : INotifyPropertyChanged
    {
        public bool _isok;
        public event PropertyChangedEventHandler PropertyChanged;
        public AlermVm()
        {
            _isok = false;
        }
        public bool isok
        {
            get { return _isok; }
            set
            {
                if (_isok != value)
                {
                    _isok = value;
                    OnPropertyChanged(nameof(isok));
                }
            }
        }
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
