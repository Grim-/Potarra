//using RimWorld;
//using System.Collections.Generic;
//using Verse;

//namespace Potarra
//{
//    public class GeneDef_PotaraFusion : GeneDef
//    {
//        // Additional properties specific to fusion if needed
//    }

//    public class Gene_PotaraFusion : Gene
//    {

//    }

//    public class Hediff_PotaraFusion : HediffWithComps
//    {
//        private List<StatModifier> combinedStatModifiers = new List<StatModifier>();
//        public List<GeneDef> originalGenes = new List<GeneDef>();


//        override s
//        public override void PostAdd(DamageInfo? dinfo)
//        {
//            base.PostAdd(dinfo);
//            // In case we need any initialization
//        }

//        public override IEnumerable<StatModifier> GetStatOffsets()
//        {
//            foreach (var mod in combinedStatModifiers)
//            {
//                yield return mod;
//            }
//        }

//        private void ProcessGeneStats(Gene gene, Dictionary<StatDef, float> combinedStats)
//        {
//            if (gene.def.statFactors == null) return;

//            foreach (StatModifier mod in gene.def.statFactors)
//            {
//                if (combinedStats.ContainsKey(mod.stat))
//                {
//                    combinedStats[mod.stat] *= mod.value;
//                }
//                else
//                {
//                    combinedStats[mod.stat] = mod.value;
//                }
//            }
//        }

//        private void RebuildStatModifiers(Dictionary<StatDef, float> combinedStats)
//        {
//            combinedStatModifiers.Clear();
//            foreach (var stat in combinedStats)
//            {
//                combinedStatModifiers.Add(new StatModifier
//                {
//                    stat = stat.Key,
//                    value = stat.Value
//                });
//            }
//        }

//        private void RecreateStatsFromSavedGenes()
//        {
//            Dictionary<StatDef, float> combinedStats = new Dictionary<StatDef, float>();

//            foreach (GeneDef geneDef in originalGenes)
//            {
//                if (geneDef.statFactors == null) continue;

//                foreach (StatModifier mod in geneDef.statFactors)
//                {
//                    if (combinedStats.ContainsKey(mod.stat))
//                    {
//                        combinedStats[mod.stat] *= mod.value;
//                    }
//                    else
//                    {
//                        combinedStats[mod.stat] = mod.value;
//                    }
//                }
//            }

//            RebuildStatModifiers(combinedStats);
//        }

//        public void CombineGenesFrom(Pawn pawn1, Pawn pawn2)
//        {
//            originalGenes.Clear();

//            foreach (Gene gene in pawn1.genes.GenesListForReading)
//            {
//                if (gene.def.statFactors?.Count > 0)
//                {
//                    originalGenes.Add(gene.def);
//                }
//            }
//            foreach (Gene gene in pawn2.genes.GenesListForReading)
//            {
//                if (gene.def.statFactors?.Count > 0)
//                {
//                    originalGenes.Add(gene.def);
//                }
//            }

//            Dictionary<StatDef, float> combinedStats = new Dictionary<StatDef, float>();

//            foreach (Gene gene in pawn1.genes.GenesListForReading)
//            {
//                ProcessGeneStats(gene, combinedStats);
//            }
//            foreach (Gene gene in pawn2.genes.GenesListForReading)
//            {
//                ProcessGeneStats(gene, combinedStats);
//            }

//            RebuildStatModifiers(combinedStats);
//        }

//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_Collections.Look(ref originalGenes, "originalGenes", LookMode.Def);
//            Scribe_Collections.Look(ref combinedStatModifiers, "combinedStatModifiers", LookMode.Deep);

//            if (Scribe.mode == LoadSaveMode.LoadingVars)
//            {
//                RecreateStatsFromSavedGenes();
//            }
//        }
//    }
//}
