using WarehousePacking.API.Integrations.Email;
using WarehousePacking.Shared.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WarehousePacking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly Serilog.ILogger _logger;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
            _logger = Log.ForContext<EmailController>();
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            _logger.Information("Email send request to {Recipient} with subject {Subject}", request?.To, request?.Subject);

            // Walidacja wejścia
            if (string.IsNullOrWhiteSpace(request.To))
            {
                _logger.Warning("Email send failed: recipient missing");
                return BadRequest("Recipient is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                _logger.Warning("Email send failed: subject missing (to {Recipient})", request.To);
                return BadRequest("Subject is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                _logger.Warning("Email send failed: body missing (to {Recipient})", request.To);
                return BadRequest("Body is required.");
            }

            try
            {
                await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);

                _logger.Information("Email successfully sent to {Recipient} with subject {Subject})", request.To, request.Subject);

                return Ok(new { Message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while sending email to {Recipient} with subject {Subject}", request.To, request.Subject);

                return StatusCode(500, new
                {
                    Message = "Failed to send email.",
                    Error = ex.Message
                });
            }
        }
    }
}