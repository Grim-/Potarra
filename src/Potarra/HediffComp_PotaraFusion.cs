using RimWorld;
using Verse;

namespace Potarra
{
    public class HediffCompProperties_PotaraFusion : HediffCompProperties
    {
        public int durationTicks = 60000; // Default duration, can be overridden in XML

        public HediffCompProperties_PotaraFusion()
        {
            compClass = typeof(HediffComp_PotaraFusion);
        }
    }

    public class HediffComp_PotaraFusion : HediffComp
    {
        private Pawn originalPawn1;
        private Pawn originalPawn2;
        private int ticksRemaining;

        public HediffCompProperties_PotaraFusion Props => (HediffCompProperties_PotaraFusion)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ticksRemaining = Props.durationTicks;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref originalPawn1, "originalPawn1");
            Scribe_References.Look(ref originalPawn2, "originalPawn2");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
        }

        public void SetOriginalPawns(Pawn pawn1, Pawn pawn2)
        {
            originalPawn1 = pawn1;
            originalPawn2 = pawn2;
            ticksRemaining = Props.durationTicks;

            if (!parent.pawn.HasAbility(PotarraDefOf.DBZ_EndPotaraFusion))
            {
                parent.pawn.abilities.GainAbility(PotarraDefOf.DBZ_EndPotaraFusion);
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            EndFusion(true);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                EndFusion();
            }
        }

        public void EndFusion(bool FusionPawnDied = false)
        {
            if (originalPawn1 != null && originalPawn2 != null)
            {
                GenSpawn.Spawn(originalPawn1, Pawn.Position, Pawn.Map);
                GenSpawn.Spawn(originalPawn2, Pawn.Position, Pawn.Map);


                originalPawn1.health.AddHediff(PotarraDefOf.DBZ_PotaraFusionFatigue);
                originalPawn2.health.AddHediff(PotarraDefOf.DBZ_PotaraFusionFatigue);

                foreach (Apparel apparel in originalPawn1.apparel.WornApparel)
                {
                    var grantComp = apparel.GetComp<CompGrantAbilityOnEquip>();
                    if (grantComp != null)
                    {
                        grantComp.Notify_Equipped(originalPawn1);
                    }
                }

                foreach (Apparel apparel in originalPawn2.apparel.WornApparel)
                {
                    var grantComp = apparel.GetComp<CompGrantAbilityOnEquip>();
                    if (grantComp != null)
                    {
                        grantComp.Notify_Equipped(originalPawn2);
                    }
                }

                if (FusionPawnDied)
                {
                    HealthUtility.DamageUntilDowned(originalPawn1);
                    HealthUtility.DamageUntilDowned(originalPawn2);
                    Pawn.health.Reset();
                }             
                // Remove fused pawn
                Pawn.Destroy();

            }
        }
    }
}
