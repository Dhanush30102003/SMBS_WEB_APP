namespace SMBS_SERVER.Models.Masters
{
    public class CategoryMaster
    {
        public int CategoryID { get; set; }
        public int TenantID { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}
