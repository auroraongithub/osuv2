using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Logging;
using osu.Game.Utils;

namespace osu.Game.Rulesets.AuthlibInjection.Patches
{
    public static class DisableSentryPatch
    {
        private const BindingFlags instance_flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField;

        /// <summary>
        /// Dispose the Sentry telemetry logger.
        /// </summary>
        /// <param name="game">The <see cref="OsuGame"/> instance where the logger exists.</param>
        /// <remarks>Most of this workaround comes from
        /// https://github.com/MATRIX-feather/LLin/blob/ruleset/osu.Game.Rulesets.Hikariii/Features/ListenerLoader/Handlers/SentryLoggerDisabler.cs</remarks>
        public static void Run(OsuGame game)
        {
            try
            {
                var field = game.GetType().GetFields(instance_flag)
                                .FirstOrDefault(f => f.FieldType == typeof(SentryLogger));

                if (field == null)
                {
                    Logger.Log("Cannot find SentryLogger. It might have been disposed.");
                    return;
                }

                object val = field.GetValue(game);
                if (val is not SentryLogger sentryLogger)
                    throw new Exception($"Type mismatch: expect SentryLogger, got {val?.GetType()}.");

                sentryLogger.Dispose();
                Logger.Log("SentryLogger has been disposed successfully.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to disable sentry logger.");
            }
        }
    }
}
