using System.Text.RegularExpressions;

namespace InternalTrainingSystem.WebApp.Helpers
{
    /// <summary>
    /// Custom parameter transformer to convert PascalCase action names to kebab-case URLs
    /// Example: "TrangChu" becomes "trang-chu"
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;

            string input = value.ToString()!;
            
            // Convert PascalCase to kebab-case
            return Regex.Replace(input, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }
}