namespace EasyAuthentication.DTOs
{
    public class TokenRequest
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceSecret { get; set; } = string.Empty;
    }
}
