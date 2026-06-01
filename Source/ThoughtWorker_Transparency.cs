using RimWorld;
using Verse;

namespace CrystalTech
{
    public class ThoughtWorker_Transparency : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (Main.Settings != null && !Main.Settings.enableMoodEffect) return ThoughtState.Inactive;
            if (p == null || !p.Spawned) return ThoughtState.Inactive;

            Room room = p.GetRoom();
            if (room == null || !room.ProperRoom) return ThoughtState.Inactive;

            float transparentRatio = CrystalTechCore.GetTransparentWallRatio(room);
            
            if (transparentRatio >= 0.8f)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (transparentRatio >= 0.5f)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (transparentRatio > 0f)
            {
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}
