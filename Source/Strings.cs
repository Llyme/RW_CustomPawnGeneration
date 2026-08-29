using Verse;

namespace RW_CustomPawnGeneration
{
	public static class Strings
	{
		public static class Keys
		{
			public const string AdvancedMode = "AdvancedMode";

			public const string UseRaceSpecific = "UseRaceSpecific";

			public const string CustomAging = "CustomAging";

			public const string UngenderedParent = "UngenderedParent";
		}

		public static class Labels
		{
			public static string CPGName => "CPG_CPGName_Label".Translate();

			public static string Reset => "CPG_Reset_Label".Translate();

			public static string ResetAll => "CPG_ResetAll_Label".Translate();

			public static string Yes => "CPG_Yes_Label".Translate();

			public static string No => "CPG_No_Label".Translate();

			public static string CopyTo => "CPG_CopyTo_Label".Translate();

			public static string Edit => "CPG_Edit_Label".Translate();

			public static string ShowConfig => "CPG_ShowConfig_Label".Translate();

			public static string AdvancedMode => "CPG_AdvancedMode_Label".Translate();

			public static string UseRaceSpecific => "CPG_UseRaceSpecific_Label".Translate();

			public static string CustomAging => "CPG_CustomAging_Label".Translate();

			public static string UngenderedParent => "CPG_UngenderedParent_Label".Translate();

			public static string GlobalConfig => "CPG_GlobalConfig_Label".Translate();

			public static string Search => "CPG_Search_Label".Translate();
		}

		public static class Descriptions
		{
			public static string AdvancedMode => "CPG_AdvancedMode_Description".Translate();

			public static string ResetAll => "CPG_ResetAll_Description".Translate();

			public static string UseRaceSpecific => "CPG_UseRaceSpecific_Description".Translate();

			public static string CustomAging => "CPG_CustomAging_Description".Translate();

			public static string GlobalConfig => "CPG_GlobalConfig_Description".Translate();

			public static string UngenderedParent => "CPG_UngenderedParent_Description".Translate();
		}

		public static class Messages
		{
			public static string Reset(NamedArgument name) =>
				"CPG_Reset_Message".Translate(name);
		}
	}
}
