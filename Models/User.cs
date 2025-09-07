namespace DatingAppBackend.Models
{
    public class User
    {
        public string? PhoneNumber { get; set; }
        public string? Otp { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
