namespace MCModVersionChecker.Helpers;

internal static class GameVersionComparer
{
    public static int Compare(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
            return 0;
        if (string.IsNullOrWhiteSpace(left))
            return -1;
        if (string.IsNullOrWhiteSpace(right))
            return 1;

        var leftParts = Parse(left);
        var rightParts = Parse(right);
        var length = Math.Max(leftParts.Length, rightParts.Length);

        for (var i = 0; i < length; i++)
        {
            var l = i < leftParts.Length ? leftParts[i] : 0;
            var r = i < rightParts.Length ? rightParts[i] : 0;
            if (l != r)
                return l.CompareTo(r);
        }

        return 0;
    }

    public static string? Max(IEnumerable<string?> versions)
    {
        string? best = null;
        foreach (var version in versions)
        {
            if (string.IsNullOrWhiteSpace(version))
                continue;

            if (best == null || Compare(version, best) > 0)
                best = version;
        }

        return best;
    }

    private static int[] Parse(string version)
    {
        var core = version.Split('-', 2)[0];
        return core
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var value) ? value : 0)
            .ToArray();
    }
}
