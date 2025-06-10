using System.Diagnostics.CodeAnalysis;

namespace BliveHelper.Utils.QRCoder
{
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the <paramref name="value"/> is null, empty, or white space; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}