namespace Document_Management_System.Models.DTOS
{
    public class DocumentPermissionDTOs
    {
        public class DocumentPermissionGrantDto
        {
            public string UserId { get; set; } = string.Empty;
            public bool CanView { get; set; }
            public bool CanDownload { get; set; }
            public bool CanEdit { get; set; }
            public bool CanDelete { get; set; }
            public bool CanApprove { get; set; }
        }

        public class DocumentPermissionResponseDto
        {
            public int Id { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
            public bool CanView { get; set; }
            public bool CanDownload { get; set; }
            public bool CanEdit { get; set; }
            public bool CanDelete { get; set; }
            public bool CanApprove { get; set; }
            public DateTime GrantedAt { get; set; }
        }
    }
}
