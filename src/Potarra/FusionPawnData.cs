using Verse;

namespace Potarra
{
    public class FusionPawnData : IExposable
    {
        public Pawn originalPawn1;
        public Pawn originalPawn2;
        public Pawn fusedPawn;
        public Pawn failedFusePawn;

        public FusionPawnData() { }

        public FusionPawnData(Pawn pawn1, Pawn pawn2, Pawn fused, bool isFailure = false)
        {
            originalPawn1 = pawn1;
            originalPawn2 = pawn2;
            if (isFailure)
            {
                failedFusePawn = fused;
            }
            else
            {
                fusedPawn = fused;
            }
        }


        public bool HasFusionForm(bool IsFailure)
        {
            if (IsFailure)
            {
                return failedFusePawn != null;
            }

            return fusedPawn != null;
        }

        public void SetFusionForm(Pawn FusionPawn)
        {
            fusedPawn = FusionPawn;
        }

        public void SetFailureForm(Pawn FailurePawn)
        {
            failedFusePawn = FailurePawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref originalPawn1, "originalPawn1", true);
            Scribe_References.Look(ref originalPawn2, "originalPawn2", true);
            Scribe_References.Look(ref fusedPawn, "fusedPawn");
            Scribe_References.Look(ref failedFusePawn, "failedFusePawn", true);
        }

        public bool MatchesPawns(Pawn pawn1, Pawn pawn2)
        {
            return (originalPawn1 == pawn1 && originalPawn2 == pawn2) ||
                   (originalPawn1 == pawn2 && originalPawn2 == pawn1);
        }
    }
}
