using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
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

            if (!Settings.GBool(
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific) ? pawn.kindDef.race : null,
                Strings.Keys.UnforcedGender
            ))
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

            if (!Settings.GBool(
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific) ? pawn.kindDef.race : null,
                Strings.Keys.UnforcedGender
            ))
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

    /// <summary>
    /// Patches the private `GetParent` instead of `GetFather`/`GetMother`.
    /// Those two are one-line wrappers that the .NET JIT (RimWorld 1.7+)
    /// can inline into their callers, which skips a patch on them.
    /// </summary>
    [HarmonyPatch(typeof(ParentRelationUtility), "GetParent")]
    public static class Patch_ParentRelationUtility_GetParent
    {
        public static readonly Module module =
            new Module(
                Strings.Keys.UngenderedParent,
                AccessTools.Method(typeof(ParentRelationUtility), "GetParent"),
                prefix: new HarmonyMethod(typeof(Patch_ParentRelationUtility_GetParent).GetMethod("Patch"))
            );

        [HarmonyPriority(Priority.Last)]
        [HarmonyPrefix]
        public static bool Patch(Pawn pawn, Gender parentGender, ref Pawn __result)
        {
            if (pawn == null)
                return true;

            if (parentGender != Gender.Male && parentGender != Gender.Female)
                return true;

            if (!Settings.GBool(
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific) ? pawn.kindDef.race : null,
                Strings.Keys.UnforcedGender
            ))
                return true;

            if (!pawn.RaceProps.IsFlesh)
                return false;

            if (pawn.relations == null)
                return false;

            bool wantFemale = parentGender == Gender.Female;
            bool skippedOther = false;

            foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
            {
                if (relation.def != PawnRelationDefOf.Parent)
                    continue;

                bool isFemale = relation.otherPawn.gender == Gender.Female;

                if (isFemale == wantFemale)
                {
                    // Found a parent of the wanted gender.
                    __result = relation.otherPawn;
                    break;
                }

                // Use the 2nd parent of the other gender in place of the missing one.
                if (skippedOther)
                    __result = relation.otherPawn;
                else
                    skippedOther = true;
            }

            return false;
        }
    }

    /// <summary>
    /// Vanilla calls `GetFather().Name` and `GetMother().Name` without a null check.
    /// This throws when a sibling has no parent of one gender,
    /// for example when only one gender is generated.
    /// </summary>
    [HarmonyPatch(typeof(PawnRelationWorker_Sibling), "ResolveMyName")]
    public static class Patch_PawnRelationWorker_Sibling_ResolveMyName
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPrefix]
        public static bool Prefix(ref PawnGenerationRequest request, Pawn generated)
        {
            if (generated == null)
                return true;

            Pawn father = generated.GetFather();
            Pawn mother = generated.GetMother();

            if (father != null && mother != null)
                return true;

            if (request.FixedLastName != null ||
                !ChildRelationUtility.ChildWantsNameOfAnyParent(generated))
                return false;

            if (Rand.Value < 0.5f && father?.Name is NameTriple fatherName)
                request.SetFixedLastName(fatherName.Last);
            else if (mother?.Name is NameTriple motherName)
                request.SetFixedLastName(motherName.Last);

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

            Settings.GetState(
                ___pawn,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out Settings.State global,
                out Settings.State state
            );

            if (!Settings.Bool(global, state, Strings.Keys.HasMaxAge))
                return;

            bool isGlobal = Settings.IsGlobal(state, Strings.Keys.HasMaxAge);
            int maxAge = Settings.Int(global, state, Strings.Keys.MaxAge, isGlobal);
            int ageYears = __instance.AgeBiologicalYears;

            if (ageYears > maxAge)
            {
                long ticks = (ageYears - maxAge) * 3600000;
                __instance.AgeBiologicalTicks -= ticks;

                if (Settings.Bool(global, state, Strings.Keys.MaxAgeChrono))
                    __instance.AgeChronologicalTicks += ticks;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "AgeTickInterval")]
    public static class Patch_Pawn_AgeTracker_AgeTickInterval
    {
        public static readonly Module module =
            new Module(
                Strings.Keys.CustomAging,
                typeof(Pawn_AgeTracker)
                .GetMethod("AgeTickInterval"),
                prefix: new HarmonyMethod(typeof(Patch_Pawn_AgeTracker_AgeTickInterval).GetMethod("Patch"))
            );

        [HarmonyPrefix]
        public static void Patch(
            Pawn_AgeTracker __instance,
            Pawn ___pawn,
            int delta
        )
        {
            if (___pawn == null)
                return;

            Settings.GetState(
                ___pawn,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out Settings.State global,
                out Settings.State state
            );

            int tick = Settings.Int(
                global,
                state,
                Strings.Keys.AgeTick,
                Settings.IsGlobal(state, Strings.Keys.HasAgeTick)
            );

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
            Settings.GetStateMale(
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific) ? request.KindDef.race : null,
                out Settings.State global,
                out Settings.State state
            );

            if (!Settings.Bool(global, state, Strings.Keys.OverrideGender))
                return;

            if (!Settings.Bool(global, state, Strings.Keys.ModifyAggressively))
                return;

            Patch_PawnGenerator_TryGenerateNewPawnInternal.SetPending(pawn);
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn == null)
                return;

            long min0 = (long)(pawn.GetYoungestAdultStage()?.minAge ?? pawn.kindDef.minGenerationAge);

            if (pawn.ageTracker.AgeBiologicalYears < min0)
                return;

            Settings.GetState(
                pawn,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out Settings.State global,
                out Settings.State state
            );

            bool HasMinAge = Settings.Bool(global, state, Strings.Keys.HasMinAge);
            bool HasMaxAge = Settings.Bool(global, state, Strings.Keys.HasMaxAge);
            bool MinAgeSoft = Settings.Bool(global, state, Strings.Keys.MinAgeSoft);
            bool AgeCurve = Settings.Bool(global, state, Strings.Keys.AgeCurve);
            bool HasMinAge_Global = Settings.IsGlobal(state, Strings.Keys.HasMinAge);
            bool HasMaxAge_Global = Settings.IsGlobal(state, Strings.Keys.HasMaxAge);
            int MinAge = Settings.Int(global, state, Strings.Keys.MinAge, HasMinAge_Global);
            int MaxAge = Settings.Int(global, state, Strings.Keys.MaxAge, HasMaxAge_Global);

            if (HasMinAge || HasMaxAge)
            {
                if (HasMinAge &&
                    MinAgeSoft &&
                    pawn.ageTracker.AgeBiologicalYears <= MinAge)
                    return;

                long age = pawn.ageTracker.AgeBiologicalTicks;
                long min1 = HasMinAge ? MinAge : min0;
                long max0 = (long)pawn.RaceProps.lifeExpectancy;
                //long max0 = pawn.kindDef.maxGenerationAge;
                long max1 = HasMaxAge ? MaxAge : max0;
                long len0 = max0 - min0;
                long len1 = max1 - min1;

                if (AgeCurve)
                    //age = PseudoPreserveCurve(age, min0, min1, max1, len0, len1);
                    age = PseudoPreserveCurveV2(age, min0, min1, len0, len1);

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

        [Obsolete("This uses `PawnKindDef.maxGenerationAge`, which is inaccurate.")]
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

        /// <summary>
        /// Uses `RaceProperties.lifeExpectancy` to mimic a pawn's age curve.
        /// </summary>
        public static long PseudoPreserveCurveV2
            (long age,
            long min0,
            long min1,
            long len0,
            long len1) => (age - min0 * AGE) / len0 * len1 + min1 * AGE;
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class Patch_GeneUtility_ToBodyType
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request)
        {
            pawn.RandomizeBodyType();
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GetBodyTypeFor")]
    public static class Patch_PawnGenerator_GetBodyTypeFor
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void Patch(Pawn pawn, ref BodyTypeDef __result)
        {
            pawn.RandomizeBodyType();
        }
    }

    //[HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    //public static class Patch_PawnGenerator_GenerateBodyType
    //{
    //	[HarmonyPriority(Priority.Last)]
    //	[HarmonyPostfix]
    //	public static void Patch(Pawn pawn, PawnGenerationRequest request)
    //	{
    //		if (pawn == null)
    //			return;

    //		if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
    //			// Biotech stuff.
    //			return;

    //		Settings.GetState(pawn, out Settings.State global, out Settings.State state);

    //		if (!Settings.Bool(global, state, BodyWindow.FilterBody))
    //			return;

    //		bool isGlobal = Settings.IsGlobal(state, BodyWindow.FilterBody);

    //		if (pawn.story.bodyType.CPGEnabled(global, state, isGlobal))
    //			// Current body type is good.
    //			return;

    //		BodyTypeDef type = pawn.RandomBodyType(global, state, isGlobal);

    //		if (type != null)
    //		{
    //			request.ForceBodyType =
    //				pawn.story.bodyType = type;
    //			return;
    //		}

    //		Log.Warning(
    //			"[CustomPawnGeneration] A pawn's body type was not filtered properly! " +
    //			"You may be blocking too many body types."
    //		);
    //	}

    //	/// <summary>
    //	/// A filtered version of the vanilla `GetBodyTypeFor` function,
    //	/// with respect to the Biotech `DevelopmentalStage`.
    //	/// </summary>
    //	public static BodyTypeDef GetBodyTypeFor
    //		(Pawn pawn,
    //		Settings.State global,
    //		Settings.State state,
    //		bool isGlobal)
    //	{
    //		if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Juvenile())
    //		{
    //			if (pawn.DevelopmentalStage == DevelopmentalStage.Baby)
    //				return BodyTypeDefOf.Baby;

    //			return BodyTypeDefOf.Child;
    //		}

    //		if (ModsConfig.BiotechActive && pawn.genes != null)
    //		{
    //			HashSet<BodyTypeDef> bodyTypes = new HashSet<BodyTypeDef>();
    //			List<Gene> genesListForReading = pawn.genes.GenesListForReading;

    //			for (int i = 0; i < genesListForReading.Count; i++)
    //				if (genesListForReading[i].def.bodyType != null)
    //				{
    //					BodyTypeDef bodyType =
    //						genesListForReading[i]
    //						.def
    //						.bodyType
    //						.Value
    //						.ToBodyType(pawn);

    //					if (bodyType.CPGEnabled(global, state, isGlobal))
    //						bodyTypes.Add(bodyType);
    //				}

    //			if (bodyTypes.TryRandomElement(out BodyTypeDef result))
    //				return result;
    //		}

    //		if (pawn.story.Adulthood != null)
    //		{
    //			BodyTypeDef bodyType = pawn.story.Adulthood.BodyTypeFor(pawn.gender);

    //			if (bodyType.CPGEnabled(global, state, isGlobal))
    //				return bodyType;
    //		}

    //		return pawn.RandomBodyType(
    //			global,
    //			state,
    //			isGlobal
    //		);
    //	}
    //}

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

            Settings.GetState(
                pawn,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out Settings.State global,
                out Settings.State state
            );

            bool OverrideTraits = Settings.Bool(global, state, Strings.Keys.OverrideTraits);

            if (pawn.story == null || !OverrideTraits)
                return;

            bool IsGlobal = Settings.IsGlobal(state, Strings.Keys.OverrideTraits);

            foreach (TraitDef def in DefDatabase<TraitDef>.AllDefs)
                foreach (TraitDegreeData data in def.degreeDatas)
                {
                    bool flag = Settings.Int(
                        global,
                        state,
                        $"{Strings.Keys.Trait}|{def.defName}|{data.degree}",
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

            Settings.GetState(
                ___pawn,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out Settings.State global,
                out Settings.State state
            );

            if (!Settings.Bool(global, state, Strings.Keys.OverrideTraits))
                return true;

            if (Patch_PawnGenerator_GenerateTraits.traitsPending[___pawn] > MAX_STACK)
            {
                Log.Warning("[CustomPawnGeneration] Rolled for traits too many times! Try not to block/force too many of them!");
                Patch_PawnGenerator_GenerateTraits.traitsPending.Remove(___pawn);
                return true;
            }

            bool IsGlobal = Settings.IsGlobal(state, Strings.Keys.OverrideTraits);

            Patch_PawnGenerator_GenerateTraits.traitsPending[___pawn]++;
            return Settings.Int(global, state, $"{Strings.Keys.Trait}|{trait.def.defName}|{trait.Degree}", IsGlobal) == 0;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class Patch_PawnGenerator_TryGenerateNewPawnInternal
    {
        public const string REQUIRED_WORK_TAGS_ERROR = "Generated pawn with disabled requiredWorkTags.";

        /// <summary>
        /// One frame per `TryGenerateNewPawnInternal` call, so nested pawn generation
        /// (relatives) keeps its own state.
        /// </summary>
        public class Frame
        {
            /// <summary>
            /// The pawn whose discard is held back until the postfix.
            /// </summary>
            public Pawn pawn;
            /// <summary>
            /// True when the gender of this request was changed by this mod.
            /// </summary>
            public bool genderChanged;
            /// <summary>
            /// The vanilla generation status (boxed) captured when vanilla tried to discard `pawn`.
            /// Null when vanilla never tried to discard it.
            /// </summary>
            public object status;
        }

        private static readonly List<Frame> frames = new List<Frame>();

        /// <summary>
        /// One entry per running `GeneratePawn` call.
        /// True when this mod changed the gender of that request.
        /// </summary>
        public static readonly List<bool> genderChanges = new List<bool>();

        public static Frame Current => frames.Count > 0 ? frames[frames.Count - 1] : null;

        public static void SetPending(Pawn pawn)
        {
            Frame frame = Current;

            if (frame != null)
                frame.pawn = pawn;
        }

        /// <summary>
        /// Returns the frame that holds back the discard of this pawn, if any.
        /// </summary>
        public static Frame FindPending(Pawn pawn)
        {
            if (pawn == null)
                return null;

            for (int i = frames.Count - 1; i >= 0; i--)
                if (frames[i].pawn == pawn)
                    return frames[i];

            return null;
        }

        /// <summary>
        /// Runs the vanilla discard that was held back.
        /// Vanilla reads the last entry of `pawnsBeingGenerated`,
        /// but that entry is already removed when the postfix runs,
        /// so the captured entry is put back for the call.
        /// </summary>
        private static void DiscardGeneratedPawn(Frame frame, Pawn pawn)
        {
            if (frame.status == null)
                // Vanilla never tried to discard this pawn. Do the same.
                return;

            MethodInfo method = AccessTools.Method(
                typeof(PawnGenerator),
                "DiscardGeneratedPawn"
            );

            if (method == null)
                return;

            IList pawnsBeingGenerated = Tools.PawnsBeingGenerated;

            if (pawnsBeingGenerated == null)
                return;

            pawnsBeingGenerated.Add(frame.status);

            try
            {
                method.Invoke(null, new object[] { pawn });
            }
            finally
            {
                pawnsBeingGenerated.RemoveAt(pawnsBeingGenerated.Count - 1);
            }
        }

        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static void Prefix()
        {
            frames.Add(new Frame
            {
                genderChanged = genderChanges.Count > 0 && genderChanges[genderChanges.Count - 1]
            });
        }

        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void Postfix(ref Pawn __result, ref string error)
        {
            Frame frame = Current;

            if (frame == null || frame.pawn == null)
                return;

            Pawn pawn = frame.pawn;

            // Clear first, so the discard prefix lets the vanilla discard run.
            frame.pawn = null;

            if (__result != null)
                return;

            if (frame.genderChanged && error == REQUIRED_WORK_TAGS_ERROR)
            {
                Log.Warning($"[CustomPawnGeneration] '{pawn.Name}' was generated with an error '{error}'!");

                __result = pawn;
                error = null;
                return;
            }

            DiscardGeneratedPawn(frame, pawn);
        }

        /// <summary>
        /// Runs even when generation throws, so a frame is never left behind.
        /// </summary>
        [HarmonyFinalizer]
        public static void Finalizer()
        {
            if (frames.Count > 0)
                frames.RemoveAt(frames.Count - 1);
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "DiscardGeneratedPawn")]
    public static class Patch_PawnGenerator_DiscardGeneratedPawn
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            Patch_PawnGenerator_TryGenerateNewPawnInternal.Frame frame =
                Patch_PawnGenerator_TryGenerateNewPawnInternal.FindPending(pawn);

            if (frame == null)
                return true;

            // Hold back the discard. Keep the status vanilla needs to run it later.
            IList pawnsBeingGenerated = Tools.PawnsBeingGenerated;

            if (pawnsBeingGenerated != null && pawnsBeingGenerated.Count > 0)
                frame.status = pawnsBeingGenerated[pawnsBeingGenerated.Count - 1];

            return false;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
    public static class Patch_PawnGenerator_GeneratePawn
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static void Prefix(ref PawnGenerationRequest request, out bool __state)
        {
            Patch_PawnGenerator_TryGenerateNewPawnInternal.genderChanges.Add(ChangeGender(ref request));
            __state = true;
        }

        [HarmonyFinalizer]
        public static void Finalizer(bool __state)
        {
            List<bool> genderChanges = Patch_PawnGenerator_TryGenerateNewPawnInternal.genderChanges;

            if (__state && genderChanges.Count > 0)
                genderChanges.RemoveAt(genderChanges.Count - 1);
        }

        /// <summary>
        /// Returns true when the gender of the request was changed.
        /// </summary>
        private static bool ChangeGender(ref PawnGenerationRequest request)
        {
            if (!request.KindDef.RaceProps.hasGenders)
                return false;

            Settings.GetStateMale(
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific) ? request.KindDef.race : null,
                out Settings.State global,
                out Settings.State state
            );

            if (!Settings.Bool(global, state, Strings.Keys.OverrideGender))
                return false;

            if (!Settings.Bool(global, state, Strings.Keys.UnforcedGender) &&
                request.FixedGender != null)
                return false;

            bool isGlobal = Settings.IsGlobal(state, Strings.Keys.OverrideGender);
            int value = Settings.Int(global, state, Strings.Keys.GenderSlider, isGlobal);
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
                return false;

            request.FixedGender = gender;

            return true;
        }

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Patch(Pawn __result, PawnGenerationRequest request)
        {
            if (__result == null)
                return;

            Settings.GetState(
                __result,
                Settings.State.GLOBAL.Bool(Strings.Keys.UseRaceSpecific),
                out _,
                out Settings.State state
            );

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

            Patch_ParentRelationUtility_GetParent.module.Unpatch();
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

            if (!Patch_ParentRelationUtility_GetParent.module.IsPatched)
            {
                in_postfix = true;
                should_unpatch = 1;

                Patch_ParentRelationUtility_GetParent.module.Patch();
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
