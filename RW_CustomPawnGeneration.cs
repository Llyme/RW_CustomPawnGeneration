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

			Settings.GlobalIntDefaults[GenderWindow.GenderSlider] = 50;
			Settings.LocalIntDefaults[GenderWindow.GenderSlider] = 50;

			Settings.GlobalIntDefaults[AgeWindow.MaxAge] = 99;
			Settings.LocalIntDefaults[AgeWindow.MaxAge] = 99;
			Settings.GlobalIntDefaults[AgeWindow.AgeTick] = 1;
			Settings.LocalIntDefaults[AgeWindow.AgeTick] = 1;
			Settings.GlobalIntDefaults[Settings.CustomAging] = 0;
			Settings.GlobalIntDefaults[Settings.UngenderedParent] = 0;
			Settings.GlobalIntDefaults[GenderWindow.ModifyAggressively] = 1;

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
