using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Data;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Repositories;
using SMBS_SERVER.Services;




namespace SMBS_SERVER.Controllers.Api
{
    [ApiController]
    [Route("api/sync")]
    [Authorize] // uses JWT
    public class SyncController : ControllerBase
    {
        private readonly SalesService _salesService;
        private readonly ApplicationDbContext _context;
        private readonly ProductRepository _productRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly UserRepository _userRepo;
        private readonly StockRepository _stockRepo;

        public SyncController(SalesService salesService, ApplicationDbContext context,
    ProductRepository productRepo,
    CustomerRepository customerRepo,
    UserRepository userRepo,
    StockRepository stockRepo)
        {
            _salesService = salesService;
            _context = context;
            _productRepo = productRepo;
            _customerRepo = customerRepo;
            _userRepo = userRepo;
            _stockRepo = stockRepo;
        
        }

        [HttpPost("sales")]
        public IActionResult SyncSales(
     [FromBody] List<SalesInvoiceSyncDto> invoices)
        {
            if (invoices == null || invoices.Count == 0)
                return BadRequest("No invoices received");

            foreach (var dto in invoices)
            {
                _salesService.SaveSyncedInvoice(dto);
            }

            return Ok(new
            {
                success = true,
                count = invoices.Count
            });
        }

        [HttpPost("sales/details")]
        public IActionResult SyncSalesDetail(
     [FromBody] List<SalesInvoiceDetailSyncDto> details)
        {
            foreach (var d in details)
            {
                _salesService.SaveSyncedInvoiceDetail(d);
            }

            return Ok(new
            {
                success = true,
                count = details.Count
            });
        }

        [HttpPost("product")]
        public IActionResult SyncProduct([FromBody] ProductMaster model)
        {
            try
            {
                var existingId = _productRepo
                    .GetProductIdByCode(model.ProductCode, model.TenantID);

                if (existingId > 0)
                {
                    model.ProductID = existingId;

                    _productRepo.Update(
                        model,
                        model.SellingPrice,
                        model.MRP,
                        model.DiscountPercentage);

                    Console.WriteLine("Updated Product: " + model.ProductCode);
                }
                else
                {
                    _productRepo.Insert(model);
                    Console.WriteLine("Inserted Product: " + model.ProductCode);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Product Sync Error: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("customer")]
        public IActionResult SyncCustomer([FromBody] CustomerSyncDto dto)
        {
            try
            {
                var existing = _customerRepo
                    .GetByCode(dto.CustomerCode, dto.TenantID);

                var model = new CustomerMaster
                {
                    CustomerCode = dto.CustomerCode,
                    CustomerName = dto.CustomerName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    Pincode = dto.Pincode,
                    TenantID = dto.TenantID,

                    // Default values
                    Username = null,
                    PasswordHash = null
                };

                if (existing != null)
                {
                    model.CustomerID = existing.CustomerID;
                    _customerRepo.Update(model);
                }
                else
                {
                    _customerRepo.Insert(model);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("user")]
        public IActionResult SyncUser([FromBody] UserMaster model)
        {
            try
            {
                var existing = _userRepo
                    .GetByUserName(model.UserName, model.TenantID);

                if (existing != null)
                {
                    model.UserID = existing.UserID;
                    _userRepo.Update(model);
                    Console.WriteLine("Updated User: " + model.UserName);
                }
                else
                {
                    _userRepo.Insert(model);
                    Console.WriteLine("Inserted User: " + model.UserName);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("User Sync Error: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("stock")]
        public IActionResult SyncStock([FromBody] StockMaster model)
        {
            try
            {
                var existing = _stockRepo
                    .GetByProduct(model.ProductID,
                                  model.BranchID,
                                  model.TenantID);

                if (existing != null)
                {
                    model.StockID = existing.StockID;
                    _stockRepo.Update(model);
                    Console.WriteLine("Updated Stock for ProductID: " + model.ProductID);
                }
                else
                {
                    _stockRepo.Insert(model);
                    Console.WriteLine("Inserted Stock for ProductID: " + model.ProductID);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Stock Sync Error: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }




    }
}
