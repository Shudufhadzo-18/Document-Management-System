namespace Document_Management_System.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // who receives it
        public string Message { get; set; } = string.Empty;
        public int? DocumentId { get; set; } // optional link back to the relevant document
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
