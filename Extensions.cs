using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RW_CustomPawnGeneration
{
	public static class Extensions
	{
		public static bool CPGEnabled
			(this BodyTypeDef body_type,
			Settings.State global,
			Settings.State state,
			bool is_global)
		{
			if (body_type == null)
				return false;

			return Settings.Bool(
				global,
				state,
				$"{BodyWindow.FilterBody}|{body_type.defName}",
				is_global
			);
		}

		public static void RandomizeBodyType(this Pawn pawn)
		{
			Settings.GetState(pawn, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, BodyWindow.FilterBody))
				return;

			bool isGlobal = Settings.IsGlobal(state, BodyWindow.FilterBody);

			if (pawn.story.bodyType.CPGEnabled(global, state, isGlobal))
				// Current body type is good.
				return;

			BodyTypeDef type = pawn.GetRandomBodyType(global, state, isGlobal);

			if (type != null)
			{
				pawn.story.bodyType = type;
				return;
			}

			Log.Warning(
				"[CustomPawnGeneration] A pawn's body type was not filtered properly! " +
				"You may be blocking too many body types."
			);
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
