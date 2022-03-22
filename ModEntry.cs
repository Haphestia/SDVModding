using StardewModdingAPI;
using System;

namespace SDVFactory
{
    public class ModEntry : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Hello world from Factory!", LogLevel.Alert);
        }
    }
}
