namespace SMBS_SERVER.Models.DTOs
{
    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}