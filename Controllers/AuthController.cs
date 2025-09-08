using Microsoft.AspNetCore.Mvc;
using DatingAppBackend.Models;
using DatingAppBackend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly OtpService _otpService;
        private static readonly Dictionary<string, User> users = new();

        public AuthController(OtpService otpService)
        {
            _otpService = otpService;
        }

        // STEP 1: Send OTP
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                return BadRequest(new { success = false, message = "Phone number is required" });

            string otp = new Random().Next(1000, 9999).ToString();
            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(2);
            users[user.PhoneNumber] = user;

            try
            {
                await _otpService.SendOtp(user.PhoneNumber, otp);
                return Ok(new { success = true, message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to send OTP: {ex.Message}" });
            }
        }

        // STEP 2: Verify OTP
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber) || string.IsNullOrWhiteSpace(user.Otp))
                return BadRequest(new { success = false, message = "Phone and OTP are required" });

            if (users.TryGetValue(user.PhoneNumber, out var saved) &&
                saved.Otp == user.Otp &&
                saved.OtpExpiry > DateTime.UtcNow)
            {
                return Ok(new { success = true, message = "OTP verified successfully" });
            }

            return BadRequest(new { success = false, message = "Invalid or expired OTP" });
        }
    }
}
