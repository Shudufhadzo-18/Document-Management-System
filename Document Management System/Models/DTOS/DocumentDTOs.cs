namespace Document_Management_System.Models.DTOS
{
    // Used when creating a document record (metadata only — file comes via a separate upload DTO)
    public class DocumentCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Description { get; set; }
        public string? DocumentType { get; set; }
        public string? Tags { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
    // What the client sees when listing/viewing documents
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DocumentType { get; set; }
        public string? Tags { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CurrentVersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

        // Lightweight version for dropdowns/lists where full detail isn't needed
        public class DocumentSummaryDto
        {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DocumentType { get; set; }
        public DateTime? ReviewDate { get; set; } // useful for a "due soon" badge in the list view
        public string Status { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
    }

