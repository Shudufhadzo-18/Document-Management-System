namespace Document_Management_System.Models.Entities
{
    public class DocumentVersion
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Document Document { get; set; }
    }
}
