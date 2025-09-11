using System;

namespace DatingAppBackend.Models
{
    public class User
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }   // NEW for email signup
        public string? Otp { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
