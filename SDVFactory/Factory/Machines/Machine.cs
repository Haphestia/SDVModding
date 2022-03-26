using StardewValley;
using StardewValley.Objects;
using xTile.Dimensions;

namespace SDVFactory.Factory.Machines
{
    public class Machine
    {
        public long Id { get; set; } = -1;

        public virtual void OnActivate(GameLocation l, Farmer who, Furniture f, Location vect)
        {
            FactoryGame.Logger.Alert("Welcome to machine: " + Id);
        }

        public static Furniture CreateOne()
        {
            long id = ++FactoryGame.World.NextMachineId;
            var m = new Machine();
            m.Id = id;
            FactoryGame.World.Machines.Add(id, m);
            var f = new Furniture("Factory.FurnitureTest", Microsoft.Xna.Framework.Vector2.Zero);
            f.modData.Add("FactoryMod", "true");
            f.modData.Add("FactoryId", id.ToString());
            return f;
        }
    }
}
