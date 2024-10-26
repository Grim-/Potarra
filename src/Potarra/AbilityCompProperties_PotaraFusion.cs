using RimWorld;
using SaiyanMod;
using System;
using System.Collections.Generic;
using TaranMagicFramework;
using Verse;

namespace Potarra
{
    public class AbilityCompProperties_PotaraFusion : CompProperties_AbilityEffect
    {
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

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Thing is Pawn targetPawn && parent.pawn != null)
            {
                if (parent.pawn.IsAFusionPawn() || targetPawn.IsAFusionPawn())
                {
                    //dont let merged pawns merge again
                    return;
                }

                InitiateFusion(parent.pawn, targetPawn);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) &&
                   target.Thing is Pawn targetPawn &&
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

        private void InitiateFusion(Pawn pawn1, Pawn pawn2)
        {
            bool IsFusionFailure = Rand.Value <= Props.FailureChance;
            Pawn fusedPawn = null;

            int MergedLevel = PawnFusionUtil.GetMergedLevel(pawn1, pawn2) + Props.FusionLevelFlatBonus;

            if (IsFusionFailure)
            {
                fusedPawn = PawnFusionUtil.CreateFusionPawn(pawn1, pawn2, PawnKindDefOf.Colonist);
                fusedPawn.health.GetOrAddHediff(PotarraDefOf.DBZ_PotaraFusionFailure);
                MergedLevel /= 2 + 1;
                fusedPawn.story.bodyType = BodyTypeDefOf.Fat;
                fusedPawn.ResolveAllGraphicsSafely();
            }
            else
            {
                fusedPawn = PawnFusionUtil.CreateFusionPawn(pawn1, pawn2);
            }

            PawnSkillExtensions.RemoveKiGene(fusedPawn);
            GeneDef FusedGeneChoice = PawnSkillExtensions.PickBestKiGene(pawn1, pawn2);

            if (FusedGeneChoice != null)
            {
                if (!fusedPawn.genes.HasActiveGene(FusedGeneChoice))
                {
                    fusedPawn.genes.AddGene(FusedGeneChoice, true);
                }
            }
            var abilityKi = fusedPawn.GetPawnAbilityClassKI();

            abilityKi.SetLevel(MergedLevel);
            abilityKi.skillPoints = MergedLevel;
            PawnSkillExtensions.LearnAndLevelAbilities(fusedPawn);
            pawn1.DeSpawn();
            pawn2.DeSpawn();
        }
    }

    public class CompProperties_GrantAbilityOnUse : CompProperties_UseEffect
    {
        public RimWorld.AbilityDef ability;

        public CompProperties_GrantAbilityOnUse()
        {
            compClass = typeof(GrantAbilityOnUse);
        }
    }

    public class GrantAbilityOnUse : CompUseEffect
    {
        public new CompProperties_GrantAbilityOnUse Props => (CompProperties_GrantAbilityOnUse)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            if (!usedBy.HasAbility(Props.ability))
            {
                usedBy.abilities.GainAbility(Props.ability);
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.HasAbility(Props.ability))
            {
                return "PsycastNeurotrainerAbilityAlreadyLearned".Translate(p.Named("USER"), this.Props.ability.LabelCap);
            }
            return base.CanBeUsedBy(p);
        }
    }
}


