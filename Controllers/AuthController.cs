using Microsoft.AspNetCore.Mvc;
using DatingAppBackend.Models;
using DatingAppBackend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;

namespace DatingAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly OtpService _otpService;       // SMS OTP
        private readonly EmailService _emailService;   // Email OTP

        // In-memory dictionaries (keep phone and email separate for clarity)
        private static readonly Dictionary<string, User> phoneUsers = new();
        private static readonly Dictionary<string, User> emailUsers = new();

        public AuthController(OtpService otpService, EmailService emailService)
        {
            _otpService = otpService;
            _emailService = emailService;
        }

        // ----------------------------
        // PHONE OTP (existing)
        // ----------------------------

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                return BadRequest(new { message = "Phone number is required" });

            string otp = new Random().Next(1000, 10000).ToString(); // 4-digit
            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(2);
            phoneUsers[user.PhoneNumber] = user;

            await _otpService.SendOtp(user.PhoneNumber, otp);
            return Ok(new { message = "OTP sent to phone" });
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber) || string.IsNullOrWhiteSpace(user.Otp))
                return BadRequest(new { message = "Phone and OTP are required" });

            if (phoneUsers.TryGetValue(user.PhoneNumber, out var saved) &&
                saved.Otp == user.Otp &&
                saved.OtpExpiry > DateTime.UtcNow)
            {
                return Ok(new { message = "Phone OTP verified" });
            }

            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        // ----------------------------
        // EMAIL OTP (new)
        // ----------------------------

        [HttpPost("send-email-otp")]
        public async Task<IActionResult> SendEmailOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { message = "Email is required" });

            if (!IsValidEmail(user.Email))
                return BadRequest(new { message = "Invalid email format" });

            string otp = new Random().Next(1000, 10000).ToString(); // 4-digit
            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(2);
            emailUsers[user.Email] = user;

            await _emailService.SendOtpEmail(user.Email, otp);
            return Ok(new { message = "OTP sent to email" });
        }

        [HttpPost("verify-email-otp")]
        public IActionResult VerifyEmailOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Otp))
                return BadRequest(new { message = "Email and OTP are required" });

            if (!emailUsers.TryGetValue(user.Email, out var saved))
                return BadRequest(new { message = "No OTP requested for this email" });

            if (saved.Otp == user.Otp && saved.OtpExpiry > DateTime.UtcNow)
                return Ok(new { message = "Email OTP verified" });

            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        // Simple email validator
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
