using SMBS_SERVER.Models;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Repositories;
using System.Security.Claims;

namespace SMBS_SERVER.Services
{
    public class SalesService
    {
        private readonly SalesInvoiceRepository _invoiceRepository;
        private readonly SalesInvoiceDetailRepository _detailRepository;
        private readonly ProductRepository _productRepository;
        private readonly FcmService _fcmService;
        private readonly CustomerService _customerService;

        public SalesService(SalesInvoiceRepository invoiceRepository, SalesInvoiceDetailRepository detailRepository, ProductRepository productRepository, FcmService fcmService,
    CustomerService customerService)
        {
            _invoiceRepository = invoiceRepository;
            _detailRepository = detailRepository;
             _productRepository = productRepository;
            _fcmService = fcmService;
            _customerService = customerService;
        }
        public List<SalesInvoiceDetails> GetInvoiceDetails(int invoiceId)
        {
            return _detailRepository.GetByInvoiceId(invoiceId);
        }


        // 🔹 Called by SyncController
        public async Task<bool> SaveSyncedInvoice(SalesInvoiceSyncDto dto)
        {
            const int SYSTEM_SYNC_USER_ID = 1;

            if (_invoiceRepository.ExistsByInvoiceNumber(
                dto.InvoiceNumber, dto.TenantID))
            {
                return true;
            }

            var invoice = new SalesInvoice
            {
                TenantID = dto.TenantID,
                BranchID = dto.BranchID,

                InvoiceNumber = dto.InvoiceNumber,
                InvoiceDate = dto.InvoiceDate,
                InvoiceTime = dto.InvoiceTime,

                // 🔹 User Info
                CashierUserID = SYSTEM_SYNC_USER_ID,
                POSTerminalID = dto.POSTerminalID,
                ShiftTypeID = dto.ShiftTypeID ?? 1,

                // 🔹 Financials
                Subtotal = dto.Subtotal,
                DiscountPercentage = dto.DiscountPercentage,
                TotalDiscountAmount = dto.TotalDiscountAmount,
                TotalTaxAmount = dto.TotalTaxAmount,
                RoundOff = dto.RoundOff,

                TotalAmount = dto.TotalAmount,
                PaidAmount = dto.PaidAmount,
                BalanceAmount = dto.BalanceAmount,

                // 🔹 Loyalty & Promotion
                PromotionID = dto.PromotionID,
                LoyaltyPointsEarned = dto.LoyaltyPointsEarned,
                LoyaltyPointsRedeemed = dto.LoyaltyPointsRedeemed,

                // 🔹 Customer Info  ✅ IMPORTANT
                CustomerID = dto.CustomerID,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,

                // 🔹 Status
                PaymentStatus = dto.PaymentStatus,
                InvoiceStatus = "Active",

                IsPaused = false,
                IsCompleted = true,

                Remarks = dto.Remarks,
                CreatedDate = DateTime.Now
            };

            _invoiceRepository.Insert(invoice);

            if (dto.CustomerID > 0)
            {
                var customer = _customerService.Get(dto.CustomerID.Value);

                if (customer != null && !string.IsNullOrEmpty(customer.FCMToken))
                {
                    await _fcmService.SendNotification(
                        customer.FCMToken,
                        "Invoice Created",
                        $"Your invoice {dto.InvoiceNumber} has been generated."
                    );
                }   
            }
            return true;
        }
        public void SaveSyncedInvoiceDetail(SalesInvoiceDetailSyncDto dto)
        {
            int invoiceId = _invoiceRepository.GetInvoiceIdByNumber(
                dto.InvoiceNumber,
                dto.TenantID
            );
            int productId = _productRepository.GetProductIdByCode(
      dto.ProductCode,
      dto.TenantID
  );

            _detailRepository.Insert(new SalesInvoiceDetails
            {
                InvoiceID = invoiceId,   // ✅ SERVER ID
                ProductID = productId,
                VariantID = dto.VariantID ?? 0,

                Quantity = dto.Quantity,
                UOMID = dto.UOMID,

                UnitPrice = dto.UnitPrice,
                MRP = dto.MRP,

                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = dto.DiscountAmount,

                TaxPercentage = dto.TaxPercentage,
                TaxAmount = dto.TaxAmount,

                LineTotal = dto.LineTotal,

                BatchNumber = dto.BatchNumber,
                ExpiryDate = dto.ExpiryDate,
                SerialNumber = dto.SerialNumber,

                Remarks = dto.Remarks,
                IsActive = true,
                CreatedDate = DateTime.Now,
                IsSynced = true
            });
        }


    }

}
