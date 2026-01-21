namespace Sister_Communication.Static;

public static class DataUtils
{
    public static int Score(string input, string candidate)
    {
        var i = input.ToLowerInvariant();
        var c = candidate.ToLowerInvariant();

        if (c == i) return 0;
        if (c.StartsWith(i) || i.StartsWith(c)) return 1;
        if (c.Contains(i) || i.Contains(c)) return 2;
        return 3;
    }
}