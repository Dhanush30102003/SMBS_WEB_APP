using static Org.BouncyCastle.Math.EC.ECCurve;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;
public class FlashSaleRepository
{
    private readonly IConfiguration _config;

    public FlashSaleRepository(IConfiguration config)
    {
        _config = config;
    }

    private MySqlConnection GetConnection()
          => new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

    public List<FlashSaleMaster> GetPendingSales()
    {
        var list = new List<FlashSaleMaster>();

        using var con = GetConnection();
        con.Open();

        string sql = @"
            SELECT * FROM flashsalemaster
WHERE IsActive = 1
AND IsNotified = 0";

        using var cmd = new MySqlCommand(sql, con);
        using var rd = cmd.ExecuteReader();

        while (rd.Read())
        {
            list.Add(new FlashSaleMaster
            {
                FlashSaleID = Convert.ToInt32(rd["FlashSaleID"]),
                TenantID = Convert.ToInt32(rd["TenantID"]),
                Title = rd["Title"].ToString(),
                Description = rd["Description"].ToString(),
                DiscountPercentage = Convert.ToDecimal(rd["DiscountPercentage"]),
                StartTime = Convert.ToDateTime(rd["StartTime"]),
                EndTime = Convert.ToDateTime(rd["EndTime"]),
                IsActive = Convert.ToBoolean(rd["IsActive"]),
                IsNotified = Convert.ToBoolean(rd["IsNotified"])
            });
        }

        return list;
    }

    public FlashSaleMaster GetById(int id)
    {
        using var con = GetConnection();
        con.Open();

        string sql = "SELECT * FROM flashsalemaster WHERE FlashSaleID = @id";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);

        using var rd = cmd.ExecuteReader();

        if (!rd.Read())
            return null;

        return new FlashSaleMaster
        {
            FlashSaleID = Convert.ToInt32(rd["FlashSaleID"]),
            TenantID = Convert.ToInt32(rd["TenantID"]),
            Title = rd["Title"].ToString(),
            Description = rd["Description"].ToString(),
            DiscountPercentage = Convert.ToDecimal(rd["DiscountPercentage"]),
            StartTime = Convert.ToDateTime(rd["StartTime"]),
            EndTime = Convert.ToDateTime(rd["EndTime"]),
            IsActive = Convert.ToBoolean(rd["IsActive"]),
            IsNotified = Convert.ToBoolean(rd["IsNotified"])
        };
    }
    public void Update(FlashSaleMaster sale)
    {
        ValidateFlashSale(sale);
        using var con = GetConnection();
        con.Open();

        string sql = @"
        UPDATE flashsalemaster
        SET Title = @Title,
            Description = @Description,
            DiscountPercentage = @DiscountPercentage,
            StartTime = @StartTime,
            EndTime = @EndTime
        WHERE FlashSaleID = @FlashSaleID";

        using var cmd = new MySqlCommand(sql, con);

        cmd.Parameters.AddWithValue("@Title", sale.Title);
        cmd.Parameters.AddWithValue("@Description", sale.Description);
        cmd.Parameters.AddWithValue("@DiscountPercentage", sale.DiscountPercentage);
        cmd.Parameters.AddWithValue("@StartTime", sale.StartTime);
        cmd.Parameters.AddWithValue("@EndTime", sale.EndTime);
        cmd.Parameters.AddWithValue("@FlashSaleID", sale.FlashSaleID);

        cmd.ExecuteNonQuery();
    }
    public void Delete(int id)
    {
        using var con = GetConnection();
        con.Open();

        string sql = "UPDATE flashsalemaster SET IsActive = 0 WHERE FlashSaleID = @id";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);

        cmd.ExecuteNonQuery();
    }
    public List<FlashSaleMaster> GetAll()
    {
        var list = new List<FlashSaleMaster>();

        using var con = GetConnection();
        con.Open();

        string sql = "SELECT * FROM flashsalemaster ORDER BY FlashSaleID DESC";

        using var cmd = new MySqlCommand(sql, con);
        using var rd = cmd.ExecuteReader();

        while (rd.Read())
        {
            list.Add(new FlashSaleMaster
            {
                FlashSaleID = Convert.ToInt32(rd["FlashSaleID"]),
                TenantID = Convert.ToInt32(rd["TenantID"]),
                Title = rd["Title"].ToString(),
                Description = rd["Description"].ToString(),
                DiscountPercentage = Convert.ToDecimal(rd["DiscountPercentage"]),
                StartTime = Convert.ToDateTime(rd["StartTime"]),
                EndTime = Convert.ToDateTime(rd["EndTime"]),
                IsActive = Convert.ToBoolean(rd["IsActive"]),
                IsNotified = Convert.ToBoolean(rd["IsNotified"])
            });
        }

        return list;
    }
    public void Insert(FlashSaleMaster sale)
    {
        ValidateFlashSale(sale);
        using var con = GetConnection();
        con.Open();

        string sql = @"
    INSERT INTO flashsalemaster
    (TenantID, Title, Description, DiscountPercentage, StartTime, EndTime)
    VALUES
    (@TenantID, @Title, @Description, @DiscountPercentage, @StartTime, @EndTime)";

        using var cmd = new MySqlCommand(sql, con);

        cmd.Parameters.AddWithValue("@TenantID", sale.TenantID);
        cmd.Parameters.AddWithValue("@Title", sale.Title);
        cmd.Parameters.AddWithValue("@Description", sale.Description);
        cmd.Parameters.AddWithValue("@DiscountPercentage", sale.DiscountPercentage);
        cmd.Parameters.AddWithValue("@StartTime", sale.StartTime);
        cmd.Parameters.AddWithValue("@EndTime", sale.EndTime);

        cmd.ExecuteNonQuery();
    }
    private void ValidateFlashSale(FlashSaleMaster sale)
    {
        if (string.IsNullOrWhiteSpace(sale.Title))
            throw new Exception("Title is required.");

        if (string.IsNullOrWhiteSpace(sale.Description))
            throw new Exception("Description is required.");

        if (sale.DiscountPercentage <= 0 || sale.DiscountPercentage > 100)
            throw new Exception("Discount percentage must be between 1 and 100.");

        if (sale.StartTime < DateTime.Now)
            throw new Exception("Start time cannot be in the past.");

        if (sale.EndTime <= sale.StartTime)
            throw new Exception("End time must be greater than start time.");
    }
    public void MarkAsNotified(int id)
    {
        using var con = GetConnection();
        con.Open();

        string sql = "UPDATE flashsalemaster SET IsNotified = 1 WHERE FlashSaleID = @id";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}