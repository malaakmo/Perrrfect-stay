using System;
namespace PurrfectStayAPI.Models
{
    public class SitterProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Bio { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal Rating { get; set; }
        public bool IsAvailable { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class SittingBooking
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int SitterId { get; set; }
        public int CatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "pending";
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSittingBookingRequest
    {
        public int OwnerId { get; set; }
        public int SitterId { get; set; }
        public int CatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Message { get; set; }
    }

    public class CreateReviewRequest
    {
        public int BookingId { get; set; }
        public int ReviewerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}