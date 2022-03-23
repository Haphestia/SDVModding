using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMinecarts
{
    public class ModEntry : StardewModdingAPI.Mod
    {
		public List<MinecartInstance> Minecarts;

		//landing point needs to be within 5 tiles of the minecart for detections to work correctly.
		//multiple minecarts on the same map should be at least 15 tiles apart.

        public override void Entry(IModHelper helper)
        {
			helper.Content.AssetLoaders.Add(new AssetLoader());
			Dictionary<string, MinecartInstance> minecarts = Helper.Content.Load<Dictionary<string, MinecartInstance>>("JsonMinecarts.Minecarts", ContentSource.GameContent);
			Minecarts = new List<MinecartInstance>();
			Minecarts.AddRange(minecarts.Values);
			new JMCModHooks(this);
		}

        public bool OnMinecartActivation(GameLocation l, Vector2 vec)
        {
			if (Game1.player.mount != null) return true;
			if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom")) {
				if (!Game1.player.isRidingHorse() || Game1.player.mount == null) {
					drawMinecartDialogue(l);
					return true;
				}
				Game1.player.mount.checkAction(Game1.player, l);
			} else Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
			return true;
        }

		public int RawDistance(int x1, int y1, int x2, int y2)
        {
			return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

		public void drawMinecartDialogue(GameLocation l)
		{
			List<Response> responses = new List<Response>();
			foreach(var mc in Minecarts.OrderBy(x => x.DisplayName))
            {
				if (mc.LocationName == l.Name)
				{
					if (RawDistance(mc.LandingPointX, mc.LandingPointY, Game1.player.getTileX(), Game1.player.getTileY()) < 6) continue;
				}
				if (mc.MailCondition != null && !Game1.MasterPlayer.mailReceived.Contains(mc.MailCondition)) continue;
				responses.Add(new Response(mc.UniqueId, mc.DisplayName));
            }
			responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			Game1.activeClickableMenu = new DialogueBox(this, responses);
			Game1.dialogueUp = true;
			Game1.player.CanMove = false;
		}

		public bool onDialogueSelect(string key)
        {
			if (key == "Cancel") return true;
			MinecartInstance mc = Minecarts.Where(x => x.UniqueId == key).FirstOrDefault();
			if (mc == null) return true;
			Monitor.Log("Minecart key: " + key, LogLevel.Alert);
			WarpFarmer(mc);
			return true;
		}

		public bool IsMinecart(GameLocation l, Vector2 vec)
        {
            var tids = new List<int>{ 958, 1080, 1081 };
            int tid = l.getTileIndexAt(Utility.Vector2ToPoint(vec), "Buildings");
            if (tids.Contains(tid))
            {
				//this is a minecart, but is it one of ours?
				foreach(var mc in Minecarts.Where(mc => mc.LocationName == l.Name))
                {
					if(RawDistance(mc.LandingPointX, mc.LandingPointY, (int)vec.X, (int)vec.Y) < 6){
						return true; //found it!
                    }
                }
            }
			return false;
        }

        public void WarpFarmer(MinecartInstance mc)
        {
			if(mc.VanillaPassthrough != null)
            {
				//let vanilla handle this one (in case it's been overridden. usually just used for the vanilla carts.)
				Game1.player.currentLocation.answerDialogueAction(mc.VanillaPassthrough, new string[] { });
			} 
			else
            {
				//let's handle it ourselves!
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				Game1.warpFarmer(mc.LocationName, mc.LandingPointX, mc.LandingPointY, mc.LandingPointDirection);
                if (mc.IsUnderground)
                {
					if (Game1.getMusicTrackName() == "springtown")
					{
						Game1.changeMusicTrack("none");
					}
				}
            }
        }
    }
}
