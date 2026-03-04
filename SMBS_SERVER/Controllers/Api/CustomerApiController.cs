using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;

[Route("api/customer")]
[ApiController]
public class CustomerApiController : ControllerBase
{
    private readonly CustomerService _service;
    private readonly SalesService _salesService;
    private readonly JwtService _jwtService;
    private readonly OtpService _otpService;
    public CustomerApiController(CustomerService service, SalesService salesService, JwtService jwtService, OtpService otpService)
    {
        _service = service;
        _salesService = salesService;
        _jwtService = jwtService;
        _otpService = otpService;
    }



    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        Console.WriteLine("🔥 CUSTOMER LOGIN HIT");
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { status = "EMAIL_PASSWORD_REQUIRED" });

        var customer = _service.Authenticate(request.Email, request.Password);

        if (customer == null)
            return Unauthorized(new { status = "INVALID_CREDENTIALS" });

        var token = _jwtService.GenerateCustomerToken(customer);

        return Ok(new
        {
            token = token,
            customerId = customer.CustomerID,
            name = customer.CustomerName
        });
    }
    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetCustomerById(int id)
    {
        var customer = _service.Get(id);

        if (customer == null)
            return NotFound();

        return Ok(new
        {
            customerID = customer.CustomerID,
            customerName = customer.CustomerName,
            phone = customer.Phone,
            email = customer.Email,
            gender = customer.Gender,
            address = customer.Address,
            pincode = customer.Pincode,
            customerType = customer.CustomerType
        });
    }


    [HttpGet("invoice-details/{invoiceId}")]
    public IActionResult GetInvoiceDetails(int invoiceId)
    {
        var details = _salesService.GetInvoiceDetails(invoiceId);
        return Ok(details);
    }

    [HttpGet("invoices/{customerId}")]
    public IActionResult GetInvoicesByCustomer(int customerId)
    {
        var invoices = _service.GetInvoicesByCustomer(customerId);
        return Ok(invoices);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Phone) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { status = "INVALID_DATA" });
        }

        // Email format validation
        if (!dto.Email.Contains("@") || !dto.Email.Contains("."))
        {
            return BadRequest(new { status = "INVALID_EMAIL" });
        }

        // Phone validation
        if (dto.Phone.Length != 10)
        {
            return BadRequest(new { status = "INVALID_PHONE" });
        }

        // Check if email already exists
        var existing = _service.GetByEmail(dto.Email);

        if (existing != null)
        {
            return Ok(new { status = "EMAIL_EXISTS" });
        }

        await _otpService.SendOtp(dto.Email);

        return Ok(new { status = "OTP_SENT" });
    }
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) ||
            string.IsNullOrEmpty(dto.Otp) ||
            string.IsNullOrEmpty(dto.NewPassword))
            return BadRequest(new { status = "INVALID_DATA" });

        bool isValid = await _otpService.VerifyOtp(dto.Email, dto.Otp);

        if (!isValid)
            return BadRequest(new { status = "INVALID_OTP" });

        string newHash = PasswordHasher.Hash(dto.NewPassword);

        _service.UpdatePassword(dto.Email, newHash);

      return Ok(new { status = "PASSWORD_RESET_SUCCESS" });
    }
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email))
            return BadRequest(new { status = "EMAIL_REQUIRED" });

        var customer = _service.GetByEmail(dto.Email);

        // IMPORTANT: Don't reveal if user exists
        if (customer != null)
        {
            await _otpService.SendOtp(dto.Email);
        }
        return Ok(new { status = "OTP_SENT" });
    }
    [Authorize]  // if not already present
    [HttpPost("update-fcm-token")]
    public IActionResult UpdateFcmToken([FromBody] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { status = "TOKEN_REQUIRED" });

        var userId = int.Parse(User.FindFirst("CustomerID")?.Value);

        _service.UpdateFcmToken(userId, token);

        return Ok(new { message = "Token updated successfully" });
    }
    [HttpPost("verify-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyRegistration([FromBody] OtpVerifyDto dto)
    {
        var existing = _service.GetByEmail(dto.Email);

        if (existing != null)
            return BadRequest(new { status = "EMAIL_EXISTS" });

        bool isValid = await _otpService.VerifyOtp(dto.Email, dto.Otp);

        if (!isValid)
            return BadRequest(new { status = "INVALID_OTP" });

        string hash = PasswordHasher.Hash(dto.Password);

        var customer = new CustomerMaster
        {
            TenantID = 1,
            CustomerCode = Guid.NewGuid().ToString(),
            Username = dto.Email,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = hash,
            CustomerName = dto.Name,
            CustomerType = "Regular",
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        _service.Create(customer);

        return Ok(new { status = "REGISTER_SUCCESS" });
    }
    [Authorize(Roles = "Customer")]
    [HttpPut("update-profile")]
    public IActionResult UpdateProfile([FromBody] UpdateCustomerProfileDto dto)
    {
        var customerId = int.Parse(User.FindFirst("CustomerID").Value);

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            return BadRequest(new { status = "NAME_REQUIRED" });

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest(new { status = "PHONE_REQUIRED" });

        _service.UpdateProfile(customerId, dto);

        return Ok(new { message = "Profile updated successfully" });
    }
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtp([FromBody] string email)
    {
        var existing = _service.GetByEmail(email);
        if (existing != null)
            return BadRequest("Email already registered");

        await _otpService.SendOtp(email);

        return Ok(new { status = "OTP_RESENT" });
    }
    [Authorize(Roles = "Customer")]
    [HttpGet("my-invoices")]
    public IActionResult GetMyInvoices()
    {
        var customerId = int.Parse(User.FindFirst("CustomerID").Value);

        var invoices = _service.GetInvoicesByCustomer(customerId);

        return Ok(invoices);
    }


}
