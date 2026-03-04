namespace SMBS_SERVER.Models.Masters
{
    public class PendingUserRegistration
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
