#nullable enable
using System.Collections.Generic;
using System.IO;
using osu.Framework.Extensions;

namespace osu.Game.Rulesets.AuthlibInjection;

public class RulesetHashCache
{
    private readonly Dictionary<int, string> onlineIdToName = new();
    public readonly Dictionary<string, string> RulesetsHashes = new();

    public RulesetHashCache(RulesetStore store)
    {
        foreach (var rulesetInfo in store.AvailableRulesets)
        {
            if (rulesetInfo.OnlineID is >= 0 and <= 3)
            {
                // Skip official rulesets as their hashes are hardcoded elsewhere.
                // Read it maybe crashes in some environments (like Android).
                continue;
            }

            Ruleset instance = rulesetInfo.CreateInstance();
            using var str = File.OpenRead(instance.GetType().Assembly.Location);
            RulesetsHashes[instance.ShortName] = str.ComputeMD5Hash();

            if (rulesetInfo.OnlineID >= 0)
            {
                onlineIdToName[rulesetInfo.OnlineID] = instance.ShortName;
            }
        }
    }

    public string? GetHash(string shortName)
    {
        RulesetsHashes.TryGetValue(shortName, out string? hash);
        return hash;
    }

    public string? GetHash(RulesetInfo rulesetInfo)
    {
        return GetHash(rulesetInfo.ShortName);
    }

    public string? GetHash(Ruleset ruleset)
    {
        return GetHash(ruleset.ShortName);
    }

    public string? GetHash(int rulesetId)
    {
        if (onlineIdToName.TryGetValue(rulesetId, out string? shortName))
        {
            return GetHash(shortName);
        }

        return null;
    }
}
