using System.Net.Http.Headers;

namespace InternalTrainingSystem.WebApp.Handlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticationHandler> _logger;

        public AuthenticationHandler(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lấy access token từ session
            var accessToken = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Thêm Authorization header nếu chưa có
                if (request.Headers.Authorization == null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.LogDebug($"Added Authorization header to request: {request.RequestUri}");
                }
            }
            else
            {
                _logger.LogWarning($"No access token found for request: {request.RequestUri}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}