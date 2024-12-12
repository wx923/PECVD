using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WpfApp4.Models
{
    public partial class ProcessExcelModel : ObservableObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [ObservableProperty]
        private string fileId = string.Empty;

        // 时间参数
        [ObservableProperty]
        private int time;  // 工艺步骤时间

        // 温度设定值 T1-T9
        [ObservableProperty]
        private int t1;

        [ObservableProperty]
        private int t2;

        [ObservableProperty]
        private int t3;

        [ObservableProperty]
        private int t4;

        [ObservableProperty]
        private int t5;

        [ObservableProperty]
        private int t6;

        [ObservableProperty]
        private int t7;

        [ObservableProperty]
        private int t8;

        [ObservableProperty]
        private int t9;

        // 气体流量设定值
        [ObservableProperty]
        private int n2;

        [ObservableProperty]
        private int siH4;

        [ObservableProperty]
        private int n2O;

        [ObservableProperty]
        private int h2;

        [ObservableProperty]
        private int ph3;

        // 压力设定值
        [ObservableProperty]
        private int pressure;

        // 功率设定值
        [ObservableProperty]
        private int power1;

        [ObservableProperty]
        private int power2;

        // 运动参数
        [ObservableProperty]
        private int boatDirection;  // 进出舟方向（0不动，1:出舟, 2:进舟）

        [ObservableProperty]
        private int moveSpeed;  // 平移速度

        [ObservableProperty]
        private int upDownSpeed;  // 上下速度

        // 加热参数
        [ObservableProperty]
        private int heatTime;  // 预热时间

        [ObservableProperty]
        private int heatTemp;  // 预热温度

        // 脉冲参数
        [ObservableProperty]
        private int pulseOn1;  // 脉冲开启时间1

        [ObservableProperty]
        private int pulseOff1;  // 脉冲关闭时间1

        [ObservableProperty]
        private int pulseOn2;  // 脉冲开启时间2

        [ObservableProperty]
        private int pulseOff2;  // 脉冲关闭时间2

        // 电流电压参考值和限制值
        [ObservableProperty]
        private int currentReference;  // 电流参考值(A)

        [ObservableProperty]
        private int currentLimit;  // 电流卡控值(A)

        [ObservableProperty]
        private int voltageReference;  // 电压参考值(V)

        [ObservableProperty]
        private int voltageLimit;  // 电压卡控值(V)
    }

    public partial class ProcessFileInfo : ObservableObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [ObservableProperty]
        public string fileName = string.Empty;

        [ObservableProperty]
        public DateTime createTime = DateTime.Now;

        [ObservableProperty]
        public string description = string.Empty;

        [ObservableProperty]
        public string collectionName = string.Empty;

        [ObservableProperty]
        public bool isSelected;
    }
} 