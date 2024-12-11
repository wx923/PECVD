using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WpfApp4.Models
{
    #region 枚举定义
    // 舟位置枚举
    public enum BoatPosition
    {
        CarArea,         // 存储为 "CarArea"
        StorageArea1,    // 存储为 "StorageArea1"
        StorageArea2,    // 存储为 "StorageArea2"
        ClampArea,       // 存储为 "ClampArea"
        FurnaceArea      // 存储为 "FurnaceArea"
        }
    #endregion

    // 舟类
    [BsonIgnoreExtraElements]
    public partial class Boat : ObservableObject
    {
        #region 属性
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        private BoatPosition _currentPosition;
        public BoatPosition CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (SetProperty(ref _currentPosition, value))
                {
                    IsModified = true;
                }
            }
        }

        private string _monitorBoatNumber;
        public string MonitorBoatNumber
        {
            get => _monitorBoatNumber;
            set
            {
                if (SetProperty(ref _monitorBoatNumber, value))
                {
                    IsModified = true;
                }
            }
        }

        [BsonIgnore]
        public bool IsModified { get; set; }
        #endregion

        #region 构造函数
        public Boat()
        {
            _id = ObjectId.GenerateNewId().ToString();
            CurrentPosition = BoatPosition.CarArea;
            MonitorBoatNumber = string.Empty;
            IsModified = false;
        }
        #endregion
    }

    // 舟监控类
    [BsonIgnoreExtraElements]
    public partial class BoatMonitor : ObservableObject
    {
        #region 属性
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        private string _boatNumber;
        public string BoatNumber
        {
            get => _boatNumber;
            set => SetProperty(ref _boatNumber, value);
        }

        private DateTime? _processStartTime;
        public DateTime? ProcessStartTime
        {
            get => _processStartTime;
            set => SetProperty(ref _processStartTime, value);
        }

        private DateTime? _processEndTime;
        public DateTime? ProcessEndTime
        {
            get => _processEndTime;
            set => SetProperty(ref _processEndTime, value);
        }

        private int _processCount;
        public int ProcessCount
        {
            get => _processCount;
            set => SetProperty(ref _processCount, value);
        }

        private string _currentProcess;
        public string CurrentProcess
        {
            get => _currentProcess;
            set => SetProperty(ref _currentProcess, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private bool _isSubmitted;
        public bool IsSubmitted
        {
            get => _isSubmitted;
            set => SetProperty(ref _isSubmitted, value);
        }
        #endregion

        #region 构造函数
        public BoatMonitor()
        {
            _id = ObjectId.GenerateNewId().ToString();
            ProcessCount = 0;
            CurrentProcess = string.Empty;
            BoatNumber = string.Empty;
            IsSelected = false;
            IsSubmitted = false;
        }
        #endregion
    }
} 