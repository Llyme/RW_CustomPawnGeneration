﻿using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class Settings : ModSettings
	{
		public static Dictionary<string, int> IntDefaults = new Dictionary<string, int>();
		public static Dictionary<string, int> IntStates = null;

		public static void DoWindowContents(Rect inRect)
		{
			if (IntStates == null)
				IntStates = new Dictionary<string, int>();

			Listing_Standard gui = new Listing_Standard();

			gui.Begin(inRect);
			{
				Draw_Root(gui, inRect);
			}
			gui.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Collections.Look(ref IntStates, "IntStates", LookMode.Value);
		}

		public static bool IsGlobal(State state, string key)
		{
			return state.Get(key) == 0;
		}

		/// <summary>
		/// A bool value but has an option to redirect to the global config.
		/// </summary>
		public static bool Bool(Pawn pawn, string key)
		{
			GetState(pawn, out State global, out State state);

			int value = state.Get(key);

			if (value == 0)
				return global.Get(key) == 1;
			else
				return value == 2;
		}

		public static bool Bool(State global, State state, string key)
		{
			int value = state.Get(key);

			if (value == 0)
				return global.Get(key) == 1;
			else
				return value == 2;
		}

		public static int Int(State global, State state, string key, bool isGlobal)
		{
			if (isGlobal)
				return global.Get(key);
			else
				return state.Get(key);
		}

		public static bool Bool(State global, State state, string key, bool isGlobal)
		{
			return Int(global, state, key, isGlobal) == 1;
		}

		public static void GetState(Pawn pawn, out State global, out State state)
		{
			global = new State(null);
			state = new State(pawn.kindDef.race);

			if (pawn.RaceProps.hasGenders &&
				pawn.gender == Gender.Female &&
				Bool(global, state, GenderWindow.SeparateGender))
			{
				global = new State(pawn.gender);
				state = new State(pawn.gender, pawn.kindDef.race);
			}
		}
	}
}
