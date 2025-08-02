using desco_report_server.DTOs.Auth;
using desco_report_server.Models;
using desco_report_server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace desco_report_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                var existingUsername = await _userManager.FindByNameAsync(registerDto.Username);
                if (existingUsername != null)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                var user = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user, roles);

                    var response = new AuthResponseDto
                    {
                        Id = user.Id,
                        Username = user.UserName!,
                        Email = user.Email!,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        Token = token,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        Roles = roles
                    };

                    _logger.LogInformation("User registered successfully: {Email}", user.Email);
                    return Ok(response);
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail) ??
                          await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Account is disabled" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user, roles);

                    var response = new AuthResponseDto
                    {
                        Id = user.Id,
                        Username = user.UserName!,
                        Email = user.Email!,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        Token = token,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        Roles = roles
                    };

                    _logger.LogInformation("User logged in successfully: {Email}", user.Email);
                    return Ok(response);
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpGet("me")]
        public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var response = new AuthResponseDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roles
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}