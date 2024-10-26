using RimWorld;
using System.Collections.Generic;
using Verse;
using TaranMagicFramework;
using SaiyanMod;

namespace Potarra
{
    public static class PawnFusionUtil
    {
        public static void MergeTraits(this Pawn FusionPawn, Pawn Pawn1, Pawn Pawn2)
        {
            foreach (var item in Pawn1.story.traits.allTraits)
            {
                if (!FusionPawn.story.traits.HasTrait(item.def))
                {
                    FusionPawn.story.traits.GainTrait(new Trait(item.def, item.Degree));
                }
            }

            foreach (var item in Pawn2.story.traits.allTraits)
            {
                if (!FusionPawn.story.traits.HasTrait(item.def))
                {
                    FusionPawn.story.traits.GainTrait(new Trait(item.def, item.Degree));
                }
            }
        }

        public static void MergeGenes(this Pawn FusionPawn, Pawn Pawn1, Pawn Pawn2)
        {
            List<Gene> GeneChoices = Pawn1.GetNonKiGenes();
            GeneChoices.AddRange(Pawn2.GetNonKiGenes());

            foreach (var item in GeneChoices)
            {
                if (!FusionPawn.genes.HasActiveGene(item.def))
                {
                    FusionPawn.genes.AddGene(item.def, true);
                }
            }
        }

        public static void MergeName(this Pawn FusionPawn, Pawn Pawn1, Pawn Pawn2)
        {
            NameTriple pawnOneName = (NameTriple)Pawn1.Name;
            NameTriple pawnTwoName = (NameTriple)Pawn2.Name;

            NameTriple FusedName = new NameTriple(pawnOneName.First + pawnTwoName.First,
                pawnOneName.Nick + pawnTwoName.Nick,
                pawnOneName.Last + pawnTwoName.Last);
            FusionPawn.Name = FusedName;
        }
        public static int GetMergedLevel(Pawn Pawn1, Pawn Pawn2)
        {
            int pawnOneLevel = Pawn1.GetPawnKiLevel();
            int pawnTwoLevel = Pawn2.GetPawnKiLevel();
            return pawnOneLevel + pawnTwoLevel;
        }


        public static void MergeAbilityLevels(this Pawn FusionPawn, Pawn pawn1, Pawn pawn2)
        {
            var fusedAbilities = FusionPawn.GetComp<CompAbilities>();
            var abilities1 = pawn1.GetComp<CompAbilities>();
            var abilities2 = pawn2.GetComp<CompAbilities>();

            if (!fusedAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedKi) ||
                !abilities1.TryGetKIAbilityClass(out AbilityClassKI ki1) ||
                !abilities2.TryGetKIAbilityClass(out AbilityClassKI ki2))
                return;

            foreach (AbilityClass abilityClass in fusedAbilities.abilityClasses.Values)
            {
                foreach (AbilityTreeDef abilityTreeDef in abilityClass.UnlockedTrees)
                {
                    foreach (TaranMagicFramework.AbilityDef abilityDef in abilityTreeDef.AllAbilities)
                    {
                        if (abilityDef == SR_DefOf.SR_TrueSuperSaiyan || abilityDef == DefDatabase<TaranMagicFramework.AbilityDef>.GetNamed("SR_MajinMark"))
                        {
                            continue;
                        }

                        int level1 = ki1.Learned(abilityDef) ? 1 : 0;
                        int level2 = ki2.Learned(abilityDef) ? 1 : 0;

                        // Learn and set combined level
                        if (!fusedKi.Learned(abilityDef) && level1 + level2 > 0)
                        {
                            fusedKi.LearnAbility(abilityDef, false, level1 + level2);
                        }
                    }
                }
            }
        }


        public static Pawn CreateFusionPawn(Pawn pawn1, Pawn pawn2, PawnKindDef OverrideKindDef = null)
        {
            PawnKindDef ChosenKind = OverrideKindDef != null ? OverrideKindDef : Rand.Bool ? pawn1.kindDef : pawn2.kindDef;

            PawnGenerationRequest req = new PawnGenerationRequest(ChosenKind, pawn1.Faction, PawnGenerationContext.NonPlayer, -1,
                false, false, false, false, true, 0, false, true, false, false, false, false, false, false, false, 0, 0, null, 0,
                null, null, null, null, null, null, null, pawn1.gender == pawn2.gender ? pawn1.gender : Rand.Bool ? Gender.Male : Gender.Female);

            // Create the fused pawn
            Pawn fusedPawn = PawnGenerator.GeneratePawn(req);

            Hediff hediff = fusedPawn.health.GetOrAddHediff(PotarraDefOf.DBZ_PotaraFusion);
            GenSpawn.Spawn(fusedPawn, pawn1.Position, pawn1.Map);

            HediffComp_PotaraFusion fusionComp = hediff.TryGetComp<HediffComp_PotaraFusion>();

            if (fusionComp != null)
            {
                fusionComp.SetOriginalPawns(pawn1, pawn2);
                fusedPawn.MergeName(pawn1, pawn2);
                fusedPawn.MergeTraits(pawn1, pawn2);

                fusedPawn.MergeGenes(pawn1, pawn2);
                fusedPawn.MergeSkillsFrom(pawn1, pawn2);
                fusedPawn.MergeAbilityLevels(pawn1, pawn2);
            }

            return fusedPawn;
        }
    }
}
