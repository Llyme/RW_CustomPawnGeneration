using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class GenderWindow : BaseWindow
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(480f, 320f);
			}
		}

		public GenderWindow(ThingDef race) : base(race)
		{
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			int _GenderSlider = state.Get(Strings.Keys.GenderSlider);
			{
				Tools.GBool(gui, state, Strings.Keys.SeparateGender, Strings.Labels.Gender.SeparateGender, Strings.Descriptions.Gender.SeparateGender);
				Tools.GBool(gui, state, Strings.Keys.UnforcedGender, Strings.Labels.Gender.UnforcedGender, Strings.Descriptions.Gender.UnforcedGender);
				Tools.GBool(gui, state, Strings.Keys.OverrideGender, Strings.Labels.Gender.OverrideGender, Strings.Descriptions.Gender.OverrideGender);
				Tools.GBool(gui, state, Strings.Keys.ModifyAggressively, Strings.Labels.Gender.ModifyAggressively, Strings.Descriptions.Gender.ModifyAggressively);

				if (state.GBool(Strings.Keys.OverrideGender))
				{
					gui.Gap(10f);

					gui.LabelDouble($"{100 - _GenderSlider}% {Strings.Labels.Gender.Male}", $"{_GenderSlider}% {Strings.Labels.Gender.Female}");
					_GenderSlider = (int)gui.Slider(_GenderSlider, 0, 100);
				}
			}
			state.Set(Strings.Keys.GenderSlider, _GenderSlider);
		}
	}
}
