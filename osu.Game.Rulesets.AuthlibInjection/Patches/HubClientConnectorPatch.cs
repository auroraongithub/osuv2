using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Rulesets.AuthlibInjection.Configuration;

namespace osu.Game.Rulesets.AuthlibInjection.Patches;

[HarmonyPatch(typeof(HubClientConnector), "BuildConnectionAsync")]
[HarmonyPriority(Priority.Low)]
public class HubClientConnectorPatch
{
    private const string ruleset_hash_header = @"X-Osu-Ruleset-Hashes";

    static bool Prefix(CancellationToken cancellationToken,
                       HubClientConnector __instance, ref Task<PersistentEndpointClient> __result)
    {
        if (!GlobalConfigManager.Patched || GlobalConfigManager.Config.NonG0V0Server)
        {
            return true;
        }

        GlobalConfigManager.InitializeHashCache();

        // Copy of HubClientConnector.BuildConnectionAsync
        string versionHash = Traverse.Create(__instance).Field("versionHash").GetValue<string>();
        string endpoint = Traverse.Create(__instance).Field("endpoint").GetValue<string>();
        IAPIProvider api = Traverse.Create(__instance).Field("API").GetValue<IAPIProvider>();

        var builder = new HubConnectionBuilder()
            .WithUrl(endpoint, options =>
            {
                // Configuring proxies is not supported on iOS, see https://github.com/xamarin/xamarin-macios/issues/14632.
                if (RuntimeInfo.OS != RuntimeInfo.Platform.iOS)
                {
                    // Use HttpClient.DefaultProxy once on net6 everywhere.
                    // The credential setter can also be removed at this point.
                    options.Proxy = WebRequest.DefaultWebProxy;
                    if (options.Proxy != null)
                        options.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }

                options.Headers.Add(@"Authorization", $"Bearer {api.AccessToken}");
                // non-standard header name kept for backwards compatibility, can be removed after server side has migrated to `VERSION_HASH_HEADER`
                options.Headers.Add(@"OsuVersionHash", versionHash);
                options.Headers.Add(HubClientConnector.VERSION_HASH_HEADER, versionHash);
                options.Headers.Add(HubClientConnector.CLIENT_SESSION_ID_HEADER, api.SessionIdentifier.ToString());
                options.Headers.Add(ruleset_hash_header,
                    JsonConvert.SerializeObject(GlobalConfigManager.HashCache.RulesetsHashes));
            });

        builder.AddMessagePackProtocol(options =>
        {
            options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS;
        });

        var newConnection = builder.Build();

        __instance.ConfigureConnection?.Invoke(newConnection);

        __result = Task.FromResult((PersistentEndpointClient)new HubClient(newConnection));
        return false;
    }
}
