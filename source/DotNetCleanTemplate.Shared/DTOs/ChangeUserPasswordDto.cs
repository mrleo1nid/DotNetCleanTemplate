namespace DotNetCleanTemplate.Shared.DTOs
{
    public class ChangeUserPasswordDto
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
