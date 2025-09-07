using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DatingAppBackend.Services
{
    public class OtpService
    {
        private readonly HttpClient _http;
        private readonly string _token;
        private readonly string _sender;

        public OtpService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient("sparrow");

            // Prefer environment variables; fall back to appsettings if present.
            _token = Environment.GetEnvironmentVariable("SPARROW_TOKEN")
                     ?? config["SparrowSms:Token"]
                     ?? throw new InvalidOperationException("Sparrow token missing. Set SPARROW_TOKEN env var or appsettings.");

            _sender = Environment.GetEnvironmentVariable("SPARROW_SENDER")
                      ?? config["SparrowSms:Sender"]
                      ?? "YUGMA";
        }

        public async Task SendOtp(string toPhone, string otp)
        {
            var values = new Dictionary<string, string>
            {
                { "token", _token },
                { "from", _sender },
                { "to", toPhone },
                { "text", $"From YUGMA, your OTP for signup is: {otp}" }
            };

            using var content = new FormUrlEncodedContent(values);
            var resp = await _http.PostAsync("https://api.sparrowsms.com/v2/sms/", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Sparrow error {resp.StatusCode}: {body}");
        }
    }
}
