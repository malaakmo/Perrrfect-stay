using System;

namespace PerrrfectStayAPI.Models
{
    public class CafeTable
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
    }

    public class CafeReservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TableId { get; set; }
        public DateTime VisitDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int NumGuests { get; set; }
        public string Status { get; set; } = "confirmed";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCafeReservationRequest
    {
        public int UserId { get; set; }
        public DateTime VisitDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int NumGuests { get; set; }
    }
}
