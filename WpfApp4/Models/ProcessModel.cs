using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WpfApp4.Models
{
    // 工艺类
    public partial class Process : ObservableObject
    {
        #region 属性
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [ObservableProperty]
        private string _processId;  // 工艺ID

        [ObservableProperty]
        private string _processName;  // 工艺名称

        [ObservableProperty]
        private string _description;  // 工艺描述

        [ObservableProperty]
        private TimeSpan _standardDuration;  // 标准工艺时长

        [ObservableProperty]
        private double _temperature;  // 工艺温度

        [ObservableProperty]
        private double _pressure;  // 工艺压力

        [ObservableProperty]
        private DateTime _createTime;  // 创建时间

        [ObservableProperty]
        private string _creator;  // 创建者

        [ObservableProperty]
        private bool _isEnabled;  // 是否启用
        #endregion

        #region 构造函数
        public Process()
        {
            Id = ObjectId.GenerateNewId().ToString();
            ProcessId = Guid.NewGuid().ToString();
            CreateTime = DateTime.Now;
            IsEnabled = true;
            ProcessName = string.Empty;
            Description = string.Empty;
            Creator = string.Empty;
            StandardDuration = TimeSpan.FromHours(1);
            Temperature = 25;
            Pressure = 1;
        }
        #endregion
    }
} 