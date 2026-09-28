namespace Document_Management_System.Models.DTOS
{
    public class DocumentStatusUpdateDto
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Reason { get; set; } 
    }
}
