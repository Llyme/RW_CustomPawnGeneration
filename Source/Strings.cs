namespace RW_CustomPawnGeneration
{
	// Labels / Descriptions / Messages are code-generated into Strings.g.cs from
	// Languages/English/Keyed/*.xml by Tools/GenStrings. Do not add them here.
	//
	// Keys are save-data state keys (passed to Settings.State.Get/Set/Bool, etc.).
	// Their string VALUES are persisted in save files and settings - never change them.
	public static partial class Strings
	{
		public static class Keys
		{
			// Root / global config
			public const string AdvancedMode = "AdvancedMode";
			public const string UseRaceSpecific = "UseRaceSpecific";
			public const string CustomAging = "CustomAging";
			public const string UngenderedParent = "UngenderedParent";

			// Age
			public const string AgeCurve = "AgeCurve";
			public const string HasMinAge = "HasMinAge";
			public const string MinAgeSoft = "MinAgeSoft";
			public const string MinAge = "MinAge";
			public const string HasMaxAge = "HasMaxAge";
			public const string MaxAgeChrono = "MaxAgeChrono";
			public const string MaxAge = "MaxAge";
			public const string HasAgeTick = "HasAgeTick";
			public const string AgeTick = "AgeTick";

			// Gender
			public const string SeparateGender = "SeparateGender";
			public const string OverrideGender = "OverrideGender";
			public const string UnforcedGender = "UnforcedGender";
			public const string ModifyAggressively = "ModifyAggressively";
			public const string GenderSlider = "GenderSlider";

			// Body
			public const string FilterBody = "FilterBody";

			// Traits
			public const string OverrideTraits = "OverrideTraits";
			public const string Trait = "Trait";
		}
	}
}
