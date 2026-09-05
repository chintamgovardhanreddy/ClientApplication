using ClientApplication.Data;
using ClientApplication.model.Auth;
using ClientApplication.model.Entity;
using ClientApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twilio.Jwt.AccessToken;

namespace ClientApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public AuthenticationController(
            ApplicationDbContext context,
            IJwtService jwtService,
            IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
            {
                return BadRequest(new
                {
                    Message = "Email already exists."
                });
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User registered successfully."
            });
        }

        // Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Email == dto.Email &&
                x.Password == dto.Password);

            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new TokenResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(60)
            });
        }

        // Forgot Password
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return BadRequest(new
                {
                    Message = "Email not found."
                });
            }

            user.ResetToken = Guid.NewGuid().ToString();

            user.ResetTokenExpiry =
                DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            string resetLink =
              $"http://localhost:4200/reset-password?token={user.ResetToken}";
            //var resetLink =
            //$"http://192.168.0.5:4200/reset-password" +
            //$"?token={Uri.EscapeDataString(user.ResetToken)}" +
            //$"&email={Uri.EscapeDataString(user.Email)}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"Click the link below to reset your password:<br/><br/><a href='{resetLink}'>{resetLink}</a>");


            return Ok(new
            {
                Message = "Password reset link sent successfully."
            });
        }

        // Reset Password
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.ResetToken == dto.Token &&
                x.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest(new
                {
                    Message = "Invalid or expired token."
                });
            }

            user.Password = dto.NewPassword;
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Password changed successfully."
            });
        }
    }
}