using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class BodyWindow : BaseWindow
	{
		public const string DESCRIPTION_BODY_FIX =
			"Some backstories will give pawns average body of the opposite gender. " +
			"Enabling this will disable it.";
		public const string DESCRIPTION_FILTER_BODY =
			"When enabled, allows you to disable body types. " +
			"Body types that are not checked will be disabled. " +
			"There should be at least 1 body type. " +
			"This only applies to humans.";

		public const string FILTER_BODY = "Filter Body Types";
		public const string FilterBody = "FilterBody";

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
			Tools.GBool(gui, state, FilterBody, FILTER_BODY, DESCRIPTION_FILTER_BODY);

			if (state.GBool(FilterBody))
				foreach (BodyTypeDef def in DefDatabase<BodyTypeDef>.AllDefs)
				{
					if (def == BodyTypeDefOf.Baby)
						continue;

					if (def == BodyTypeDefOf.Child)
						continue;

					Tools.Bool(gui, state, $"{FilterBody}|{def.defName}", def.defName);
				}
		}
	}
}
