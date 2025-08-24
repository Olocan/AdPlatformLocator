using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace AdPlatformLocatorApi.Services
{
public sealed class InMemoryPlatformIndex
{
    private ImmutableDictionary<string, ImmutableHashSet<string>> _map = ImmutableDictionary<string, ImmutableHashSet<string>>.Empty;

    private static readonly Regex LineRegex = new(
        pattern: @"^\s*(?<name>[^:]+)\s*:\s*(?<locs>.+?)\s*$",
        options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public (int TotalPlatforms, int TotalLocations) LoadFromText(string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableHashSet<string>>(StringComparer.Ordinal);
        int platformCount = 0;
        int locationRefs = 0;

        foreach (var rawLine in text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue; 

            var m = LineRegex.Match(line);
            if (!m.Success) continue; 

            var name = m.Groups["name"].Value.Trim();
            var locs = m.Groups["locs"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (string.IsNullOrWhiteSpace(name) || locs.Length == 0) continue;
            platformCount++;

            foreach (var rawLoc in locs)
            {
                var loc = Normalize(rawLoc);
                if (loc is null) continue;
                locationRefs++;

                if (!builder.TryGetValue(loc, out var set))
                {
                    set = ImmutableHashSet.Create<string>(StringComparer.Ordinal);
                }
                builder[loc] = set.Add(name);
            }
        }

        
        System.Threading.Interlocked.Exchange(ref _map, builder.ToImmutable());
        return (platformCount, locationRefs);
    }

    public List<string> FindPlatforms(string location)
    {
        var normalized = Normalize(location) ?? "/";
        var prefixes = EnumeratePrefixes(normalized);

        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var prefix in prefixes)
        {
            if (_map.TryGetValue(prefix, out var set))
            {
                result.UnionWith(set);
            }
        }
        return result.OrderBy(s => s, StringComparer.Ordinal).ToList();
    }

    private static IEnumerable<string> EnumeratePrefixes(string loc)
    {
        
        if (loc != "/") yield return "/";
        int idx = 1;
        while (true)
        {
            int next = loc.IndexOf('/', idx);
            if (next == -1) break;
            yield return loc.Substring(0, next);
            idx = next + 1;
        }
        yield return loc;
    }

    private static string? Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var s = input.Trim();
        if (!s.StartsWith('/')) s = "/" + s;
        
        while (s.Contains("//")) s = s.Replace("//", "/");
        
        if (s.Length > 1 && s.EndsWith('/')) s = s.TrimEnd('/');
        return s;
    }
}
}
