using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class BodyWindow : BaseWindow
	{
		// Not currently shown in the UI - kept for future use.
		public static string DESCRIPTION_BODY_FIX => Strings.Descriptions.Body.BodyFix;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(560f, 320f);
			}
		}

		public BodyWindow(ThingDef race, Gender? gender = null) : base(race, gender)
		{
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			Tools.GBool(gui, state, Strings.Keys.FilterBody, Strings.Labels.Body.FilterBody, Strings.Descriptions.Body.FilterBody);

			if (state.GBool(Strings.Keys.FilterBody))
				foreach (BodyTypeDef def in DefDatabase<BodyTypeDef>.AllDefs)
				{
					if (def == BodyTypeDefOf.Baby)
						continue;

					if (def == BodyTypeDefOf.Child)
						continue;

					Tools.Bool(gui, state, $"{Strings.Keys.FilterBody}|{def.defName}", def.defName);
				}
		}
	}
}
