using StardewModdingAPI;

namespace SDVFactory
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            new FactoryModHooks();
            Harmony.Patch();

            var am = new AssetManager(this);
            helper.Content.AssetLoaders.Add(am);
            helper.Content.AssetEditors.Add(am);
            Factory.FactoryGame.Initialize(helper, Monitor);
        }
    }
}
