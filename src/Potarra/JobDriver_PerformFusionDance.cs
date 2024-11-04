using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Potarra
{
    public class JobDriver_PerformFusionDance : JobDriver
    {
        private const int FusionPreparationTicks = 120;
        private const int WordsDurationTicks = 60;

        public Pawn TargetPawn => job.targetA.Thing as Pawn;

        private bool IsPawnOnLeft => pawn.Position.x < TargetPawn.Position.x;
        private Pawn LeftPawn => IsPawnOnLeft ? pawn : TargetPawn;
        private Pawn RightPawn => IsPawnOnLeft ? TargetPawn : pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var stopPartner = new Toil
            {
                initAction = () =>
                {
                    TargetPawn.pather.StopDead();
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 1
            };
            yield return stopPartner;


            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedOrNull(TargetIndex.A);

            var waitForPartner = new Toil
            {
                initAction = () =>
                {
                    pawn.pather.StopDead();
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = FusionPreparationTicks
            };
            yield return waitForPartner;

            var sayFuu = new Toil
            {
                initAction = () =>
                {
                    MoteMaker.ThrowText(LeftPawn.DrawPos, LeftPawn.Map, "FUU");
                    MoteMaker.ThrowText(RightPawn.DrawPos, RightPawn.Map, "SION");
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = WordsDurationTicks
            };
            yield return sayFuu;

            var sayHa = new Toil
            {
                initAction = () =>
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "HA!");
                    MoteMaker.ThrowText(TargetPawn.DrawPos, TargetPawn.Map, "HA!");
                },
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = WordsDurationTicks
            };
            yield return sayHa;

            yield return Toils_Combat.CastVerb(TargetIndex.A);
        }
    }
}
