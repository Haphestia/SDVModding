﻿using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace SDVFactory.Hooks
{
	public class ModHooks : StardewValley.ModHooks
	{
        StardewValley.ModHooks alias;

		public static void Patch() => new ModHooks();

		public ModHooks()
		{
			var field = typeof(Game1).GetField("hooks", BindingFlags.Static | BindingFlags.NonPublic);
            alias = (StardewValley.ModHooks)field.GetValue(null);
			field.SetValue(null, this);
		}

		public override void OnGame1_PerformTenMinuteClockUpdate(Action action)
		{
			alias.OnGame1_PerformTenMinuteClockUpdate(action);
		}

		public override void OnGame1_NewDayAfterFade(Action action)
		{
			alias.OnGame1_NewDayAfterFade(action);
		}

		public override void OnGame1_ShowEndOfNightStuff(Action action)
		{
			alias.OnGame1_ShowEndOfNightStuff(action);
		}

		public override void OnGame1_UpdateControlInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action)
		{
			alias.OnGame1_UpdateControlInput(ref keyboardState, ref mouseState, ref gamePadState, action);
		}

		public override void OnGameLocation_ResetForPlayerEntry(GameLocation location, Action action)
		{
			alias.OnGameLocation_ResetForPlayerEntry(location, action);
		}

		public override bool OnGameLocation_CheckAction(GameLocation location, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, Func<bool> action)
		{
			if(FGame.CheckAction(location, who, tileLocation)) return true;
			return alias.OnGameLocation_CheckAction(location, tileLocation, viewport, who, action);
		}

		public override FarmEvent OnUtility_PickFarmEvent(Func<FarmEvent> action)
		{
			return alias.OnUtility_PickFarmEvent(action);
		}

		public override Task StartTask(Task task, string id)
		{
			return alias.StartTask(task, id);
		}

		public override Task<T> StartTask<T>(Task<T> task, string id)
		{
			return alias.StartTask<T>(task, id);
		}
	}
}
