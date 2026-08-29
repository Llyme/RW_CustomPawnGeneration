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
			public static string CPGName => "CPGName".Translate(TextKind.Label);

			public static string Reset => "Reset".Translate(TextKind.Label);

			public static string ResetAll => "ResetAll".Translate(TextKind.Label);

			public static string Yes => "Yes".Translate(TextKind.Label);

			public static string No => "No".Translate(TextKind.Label);

			public static string CopyTo => "Copy to...".Translate(TextKind.Label);

			public static string Edit => "Edit".Translate(TextKind.Label);

			public static string ShowConfig => "ShowConfig".Translate(TextKind.Label);

			public static string AdvancedMode => "AdvancedMode".Translate(TextKind.Label);

			public static string UseRaceSpecific => "UseRaceSpecific".Translate(TextKind.Label);

			public static string CustomAging => "CustomAging".Translate(TextKind.Label);

			public static string UngenderedParent => "UngenderedParent".Translate(TextKind.Label);

			public static string GlobalConfig => "GlobalConfig".Translate(TextKind.Label);

			public static string Search => "Search".Translate(TextKind.Label);
		}

		public static class Descriptions
		{
			public static string AdvancedMode => "AdvancedMode".Translate(TextKind.Description);

			public static string ResetAll => "ResetAll".Translate(TextKind.Description);

			public static string UseRaceSpecific => "UseRaceSpecific".Translate(TextKind.Description);

			public static string CustomAging => "CustomAging".Translate(TextKind.Description);

			public static string GlobalConfig => "GlobalConfig".Translate(TextKind.Description);

			public static string UngenderedParent => "UngenderedParent".Translate(TextKind.Description);
		}

		public static class Messages
		{
			public static string Reset(NamedArgument name) =>
				"Reset".Translate(TextKind.Message, name);
		}
	}
}
