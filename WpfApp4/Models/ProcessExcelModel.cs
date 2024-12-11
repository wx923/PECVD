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

        [ObservableProperty]
        private string processName = string.Empty;

        [ObservableProperty]
        private string n2O = string.Empty;

        [ObservableProperty]
        private string h2 = string.Empty;

        [ObservableProperty]
        private string ph3 = string.Empty;

        [ObservableProperty]
        private string pressure = string.Empty;

        [ObservableProperty]
        private string power1 = string.Empty;

        [ObservableProperty]
        private string power2 = string.Empty;

        [ObservableProperty]
        private string boatInOut = string.Empty;

        [ObservableProperty]
        private string moveSpeed = string.Empty;

        [ObservableProperty]
        private string upDownSpeed = string.Empty;

        [ObservableProperty]
        private string heatTime = string.Empty;

        [ObservableProperty]
        private string heatTemp = string.Empty;

        [ObservableProperty]
        private string pulseOn1 = string.Empty;

        [ObservableProperty]
        private string pulseOff1 = string.Empty;

        [ObservableProperty]
        private string pulseOn2 = string.Empty;

        [ObservableProperty]
        private string pulseOff2 = string.Empty;

        [ObservableProperty]
        private string currentVoltage = string.Empty;
    }

    public partial class ProcessFileInfo : ObservableObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [ObservableProperty]
        private string fileName = string.Empty;

        [ObservableProperty]
        private DateTime createTime = DateTime.Now;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string collectionName = string.Empty;
    }
} 