using KontrolaPakowania.API.Services;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace KontrolaPakowania.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
                return BadRequest("Recipient is required.");
            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest("Subject is required.");
            if (string.IsNullOrWhiteSpace(request.Body))
                return BadRequest("Body is required.");

            try
            {
                await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
                return Ok(new { Message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                // You can log the exception here
                return StatusCode(500, new { Message = "Failed to send email.", Error = ex.Message });
            }
        }
    }
}