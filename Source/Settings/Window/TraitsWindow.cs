using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class TraitsWindow : BaseWindow
	{
		public static string[] COMBO_TRAITS => new string[]
		{
			Strings.Labels.Traits.Normal,
			Strings.Labels.Traits.Blocked,
			Strings.Labels.Traits.Forced
		};

		public string Search = "";

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(640f, 640f);
			}
		}

		public TraitsWindow(ThingDef race, Gender? gender = null) : base(race, gender)
		{
		}

		public override void Draw_Outside(Rect inRect, Listing_Standard gui)
		{
			Text.Font = GameFont.Tiny;
			{
				gui.Label(Strings.Descriptions.Traits.Info);
			}
			Text.Font = GameFont.Small;

			gui.Gap(10f);

			Tools.GBool(gui, state, Strings.Keys.OverrideTraits, Strings.Labels.Traits.OverrideTraits, Strings.Descriptions.Traits.OverrideTraits);

			gui.Gap(10f);

			if (!state.GBool(Strings.Keys.OverrideTraits))
				return;

			Search = gui.TextEntryLabeled(Strings.Labels.Search, Search).ToLower();

			gui.Gap(10f);

			if (gui.ButtonText(Strings.Labels.Reset))
				Find.WindowStack.Add(new Dialog_MessageBox(
					Strings.Descriptions.Traits.Reset,
					Strings.Labels.Yes,
					() =>
					{
						foreach (TraitDef def in DefDatabase<TraitDef>.AllDefs)
							try
							{
								foreach (TraitDegreeData data in def.degreeDatas)
									state.Remove($"{Strings.Keys.Trait}|{def.defName}|{data.degree}");
							}
							catch { }
					},
					Strings.Labels.No
				));
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			if (!state.GBool(Strings.Keys.OverrideTraits))
				return;

			IEnumerable<TraitDef> defs = DefDatabase<TraitDef>.AllDefs;

			foreach (TraitDef def in defs)
				try
				{
					foreach (TraitDegreeData data in def.degreeDatas)
					{
						string label = $"[{def.defName}] {data.label ?? def.label}";

						if (label.ToLower().Contains(Search))
							ComboWindow.Entry(
								gui,
								state,
								$"{Strings.Keys.Trait}|{def.defName}|{data.degree}",
								label,
								data.description ?? def.description,
								COMBO_TRAITS
							);
					}
				}
				catch { }
		}
	}
}
