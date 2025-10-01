using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InternalTrainingSystem.WebApp.Helpers
{
    /// <summary>
    /// Extension methods for URL helpers to generate kebab-case URLs
    /// </summary>
    public static class UrlExtensions
    {
        /// <summary>
        /// Generates a URL with kebab-case action names
        /// </summary>
        /// <param name="urlHelper">The URL helper instance</param>
        /// <param name="actionName">The action name (will be converted to kebab-case)</param>
        /// <param name="controllerName">The controller name (will be converted to kebab-case)</param>
        /// <param name="routeValues">Additional route values</param>
        /// <returns>A kebab-case URL</returns>
        public static string? ActionKebab(this IUrlHelper urlHelper, string actionName, string? controllerName = null, object? routeValues = null)
        {
            string kebabAction = ConvertToKebabCase(actionName);
            string? kebabController = controllerName != null ? ConvertToKebabCase(controllerName) : null;
            
            return urlHelper.Action(kebabAction, kebabController, routeValues);
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

        /// <summary>
        /// Generates a full URL with kebab-case formatting
        /// </summary>
        /// <param name="baseUrl">Base URL from configuration</param>
        /// <param name="controller">Controller name</param>
        /// <param name="action">Action name</param>
        /// <returns>Full kebab-case URL</returns>
        public static string BuildKebabUrl(string baseUrl, string controller, string action)
        {
            string kebabController = ConvertToKebabCase(controller);
            string kebabAction = ConvertToKebabCase(action);
            
            baseUrl = baseUrl.TrimEnd('/');
            return $"{baseUrl}/{kebabController}/{kebabAction}";
        }
    }
}