using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class AgeWindow : BaseWindow
	{
		public string _MinAgeBuffer = "";
		public string _MaxAgeBuffer = "";
		public string _AgeTickBuffer = "";

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(480f, 480f);
			}
		}

		public AgeWindow(ThingDef race, Gender? gender = null) : base(race, gender)
		{
			_MinAgeBuffer = state.Get(Strings.Keys.MinAge).ToString();
			_MaxAgeBuffer = state.Get(Strings.Keys.MaxAge).ToString();
			_AgeTickBuffer = state.Get(Strings.Keys.AgeTick).ToString();
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			bool _HasMinAge = state.GBool(Strings.Keys.HasMinAge);
			bool _HasMaxAge = state.GBool(Strings.Keys.HasMaxAge);
			bool _HasAgeTick = state.GBool(Strings.Keys.HasAgeTick);
			int _MinAge = state.Get(Strings.Keys.MinAge);
			int _MaxAge = state.Get(Strings.Keys.MaxAge);
			int _AgeTick = state.Get(Strings.Keys.AgeTick);

			Tools.GBool(gui, state, Strings.Keys.AgeCurve, Strings.Labels.Age.AgeCurve, Strings.Descriptions.Age.AgeCurve);
			Tools.GBool(gui, state, Strings.Keys.MaxAgeChrono, Strings.Labels.Age.MaxAgeChrono, Strings.Descriptions.Age.MaxAgeChrono);
			Tools.GBool(gui, state, Strings.Keys.HasMinAge, Strings.Labels.Age.HasMinAge, Strings.Descriptions.Age.HasMinAge);

			if (_HasMinAge)
			{
				Tools.GBool(gui, state, Strings.Keys.MinAgeSoft, Strings.Labels.Age.MinAgeSoft, Strings.Descriptions.Age.MinAgeSoft);

				gui.TextFieldNumericLabeled(
					Strings.Labels.Age.MinAge,
					ref _MinAge,
					ref _MinAgeBuffer,
					0,
					_HasMaxAge ? _MaxAge : 1E+09f
				);
				state.Set(Strings.Keys.MinAge, _MinAge);

				gui.Gap(10f);
			}

			Tools.GBool(gui, state, Strings.Keys.HasMaxAge, Strings.Labels.Age.HasMaxAge, Strings.Descriptions.Age.HasMaxAge);

			if (_HasMaxAge)
			{
				gui.TextFieldNumericLabeled(
					Strings.Labels.Age.MaxAge,
					ref _MaxAge,
					ref _MaxAgeBuffer,
					_HasMinAge ? _MinAge : 0
				);
				state.Set(Strings.Keys.MaxAge, _MaxAge);
			}

			Tools.GBool(gui, state, Strings.Keys.HasAgeTick, Strings.Labels.Age.HasAgeTick, Strings.Descriptions.Age.HasAgeTick);

			if (_HasAgeTick)
			{
				gui.TextFieldNumericLabeled(
					Strings.Labels.Age.AgeTick,
					ref _AgeTick,
					ref _AgeTickBuffer
				);

				state.Set(Strings.Keys.AgeTick, _AgeTick);
			}
		}
	}
}
