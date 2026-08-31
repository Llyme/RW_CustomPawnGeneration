using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class Settings : ModSettings
	{
		public static Dictionary<string, int> GlobalIntDefaults =
			new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<string, int> LocalIntDefaults =
			new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<string, int> IntStates = null;

		public static void DoWindowContents(Rect inRect)
		{
			if (IntStates == null)
				IntStates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			Listing_Standard gui = new Listing_Standard
			{
				maxOneColumn = true
			};

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

		public static bool IsGlobal(State state, string key) =>
			state.Get(key) == 0;

		/// <summary>
		/// A bool value but has an option to redirect to the global config.
		/// </summary>
		public static bool Bool(
			Pawn pawn,
			bool useRaceSpecific,
			string key
		)
		{
			GetState(
				pawn,
				useRaceSpecific,
				out State global,
				out State state
			);

			int value = state.Get(key);

			if (state.global)
				return value == 1;

			if (value == 0)
				return global.Get(key) == 1;
			else
				return value == 2;
		}

		public static bool GBool(ThingDef race, string key)
		{
			GetStateMale(
				race,
				out State global,
				out State state
			);

			return Bool(global, state, key);
		}

		public static bool Bool(
			State global,
			State state,
			string key
		)
		{
			int value = state.Get(key);

			if (state.global)
				return value == 1;

			if (value == 0)
				return global.Get(key) == 1;

			return value == 2;
		}

		public static int Int(
			State global,
			State state,
			string key,
			bool isGlobal
		) =>
			isGlobal ? global.Get(key) : state.Get(key);

		public static bool Bool(
			State global,
			State state,
			string key,
			bool isGlobal
		) =>
			Int(global, state, key, isGlobal) == 1;

		/// <summary>
		/// Configuration state representing the male settings.
		/// If `SeparateGender` is not enabled,
		/// female configuration points to the male configuration.
		/// </summary>
		public static void GetStateMale(
			ThingDef race,
			out State global,
			out State state
		)
		{
			global = State.GLOBAL;
			state = new State(race);
		}

		/// <summary>
		/// Automatically points to the female configuration state if `SeparateGender` is enabled.
		/// </summary>
		public static void GetState(
			Pawn pawn,
			bool useRaceSpecific,
			out State global,
			out State state
		)
		{
			GetStateMale(
				useRaceSpecific ? pawn.kindDef.race : null,
				out global,
				out state
			);

			if (!pawn.RaceProps.hasGenders)
				return;

			if (pawn.gender != Gender.Female)
				return;

			if (!Bool(global, state, Strings.Keys.SeparateGender))
				return;

			global = State.FEMALE;
			state = new State(
				useRaceSpecific ? pawn.kindDef.race : null,
				pawn.gender
			);
		}

		public static void GetState(
			ThingDef race,
			Gender? gender,
			out State global,
			out State state
		)
		{
			GetStateMale(
				race,
				out global,
				out state
			);

			if (gender != null &&
				gender == Gender.Female &&
				Bool(global, state, Strings.Keys.SeparateGender))
			{
				global = State.FEMALE;
				state = new State(race, gender.Value);
			}
		}
	}
}
