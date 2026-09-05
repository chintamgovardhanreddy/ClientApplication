using ClientApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IEmailService _emailService;

        // Inject the INTERFACE here, not EmailService
        public NotificationController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] string message)
        {
            // Example usage:
            // await _emailService.SendEmailAsync("user@example.com", "Notification", message);
            return Ok(new { Message = "Notification sent successfully!" });
        }
    }
}