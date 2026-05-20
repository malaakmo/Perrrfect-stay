using System;

namespace PurrfectStayAPI.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
    }

    public class HotelBooking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CatId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "confirmed";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateHotelBookingRequest
    {
        public int UserId { get; set; }
        public int CatId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public List<int> ExtraServiceIds { get; set; } = new();
    }

    public class ExtraService
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}