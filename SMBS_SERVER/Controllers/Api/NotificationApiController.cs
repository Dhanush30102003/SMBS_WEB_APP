using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Services;

namespace SMBS_SERVER.Controllers.Api
{
    [ApiController]
    [Route("api/notification")]
    [Authorize] // JWT protected
    public class NotificationApiController : ControllerBase
    {
        private readonly FcmService _fcmService;
        private readonly CustomerService _customerService;

        public NotificationApiController(
            FcmService fcmService,
            CustomerService customerService)
        {
            _fcmService = fcmService;
            _customerService = customerService;
        }
        [HttpPost("flashsale")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AnnounceFlashSale(
    [FromBody] SendNotificationDto dto)
        {
            var tenantClaim = User.FindFirst("TenantId");

            if (tenantClaim == null)
                return Unauthorized("TenantId claim missing");

            int tenantId = int.Parse(tenantClaim.Value);

            var customers = _customerService.GetAllWithToken(tenantId);

            var tokens = customers
                .Where(c => !string.IsNullOrEmpty(c.FCMToken))
                .Select(c => c.FCMToken)
                .Distinct()
                .ToList();

            if (!tokens.Any())
                return BadRequest("No active FCM tokens found");

            await _fcmService.SendMulticast(tokens, dto.Title, dto.Body);

            return Ok(new
            {
                success = true,
                totalUsers = tokens.Count
            });
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(
     [FromBody] SendNotificationDto dto)
        {
            var customer = _customerService.Get(dto.CustomerId);

            if (customer == null)
                return NotFound("Customer not found");

            if (string.IsNullOrEmpty(customer.FCMToken))
                return BadRequest("No FCM token found");

            await _fcmService.SendNotification(
                customer.FCMToken,
                dto.Title,
                dto.Body);

            return Ok(new
            {
                success = true,
                message = "Notification sent successfully"
            });
        }
    }
}