using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Services;

namespace WpfApp4.ViewModel
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly PlcCommunicationService _plcService;
        public ObservableCollection<PlcStatusViewModel> PlcStatuses { get; } = new();

        public HomePageViewModel()
        {
            _plcService = PlcCommunicationService.Instance;
            InitializePlcStatuses();
        }

        private void InitializePlcStatuses()
        {
            // 直接使用PlcCommunicationService中的连接状态
            foreach (var plcState in _plcService.ConnectionStates)
            {
                string name;
                if (plcState.Key == PlcCommunicationService.PlcType.Motion)
                {
                    name = "运动PLC";
                }
                else
                {
                    int furnaceNumber = (int)plcState.Key + 1;
                    name = $"炉管{furnaceNumber}";
                }

                PlcStatuses.Add(new PlcStatusViewModel(name, plcState.Key));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PlcStatusViewModel : INotifyPropertyChanged
    {
        private readonly PlcCommunicationService _plcService;

        public string Name { get; }
        public PlcCommunicationService.PlcType PlcType { get; }

        public bool IsConnected => _plcService.IsConnected(PlcType);

        public ICommand ConnectCommand { get; }

        public PlcStatusViewModel(string name, PlcCommunicationService.PlcType plcType)
        {
            Name = name;
            PlcType = plcType;
            _plcService = PlcCommunicationService.Instance;
            
            ConnectCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    if (IsConnected)
                    {
                        _plcService.Disconnect(PlcType);
                    }
                    else
                    {
                        var result = await _plcService.ConnectAsync(PlcType);
                    }
                }
                catch (Exception ex)
                {
                }
            });

            _plcService.ConnectionStateChanged += OnPlcConnectionStateChanged;
        }

        private void OnPlcConnectionStateChanged(object sender, (PlcCommunicationService.PlcType PlcType, bool IsConnected) e)
        {
            if (e.PlcType == this.PlcType)
            {
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 