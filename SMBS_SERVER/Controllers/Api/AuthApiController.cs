using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Repositories;
using SMBS_SERVER.Services;

namespace SMBS_SERVER.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly JwtService _jwtService;
        private readonly PendingUserRepository _pendingRepo;
        private readonly OtpService _otpService;
        private readonly UserService _userService;


        public AuthApiController(
            AuthenticationService authService,
            JwtService jwtService, PendingUserRepository pendingRepo,
    OtpService otpService,
    UserService userService)
        {
            _authService = authService;
            _jwtService = jwtService;
            _pendingRepo = pendingRepo;
            _otpService = otpService;
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            Console.WriteLine("🔥 API AUTH LOGIN HIT");
            Console.WriteLine($"USER: {dto.Email}");

            var result = _authService.Login(dto.Email, dto.Password, tenantId: 1);

            if (!result.Success)
                return Unauthorized(result.Message);

            var token = _jwtService.GenerateToken(result.User);

            return Ok(new
            {
                token = token,
                userId = result.User.UserID,
                roleId = result.User.RoleID
            });
        }
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok("API ALIVE");
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Check if user already exists
            var existingUser = _userService.GetByEmail(dto.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            // Hash password
            string hashedPassword = PasswordHasher.Hash(dto.Password);

            // Save pending registration
            await _pendingRepo.Save(new PendingUserRegistration
            {
                Email = dto.Email,
                PasswordHash = hashedPassword
            });

            // Send OTP
            await _otpService.SendOtp(dto.Email);

            return Ok("OTP sent to email");
        }
        [HttpPost("verify-registration")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyRegistration(OtpVerifyDto dto)
        {
            Console.WriteLine("🔥 VERIFY HIT");
            Console.WriteLine("EMAIL FROM FLUTTER: '" + dto.Email + "'");
            Console.WriteLine("OTP FROM FLUTTER: '" + dto.Otp + "'");

            bool valid = await _otpService.VerifyOtp(dto.Email, dto.Otp);

            if (!valid)
                return BadRequest("Invalid or expired OTP");

            var pending = await _pendingRepo.GetByEmail(dto.Email);

            if (pending == null)
                return BadRequest("Registration expired");

            // Create real user
            _userService.CreateUserFromRegistration(
                pending.Email,
                pending.PasswordHash);

            await _pendingRepo.Delete(pending);

            return Ok("Registration successful");
        }


    }
}
