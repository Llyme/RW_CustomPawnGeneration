using HarmonyLib;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RW_CustomPawnGeneration
{
	/*[HarmonyPatch(typeof(SpouseRelationUtility), "ResolveNameForSpouseOnGeneration")]
	public static class Patch_SpouseRelationUtility_ResolveNameForSpouseOnGeneration
	{
		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static bool Patch(ref PawnGenerationRequest request, Pawn generated)
		{
			if (generated.GetSpouse() == null)
				return false;

			return true;
		}
	}*/

	[HarmonyPatch(typeof(ParentRelationUtility), "SetMother")]
	public static class ParentRelationUtility_SetMother
	{
		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static bool Patch(this Pawn pawn, Pawn newMother)
		{
			if (pawn == null)
				return true;

			if (!Settings.GBool(pawn, GenderWindow.UnforcedGender))
				return true;

			// Ignore limitations of being a mother (gender.)

			if (newMother == null)
				return false;

			if (newMother == pawn.GetMother())
				return false;

			pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);

			return false;
		}
	}

	[HarmonyPatch(typeof(ParentRelationUtility), "SetFather")]
	public static class ParentRelationUtility_SetFather
	{
		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static bool Patch(this Pawn pawn, Pawn newFather)
		{
			if (pawn == null)
				return true;

			if (!Settings.GBool(pawn, GenderWindow.UnforcedGender))
				return true;

			// Ignore limitations of being a father (gender.)

			if (newFather == null)
				return false;

			if (newFather == pawn.GetFather())
				return false;

			pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);

			return false;
		}
	}

	[HarmonyPatch(typeof(ParentRelationUtility), "GetFather")]
	public static class Patch_ParentRelationUtility_GetFather
	{
		public static readonly Module module =
			new Module(
				Settings.UngenderedParent,
				typeof(ParentRelationUtility)
				.GetMethod("GetFather"),
				prefix: new HarmonyMethod(typeof(Patch_ParentRelationUtility_GetFather).GetMethod("Patch"))
			);

		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static bool Patch(this Pawn pawn, ref Pawn __result)
		{
			if (pawn == null)
				return true;

			if (__result != null)
				return true;

			if (!Settings.GBool(pawn, GenderWindow.UnforcedGender))
				return true;

			if (!pawn.RaceProps.IsFlesh)
				return false;

			if (pawn.relations == null)
				return false;


			bool has_mother = false;

			foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
			{
				if (relation.def != PawnRelationDefOf.Parent)
					continue;

				if (relation.otherPawn.gender == Gender.Female)
				{
					if (has_mother)
						// Use the 2nd female parent as the father.
						__result = relation.otherPawn;
					else
						has_mother = true;

					continue;
				}

				// Found male parent.
				__result = relation.otherPawn;
				break;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(ParentRelationUtility), "GetMother")]
	public static class Patch_ParentRelationUtility_GetMother
	{
		public static readonly Module module =
			new Module(
				Settings.UngenderedParent,
				typeof(ParentRelationUtility)
				.GetMethod("GetMother"),
				prefix: new HarmonyMethod(typeof(Patch_ParentRelationUtility_GetMother).GetMethod("Patch"))
			);

		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static bool Patch(this Pawn pawn, ref Pawn __result)
		{
			if (pawn == null)
				return true;

			if (__result != null)
				return true;

			if (!Settings.GBool(pawn, GenderWindow.UnforcedGender))
				return true;

			if (!pawn.RaceProps.IsFlesh)
				return false;

			if (pawn.relations == null)
				return false;


			bool has_father = false;

			foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
			{
				if (relation.def != PawnRelationDefOf.Parent)
					continue;

				if (relation.otherPawn.gender != Gender.Female)
				{
					if (has_father)
						// Use the 2nd non-female parent as the mother.
						__result = relation.otherPawn;
					else
						has_father = true;

					continue;
				}

				// Found female parent.
				__result = relation.otherPawn;
				break;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
	public static class Patch_Pawn_AgeTracker_BirthdayBiological
	{
		[HarmonyPrefix]
		public static void Patch(Pawn_AgeTracker __instance, Pawn ___pawn)
		{
			if (___pawn == null)
				return;

			Settings.GetState(___pawn, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, AgeWindow.HasMaxAge))
				return;

			bool isGlobal = Settings.IsGlobal(state, AgeWindow.HasMaxAge);
			int maxAge = Settings.Int(global, state, AgeWindow.MaxAge, isGlobal);
			int ageYears = __instance.AgeBiologicalYears;

			if (ageYears > maxAge)
			{
				long ticks = (ageYears - maxAge) * 3600000;
				__instance.AgeBiologicalTicks -= ticks;

				if (Settings.Bool(global, state, AgeWindow.MaxAgeChrono))
					__instance.AgeChronologicalTicks += ticks;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_AgeTracker), "AgeTick")]
	public static class Patch_Pawn_AgeTracker_AgeTick
	{
		public static readonly Module module =
			new Module(
				Settings.CustomAging,
				typeof(Pawn_AgeTracker)
				.GetMethod("AgeTick"),
				prefix: new HarmonyMethod(typeof(Patch_Pawn_AgeTracker_AgeTick).GetMethod("Patch"))
			);

		[HarmonyPrefix]
		public static void Patch(Pawn_AgeTracker __instance, Pawn ___pawn)
		{
			if (___pawn == null)
				return;

			Settings.GetState(___pawn, out Settings.State global, out Settings.State state);

			int tick = Settings.Int(global, state, AgeWindow.AgeTick, Settings.IsGlobal(state, AgeWindow.HasAgeTick));

			if (tick == 0)
				__instance.AgeBiologicalTicks--;
			else if (tick > 1)
				__instance.AgeTickMothballed(tick - 1);
		}
	}

	//[HarmonyPatch(typeof(PawnBioAndNameGenerator), "GiveAppropriateBioAndNameTo")]
	//public static class Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo
	//{
	//	public static Dictionary<Pawn, PawnGenerationRequest> requests = new Dictionary<Pawn, PawnGenerationRequest>();
	//	public static Pawn pawn = null;
	//	public static PawnGenerationRequest request = default;

	//	[HarmonyPriority(Priority.Last)]
	//	[HarmonyPrefix]
	//	public static void Prefix(Pawn pawn, FactionDef factionType, PawnGenerationRequest request, XenotypeDef xenotype)
	//	{
	//		Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.pawn = pawn;
	//		Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.request = request;
	//	}

	//	[HarmonyPriority(Priority.First)]
	//	[HarmonyPostfix]
	//	public static void Postfix(Pawn pawn, FactionDef factionType, PawnGenerationRequest request, XenotypeDef xenotype)
	//	{
	//		Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.pawn = null;
	//		Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.request = default;
	//	}
	//}

	//[HarmonyPatch(typeof(PawnBioAndNameGenerator), "IsBioUseable")]
	//public static class Patch_PawnBioAndNameGenerator_IsBioUseable
	//{
	//	[HarmonyPriority(Priority.Last)]
	//	[HarmonyPostfix]
	//	public static void Postfix(PawnBio bio, BackstoryCategoryFilter categoryFilter, PawnKindDef kind, Gender gender, string requiredLastName, ref bool __result)
	//	{
	//		if (!__result)
	//			return;

	//		Pawn pawn = Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.pawn;

	//		if (pawn == null)
	//			return;

	//		PawnGenerationRequest request = Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo.request;
	//		WorkTags requiredWorkTags = pawn.kindDef.requiredWorkTags | request.KindDef.requiredWorkTags;

	//		if (requiredWorkTags == WorkTags.None)
	//			return;

	//		WorkTags workDisables = pawn.CombinedDisabledWorkTags;
	//		WorkTags overlap = workDisables & requiredWorkTags;

	//		if (overlap == WorkTags.None)
	//			return;

	//		__result = false;
	//	}
	//}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateRandomAge")]
	public static class Patch_PawnGenerator_GenerateRandomAge
	{
		public const long AGE = 3600000;

		[HarmonyPriority(Priority.First)]
		[HarmonyPrefix]
		public static void Prefix(Pawn pawn, PawnGenerationRequest request)
		{
			Settings.GetStateMale(request.KindDef.race, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, GenderWindow.OverrideGender))
				return;

			if (!Settings.Bool(global, state, GenderWindow.ModifyAggressively))
				return;

			Patch_PawnGenerator_TryGenerateNewPawnInternal.genderPending[request] = pawn;
		}

		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn == null)
				return;

			Settings.GetState(pawn, out Settings.State global, out Settings.State state);

			bool HasMinAge = Settings.Bool(global, state, AgeWindow.HasMinAge);
			bool HasMaxAge = Settings.Bool(global, state, AgeWindow.HasMaxAge);
			bool MinAgeSoft = Settings.Bool(global, state, AgeWindow.MinAgeSoft);
			bool AgeCurve = Settings.Bool(global, state, AgeWindow.AgeCurve);
			bool HasMinAge_Global = Settings.IsGlobal(state, AgeWindow.HasMinAge);
			bool HasMaxAge_Global = Settings.IsGlobal(state, AgeWindow.HasMaxAge);
			int MinAge = Settings.Int(global, state, AgeWindow.MinAge, HasMinAge_Global);
			int MaxAge = Settings.Int(global, state, AgeWindow.MaxAge, HasMaxAge_Global);

			if (HasMinAge || HasMaxAge)
			{
				if (HasMinAge &&
					MinAgeSoft &&
					pawn.ageTracker.AgeBiologicalYears <= MinAge)
					return;

				long age = pawn.ageTracker.AgeBiologicalTicks;
				long min0 = pawn.kindDef.minGenerationAge;
				long min1 = HasMinAge ? MinAge : min0;
				long max0 = pawn.kindDef.maxGenerationAge;
				long max1 = HasMaxAge ? MaxAge : max0;
				long len0 = max0 - min0;
				long len1 = max1 - min1;

				if (AgeCurve)
					age = PseudoPreserveCurve(age, min0, min1, max1, len0, len1);

				min1 *= AGE;
				max1 *= AGE;

				long newAge =
					age < min1
					? min1
					: age > max1
					? max1
					: age;

				pawn.ageTracker.AgeBiologicalTicks = newAge;
				pawn.ageTracker.AgeChronologicalTicks +=
					newAge - pawn.ageTracker.AgeBiologicalTicks;
			}
		}

		public static long PseudoPreserveCurve
			(long age,
			long min0,
			long min1,
			long max1,
			long len0,
			long len1)
		{
			long factor =
				len0 > max1 ?
					(len0 - max1) / max1 :
				len0 < max1 ?
					(max1 - len0) / max1 :
					1;

			return (age - min0 * AGE) / len0 * len1 * factor + min1 * AGE / 2L;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
	public static class Patch_PawnGenerator_GenerateBodyType
	{
		[HarmonyPriority(Priority.Last)]
		[HarmonyPostfix]
		public static void Patch(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn == null)
				return;

			Settings.GetState(pawn, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, BodyWindow.FilterBody))
				return;

			bool is_global = Settings.IsGlobal(state, BodyWindow.FilterBody);
			BodyTypeDef type = pawn.story.bodyType;

			if (type.CPGEnabled(global, state, is_global))
				return;


			// Try complying with the vanilla body type generation first.

			BodyTypeDef filtered_vanilla_body =
				GetBodyTypeFor(
					pawn,
					global,
					state,
					is_global
				);

			if (filtered_vanilla_body != null)
			{
				pawn.story.bodyType = filtered_vanilla_body;
				return;
			}


			// Just pick a random body type, except the Biotech stuff.

			bool forced_random =
				Tools
				.AllCPGAdultBodyTypes(global, state, is_global)
				.TryRandomElement(out BodyTypeDef forced_random_body);

			if (forced_random)
			{
				pawn.story.bodyType = forced_random_body;
				return;
			}

			Log.Warning(
				"[CustomPawnGeneration] A pawn's body type was not filtered properly! " +
				"You may be blocking too many body types."
			);
		}

		/// <summary>
		/// A filtered version of the vanilla `GetBodyTypeFor` function,
		/// with respect to the Biotech `DevelopmentalStage`.
		/// </summary>
		public static BodyTypeDef GetBodyTypeFor
			(Pawn pawn,
			Settings.State global,
			Settings.State state,
			bool is_global)
		{
			if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
			{
				if (pawn.DevelopmentalStage == DevelopmentalStage.Baby)
					return BodyTypeDefOf.Baby;

				return BodyTypeDefOf.Child;
			}

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

						if (bodyType.CPGEnabled(global, state, is_global))
							bodyTypes.Add(bodyType);
					}

				if (bodyTypes.TryRandomElement(out BodyTypeDef result))
					return result;
			}

			if (pawn.story.Adulthood != null)
			{
				BodyTypeDef body_type = pawn.story.Adulthood.BodyTypeFor(pawn.gender);

				if (body_type.CPGEnabled(global, state, is_global))
					return body_type;
			}

			bool thin = BodyTypeDefOf.Thin.CPGEnabled(global, state, is_global);

			if (thin && Rand.Value < 0.5f)
				return BodyTypeDefOf.Thin;

			if (BodyTypeDefOf.Male.CPGEnabled(global, state, is_global) &&
				pawn.gender != Gender.Female)
				return BodyTypeDefOf.Male;

			if (BodyTypeDefOf.Female.CPGEnabled(global, state, is_global))
				return BodyTypeDefOf.Female;

			return null;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
	public static class Patch_PawnGenerator_GenerateTraits
	{
		public static Dictionary<Pawn, ushort> traitsPending = new Dictionary<Pawn, ushort>();

		[HarmonyPriority(Priority.Last)]
		[HarmonyPrefix]
		public static void Prefix(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn == null)
				return;

			traitsPending[pawn] = 0;
		}

		[HarmonyPriority(Priority.Last)]
		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn == null)
				return;

			traitsPending.Remove(pawn);

			Settings.GetState(pawn, out Settings.State global, out Settings.State state);

			bool OverrideTraits = Settings.Bool(global, state, TraitsWindow.OverrideTraits);

			if (pawn.story == null || !OverrideTraits)
				return;

			bool IsGlobal = Settings.IsGlobal(state, TraitsWindow.OverrideTraits);

			foreach (TraitDef def in DefDatabase<TraitDef>.AllDefs)
				foreach (TraitDegreeData data in def.degreeDatas)
				{
					bool flag = Settings.Int(
						global,
						state,
						$"{TraitsWindow.Trait}|{def.defName}|{data.degree}",
						IsGlobal
					) == 2;

					if (flag)
						pawn.story.traits.GainTrait(new Trait(def, data.degree));
				}
		}
	}

	[HarmonyPatch(typeof(TraitSet), "GainTrait")]
	public static class Patch_TraitSet_GainTrait
	{
		/// <summary>
		/// This limits how many times the game
		/// re-rolls a trait for a pawn,
		/// preventing it from creating
		/// a permanent loop.
		/// </summary>
		public const ushort MAX_STACK = 100;

		[HarmonyPrefix]
		public static bool Prefix(TraitSet __instance, Trait trait, Pawn ___pawn)
		{
			if (___pawn == null)
				return true;

			if (!Patch_PawnGenerator_GenerateTraits.traitsPending.ContainsKey(___pawn))
				return true;

			Settings.GetState(___pawn, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, TraitsWindow.OverrideTraits))
				return true;

			if (Patch_PawnGenerator_GenerateTraits.traitsPending[___pawn] > MAX_STACK)
			{
				Log.Warning("[CustomPawnGeneration] Rolled for traits too many times! Try not to block/force too many of them!");
				Patch_PawnGenerator_GenerateTraits.traitsPending.Remove(___pawn);
				return true;
			}

			bool IsGlobal = Settings.IsGlobal(state, TraitsWindow.OverrideTraits);

			Patch_PawnGenerator_GenerateTraits.traitsPending[___pawn]++;
			return Settings.Int(global, state, $"{TraitsWindow.Trait}|{trait.def.defName}|{trait.Degree}", IsGlobal) == 0;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
	public static class Patch_PawnGenerator_TryGenerateNewPawnInternal
	{
		public static Dictionary<PawnGenerationRequest, Pawn> genderPending = new Dictionary<PawnGenerationRequest, Pawn>();
		public static HashSet<PawnGenerationRequest> genderChanges = new HashSet<PawnGenerationRequest>();

		public static void DiscardGeneratedPawn(Pawn pawn)
		{
			MethodInfo method = AccessTools.Method(
				typeof(PawnGenerator),
				"DiscardGeneratedPawn"
			);

			if (method == null)
				return;

			IList pawnsBeingGenerated = Tools.PawnsBeingGenerated;
			object dummy = Patch_PawnGenerator_DiscardGeneratedPawn.dummy;
			bool validDummy = dummy != null && !pawnsBeingGenerated.Contains(dummy);

			if (validDummy)
				pawnsBeingGenerated.Add(dummy);

			method.Invoke(null, new object[] { pawn });

			if (validDummy)
				pawnsBeingGenerated.Remove(dummy);
		}

		[HarmonyPriority(Priority.First)]
		[HarmonyPostfix]
		public static void Postfix(ref PawnGenerationRequest request, ref Pawn __result, ref string error)
		{
			if (__result != null)
				return;

			if (!genderPending.ContainsKey(request))
				return;

			bool genderChanged = genderChanges.Contains(request);
			Pawn pawn = genderPending[request];

			genderPending.Remove(request);
			genderChanges.Remove(request);


			if (!genderChanged)
			{
				DiscardGeneratedPawn(pawn);
				return;
			}

			if (error != "Generated pawn with disabled requiredWorkTags.")
			{
				DiscardGeneratedPawn(pawn);
				return;
			}

			Log.Warning($"[CustomPawnGeneration] '{pawn.Name}' was generated with an error '{error}'!");

			__result = pawn;
			error = null;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "DiscardGeneratedPawn")]
	public static class Patch_PawnGenerator_DiscardGeneratedPawn
	{
		public static object dummy = null;

		[HarmonyPriority(Priority.First)]
		[HarmonyPrefix]
		public static bool Prefix(Pawn pawn)
		{
			if (Patch_PawnGenerator_TryGenerateNewPawnInternal.genderPending.ContainsValue(pawn))
			{
				IList pawnsBeingGenerated = Tools.PawnsBeingGenerated;

				if (pawnsBeingGenerated.Count > 0)
					dummy = pawnsBeingGenerated[0];

				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
	public static class Patch_PawnGenerator_GeneratePawn
	{
		[HarmonyPriority(Priority.First)]
		[HarmonyPrefix]
		public static void Prefix(ref PawnGenerationRequest request)
		{
			if (!request.KindDef.RaceProps.hasGenders)
				return;

			Settings.GetStateMale(request.KindDef.race, out Settings.State global, out Settings.State state);

			if (!Settings.Bool(global, state, GenderWindow.OverrideGender))
				return;

			if (!Settings.Bool(global, state, GenderWindow.UnforcedGender) &&
				request.FixedGender != null)
				return;

			bool isGlobal = Settings.IsGlobal(state, GenderWindow.OverrideGender);
			int value = Settings.Int(global, state, GenderWindow.GenderSlider, isGlobal);
			Gender gender;

			if (value == 100)
				gender = Gender.Female;
			else if (value == 0)
				gender = Gender.Male;
			else if (Rand.Value < value / 100f)
				gender = Gender.Female;
			else
				gender = Gender.Male;

			if (request.FixedGender == gender)
				return;

			request.FixedGender = gender;

			Patch_PawnGenerator_TryGenerateNewPawnInternal.genderChanges.Add(request);
		}

		[HarmonyPostfix, HarmonyPriority(Priority.Last)]
		public static void Patch(Pawn __result, PawnGenerationRequest request)
		{
			if (__result == null)
				return;

			Settings.GetState(__result, out _, out Settings.State state);

			foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs)
			{
				int v0 = state.Get($"Hediff|{def.defName}");

				if (v0 > 0 && Rand.Value < v0 / 100f)
					__result.health.AddHediff(def);

				foreach (BodyPartRecord part in __result.RaceProps.body.AllParts)
				{
					int v1 = state.Get($"Hediff|{part.Label}|{def.defName}");

					if (v1 > 0 && Rand.Value < v1 / 100f)
						__result.health.AddHediff(def, part);
				}
			}
		}
	}

	/// <summary>
	/// Dirty fix for the pawn relations.
	/// </summary>
	[HarmonyPatch(typeof(PawnGenerator), "GeneratePawnRelations")]
	public static class Patch_PawnGenerator_GeneratePawnRelations
	{
		/// <summary>
		/// 0 = Do nothing;
		/// 1 = Unpatch at postfix then set to 0;
		/// 2+ = Decrement;
		/// </summary>
		private static ushort should_unpatch = 0;
		private static bool in_postfix = false;

		public static void Unpatch()
		{
			should_unpatch = 0;

			Patch_ParentRelationUtility_GetFather.module.Unpatch();
			Patch_ParentRelationUtility_GetMother.module.Unpatch();
		}

		[HarmonyPrefix]
		public static void Prefix(Pawn pawn, ref PawnGenerationRequest request)
		{
			// Forcibly patch `ParentRelationUtility`.

			if (in_postfix)
			{
				// Something went wrong. Reset.
				in_postfix = false;

				if (should_unpatch > 0)
					Unpatch();
			}

			if (!Patch_ParentRelationUtility_GetFather.module.IsPatched)
			{
				in_postfix = true;
				should_unpatch = 1;

				Patch_ParentRelationUtility_GetFather.module.Patch();
				Patch_ParentRelationUtility_GetMother.module.Patch();
			}
			else if (should_unpatch > 0)
			{
				in_postfix = true;
				should_unpatch++;
			}
		}

		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, ref PawnGenerationRequest request)
		{
			// Disable patches.

			if (should_unpatch > 1)
				should_unpatch--;

			else if (should_unpatch == 1)
				Unpatch();

			in_postfix = false;
		}
	}
}
