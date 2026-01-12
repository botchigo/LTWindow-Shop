using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MyShop.Shared.Helpers
{
    public static class StringHelper
    {
        private static readonly Regex _slugInvalidCharsRegex =
            new Regex(@"[^a-z0-9\s]", RegexOptions.Compiled);

        private static readonly Regex _slugMultipleSpacesRegex =
            new Regex(@"\s+", RegexOptions.Compiled);

        public static string GenerateSku(string name, IEnumerable<string>? options = null)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            //options
            if (options is not null && options.Any())
            {
                name += " " + string.Join(" ", options);
            }

            string slug = name.Trim().ToLowerInvariant();

            //remove sign
            slug = slug.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder();
            foreach (var c in slug)
            {
                //remove sign characters
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            slug = stringBuilder.ToString();
            slug = slug.Normalize(NormalizationForm.FormC);

            slug = slug.Replace('đ', 'd');

            //remove invalid char
            slug = _slugInvalidCharsRegex.Replace(slug, "");
            //replace one or multiple space to -
            slug = _slugMultipleSpacesRegex.Replace(slug, "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
