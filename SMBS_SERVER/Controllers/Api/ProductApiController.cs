using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Services;
using static Org.BouncyCastle.Math.EC.ECCurve;
using MySql.Data.MySqlClient;
[Route("api/[controller]")]
[ApiController]
public class ProductApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IConfiguration _config;
    public ProductApiController(IProductService productService, IConfiguration config)
    {
        _productService = productService;
        _config = config;
    }
    private MySqlConnection GetConnection()
        => new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

    [HttpGet("barcode/{barcode}")]
    public IActionResult GetByBarcode(string barcode)
    {
        barcode = barcode.Trim().Replace("\n", "").Replace("\r", "");
        var product = _productService.GetByBarcode(barcode);

        if (product == null)
            return NotFound("Product not found");

        return Ok(product);
    }
    [HttpGet("image/{id}")]
    public IActionResult GetProductImage(int id)
    {
        using var con = GetConnection();
        con.Open();

        string sql = "SELECT ProductImage FROM productmaster WHERE ProductID=@Id LIMIT 1";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Id", id);

        var result = cmd.ExecuteScalar();

        if (result == null || result == DBNull.Value)
            return NotFound();

        byte[] imageBytes = (byte[])result;

        return File(imageBytes, "image/jpeg");
    }
}
