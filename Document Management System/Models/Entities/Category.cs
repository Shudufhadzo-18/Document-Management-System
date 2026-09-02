namespace Document_Management_System.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public DateTime CreatedAt{ get; set; }
    }
}
