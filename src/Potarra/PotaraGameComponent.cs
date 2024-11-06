using RimWorld;
using RimWorld.Planet;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using Verse;

namespace Potarra
{
    public class FusionManager : GameComponent
    {
        private List<FusionPawnData> fusionData = new List<FusionPawnData>();

        public FusionManager(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref fusionData, "fusionData", LookMode.Deep);
        }
        private Pawn CreateNewFusedPawn(Pawn pawn1, Pawn pawn2, IntVec3 position, Map map, int mergedLevel, float mergedXP, bool isFailure)
        {
            Pawn fusedPawn;
            if (isFailure)
            {
                fusedPawn = CreateFusionPawn(pawn1, pawn2, isFailure);
                fusedPawn.health.GetOrAddHediff(PotarraDefOf.DBZ_PotaraFusionFailure);
                fusedPawn.story.bodyType = Rand.Bool ? BodyTypeDefOf.Fat : BodyTypeDefOf.Thin;
                fusedPawn.ResolveAllGraphicsSafely();
            }
            else
            {
                fusedPawn = CreateFusionPawn(pawn1, pawn2, isFailure);
            }

            UpdateFusedPawnSkills(fusedPawn, pawn1, pawn2);
            SetupFusedPawnAbilities(fusedPawn, pawn1, pawn2, mergedLevel, mergedXP);
            ApplyFusionClothes(fusedPawn);

            RestorePawn(fusedPawn, position, map);

            return fusedPawn;
        }

        public Pawn CreateFusionPawn(Pawn pawn1, Pawn pawn2, bool isFailure)
        {
            PawnKindDef ChosenKind = ChooseKind(pawn1, pawn2, isFailure);
            Gender ChosenGender = ChooseGender(pawn1, pawn2);

            Log.Message($"Generating new fusion Pawn for {pawn1.Name} & {pawn2.Name} Gender {ChosenGender} KindDef {ChosenKind.defName} IsFailure? {isFailure}");

            PawnGenerationRequest req = new PawnGenerationRequest(ChosenKind, pawn1.Faction, PawnGenerationContext.NonPlayer, -1,
                false, false, false, false, true, 0, false, true, false, false, false, false, false, false, false, 0, 0, null, 0,
                null, null, null, null, null, null, null, ChosenGender, null, null, null);

            Pawn fusedPawn = PawnGenerator.GeneratePawn(req);
            GenSpawn.Spawn(fusedPawn, pawn1.Position, pawn1.Map);

            HediffComp_PotaraFusion fusionComp = GetOrAddPotara(fusedPawn);

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


        public static HediffComp_PotaraFusion GetOrAddPotara(Pawn Pawn)
        {
            Log.Message($"Getting or Adding Fusion Hediff to {Pawn.Label}..");

            Hediff hediff = Pawn.health.GetOrAddHediff(PotarraDefOf.DBZ_PotaraFusion);

            if (hediff.TryGetComp(out HediffComp_PotaraFusion potaraFusion))
            {
                return potaraFusion;
            }
            Log.Message($"Failed to find or add Fusion Hediff to {Pawn.Label}..");
            return null;
        }

        public PawnKindDef ChooseKind(Pawn pawn1, Pawn pawn2, bool IsFailure)
        {
            if (IsFailure)
            {
                return PawnKindDefOf.Colonist;
            }
            else
            {
                return Rand.Bool ? pawn1.kindDef : pawn2.kindDef;
            }
        }


        public Gender ChooseGender(Pawn pawn1, Pawn pawn2)
        {
            return pawn1.gender == pawn2.gender ? pawn1.gender : Rand.Bool ? Gender.Male : Gender.Female;
        }

        public Pawn GetOrCreateFusedPawn(Pawn pawn1, Pawn pawn2, IntVec3 position, Map map, AbilityCompProperties_PotaraFusion props, bool isFailure)
        {
            int mergedLevel = PawnFusionUtil.GetMergedLevel(pawn1, pawn2) + props.FusionLevelFlatBonus;
            float mergedExperience = PawnFusionUtil.GetMergedExperience(pawn1, pawn2);
            if (isFailure)
            {
                mergedLevel /= 2 + 1;
                mergedExperience /= 2 + 1;
            }

            if (TryGetFusionDataFor(pawn1, pawn2, out FusionPawnData existingFusion))
            {

                Pawn existingPawn = isFailure ? existingFusion.failedFusePawn : existingFusion.fusedPawn;

                if (existingPawn == null)
                {
                    existingPawn = CreateNewFusedPawn(pawn1, pawn2, position, map, mergedLevel, mergedExperience, isFailure);

                    if (isFailure)
                        existingFusion.SetFailureForm(existingPawn);
                    else
                        existingFusion.SetFusionForm(existingPawn);
                }
                else
                {
                    Log.Message($"Existig fusion pawn found for {pawn1.Label} + {pawn2.Label} : ({existingPawn.Label} (is failure? {isFailure}");
                    // Restore and update existing fusion
                    RestorePawn(existingPawn, position, map);
                    UpdateFusedPawnSkills(existingPawn, pawn1, pawn2);
                    GetOrAddPotara(existingPawn).ResetDuration();
                    var existingAbilityKi = existingPawn.GetPawnAbilityClassKI();
                    existingAbilityKi.SetLevel(mergedLevel);
                    existingAbilityKi.GainXP(mergedExperience);
                    existingAbilityKi.skillPoints = mergedLevel;
                }

                if (!existingPawn.Faction.IsPlayer)
                {
                    existingPawn.SetFaction(Faction.OfPlayer);
                }

                return existingPawn;
            }
            else
            {
                Pawn newFusedPawn = CreateNewFusedPawn(pawn1, pawn2, position, map, mergedLevel, mergedExperience, isFailure);

                if (!newFusedPawn.Faction.IsPlayer)
                {
                    newFusedPawn.SetFaction(Faction.OfPlayer);
                }

                fusionData.Add(new FusionPawnData(pawn1, pawn2, newFusedPawn, isFailure));
                return newFusedPawn;
            }
        }

        private bool TryGetFusionDataFor(Pawn Pawn1, Pawn Pawn2, out FusionPawnData FusionData)
        {
            foreach (var data in fusionData)
            {
                if (data.MatchesPawns(Pawn1, Pawn2))               
                {
                    FusionData = data;
                    LogFusionData(Pawn1, Pawn2, FusionData);
                    return true;
                }
            }

            Log.Message($"No fusion data found for p1={Pawn1.Label}, p2={Pawn2.Label}");
            FusionData = null;
            return false;
        }

        public void LogFusionData(Pawn pawn1, Pawn pawn2, FusionPawnData data)
        {
            var p1Label = pawn1?.Label ?? "null";
            var p1Id = pawn1?.ThingID ?? "null";
            var p2Label = pawn2?.Label ?? "null";
            var p2Id = pawn2?.ThingID ?? "null";
            var fusedLabel = data?.fusedPawn?.Label ?? "null";
            var fusedId = data?.fusedPawn?.ThingID ?? "null";
            var failedLabel = data?.failedFusePawn?.Label ?? "null";
            var failedId = data?.failedFusePawn?.ThingID ?? "null";

            Log.Message(
                $"Fusion data found for p1={p1Label} ({p1Id}), " +
                $"p2={p2Label} ({p2Id}) " +
                $"fusion form: {fusedLabel} ({fusedId}) " +
                $"failed form: {failedLabel} ({failedId})"
            );
        }
        public static void RestorePawn(Pawn pawn, IntVec3 position, Map map)
        {
            Log.Message($"Restoring {pawn.Label} health...");

            List<Hediff> hediffsToRemove = pawn.health.hediffSet.hediffs
                .Where(h =>
                    h.def != PotarraDefOf.DBZ_PotaraFusion &&
                    h.def != SR_DefOf.SR_SaiyanTail &&        
                    (h.def.isBad || h.def.tendable))          
                .ToList();

            foreach (Hediff hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }

            // Reset needs
            if (pawn.needs != null)
            {
                foreach (Need need in pawn.needs.AllNeeds)
                {
                    need.CurLevel = need.MaxLevel;
                }
            }

            pawn.mindState?.Reset();

            if (pawn.Dead)
            {
                ResurrectionUtility.TryResurrect(pawn, new ResurrectionParams
                {
                    gettingScarsChance = 0f,
                    canKidnap = false,
                    canTimeoutOrFlee = false,
                    useAvoidGridSmart = true,
                    canSteal = false,
                    invisibleStun = true
                });
                pawn.ClearAllReservations();
            }
            pawn.ResolveAllGraphicsSafely();
        }
        private void SetupFusedPawnAbilities(Pawn fusedPawn, Pawn pawn1, Pawn pawn2, int mergedLevel, float mergedXP)
        {
            PawnSkillExtensions.RemoveKiGene(fusedPawn);
            GeneDef fusedGeneChoice = PawnSkillExtensions.PickBestKiGene(pawn1, pawn2);
            if (fusedGeneChoice != null && !fusedPawn.genes.HasActiveGene(fusedGeneChoice))
            {
                fusedPawn.genes.AddGene(fusedGeneChoice, true);
            }

            var abilityKi = fusedPawn.GetPawnAbilityClassKI();
            abilityKi.SetLevel(mergedLevel);
            abilityKi.skillPoints = mergedLevel;
            //abilityKi.GainXP(mergedXP);
            PawnSkillExtensions.LearnAndLevelAbilities(fusedPawn);
        }

        private void ApplyFusionClothes(Pawn fusedPawn)
        {
            bool hasWornRobes = fusedPawn.apparel.WornApparel.Any(x => x.def == PotarraDefOf.SR_KaiRobes);
            bool hasRobesInInventory = fusedPawn.inventory.innerContainer.Any(x => x.def == PotarraDefOf.SR_KaiRobes);

            if (!hasWornRobes && !hasRobesInInventory)
            {
                Apparel newRobes = (Apparel)ThingMaker.MakeThing(PotarraDefOf.SR_KaiRobes);
                fusedPawn.apparel.Wear(newRobes, false, false);
            }
        }

        private void UpdateFusedPawnSkills(Pawn fusedPawn, Pawn pawn1, Pawn pawn2)
        {
            if (fusedPawn.skills == null || pawn1.skills == null || pawn2.skills == null) return;

            foreach (SkillRecord skill in fusedPawn.skills.skills)
            {
                SkillRecord skill1 = pawn1.skills.GetSkill(skill.def);
                SkillRecord skill2 = pawn2.skills.GetSkill(skill.def);

                int newLevel = System.Math.Max(skill1.Level, skill2.Level);
                skill.Level = newLevel;

                skill.passion = skill1.Level >= skill2.Level ? skill1.passion : skill2.passion;
            }
        }

        public void RemoveFusionData(Pawn fusedPawn)
        {
            fusionData.RemoveAll(f => f.fusedPawn == fusedPawn || f.failedFusePawn == fusedPawn);
        }

        public bool TryGetOriginalPawns(Pawn fusedPawn, out Pawn pawn1, out Pawn pawn2)
        {
            var fusion = fusionData.Find(f => f.fusedPawn == fusedPawn || f.failedFusePawn == fusedPawn);
            if (fusion != null)
            {
                pawn1 = fusion.originalPawn1;
                pawn2 = fusion.originalPawn2;
                return true;
            }

            pawn1 = null;
            pawn2 = null;
            return false;
        }
    }
}
