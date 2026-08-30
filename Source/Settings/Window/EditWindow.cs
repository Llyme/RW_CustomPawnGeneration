using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class EditWindow : BaseWindow
	{
		public static string[] COMBO_BOOL => new string[]
		{
				Strings.Labels.Combo.Disabled,
				Strings.Labels.Combo.Enabled
		};

		public static string[] COMBO_GLOBAL_BOOL => new string[]
		{
				Strings.Labels.Combo.UseGlobalConfig,
				Strings.Labels.Combo.Disabled,
				Strings.Labels.Combo.Enabled
		};

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(560f, 560f);
			}
		}

		public EditWindow(ThingDef race = null) : base(race)
		{
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			if (hasGenders)
			{
				if (gui.ButtonText(Strings.Labels.Section.Gender))
					new GenderWindow(race);

				gui.Gap(20f);
			}

			float height = gui.CurHeight;
			float width = gui.ColumnWidth;

			bool gender;

			if (race != null)
				gender = Settings.Bool(new Settings.State(null), state, Strings.Keys.SeparateGender);
			else
				gender = state.Bool(Strings.Keys.SeparateGender);

			gui.ColumnWidth = width / 2f - 8f;
			{
				Draw_Gender(inRect, gui, gender ? (Gender?)Gender.Male : null);
			}

			if (!gender)
				return;

			gui.NewColumn();
			gui.Gap(height);
			{
				Draw_Gender(inRect, gui, Gender.Female);
			}
		}

		public void Draw_Gender(Rect inRect, Listing_Standard gui, Gender? gender = null)
		{
			if (gender != null)
			{
				gui.Label(gender.Value.ToString());
				gui.Gap(10f);
			}

			if (gui.ButtonText(Strings.Labels.Section.Age))
				new AgeWindow(race, gender);

			if (isHumanlike)
			{
				if (gui.ButtonText(Strings.Labels.Section.Body))
					new BodyWindow(race, gender);

				if (gui.ButtonText(Strings.Labels.Section.Traits))
					new TraitsWindow(race, gender);
			}

			if (race != null &&
				gui.ButtonText(Strings.Labels.Section.Hediff))
				new HediffWindow(race, gender);
		}
	}
}
