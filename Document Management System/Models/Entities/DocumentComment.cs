namespace Document_Management_System.Models.Entities
{
    public class DocumentComment
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
