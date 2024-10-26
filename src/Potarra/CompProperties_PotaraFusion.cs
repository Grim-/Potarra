using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using SaiyanMod;
using TaranMagicFramework;

namespace Potarra
{
    public class CompProperties_PotaraFusion : CompProperties
    {
        public CompProperties_PotaraFusion()
        {
            compClass = typeof(CompPotaraFusion);
        }
    }

    public class CompPotaraFusion : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
           // Log.Message($"PotaraFusion comp spawned on {parent}");
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            //Log.Message("PotaraFusion comp loaded");
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }


            if (parent is Apparel apparel && apparel.Wearer != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Fusion",
                    defaultDesc = "Fuse with another being wearing a Potara earring.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct"),
                    action = () =>
                    {
                        // Start targeting
                        Find.Targeter.BeginTargeting(new TargetingParameters
                        {
                            canTargetPawns = true,
                            validator = (TargetInfo target) =>
                            {
                                if (target.Thing is Pawn targetPawn)
                                {
                                    return HasPotaraEarring(targetPawn);
                                }
                                return false;
                            }
                        },
                        (LocalTargetInfo target) =>
                        {
                            if (target.Thing is Pawn targetPawn)
                            {
                                InitiateFusion(apparel.Wearer, targetPawn);
                            }
                        }, null, null, null);
                    }
                };
            }
        }


        private Command_Action GetActions(Apparel apparel)
        {
            Command_Action commandAction = new Command_Action();
            commandAction.defaultLabel = "Fusion";
            commandAction.defaultDesc = "Fuse with another being wearing a Potara earring.";
            commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/PotaraFusion");
            commandAction.action = () =>
            {
                // Start targeting
                Find.Targeter.BeginTargeting(new TargetingParameters
                {
                    canTargetPawns = true,
                    validator = (TargetInfo target) =>
                    {
                        if (target.Thing is Pawn targetPawn)
                        {
                            return HasPotaraEarring(targetPawn) && targetPawn != apparel.Wearer;
                        }
                        return false;
                    }
                },
                (LocalTargetInfo target) =>
                {
                    if (target.Thing is Pawn targetPawn)
                    {
                        InitiateFusion(apparel.Wearer, targetPawn);
                    }
                }, null, null, null);
            };

            return commandAction;
        }

        private bool HasPotaraEarring(Pawn pawn)
        {
            return pawn.apparel?.WornApparel
                .Any(a => a.def== PotarraDefOf.DBZ_PotaraEarring) ?? false;
        }

        private void InitiateFusion(Pawn pawn1, Pawn pawn2)
        {
            // Create the fused pawn
            Pawn fusedPawn = PawnGenerator.GeneratePawn(SaiyanMod.SR_DefOf.SR_LegendarySaiyanColonist, pawn1.Faction);

            Hediff hediff = fusedPawn.health.GetOrAddHediff(PotarraDefOf.DBZ_PotaraFusion);
            GenSpawn.Spawn(fusedPawn, pawn1.Position, pawn1.Map);

            var fusionComp = hediff.TryGetComp<HediffComp_PotaraFusion>();

            if (fusionComp != null)
            {
                fusionComp.SetOriginalPawns(pawn1, pawn2);


                NameTriple pawnOneName = (NameTriple)pawn1.Name;
                NameTriple pawnTwoName = (NameTriple)pawn2.Name;

                NameTriple FusedName = new NameTriple(pawnOneName.First + pawnTwoName.First,
                    pawnOneName.Nick + pawnTwoName.Nick,
                    pawnOneName.Last + pawnTwoName.Last);

                foreach (var item in pawn1.story.traits.allTraits)
                {
                    if (!fusedPawn.story.traits.HasTrait(item.def))
                    {
                        fusedPawn.story.traits.GainTrait(new Trait(item.def, item.Degree));
                    }
                }

                foreach (var item in pawn2.story.traits.allTraits)
                {
                    if (!fusedPawn.story.traits.HasTrait(item.def))
                    {
                        fusedPawn.story.traits.GainTrait(new Trait(item.def, item.Degree));
                    }
                }


                List<Gene> GeneChoices = pawn1.GetSaiyanGenes();
                GeneChoices.AddRange(pawn2.GetSaiyanGenes());


                PawnSkillExtensions.RemoveKiGene(fusedPawn);
                GeneDef FusedGeneChoice = PawnSkillExtensions.PickBestKiGene(pawn1, pawn2);

                if (FusedGeneChoice != null)
                {
                    if (!fusedPawn.genes.HasActiveGene(FusedGeneChoice))
                    {
                        fusedPawn.genes.AddGene(FusedGeneChoice, true);
                    }
                }

                fusedPawn.Name = FusedName;

                fusedPawn.MergeSkillsFrom(pawn1, pawn2);
                MergeAllAbilities(pawn1, pawn2, fusedPawn);
                pawn1.DeSpawn();
                pawn2.DeSpawn();
            }
        }


        private static void MergeAllAbilities(Pawn pawnOne, Pawn pawnTwo, Pawn fusedPawn)
        {
            var pawnOneAbilities = pawnOne.GetComp<CompAbilities>();
            var pawnTwoAbilities = pawnTwo.GetComp<CompAbilities>();
            var fusedPawnAbilities = fusedPawn.GetComp<CompAbilities>();

            if (pawnOneAbilities == null || pawnTwoAbilities == null || fusedPawnAbilities == null)
            {
                Log.Warning($"Unable to merge abilities - missing CompAbilities on one or more pawns");
                return;
            }

            if (!pawnOneAbilities.TryGetKIAbilityClass(out AbilityClassKI oneAbility) ||
                !pawnTwoAbilities.TryGetKIAbilityClass(out AbilityClassKI twoAbility) ||
                !fusedPawnAbilities.TryGetKIAbilityClass(out AbilityClassKI fusedAbility))
            {
                Log.Warning($"Unable to merge abilities - missing KI ability class on one or more pawns");
                return;
            }


            foreach (var tree in oneAbility.removedAbilities)
            {
                fusedAbility.LearnAbility(tree.Key, false, tree.Value.MaxLevel);
            }

            foreach (var tree in twoAbility.removedAbilities)
            {
                fusedAbility.LearnAbility(tree.Key, false, tree.Value.MaxLevel);
            }

            foreach (TaranMagicFramework.AbilityClass abilityClass in fusedPawnAbilities.abilityClasses.Values)
            {
                Log.Message($"Ability Class : {abilityClass.def.label}");

                foreach (TaranMagicFramework.AbilityTreeDef abilityTreeDef in abilityClass.UnlockedTrees)
                {
                    Log.Message($"Ability Tree : {abilityTreeDef.label}");

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
}
