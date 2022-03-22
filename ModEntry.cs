using StardewModdingAPI;
using System;

namespace SDVFactory
{
    public class ModEntry : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Factory.FactoryGame.Initialize(helper, Monitor);
        }
    }
}
