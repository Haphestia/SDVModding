using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class LoadableAssets
    {
        //location tilemaps are handled automatically
        internal static Dictionary<string, string> LoadableAssetMap => new Dictionary<string, string>() {
            {"factorytiles", "Assets\\factorytiles.png"},
            {"bwdy.FactoryMod.Textures.Machines", "Assets\\machines.png"}
        };
    }
}
