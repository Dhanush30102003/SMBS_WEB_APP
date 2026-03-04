namespace SMBS_SERVER.Models.Masters
{
    public class CustomerMaster
    {
        public int CustomerID { get; set; }
        public int TenantID { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? Anniversary { get; set; }

        public string Gender { get; set; }
        public string Address { get; set; }

        public int? CityID { get; set; }
        public int? StateID { get; set; }
        public int? CountryID { get; set; }

        public string Pincode { get; set; }

        public string LoyaltyCardNumber { get; set; }
        public int TotalPoints { get; set; }
        public int RedeemedPoints { get; set; }
        public int AvailablePoints { get; set; }

        public string CustomerType { get; set; }   // Regular / VIP / Wholesale
        public decimal CreditLimit { get; set; }

        public string TaxRegistrationNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Username { get; set; }
        public string? FCMToken { get; set; }
        public string PasswordHash { get; set; }

        public decimal OutstandingBalance { get; set; }
    }
}
