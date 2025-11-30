using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class Settings
	{
		public const string DESCRIPTION_ADVANCED_MODE =
			"Shows individual settings for each race.";
		public const string DESCRIPTION_CUSTOM_AGING =
			"When enabled, allows pawns to have a custom aging speed (in game ticks.) " +
			"This option may slow down your game significiantly, particularly on larger colonies. " +
			"This does not affect other age-related options.\n" +
			"Requires a restart to take effect.";
		public const string DESCRIPTION_GLOBAL_CONFIG =
			"All races that do not have any modified settings or " +
			"uses the [Use Global Config] option will refer to this instead. " +
			"Some options will only be applied when necessary (body types for humanoid races only, etc.)";
		public const string DESCRIPTION_UNGENDERED_PARENT =
			"When enabled, all pawns can either be a mother or father, " +
			"regardless of gender.\n" +
			"Some have reported this to cause lag, " +
			"possibly from a mod incompatibility.\n" +
			"If this causes lag for you, please disable this.\n" +
			"Requires a restart to take effect.";

		public const string RESET = "Reset";
		public const string YES = "Yes";
		public const string NO = "No";
		public const string COPY_TO = "Copy to...";
		public const string EDIT = "Edit";	
		public const string SHOW_CONFIG = "Show Config";
		public const string ADVANCED_MODE = "Advanced Settings";
		public const string CUSTOM_AGING = "Enable Custom Aging Ticks";
		public const string UNGENDERED_PARENT = "Remove Parent Gender Restrictions";
		public const string GLOBAL_CONFIG = "[Global Config]";
		public const string SEARCH = "Search ";

		public const string AdvancedMode = "AdvancedMode";
		public const string CustomAging = "CustomAging";
		public const string UngenderedParent = "UngenderedParent";

		public static string Search_Buffer = "";

		//public static bool AdvancedMode = false;

		public static Vector2 scrollVector = Vector2.zero;
		public static float scrollHeight = 0f;

		public static List<ThingDef> races = null;

		public static string HEADER_RESET(string v) =>
			$"This will restore all the default values to the '{v}' settings and cannot be undone. Are you sure?";

		public static void Draw_Root_Race_Reset(ThingDef race)
		{
			Find.WindowStack.Add(new Dialog_MessageBox(
				HEADER_RESET(race != null ? race.defName : GLOBAL_CONFIG),
				YES,
				() =>
				{
					new State(race, Gender.Female).Clear();
					new State(race, Gender.Male).Clear();
				},
				NO
			));
		}

		public static void Draw_Root_Race(ThingDef race)
		{
			void Callback(int i)
			{
				switch (i)
				{
					case 0:
						new EditWindow(race);
						break;
					case 1:
						if (race != null)
							new CopyWindow(race);
						else
							Draw_Root_Race_Reset(race);
						break;
					case 2:
						Draw_Root_Race_Reset(race);
						break;
				}
			}

			if (race != null)
				new ComboWindow(
					Callback,
					$"[{race.defName}] {race.LabelCap}",
					race.DescriptionDetailed,
					EDIT,
					COPY_TO,
					RESET
				);
			else
				new ComboWindow(
					Callback,
					GLOBAL_CONFIG,
					DESCRIPTION_GLOBAL_CONFIG,
					EDIT,
					RESET
				);
		}

		public static void Draw_Root(Listing_Standard gui, Rect inRect)
		{
			if (races == null)
			{
				races = new List<ThingDef> { null };
				
				foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
					if (def.race != null)
						races.Add(def);
			}

			float width = gui.ColumnWidth;

			gui.ColumnWidth = width * 0.5f;
			{
				Tools.Bool(gui, State.GLOBAL, AdvancedMode, ADVANCED_MODE, DESCRIPTION_ADVANCED_MODE);

				bool _CustomAging = Tools.Bool(
					gui,
					State.GLOBAL,
					out bool _CustomAgingUpdated,
					CustomAging,
					CUSTOM_AGING,
					DESCRIPTION_CUSTOM_AGING
				);

				bool _UngenderedParent = Tools.Bool(
					gui,
					State.GLOBAL,
					out bool _UngenderedParentUpdated,
					UngenderedParent,
					UNGENDERED_PARENT,
					DESCRIPTION_UNGENDERED_PARENT
				);

				//gui.CheckboxLabeled(ADVANCED_MODE, ref AdvancedMode, DESCRIPTION_ADVANCED_MODE);


				// Patch/unpatch hooks since this is heavy on performance.

				if (_CustomAgingUpdated)
					if (_CustomAging)
						Patch_Pawn_AgeTracker_AgeTick.module.Patch();
					else
						Patch_Pawn_AgeTracker_AgeTick.module.Unpatch();

				if (_UngenderedParentUpdated)
					if (_UngenderedParent)
					{
						Patch_ParentRelationUtility_GetFather.module.Patch();
						Patch_ParentRelationUtility_GetMother.module.Patch();
					}
					else
					{
						Patch_ParentRelationUtility_GetFather.module.Unpatch();
						Patch_ParentRelationUtility_GetMother.module.Unpatch();
					}
			}

			gui.Gap(20f);


			// Basic Settings

			if (!State.GLOBAL.Bool(AdvancedMode))
			{
				if (gui.ButtonText(SHOW_CONFIG))
					new EditWindow();

				if (gui.ButtonText(RESET))
					Find.WindowStack.Add(new Dialog_MessageBox(
						HEADER_RESET(GLOBAL_CONFIG),
						YES,
						() => new State(null).Clear(),
						NO
					));

				return;
			}


			// Advanced Settings

			Search_Buffer = gui.TextEntryLabeled(SEARCH, Search_Buffer);

			float height = gui.CurHeight;

			Widgets.BeginScrollView(
				new Rect(
					0f,
					height,
					gui.ColumnWidth + 20f,
					inRect.height - height - 40f
				),
				ref scrollVector,
				new Rect(
					0f,
					height,
					gui.ColumnWidth - 16f,
					//inRect.height + height - 40f + races.Count * 24f
					scrollHeight
				)
			);
			{
				foreach (ThingDef race in races)
					if (race != null)
					{
						if (Search_Buffer.Length == 0 ||
							race.defName.ToLower().Contains(Search_Buffer) ||
							race.LabelCap.ToLower().ToStringSafe().Contains(Search_Buffer))
							if (gui.ButtonText(race.defName))
								Draw_Root_Race(race);
					}
					else if (gui.ButtonText(GLOBAL_CONFIG))
						Draw_Root_Race(null);

				scrollHeight = gui.CurHeight - height;
			}
			Widgets.EndScrollView();
		}
	}
}
