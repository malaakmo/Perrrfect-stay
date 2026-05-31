using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PurrfectStayAPI.Data;
using QRCoder;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace PurrfectStayAPI.Controllers
{
    [Route("api/PDF-QR")]
    [ApiController]
    public class PdfQrController : ControllerBase
    {
        private readonly Database _db;
        public PdfQrController(Database db) { _db = db; }

        // GET api/pdfqr/bookings/user/2/pdf
        [HttpGet("bookings/user/{userId}/pdf")]
        public IActionResult DownloadBookingsPdf(int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT hb.id, r.room_number, hb.check_in, hb.check_out, hb.total_price, hb.status
                FROM hotel_bookings hb
                JOIN rooms r ON hb.room_id = r.id
                WHERE hb.user_id = @userId
                ORDER BY hb.created_at DESC", conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var bookings = new List<dynamic>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new
                {
                    Id = reader.GetInt32("id"),
                    Room = reader.GetString("room_number"),
                    CheckIn = reader.GetDateTime("check_in").ToString("yyyy-MM-dd"),
                    CheckOut = reader.GetDateTime("check_out").ToString("yyyy-MM-dd"),
                    Total = reader.GetDecimal("total_price"),
                    Status = reader.GetString("status")
                });
            }

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("PurrfectStay - Hotel Booking History")
                .SetFontSize(18).SimulateBold());
            doc.Add(new Paragraph($"User ID: {userId} | Generated: {DateTime.Now:yyyy-MM-dd HH:mm}"));
            doc.Add(new Paragraph(" "));

            foreach (var b in bookings)
            {
                doc.Add(new Paragraph($"Booking #{b.Id} | Room {b.Room} | {b.CheckIn} → {b.CheckOut} | €{b.Total} | {b.Status}"));
            }

            if (!bookings.Any())
                doc.Add(new Paragraph("No bookings found."));

            doc.Close();
            return File(ms.ToArray(), "application/pdf", $"bookings_user_{userId}.pdf");
        }

        // GET api/pdfqr/qr?data=transfer:userId:amount
        [HttpGet("qr")]
        public IActionResult GenerateQrCode([FromQuery] int userId, [FromQuery] decimal amount)
        {
            string data = $"purrfectstay://transfer?to={userId}&amount={amount}";
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(10);
            return File(qrBytes, "image/png", $"qr_user_{userId}.png");
        }
    }
}