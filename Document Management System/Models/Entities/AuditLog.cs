namespace Document_Management_System.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string Action { get; set; } = string.Empty;       // Upload/View/Download/Edit/Delete/Approve
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
    }
}
