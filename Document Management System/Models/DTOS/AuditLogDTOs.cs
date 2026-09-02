namespace Document_Management_System.Models.DTOS
{
    // Read-only — clients never create audit logs directly, the API writes these internally
    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string DocumentTitle { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
