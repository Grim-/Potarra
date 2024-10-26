using RimWorld;
using System.Collections.Generic;
using TaranMagicFramework;
using Verse;
using Ability = RimWorld.Ability;

namespace Potarra
{
    public class CompProperties_GrantAbilityOnEquip : CompProperties
    {
        public RimWorld.AbilityDef AbilityToGrant;
        public Dictionary<SkillDef, int> RequiredSkills = new Dictionary<SkillDef, int>();
        public Dictionary<PawnCapacityDef, float> RequiredCapacities = new Dictionary<PawnCapacityDef, float>();
        public Dictionary<StatDef, float> RequiredStats = new Dictionary<StatDef, float>();
        public Dictionary<HediffDef, float> RequiredHediffs = new Dictionary<HediffDef, float>();
        public CompProperties_GrantAbilityOnEquip()
        {
            compClass = typeof(CompGrantAbilityOnEquip);
        }
    }

    public class CompGrantAbilityOnEquip : ThingComp
    {
        new CompProperties_GrantAbilityOnEquip Props => (CompProperties_GrantAbilityOnEquip)props;
        private bool DidGrant = false;
        private int? StoredCooldown = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            //Log.Message($"PostSpawnSetup called, respawningAfterLoad: {respawningAfterLoad}");

            TryReGrant();
        }

        public void TryReGrant()
        {
            if (parent is Apparel apparel && apparel.Wearer != null)
            {
                // Check if pawn doesn't have the ability and meets requirements
                if (!apparel.Wearer.HasAbility(Props.AbilityToGrant) && MeetsRequirements(apparel.Wearer))
                {
                    // Grant the ability
                    apparel.Wearer.abilities.GainAbility(Props.AbilityToGrant);
                    Ability grantedAbility = apparel.Wearer.abilities.GetAbility(Props.AbilityToGrant);
                    DidGrant = true;

                    // Restore cooldown if we had one stored
                    if (StoredCooldown.HasValue)
                    {
                        grantedAbility.StartCooldown(StoredCooldown.Value);
                    }
                }
            }

        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            //Log.Message($"Notify_Equipped called pawn faction {pawn.Faction} pawn has ability {pawn.HasAbility(Props.AbilityToGrant)} pawn meets requirements {MeetsRequirements(pawn)}");
            if (pawn.Faction == Faction.OfPlayer && !pawn.HasAbility(Props.AbilityToGrant) && MeetsRequirements(pawn))
            {
                pawn.abilities.GainAbility(Props.AbilityToGrant);
                Ability grantedAbility = pawn.abilities.GetAbility(Props.AbilityToGrant);
                DidGrant = true;
                if (StoredCooldown.HasValue)
                {
                    grantedAbility.StartCooldown(StoredCooldown.Value);
                }
            }
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            //Log.Message($"Notify_Unequipped called");

            if (DidGrant)
            {
                Ability abilityToRemove = pawn.abilities.GetAbility(Props.AbilityToGrant);
                if (abilityToRemove != null)
                {
                    StoredCooldown = abilityToRemove.CooldownTicksRemaining;
                    pawn.abilities.RemoveAbility(Props.AbilityToGrant);
                }
                DidGrant = false;
            }
        }

        private bool MeetsRequirements(Pawn pawn)
        {
            if (Props.RequiredSkills.Count > 0)
            {
                foreach (var skillReq in Props.RequiredSkills)
                {
                    if (pawn.skills.GetSkill(skillReq.Key).Level < skillReq.Value)
                    {
                        return false;
                    }
                }
            }

            if (Props.RequiredCapacities.Count > 0)
            {
                foreach (var capacityReq in Props.RequiredCapacities)
                {
                    if (pawn.health.capacities.GetLevel(capacityReq.Key) < capacityReq.Value)
                    {
                        return false;
                    }
                }
            }

            if (Props.RequiredStats.Count > 0)
            {
                foreach (var statReq in Props.RequiredStats)
                {
                    if (pawn.GetStatValue(statReq.Key) < statReq.Value)
                    {
                        return false;
                    }
                }
            }


            if (Props.RequiredHediffs.Count > 0)
            {
                foreach (var hediffreq in Props.RequiredHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediffreq.Key) || pawn.health.hediffSet.GetFirstHediffOfDef(hediffreq.Key).Severity < hediffreq.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref DidGrant, "DidGrant");
            Scribe_Values.Look(ref StoredCooldown, "StoredCooldown");
        }
    }
}
