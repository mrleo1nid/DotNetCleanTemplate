namespace DotNetCleanTemplate.Shared.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpires { get; set; }
    }
}
