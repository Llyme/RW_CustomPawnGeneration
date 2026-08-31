using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RW_CustomPawnGeneration
{
	public static class Extensions
	{
		public static bool CPGEnabled(
			this BodyTypeDef bodyType,
			Settings.State global,
			Settings.State state,
			bool isGlobal
		)
		{
			if (bodyType == null)
				return false;

			return Settings.Bool(
				global,
				state,
				$"{Strings.Keys.FilterBody}|{bodyType.defName}",
				isGlobal
			);
		}

		public static void RandomizeBodyType(this Pawn pawn)
		{
			if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
				// Biotech stuff.
				return;

			Settings.GetState(
				pawn,
				Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
				out Settings.State global,
				out Settings.State state
			);

			if (!Settings.Bool(global, state, Strings.Keys.FilterBody))
				return;

			bool isGlobal = Settings.IsGlobal(state, Strings.Keys.FilterBody);

			if (pawn.story.bodyType.CPGEnabled(global, state, isGlobal))
				// Current body type is good.
				return;

			BodyTypeDef type = pawn.GetRandomBodyType(
				global,
				state,
				isGlobal
			);

			if (type != null)
			{
				pawn.story.bodyType = type;
				return;
			}

			Log.Warning(Strings.Messages.BodyTypeNotFiltered);
		}

		public static BodyTypeDef GetRandomBodyType
			(this Pawn pawn,
			Settings.State global,
			Settings.State state,
			bool isGlobal)
		{
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				HashSet<BodyTypeDef> bodyTypes = new HashSet<BodyTypeDef>();
				List<Gene> genesListForReading = pawn.genes.GenesListForReading;

				for (int i = 0; i < genesListForReading.Count; i++)
					if (genesListForReading[i].def.bodyType != null)
					{
						BodyTypeDef bodyType =
							genesListForReading[i]
							.def
							.bodyType
							.Value
							.ToBodyType(pawn);

						if (bodyType.CPGEnabled(global, state, isGlobal))
							bodyTypes.Add(bodyType);
					}

				if (bodyTypes.TryRandomElement(out BodyTypeDef result))
					return result;
			}

			if (pawn.story.Adulthood != null)
			{
				BodyTypeDef bodyType = pawn.story.Adulthood.BodyTypeFor(pawn.gender);

				if (bodyType.CPGEnabled(global, state, isGlobal))
					return bodyType;
			}

			return new[]
			{
				pawn.gender == Gender.Female ? BodyTypeDefOf.Female : BodyTypeDefOf.Male,
				BodyTypeDefOf.Thin,
				BodyTypeDefOf.Fat,
				BodyTypeDefOf.Hulk,
			}.Where(bodyType =>
				bodyType.CPGEnabled(global, state, isGlobal)
			)
			.TryRandomElement(out BodyTypeDef selectedBodyType) ? selectedBodyType : null;
		}
	}
}
