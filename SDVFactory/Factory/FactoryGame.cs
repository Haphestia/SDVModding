using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDVFactory.Factory
{
    internal static class FactoryGame
    {
        internal static Logger Logger;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            Logger = new Logger(monitor);
            Logger.Info("Hello from Factory! Mod initialized okay.");
        }
    }
}
