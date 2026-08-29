using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class Settings
	{
		public static string Search_Buffer = "";

		//public static bool AdvancedMode = false;

		public static Vector2 scrollVector = Vector2.zero;
		public static float scrollHeight = 0f;

		public static List<ThingDef> races = null;

		public static void Draw_Root_Race_Reset(ThingDef race)
		{
			Find.WindowStack.Add(new Dialog_MessageBox(
				Strings.Messages.Reset(race != null ? race.defName : Strings.Labels.GlobalConfig),
				Strings.Labels.Yes,
				() =>
				{
					new State(race, Gender.Female).Clear();
					new State(race, Gender.Male).Clear();
				},
				Strings.Labels.No
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
					Strings.Labels.Edit,
					Strings.Labels.CopyTo,
					Strings.Labels.Reset
				);
			else
				new ComboWindow(
					Callback,
					Strings.Labels.GlobalConfig,
					Strings.Descriptions.GlobalConfig,
					Strings.Labels.Edit,
					Strings.Labels.Reset
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
				Tools.Bool(
					gui,
					State.GLOBAL,
					Strings.Keys.AdvancedMode,
					Strings.Labels.AdvancedMode,
					Strings.Descriptions.AdvancedMode
				);
				Tools.Bool(
					gui,
					State.GLOBAL,
					Strings.Keys.UseRaceSpecific,
					Strings.Labels.UseRaceSpecific,
					Strings.Descriptions.UseRaceSpecific
				);

				bool _CustomAging = Tools.Bool(
					gui,
					State.GLOBAL,
					out bool _CustomAgingUpdated,
					Strings.Keys.CustomAging,
					Strings.Labels.CustomAging,
					Strings.Descriptions.CustomAging
				);

				bool _UngenderedParent = Tools.Bool(
					gui,
					State.GLOBAL,
					out bool _UngenderedParentUpdated,
					Strings.Keys.UngenderedParent,
					Strings.Labels.UngenderedParent,
					Strings.Descriptions.UngenderedParent
				);

				//gui.CheckboxLabeled(ADVANCED_MODE, ref AdvancedMode, DESCRIPTION_ADVANCED_MODE);


				// Patch/unpatch hooks since this is heavy on performance.

				if (_CustomAgingUpdated)
					if (_CustomAging)
						Patch_Pawn_AgeTracker_AgeTickInterval.module.Patch();
					else
						Patch_Pawn_AgeTracker_AgeTickInterval.module.Unpatch();

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

			if (!State.GLOBAL.Bool(Strings.Keys.AdvancedMode))
			{
				if (gui.ButtonText(Strings.Labels.ShowConfig))
					new EditWindow();

				if (gui.ButtonText(Strings.Labels.Reset))
					Find.WindowStack.Add(new Dialog_MessageBox(
						Strings.Messages.Reset(Strings.Labels.GlobalConfig),
						Strings.Labels.Yes,
						() => new State(null).Clear(),
						Strings.Labels.No
					));

				return;
			}


			// Advanced Settings

			if (gui.ButtonText(Strings.Labels.ResetAll))
				Find.WindowStack.Add(new Dialog_MessageBox(
					Strings.Descriptions.ResetAll,
					Strings.Labels.Yes,
					() =>
					{
						new State(null).Clear();

						foreach (ThingDef race in races)
							new State(race).Clear();
					},
					Strings.Labels.No
				));

			Search_Buffer = gui.TextEntryLabeled(Strings.Labels.Search, Search_Buffer);

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
					else if (gui.ButtonText(Strings.Labels.GlobalConfig))
						Draw_Root_Race(null);

				scrollHeight = gui.CurHeight - height;
			}
			Widgets.EndScrollView();
		}
	}
}
