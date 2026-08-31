using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class BodyPartWindow : BaseWindow
	{
		public string Search = "";

		public BodyPartRecord part;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(480f, 480f);
			}
		}

		public BodyPartWindow(BodyPartRecord part, ThingDef race, Gender? gender) : base(race, gender)
		{
			this.part = part;
		}

		public override void Draw_Outside(Rect inRect, Listing_Standard gui)
		{
			Text.Font = GameFont.Tiny;
			{
				gui.Label(part != null ? part.Label : Strings.Labels.Hediff.NoBodyPart);
			}
			Text.Font = GameFont.Small;

			gui.Gap(10f);

			Search = gui.TextEntryLabeled(Strings.Labels.Search, Search).ToLower();
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			Text.Font = GameFont.Tiny;
			{
				string prefix = "Hediff|";

				if (part != null)
					prefix += $"{part.Label}|";

				foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs)
					try
					{
						string key = prefix + def.defName;
						float _Value = state.Get(key);
						string label = $"[{def.defName}] {def.label}: {_Value}%";

						if (Search.Length > 0 && !key.ToLower().Contains(Search))
							continue;

						{
							gui.Label(label);
							_Value = gui.Slider(_Value, 0, 100);
						}
						state.Set(key, (int)_Value);
					}
					catch { }
			}
			Text.Font = GameFont.Small;
		}
	}
}
