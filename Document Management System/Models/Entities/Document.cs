namespace Document_Management_System.Models.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string OwnerId { get; set; } = string.Empty;     // FK to Identity user
        public int? CurrentVersionId { get; set; }
        public string Status { get; set; } = string.Empty;       // Draft / Active / Archived
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Category Category { get; set; }
        public ICollection<DocumentVersion> Versions { get; set; }
    }
}
