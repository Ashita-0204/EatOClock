using Auth_Service.DTOs;
public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDTO? User { get; set; }
    public string? Message { get; set; }
}