namespace Document_Management_System.Models.DTOS
{
    public class DocumentCommentDTOs
    {
        public class DocumentCommentCreateDto
        {
            public string Content { get; set; } = string.Empty;
        }

        public class DocumentCommentResponseDto
        {
            public int Id { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }

}
