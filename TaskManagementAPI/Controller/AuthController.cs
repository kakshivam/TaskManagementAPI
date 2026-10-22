using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _authService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured during registration", detials = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.LoginAsync(loginDto, ipAddress);
                SetRefreshTokenCookie(response.RefreshToken);       // Optional: store in HTTP-only cookie
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured during login", details = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);
                SetRefreshTokenCookie(response.RefreshToken);     // Optional: update cookie
                return Ok(response);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message ="An error occured while refreshing the token", details= ex.Message});
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]      // Must be authenticated to revoke
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var success = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

                if(!success)
                {
                    return BadRequest(new { message = "Token is invalid or already revoked" });
                }

                return Ok(new { message = "Token Revoke Successfully" });
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while revoking the token", details = ex.Message });
            }
        }

        private string GetIpAddress()
        {
            // Get IP Address from request
            if(Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,     // Cannot be access by javascript
                Expires = DateTime.UtcNow.AddDays(7),        // Match refresh token expiry
                Secure = true,       // Only sent over HTTPS
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh-token"     // Only sent to refresh endpoint
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
