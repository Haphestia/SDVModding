using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDVFactory.Data
{
    internal class Factoryverse
    {
        public long NextMachineId { get; set; } = -1;
        public List<long> RecycleableIds = new List<long>();
        public Dictionary<long, MachineState> MachineStates { get; set; } = new Dictionary<long, MachineState>();

        public long CreateMachineState(string shortId)
        {
            long id = 0;
            if(RecycleableIds.Count > 0)
            {
                id = RecycleableIds[0];
                RecycleableIds.RemoveAt(0);
            } else
            {
                id = ++NextMachineId;
            }
            MachineStates[id] = new MachineState(shortId, id);
            return id;
        }

        public void DestroyMachineState(long id)
        {
            if (MachineStates.ContainsKey(id))
            {
                MachineStates.Remove(id);
                RecycleableIds.Add(id);
            }
        }

        public void Tick(uint elapsedGameMinutes)
        {
            //max one day
            if (elapsedGameMinutes > 1440) elapsedGameMinutes = 1400;
            while (elapsedGameMinutes-- > 0)
                foreach (var machine in MachineStates.Values) machine.OnTick();
        }

        public void ActivateMachine(GameLocation l, Farmer who, Furniture f, xTile.Dimensions.Location vect, long machineNumber)
        {
            if (MachineStates.ContainsKey(machineNumber))
            {
                MachineStates[machineNumber].OnActivate(l, who, f, vect);
            }
        }
    }
}
