using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WpfApp4.Models
{
    public partial class FurnaceData : ObservableObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [ObservableProperty]
        private string fileId = string.Empty;

        [ObservableProperty]
        private string processCollectionName = string.Empty;

        [ObservableProperty]
        private float measuredT1;

        [ObservableProperty]
        private float measuredT2;

        [ObservableProperty]
        private float measuredT3;

        [ObservableProperty]
        private float measuredT4;

        [ObservableProperty]
        private float measuredT5;

        [ObservableProperty]
        private float measuredT6;

        [ObservableProperty]
        private float measuredT7;

        [ObservableProperty]
        private float measuredT8;

        [ObservableProperty]
        private float measuredT9;

        [ObservableProperty]
        private float measuredSiH4;

        [ObservableProperty]
        private float measuredN2;

        [ObservableProperty]
        private float measuredN2O;

        [ObservableProperty]
        private float measuredH2;

        [ObservableProperty]
        private float measuredPh3;

        [ObservableProperty]
        private float measuredPressure;

        [ObservableProperty]
        private float measuredPower1;

        [ObservableProperty]
        private float measuredPower2;

        [ObservableProperty]
        private float measuredCurrent;

        [ObservableProperty]
        private float measuredVoltage;

        [ObservableProperty]
        private float pulseFrequency;

        [ObservableProperty]
        private float pulseVoltage;
    }
}