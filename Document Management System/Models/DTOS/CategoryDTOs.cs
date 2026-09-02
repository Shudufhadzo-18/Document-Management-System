namespace Document_Management_System.Models.DTOS
{
    public class CategoryCreateDto
    {

        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }= string.Empty;
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
