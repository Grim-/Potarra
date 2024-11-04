using RimWorld;
using RimWorld.Planet;
using SaiyanMod;
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

        public override void CompPostPostRemoved()
        {
            EndFusion(false);
            base.CompPostPostRemoved();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            EndFusion(true);
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Props.durationTicks > 0)
            {
                if (Pawn.IsHashIntervalTick(500))
                {
                    if (Pawn.health.hediffSet.HasHediff(SR_DefOf.SR_LegendarySuperSaiyanHediff)
                        || Pawn.health.hediffSet.HasHediff(SR_DefOf.SR_SuperSaiyanIIIHediff)
                        || Pawn.health.hediffSet.HasHediff(SR_DefOf.SR_TrueSuperSaiyanHediff)
                        || Pawn.health.hediffSet.HasHediff(SR_DefOf.SR_AwakenedHediff)
                        || Pawn.HasSuperSaiyanActive())
                    {
                        ticksRemaining -= 2;
                    }
                }

                ticksRemaining--;
                if (ticksRemaining <= 0)
                {
                    EndFusion();
                }
            }
        }

        public void EndFusion(bool FusionPawnDied = false)
        {
            IntVec3 position = Pawn.Position;
            Map map = Pawn.MapHeld;

            RestoreOriginalPawn(originalPawn1, position, map);
            RestoreOriginalPawn(originalPawn2, position, map);

            if (FusionPawnDied)
            {
                DamageOriginalPawns(originalPawn1, originalPawn2);
            }


            if (Pawn.Spawned && !FusionPawnDied)
            {
                Pawn.DeSpawn();
            }

            if (!Find.WorldPawns.Contains(Pawn))
            {
                Find.WorldPawns.PassToWorld(Pawn, PawnDiscardDecideMode.KeepForever);
            }       
        }

        private void RestoreOriginalPawn(Pawn originalPawn, IntVec3 position, Map map)
        {
            if (originalPawn != null)
            {
                if (!originalPawn.Spawned)
                {
                    GenSpawn.Spawn(originalPawn, position, map);

                    if (!originalPawn.Faction.IsPlayer)
                    {
                        originalPawn.SetFaction(Faction.OfPlayer);
                    }
                }

                originalPawn.health.AddHediff(PotarraDefOf.DBZ_PotaraFusionFatigue);
                NotifyComp(originalPawn);
            }
        }

        private void DamageOriginalPawns(Pawn Pawn1, Pawn Pawn2)
        {
            if (!Pawn1.Dead && Pawn1.Spawned) 
                HealthUtility.DamageUntilDowned(originalPawn1);
            if (!Pawn2.Dead && Pawn2.Spawned) 
                HealthUtility.DamageUntilDowned(originalPawn2);

        }


        private void NotifyComp(Pawn Pawn)
        {
            foreach (Apparel apparel in Pawn.apparel.WornApparel)
            {
                var grantComp = apparel.GetComp<CompGrantAbilityOnEquip>();
                if (grantComp != null)
                {
                    grantComp.Notify_Equipped(Pawn);
                }
            }

        }
    }
}
