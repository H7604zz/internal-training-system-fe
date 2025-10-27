using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<LogoutResponseDto> LogoutAsync();
        Task<UserProfileDto?> GetProfileAsync();
        Task<bool> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest);
        Task<string> GetChangePasswordErrorAsync(ChangePasswordRequestDto changePasswordRequest);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        bool IsTokenExpired();
        void ClearTokens();
        string? GetAccessToken();
        string? GetRefreshToken();
        Task<string> UpdateProfileAsync(UpdateProfileDto model);
    }
}