namespace Document_Management_System.Models.DTOS
{
    public class DocumentVersionResponseDto
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public string FileName { get; set; } = string.Empty;  // derived from FilePath, not the raw server path
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // Metadata sent alongside the file upload (the file itself comes as IFormFile in the controller, not in the DTO)
    public class DocumentVersionUploadDto
    {
        public int DocumentId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
