namespace Document_Management_System.Models.DTOS
{
    // Used when creating a document record (metadata only — file comes via a separate upload DTO)
    public class DocumentCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
    // What the client sees when listing/viewing documents
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;  // flattened, not the whole Category object
        public string OwnerName { get; set; } = string.Empty;     // resolved from Identity, not raw OwnerId
        public string Status { get; set; } = string.Empty;
        public int? CurrentVersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Lightweight version for dropdowns/lists where full detail isn't needed
        public class DocumentSummaryDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
        }
    }
}
