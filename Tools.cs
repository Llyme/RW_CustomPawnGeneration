using HarmonyLib;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace RW_CustomPawnGeneration
{
	public static class Tools
	{
		public static IList PawnsBeingGenerated =>
			AccessTools.Field(
				typeof(PawnGenerator),
				"pawnsBeingGenerated"
			).GetValue(null) as IList;

		public static IEnumerable<BodyTypeDef> AllCPGAdultBodyTypes
			(Settings.State global,
			Settings.State state,
			bool is_global)
		{
			foreach (BodyTypeDef def in DefDatabase<BodyTypeDef>.AllDefs)
				if (def != BodyTypeDefOf.Baby &&
					def != BodyTypeDefOf.Child &&
					def.CPGEnabled(global, state, is_global))
					yield return def;
		}

		public static void GBool
			(Listing_Standard gui,
			Settings.State state,
			string key,
			string header,
			string description = null)
		{
			if (state.global)
				Bool(gui, state, key, header, description);
			else
				ComboWindow.Entry(
					gui,
					state,
					key,
					header,
					description,
					EditWindow.COMBO_GLOBAL_BOOL
				);
		}

		public static bool Bool
			(Listing_Standard gui,
			Settings.State state,
			string key,
			string header,
			string description = null) =>
			Bool(gui, state, out bool _, key, header, description);

		public static bool Bool
			(Listing_Standard gui,
			Settings.State state,
			out bool updated,
			string key,
			string header,
			string description = null)
		{
			bool curr = state.Get(key) == 1;
			bool next = curr;

			gui.CheckboxLabeled(header, ref next, description);
			updated = curr != next;

			if (updated)
				state.Set(key, next ? 1 : 0);

			return next;
		}
	}
}
