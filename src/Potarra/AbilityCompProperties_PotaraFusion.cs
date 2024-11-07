using RimWorld;
using RimWorld.Planet;
using SaiyanMod;
using System;
using System.Collections.Generic;
using TaranMagicFramework;
using Verse;

namespace Potarra
{
    public class AbilityCompProperties_PotaraFusion : CompProperties_AbilityEffect
    {
        public bool IsPermanent = false;
        public int FusionLevelFlatBonus = 30;
        public float FailureChance = 0.2f;

        public AbilityCompProperties_PotaraFusion()
        {
            compClass = typeof(AbilityComp_PotaraFusion);
        }
    }


    public class AbilityComp_PotaraFusion : CompAbilityEffect
    {
        new AbilityCompProperties_PotaraFusion Props => (AbilityCompProperties_PotaraFusion)props;
        private FusionManager FusionManager => Current.Game.GetComponent<FusionManager>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Thing is Pawn targetPawn && parent.pawn != null)
            {
                if (targetPawn.GetComp<CompAbilities>() == null)
                {
                    return;
                }

                if (parent.pawn.IsAFusionPawn() || targetPawn.IsAFusionPawn())
                {
                    Messages.Message($"{parent.pawn.NameShortColored} cannot fuse with {targetPawn.NameShortColored}, pawns that are already fused cannot fuse with any other pawn.", MessageTypeDefOf.NegativeEvent);
                    //dont let merged pawns merge again
                    return;
                }

                InitiateFusion(parent.pawn, targetPawn);
            }
        }

        private void InitiateFusion(Pawn pawn1, Pawn pawn2)
        {
            bool IsFusionFailure = Rand.Value < Props.FailureChance;
            Map map = parent.pawn.Map;
            IntVec3 position = parent.pawn.Position;
            Pawn fusedPawn = FusionManager.GetOrCreateFusedPawn(pawn1, pawn2, position, map, Props, IsFusionFailure, Props.IsPermanent);
            pawn1.DeSpawn();
            Find.WorldPawns.PassToWorld(pawn1, PawnDiscardDecideMode.KeepForever);

            pawn2.DeSpawn();
            Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.KeepForever);

            if (!fusedPawn.Spawned)
            {
                GenSpawn.Spawn(fusedPawn, position, map);
            }



            if (!IsFusionFailure)
            {
                Messages.Message($"{pawn1.NameShortColored} fused with {pawn2.NameShortColored} creating {fusedPawn.NameShortColored}!", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message($"Fusion Failure! {pawn1.NameShortColored} failed to fuse properly with {pawn2.NameShortColored} creating {fusedPawn.NameShortColored}!", MessageTypeDefOf.NegativeHealthEvent);
            }
 
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) &&
                   target.Thing is Pawn targetPawn &&
                   targetPawn.IsAbilityUser() &&
                   parent.pawn.IsAbilityUser() &&
                   !targetPawn.DeadOrDowned &&
                   (HasPotaraEarring(targetPawn) || PawnSkillExtensions.HasFusionAbility(targetPawn)) &&
                   targetPawn != parent.pawn &&
                   !HasFusionFatigue(targetPawn) &&
                   !HasFusionFatigue(parent.pawn);
        }

        private bool HasPotaraEarring(Pawn pawn)
        {
            return pawn.apparel?.WornApparel
                .Any(a => a.def == PotarraDefOf.DBZ_PotaraEarring) ?? false;
        }

        private bool HasFusionFatigue(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(PotarraDefOf.DBZ_PotaraFusionFatigue);
        }

        private bool HasKi(Pawn Pawn)
        {
            return Pawn.IsAbilityUser();
        }
    }
}


