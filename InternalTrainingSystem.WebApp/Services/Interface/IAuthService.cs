using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<ApiResponseDto> LogoutAsync();
        Task<ApiResponseDto> GetProfileAsync();
        Task<ApiResponseDto> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        bool IsTokenExpired();
        void ClearTokens();
        string? GetAccessToken();
        string? GetRefreshToken();
    }
}