using System.Text.RegularExpressions;

namespace InternalTrainingSystem.WebApp.Helpers
{
    public class Utilities
    {
        private static string _baseUrl = null!; 

        public static void Initialize(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _baseUrl = configuration["GivenAPIBaseUrl"]
                ?? throw new InvalidOperationException("GivenAPIBaseUrl is not configured in appsettings.json");
        }

        /// <summary>
        /// Generates an absolute URL by combining a base URL with a relative path.
        /// </summary>
        /// <param name="path">The relative path to append to the base URL. Can be null or empty.</param>
        /// <returns>A string representing the absolute URL formed by combining the base URL and the provided path.</returns>
        /// <exception cref="InvalidOperationException">Thrown if Utilities has not been initialized with a base URL.</exception>
        public static string GetAbsoluteUrl(string path)
        {
            if (_baseUrl == null)
                throw new InvalidOperationException("UrlUtilities has not been initialized. Call Initialize(IConfiguration) first.");

            // Remove the '/' character at the beginning or end
            string baseUrl = _baseUrl.TrimEnd('/');
            path = path?.TrimStart('/') ?? string.Empty;

            // Combine baseUrl and path
            return $"{baseUrl}/{path}";
        }

        /// <summary>
        /// Generates an absolute URL with kebab-case formatting for controller and action
        /// </summary>
        /// <param name="controller">Controller name (will be converted to kebab-case)</param>
        /// <param name="action">Action name (will be converted to kebab-case)</param>
        /// <param name="additionalPath">Additional path segments</param>
        /// <returns>A kebab-case formatted absolute URL</returns>
        public static string GetKebabUrl(string controller, string action, string? additionalPath = null)
        {
            if (_baseUrl == null)
                throw new InvalidOperationException("UrlUtilities has not been initialized. Call Initialize(IConfiguration) first.");

            string kebabController = ConvertToKebabCase(controller);
            string kebabAction = ConvertToKebabCase(action);
            
            string baseUrl = _baseUrl.TrimEnd('/');
            string path = $"{kebabController}/{kebabAction}";
            
            if (!string.IsNullOrEmpty(additionalPath))
            {
                additionalPath = additionalPath.TrimStart('/');
                path = $"{path}/{additionalPath}";
            }

            return $"{baseUrl}/{path}";
        }

        /// <summary>
        /// Converts PascalCase string to kebab-case
        /// </summary>
        /// <param name="input">Input string in PascalCase</param>
        /// <returns>String in kebab-case format</returns>
        private static string ConvertToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Convert PascalCase to kebab-case
            return Regex.Replace(input, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }
}
