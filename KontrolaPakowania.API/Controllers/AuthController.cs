using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KontrolaPakowania.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                bool isValid = await _authService.Login(login);
                return Ok(isValid);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-logged-users")]
        public async Task<IActionResult> GetLoggedOperators()
        {
            try
            {
                var items = await _authService.GetLoggedUsersAsync();
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] string username)
        {
            try
            {
                bool success = await _authService.LogoutAsync(username);
                return Ok(success);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("validate-password")]
        public async Task<IActionResult> ValidatePassword([FromQuery] string password)
        {
            try
            {
                bool isValid = await _authService.ValidatePasswordAsync(password);
                return Ok(isValid);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}