namespace Document_Management_System.Models.DTOS
{
    public class UserDTOs
    {
        public class UserResponseDto
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public List<string> Roles { get; set; } = new();
        }

        public class AssignRoleDto
        {
            public string Role { get; set; } = string.Empty;
        }
    }
}
