using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class AgeWindow : BaseWindow
	{
		public const string DESCRIPTION_AGE_CURVE =
			"If enabled, this will attempt to translate a pawn's current age " +
			"to the new age limit. This may not be accurate but it would " +
			"generate a similar effect with the vanilla age variation. " +
			"Disabling this will only clamp the pawn's age.";
		public const string DESCRIPTION_HAS_MIN_AGE =
			"If enabled, this will make sure all pawns " +
			"can't be below a certain age. " +
			"This will also affect pregnancy mods, " +
			"meaning newborn babies will start at the minimum age.";
		public const string DESCRIPTION_MIN_AGE_SOFT =
			"If enabled, pawns being generated below " +
			"the minimum age will not be changed (e.g. baby pawns).";
		public const string DESCRIPTION_HAS_MAX_AGE =
			"If enabled, this will make sure all pawns " +
			"can't be above a certain age.";
		public const string DESCRIPTION_MAX_AGE_CHRONO =
			"If enabled, this will increase the pawn's chronological age " +
			"on every birthday if exceeding the maximum age.";
		public const string DESCRIPTION_HAS_AGE_TICK =
			"If enabled, allows you to change how fast the pawns age.";

		public const string AGE_CURVE = "Preserve Age Curve";
		public const string HAS_MIN_AGE = "Has Minimum Age";
		public const string MIN_AGE_SOFT = "Do Not Affect Pawns Below Minimum Age";
		public const string MIN_AGE = "Minimum Age ";
		public const string HAS_MAX_AGE = "Has Maximum Age";
		public const string MAX_AGE_CHRONO = "Add Excess Biological Age as Chronological Age";
		public const string MAX_AGE = "Maximum Age ";
		public const string HAS_AGE_TICK = "Override Age Tick";
		public const string AGE_TICK = "Aging Tick Speed [Default: 1] ";

		public const string AgeCurve = "AgeCurve";
		public const string HasMinAge = "HasMinAge";
		public const string MinAgeSoft = "MinAgeSoft";
		public const string MinAge = "MinAge";
		public const string HasMaxAge = "HasMaxAge";
		public const string MaxAgeChrono = "MaxAgeChrono";
		public const string MaxAge = "MaxAge";
		public const string HasAgeTick = "HasAgeTick";
		public const string AgeTick = "AgeTick";

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
			_MinAgeBuffer = state.Get(MinAge).ToString();
			_MaxAgeBuffer = state.Get(MaxAge).ToString();
			_AgeTickBuffer = state.Get(AgeTick).ToString();
		}

		public override void Draw_Inside(Rect inRect, Listing_Standard gui)
		{
			bool _HasMinAge = state.GBool(HasMinAge);
			bool _HasMaxAge = state.GBool(HasMaxAge);
			bool _HasAgeTick = state.GBool(HasAgeTick);
			int _MinAge = state.Get(MinAge);
			int _MaxAge = state.Get(MaxAge);
			int _AgeTick = state.Get(AgeTick);

			Tools.GBool(gui, state, AgeCurve, AGE_CURVE, DESCRIPTION_AGE_CURVE);
			Tools.GBool(gui, state, MaxAgeChrono, MAX_AGE_CHRONO, DESCRIPTION_MAX_AGE_CHRONO);
			Tools.GBool(gui, state, HasMinAge, HAS_MIN_AGE, DESCRIPTION_HAS_MIN_AGE);

			if (_HasMinAge)
			{
				Tools.GBool(gui, state, MinAgeSoft, MIN_AGE_SOFT, DESCRIPTION_MIN_AGE_SOFT);

				gui.TextFieldNumericLabeled(
					MIN_AGE,
					ref _MinAge,
					ref _MinAgeBuffer,
					0,
					_HasMaxAge ? _MaxAge : 1E+09f
				);
				state.Set(MinAge, _MinAge);

				gui.Gap(10f);
			}

			Tools.GBool(gui, state, HasMaxAge, HAS_MAX_AGE, DESCRIPTION_HAS_MAX_AGE);

			if (_HasMaxAge)
			{
				gui.TextFieldNumericLabeled(
					MAX_AGE,
					ref _MaxAge,
					ref _MaxAgeBuffer,
					_HasMinAge ? _MinAge : 0
				);
				state.Set(MaxAge, _MaxAge);
			}

			Tools.GBool(gui, state, HasAgeTick, HAS_AGE_TICK, DESCRIPTION_HAS_AGE_TICK);

			if (_HasAgeTick)
			{
				gui.TextFieldNumericLabeled(
					AGE_TICK,
					ref _AgeTick,
					ref _AgeTickBuffer
				);

				state.Set(AgeTick, _AgeTick);
			}
		}
	}
}
