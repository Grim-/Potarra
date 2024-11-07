using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using LudeonTK;
using TaranMagicFramework;
using SaiyanMod;
using System.Linq;
using System;
using System.Reflection;
using HarmonyLib;

namespace Potarra
{
    public static class PawnSkillExtensions
    {
        public enum SaiyanRank
        {
            KIUSER = 0,
            HALF = 1,
            LOW = 2,
            NORMAL = 3,
            ELITE = 4,
            LEGENDARY = 5,
        }

        public static Dictionary<SaiyanRank, GeneDef> SaiyanRanks = new Dictionary<SaiyanRank, GeneDef>()
        {
            {SaiyanRank.HALF, PotarraDefOf.SR_HalfSaiyanClassSaiyan},
            {SaiyanRank.LOW, PotarraDefOf.SR_LowClassSaiyan},
            {SaiyanRank.NORMAL, PotarraDefOf.SR_NormalClassSaiyan},
            {SaiyanRank.ELITE, PotarraDefOf.SR_EliteClassSaiyan},
            {SaiyanRank.LEGENDARY, PotarraDefOf.SR_LegendaryClassSaiyan}
        };

        private static readonly Dictionary<SaiyanRank, (int minScore, int maxScore)> RankScores = new Dictionary<SaiyanRank, (int minScore, int maxScore)>
        {
            { SaiyanRank.HALF, (-20, 15) },    // Half-Saiyans start lower but have potential
            { SaiyanRank.LOW, (16, 30) },      // Adjusted ranges to account for Half-Saiyan
            { SaiyanRank.NORMAL, (31, 60) },
            { SaiyanRank.ELITE, (61, 90) },
            { SaiyanRank.LEGENDARY, (91, 100) }
        };


        [DebugAction("The Saiyans", "TryAutoGainAbilities", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LearnAllAbilityClassAbilities(Pawn p)
        {
            var pawnOneAbilities = p.GetComp<CompAbilities>();

            if (pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                foreach (TaranMagicFramework.AbilityClass abilityClass in pawnOneAbilities.abilityClasses.Values)
                {
                    Log.Message($"Ability Class : {abilityClass.def.label}");
                    foreach (TaranMagicFramework.AbilityTreeDef abilityTreeDef in abilityClass.UnlockedTrees)
                    {
                        Log.Message($"Ability Tree : {abilityTreeDef.defName}");
                        foreach (TaranMagicFramework.AbilityDef abilityDef in abilityTreeDef.AllAbilities)
                        {
                            Log.Message($"Ability : {abilityDef.label}");

                            if (!fusedAbility.Learned(abilityDef))
                            {
                                fusedAbility.LearnAbility(abilityDef, false, abilityDef.abilityTiers.Count - 1);
                            }
                        }
                    }
                }
            }
        }

        [DebugAction("The Saiyans", "GrantLevel", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GrantLevel(Pawn p)
        {
            var pawnOneAbilities = p.GetComp<CompAbilities>();

            if (pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                fusedAbility.SetLevel(fusedAbility.level + 1);
            }
        }

        [DebugAction("The Saiyans", "GrantSuperSaiyan", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GrantSuperSaiyan(Pawn p)
        {
            var pawnOneAbilities = p.GetComp<CompAbilities>();

            if (pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                if (!fusedAbility.Learned(SR_DefOf.SR_SuperSaiyan))
                {
                    fusedAbility.LearnAbility(SR_DefOf.SR_SuperSaiyan, false, SR_DefOf.SR_SuperSaiyan.abilityTiers.Count - 1);
                }
            }
        }


        [DebugAction("The Saiyans", "RemovePotarraFatigue", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemovePotarraFatigue(Pawn p)
        {
            if (p.health.hediffSet.TryGetHediff(PotarraDefOf.DBZ_PotaraFusionFatigue, out Hediff hediff))
            {
                p.health.RemoveHediff(hediff);
            }
        }

        [DebugAction("The Saiyans", "MaxLevel", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MaxLevel(Pawn p)
        {
            var pawnOneAbilities = p.GetComp<CompAbilities>();

            if (pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                fusedAbility.SetLevel(fusedAbility.MaxLevel);
            }
        }

        [DebugAction("The Saiyans", "MaxSkillPoints", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MaxSkillPoints(Pawn p)
        {
            var pawnOneAbilities = p.GetComp<CompAbilities>();

            if (pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                fusedAbility.skillPoints = fusedAbility.MaxLevel;
            }
        }

        public static bool HasFusionAbility(Pawn p)
        {
            return p.abilities.AllAbilitiesForReading.Any(x => x.def == PotarraDefOf.DBZ_FusionAbility || x.def == PotarraDefOf.DBZ_PotaraFusionAbility || x.def == PotarraDefOf.DBZ_PotaraPermanentFusionAbility);
        }

        public static void LearnAndLevelAbilities(Pawn p)
        {
            var pawnAbilities = p.GetComp<CompAbilities>();
            if (!pawnAbilities.TryGetKIAbilityClass(out AbilityClassKI kiAbility)) return;

            int pointsAvailable = kiAbility.skillPoints;

            foreach (TaranMagicFramework.AbilityClass abilityClass in pawnAbilities.abilityClasses.Values)
            {
                foreach (TaranMagicFramework.AbilityTreeDef abilityTreeDef in abilityClass.UnlockedTrees)
                {
                    foreach (TaranMagicFramework.AbilityDef abilityDef in abilityTreeDef.AllAbilities)
                    {
                        // Learn the ability at base level if not known
                        if (!kiAbility.Learned(abilityDef))
                        {
                            kiAbility.LearnAbility(abilityDef, false, 0);
                        }


                        // Get current ability instance
                        if (kiAbility.learnedAbilities.TryGetValue(abilityDef, out var ability))
                        {
                            if (ability.FullyLearned)
                            {
                                continue;
                            }

                            int pointsNeeded = ability.MaxLevel - ability.level;
                            if (pointsNeeded <= 0) 
                                continue;

                            int pointsToSpend = Math.Min(pointsNeeded, pointsAvailable);
                            ability.ChangeLevel(ability.level + pointsToSpend);
                            kiAbility.skillPoints -= Mathf.Clamp(pointsToSpend, 0, 5000);
                            pointsAvailable = kiAbility.skillPoints;
                            if (pointsAvailable <= 0) return;
                        }
                    }
                }
            }
        }


        public static int GetPawnKiLevel(this Pawn Pawn)
        {
            var pawnAbilities = Pawn.GetPawnAbilityClassKI();
            if (pawnAbilities == null) return -1;
            return pawnAbilities.level;
        }
        public static float GetPawnKiXP(this Pawn Pawn)
        {
            var pawnAbilities = Pawn.GetPawnAbilityClassKI();
            if (pawnAbilities == null) return -1;
            return pawnAbilities.xpPoints;
        }
        public static AbilityClassKI GetPawnAbilityClassKI(this Pawn Pawn)
        {
            var pawnAbilities = Pawn.GetComp<CompAbilities>();
            if (!pawnAbilities.TryGetKIAbilityClass(out AbilityClassKI kiAbility)) return null;
            return kiAbility;
        }


        [DebugAction("The Saiyans", "ForceEndFusion", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ForceEndFusion(Pawn p)
        {
            if (p.IsAFusionPawn())
            {
                var FusionComp = p.GetFusionHediff();
                if (FusionComp != null)
                {
                    FusionComp.EndFusion();
                }
            }
        }

        [DebugAction("The Saiyans", "RemoveKiGenes", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveKiGene(Pawn p)
        {
            List<Gene> SaiyanGenes = p.GetSaiyanGenes();
            foreach (var item in SaiyanGenes)
            {
                if (p.genes.HasActiveGene(item.def))
                {
                    p.genes.RemoveGene(item);
                }
            }
        }

        [DebugAction("The Saiyans", "GiveRandomSaiyanGene", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]

        public static void GiveRandomtSaiyanGenes(Pawn Pawn)
        {
            GeneDef RandomSaiyanGene = GetRandomSaiyanGene();

            if (!Pawn.genes.HasActiveGene(RandomSaiyanGene))
            {
                Pawn.genes.AddGene(RandomSaiyanGene, true);
            }
        }

        public static List<Gene> GetSaiyanGenes(this Pawn Pawn)
        {
            return Pawn.genes.GenesListForReading.FindAll(x => x.def.GetModExtension<GeneExtension>() != null);
        }
        public static GeneDef GetRandomSaiyanGene()
        {
            return DefDatabase<GeneDef>.AllDefsListForReading.FindAll(x => x.GetModExtension<GeneExtension>() != null).RandomElement();
        }

        public static bool HasAbility(this Pawn pawn, RimWorld.AbilityDef AbiityDef)
        {
            if (pawn?.abilities == null)
            {
                return false;
            }

            return pawn.abilities.AllAbilitiesForReading.Find(x => x.def == AbiityDef) != null;
        }

        public static GeneDef PickBestKiGene(Pawn parent1, Pawn parent2)
        {
            var parent1Genes = parent1.GetSaiyanGenes();
            var parent2Genes = parent2.GetSaiyanGenes();

            if (!parent1Genes.Any() || !parent2Genes.Any())
            {
                return PotarraDefOf.SR_LowClassSaiyan;
            }

            var parent1Rank = GetSaiyanRank(parent1Genes.First().def);
            var parent2Rank = GetSaiyanRank(parent2Genes.First().def);
            var higherRank = (SaiyanRank)Math.Max((int)parent1Rank, (int)parent2Rank);

            var fusedRank = (SaiyanRank)Math.Min((int)SaiyanRank.LEGENDARY, (int)higherRank + 1);
            return SaiyanRanks[fusedRank];
        }


        public static HediffComp_PotaraFusion GetFusionHediff(this Pawn Pawn)
        {
            Hediff permanentHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(PotarraDefOf.DBZ_PermanentPotaraFusion);
            if (permanentHediff != null && permanentHediff.TryGetComp(out HediffComp_PotaraFusion permanentComp))
            {
                return permanentComp;
            }

            Hediff regularHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(PotarraDefOf.DBZ_PotaraFusion);
            if (regularHediff != null && regularHediff.TryGetComp(out HediffComp_PotaraFusion regularComp))
            {
                return regularComp;
            }

            return null;
        }

        public static List<Gene> GetNonKiGenes(this Pawn pawn)
        {
            return pawn.genes.GenesListForReading.FindAll(x => x.def.GetModExtension<GeneExtension>() == null).ToList();
        }

        private static SaiyanRank GetSaiyanRank(GeneDef geneDef)
        {
            var foundRank = SaiyanRanks.FirstOrDefault(x => x.Value == geneDef);
            return foundRank.Equals(default(KeyValuePair<SaiyanRank, GeneDef>)) ? SaiyanRank.LOW : foundRank.Key;
        }


        private static int GetSaiyanClass(GeneDef geneDef)
        {
            if (geneDef == PotarraDefOf.SR_LegendaryClassSaiyan)
                return 5;
            if (geneDef == PotarraDefOf.SR_EliteClassSaiyan)
                return 4;
            if (geneDef == PotarraDefOf.SR_NormalClassSaiyan)
                return 2;
            return 1;
        }

        public static void UnlockTree(AbilityClassKI CompAbilities, AbilityTreeDef TreeDef)
        {
            if (!CompAbilities.TreeUnlocked(TreeDef))
            {
                CompAbilities.UnlockTree(TreeDef);
            }
        }


        public static bool IsAFusionPawn(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(PotarraDefOf.DBZ_PotaraFusion);
        }

        public static void MergeSkillsFrom(this Pawn targetPawn, Pawn pawn1, Pawn pawn2)
        {
            // Get all skill definitions to ensure we process every skill type
            IEnumerable<SkillDef> allSkillDefs = DefDatabase<SkillDef>.AllDefs;

            foreach (SkillDef skillDef in allSkillDefs)
            {
                SkillRecord pawn1Skill = pawn1.skills.GetSkill(skillDef);
                SkillRecord pawn2Skill = pawn2.skills.GetSkill(skillDef);
                SkillRecord targetSkill = targetPawn.skills.GetSkill(skillDef);

                // Get the best level between the two pawns
                int bestLevel = Mathf.Max(pawn1Skill.Level, pawn2Skill.Level);

                // Get the highest passion level (None = 0, Minor = 1, Major = 2)
                Passion bestPassion = (Passion)Mathf.Max(
                    (int)pawn1Skill.passion,
                    (int)pawn2Skill.passion
                );

                // Get the highest xp progress
                float bestXp = Mathf.Max(pawn1Skill.xpSinceLastLevel, pawn2Skill.xpSinceLastLevel);

                targetSkill.Level = bestLevel;
                targetSkill.passion = bestPassion;
                targetSkill.xpSinceLastLevel = bestXp;

                targetSkill.xpSinceMidnight = Mathf.Max(
                    pawn1Skill.xpSinceMidnight,
                    pawn2Skill.xpSinceMidnight
                );
            }
        }
    }
}
