using Microsoft.AspNetCore.Http;

namespace SMBS_SERVER.DTOs
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
