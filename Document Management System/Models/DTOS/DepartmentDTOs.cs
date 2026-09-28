namespace Document_Management_System.Models.DTOS
{
    public class DepartmentCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DepartmentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DocumentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
