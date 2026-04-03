using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.AuthlibInjection.Objects.Drawables
{
    public partial class DrawableAuthlibInjectionHitObject(AuthlibInjectionHitObject hitObject)
        : DrawableHitObject<AuthlibInjectionHitObject>(hitObject)
    {
    }
}
