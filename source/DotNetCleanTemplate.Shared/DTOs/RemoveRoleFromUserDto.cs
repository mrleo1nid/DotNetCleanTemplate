namespace DotNetCleanTemplate.Shared.DTOs
{
    public class RemoveRoleFromUserDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
