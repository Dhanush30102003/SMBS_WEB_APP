namespace SMBS_SERVER.Models.DTOs
{
    public class SendNotificationDto
    {
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}