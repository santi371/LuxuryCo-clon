namespace LuxuryCo.Back.DTOs;

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Code2FA { get; set; } // Optional for first step
}
