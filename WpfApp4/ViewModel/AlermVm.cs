using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp4.ViewModel
{

    public class AlermVm : INotifyPropertyChanged
    {
        public bool _isok;
        private bool _isAutoMode;
        public event PropertyChangedEventHandler PropertyChanged;
        public AlermVm()
        {
            _isok = false;
            ClearRunningLogCommand = new RelayCommand(ExecuteClearRunningLog);
            ClearAlarmLogCommand = new RelayCommand(ExecuteClearAlarmLog);
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
        public bool IsAutoMode
        {
            get => _isAutoMode;
            set
            {
                _isAutoMode = value;
                OnPropertyChanged(nameof(IsAutoMode));
            }
        }

        public ICommand ClearRunningLogCommand { get; }
        public ICommand ClearAlarmLogCommand { get; }

        private void ExecuteClearRunningLog()
        {
            // 实现清除运行日志的逻辑
        }

        private void ExecuteClearAlarmLog()
        {
            // 实现清除报警日志的逻辑
        }

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
