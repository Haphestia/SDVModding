using StardewModdingAPI;
using SDVFactory.Hooks;

namespace SDVFactory
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            ModHooks.Patch();
            Harmony.Patch();
            Assets.Patch(this);
            Misc.Patch();
            FGame.Initialize(this);
        }
    }
}
