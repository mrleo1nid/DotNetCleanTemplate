namespace DotNetCleanTemplate.Shared.DTOs
{
    public class UserWithRolesDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<RoleDto> Roles { get; set; } = new();
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDefault { get; set; }
    }
}
