using WarehousePacking.API.Services.Auth;
using WarehousePacking.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WarehousePacking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly Serilog.ILogger _logger;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _logger = Log.ForContext<AuthController>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            _logger.Information("Login attempt for user {@Username}", login?.Username);

            try
            {
                string username = await _authService.Login(login);

                if (!string.IsNullOrEmpty(username))
                {
                    _logger.Information("User {@Username} logged in successfully", username);
                }
                else
                {
                    _logger.Warning("Invalid login credentials for user {@Username}", login?.Username);
                    return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");
                }

                return Ok(username);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Bad request while logging in user {@Username}", login?.Username);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled error while logging in user {@Username}", login?.Username);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-logged-users")]
        public async Task<IActionResult> GetLoggedOperators()
        {
            _logger.Information("Fetching logged users list");

            try
            {
                var items = await _authService.GetLoggedUsersAsync();
                _logger.Information("Fetched {Count} logged users", items?.Count());
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Bad request while fetching logged users");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled error while fetching logged users");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] string username)
        {
            _logger.Information("Logout request for user {@Username}", username);

            try
            {
                bool success = await _authService.LogoutAsync(username);

                if (success)
                    _logger.Information("User {@Username} logged out successfully", username);
                else
                    _logger.Warning("Logout failed for user {@Username}", username);

                return Ok(success);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Bad request during logout for user {@Username}", username);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled error during logout for user {@Username}", username);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("validate-password")]
        public async Task<IActionResult> ValidatePassword([FromQuery] string password)
        {
            _logger.Information("Password validation requested");

            try
            {
                bool isValid = await _authService.ValidatePasswordAsync(password);
                _logger.Information("Password validation result: {IsValid}", isValid);
                return Ok(isValid);
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Bad request during password validation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled error during password validation");
                return StatusCode(500, ex.Message);
            }
        }
    }
}