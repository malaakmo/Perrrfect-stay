using System;

namespace Perrrfect_stay
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
     
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "user";
        public string ProfilePicture { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
