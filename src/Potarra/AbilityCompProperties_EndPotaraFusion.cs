using RimWorld;
using Verse;

namespace Potarra
{
    public class AbilityCompProperties_EndPotaraFusion : CompProperties_AbilityEffect
    {
        public AbilityCompProperties_EndPotaraFusion()
        {
            compClass = typeof(AbilityComp_EndPotaraFusion);
        }
    }

    public class AbilityComp_EndPotaraFusion : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(PotarraDefOf.DBZ_PotaraFusion);

            if (hediff != null && hediff.TryGetComp(out HediffComp_PotaraFusion potaraFusion))
            {
                potaraFusion.EndFusion();
            }

        }
    }
}


