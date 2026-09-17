using DTS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace DTS.Application.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterDto dto);
    Task<(SignInResult Result, UserDto? User)> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<IdentityResult> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
}
