using System.Collections.Generic;

namespace SMBS_SERVER.Models.DTOs
{
    public class CreateInvoiceRequest
    {
        public decimal TotalAmount { get; set; }
        public List<InvoiceItemDto> Items { get; set; }
    }
   
}