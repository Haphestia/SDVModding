using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDVFactory.Data
{
    internal static class Machines
    {
        internal static Dictionary<string, Machine> MachineList = new Dictionary<string, Machine>()
        {
            { "Generator1", new Machine() {
                ShortId = "Generator1",
                DisplayName = "Primitive Generator",
                Description = "Generates power from solid fuels.",
                Size = new Point(2, 4),
                TextureName = "bwdy.FactoryMod.Textures.Machines",
                TextureTopLeftTile = new Point(0,0)
            }}
        };
    }

    internal class Machine
    {
        public string UniqueId { get => "bwdy.FactoryMod.Machines." + ShortId; }
        public string ShortId;
        public string DisplayName;
        public string Description;
        public Point Size;
        public string TextureName;
        public Point TextureTopLeftTile;
    }
}
