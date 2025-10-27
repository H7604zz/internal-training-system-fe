using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<LogoutResponseDto> LogoutAsync();
        Task<UserProfileDto?> GetProfileAsync();
        Task<ChangePasswordResponseDto> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        bool IsTokenExpired();
        void ClearTokens();
        string? GetAccessToken();
        string? GetRefreshToken();
        Task<string> UpdateProfileAsync(UpdateProfileDto model);
    }
}