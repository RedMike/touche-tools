namespace ToucheTools.App.Utils;

internal static class StringExtensions
{
    internal static string ShortenPath(this string s)
    {
        return s.Substring(s.LastIndexOfAny(new[] { '\\', '/' }) + 1);
    }
}