namespace DotNetCleanTemplate.Shared.DTOs
{
    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime Expires { get; set; }
    }
}
