using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class HediffWindow : BaseWindow
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(320f, 480f);
			}
		}

		public HediffWindow(ThingDef race, Gender? gender) : base(race, gender)
		{
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			if (gui.ButtonText(Strings.Labels.Hediff.NoBodyPart))
				new BodyPartWindow(null, race, gender);

			foreach (BodyPartRecord part in race.race.body.AllParts)
				try
				{
					if (gui.ButtonText($"[{part.def.defName}] {part.Label}"))
						new BodyPartWindow(part, race, gender);
				}
				catch { }
		}
	}
}
