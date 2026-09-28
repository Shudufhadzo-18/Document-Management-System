namespace Document_Management_System.Models.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DocumentType { get; set; }   // e.g. "Policy", "Report", "Form", "Contract"
        public string? Tags { get; set; }            // comma-separated, e.g. "hr,leave,2026"
        public DateTime? EffectiveDate { get; set; }  // when the document takes effect
        public DateTime? ReviewDate { get; set; }     // when it's next due for review
        public int CategoryId { get; set; }
        public int? DepartmentId { get; set; }
        public string OwnerId { get; set; } = string.Empty;     // FK to Identity user
        public int? CurrentVersionId { get; set; }
        public string Status { get; set; } = string.Empty;       // Draft / Active / Archived
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Category? Category { get; set; }
        public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    }
}
