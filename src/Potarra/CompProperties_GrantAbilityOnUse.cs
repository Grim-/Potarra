using RimWorld;
using Verse;

namespace Potarra
{
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


