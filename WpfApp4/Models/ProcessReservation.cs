using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class ProcessReservation : ObservableObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [ObservableProperty]
        private string _boatNumber;
        
        [ObservableProperty]
        private string _processName;
        
        [ObservableProperty]
        private DateTime _reservationTime;
        
        [ObservableProperty]
        private bool _isCompleted;

        public ProcessReservation()
        {
            Id = ObjectId.GenerateNewId().ToString();
            BoatNumber = string.Empty;
            ProcessName = string.Empty;
            ReservationTime = DateTime.Now;
            IsCompleted = false;
        }
    }
} 