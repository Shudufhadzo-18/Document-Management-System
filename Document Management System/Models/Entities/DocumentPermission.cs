namespace Document_Management_System.Models.Entities
{
    public class DocumentPermission
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanDownload { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
    }
}
