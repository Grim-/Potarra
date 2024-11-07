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

            HediffComp_PotaraFusion potaraFusion = parent.pawn.GetFusionHediff();
            if (potaraFusion != null)
            {
                potaraFusion.EndFusion();
            }

        }


    }
}


