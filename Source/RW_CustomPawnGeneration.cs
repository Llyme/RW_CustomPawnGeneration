using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace RW_CustomPawnGeneration
{
	[StaticConstructorOnStartup]
	public class RW_CustomPawnGeneration
	{
		public const string ID = "com.rimworld.mod.nyan.custom_pawn_generation";

		public static Harmony patcher;

		static RW_CustomPawnGeneration()
		{
			// Initialize default settings.

			Settings.GlobalIntDefaults[Strings.Keys.UseRaceSpecific] = 1;
			Settings.GlobalIntDefaults[Strings.Keys.GenderSlider] = 50;
			Settings.LocalIntDefaults[Strings.Keys.GenderSlider] = 50;

			Settings.GlobalIntDefaults[Strings.Keys.MaxAge] = 99;
			Settings.LocalIntDefaults[Strings.Keys.MaxAge] = 99;
			Settings.GlobalIntDefaults[Strings.Keys.AgeTick] = 1;
			Settings.LocalIntDefaults[Strings.Keys.AgeTick] = 1;
			Settings.GlobalIntDefaults[Strings.Keys.CustomAging] = 0;
			Settings.GlobalIntDefaults[Strings.Keys.UngenderedParent] = 0;
			Settings.GlobalIntDefaults[Strings.Keys.ModifyAggressively] = 1;

			foreach (BodyTypeDef def in DefDatabase<BodyTypeDef>.AllDefs)
			{
				Settings.GlobalIntDefaults[def.defName] = 1;
				Settings.LocalIntDefaults[def.defName] = 1;
			}


			// Patch hooks.

			patcher = new Harmony(ID);
			patcher.PatchAll(Assembly.GetExecutingAssembly());

			Module.InitializeAll();
		}
	}
}
