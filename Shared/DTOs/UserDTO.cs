namespace Shared.Dtos;

public class UserDTO
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
     public string Role { get; set; }
    public string ProfilePicUrl { get; set; }
    public bool IsActive { get; set; }
}