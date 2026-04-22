using Microsoft.AspNetCore.Mvc;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.Services;

namespace WarehousePacking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            _logger.LogInformation("Login attempt for user {Username}", login?.Username);

            try
            {
                string username = await _authService.Login(login);

                if (!string.IsNullOrEmpty(username))
                {
                    _logger.LogInformation("User {Username} logged in successfully", username);
                }
                else
                {
                    _logger.LogWarning("Invalid login credentials for user {Username}", login?.Username);
                    return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");
                }

                return Ok(username);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request while logging in user {Username}", login?.Username);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while logging in user {Username}", login?.Username);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-logged-users")]
        public async Task<IActionResult> GetLoggedOperators()
        {
            _logger.LogInformation("Fetching logged users list");

            try
            {
                var items = await _authService.GetLoggedUsersAsync();
                _logger.LogInformation("Fetched {Count} logged users", items?.Count());
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request while fetching logged users");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while fetching logged users");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] string username)
        {
            _logger.LogInformation("Logout request for user {Username}", username);

            try
            {
                bool success = await _authService.LogoutAsync(username);

                if (success)
                    _logger.LogInformation("User {Username} logged out successfully", username);
                else
                    _logger.LogWarning("Logout failed for user {Username}", username);

                return Ok(success);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request during logout for user {Username}", username);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during logout for user {Username}", username);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("validate-password")]
        public async Task<IActionResult> ValidatePassword([FromQuery] string password)
        {
            _logger.LogInformation("Password validation requested");

            try
            {
                bool isValid = await _authService.ValidatePasswordAsync(password);
                _logger.LogInformation("Password validation result: {IsValid}", isValid);
                return Ok(isValid);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request during password validation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during password validation");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("change-manager-password")]
        public async Task<IActionResult> ChangeManagerPassword([FromBody] string newPassword)
        {
            _logger.LogInformation("Password change requested");

            try
            {
                await _authService.ChangePassword(newPassword);
                _logger.LogInformation("Password change successful");
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request during password change");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during password change");
                return StatusCode(500, ex.Message);
            }
        }
    }
}