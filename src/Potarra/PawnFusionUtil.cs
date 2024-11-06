using RimWorld;
using System.Collections.Generic;
using Verse;
using TaranMagicFramework;
using SaiyanMod;
using UnityEngine;

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
        public static float GetMergedExperience(Pawn Pawn1, Pawn Pawn2)
        {
            float pawnOneLevel = Pawn1.GetPawnKiXP();
            float pawnTwoLevel = Pawn2.GetPawnKiXP();
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
                        if (abilityDef == SR_DefOf.SR_TrueSuperSaiyan || abilityDef == PotarraDefOf.SR_MajinMark)
                        {
                            continue;
                        }

                        int level1 = ki1.Learned(abilityDef) ? 1 : 0;
                        int level2 = ki2.Learned(abilityDef) ? 1 : 0;

                        // Learn and set combined level
                        if (!fusedKi.Learned(abilityDef) && level1 + level2 > 0)
                        {

                            fusedKi.LearnAbility(abilityDef, false, Mathf.Clamp(level1 + level2, 0, abilityDef.abilityTiers.Count -1));
                        }
                    }
                }
            }
        }
    }
}
