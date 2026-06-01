using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CrystalTech
{
    public class CrystalMilestones : GameComponent
    {
        private int totalCrystalBuildingsBuilt = 0;
        private bool milestone25;
        private bool milestone50;
        private bool milestone100;

    public CrystalMilestones() { }
    public CrystalMilestones(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalCrystalBuildingsBuilt, "totalCrystalBuildingsBuilt", 0);
            Scribe_Values.Look(ref milestone25, "milestone25", false);
            Scribe_Values.Look(ref milestone50, "milestone50", false);
            Scribe_Values.Look(ref milestone100, "milestone100", false);
        }

        public void Notify_CrystalBuildingBuilt()
        {
            totalCrystalBuildingsBuilt++;
            CheckMilestones();
        }

        private void CheckMilestones()
        {
            if (!milestone25 && totalCrystalBuildingsBuilt >= 25)
            {
                milestone25 = true;
                ApplyMilestoneThought("CrystalMilestone25");
            }
            if (!milestone50 && totalCrystalBuildingsBuilt >= 50)
            {
                milestone50 = true;
                ApplyMilestoneThought("CrystalMilestone50");
            }
            if (!milestone100 && totalCrystalBuildingsBuilt >= 100)
            {
                milestone100 = true;
                ApplyMilestoneThought("CrystalMilestone100");
            }
        }

        private void ApplyMilestoneThought(string thoughtDefName)
        {
            var def = DefDatabase<ThoughtDef>.GetNamedSilentFail(thoughtDefName);
            if (def == null) return;
            foreach (var pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisoners)
            {
                if (pawn?.needs?.mood?.thoughts?.memories != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(def);
                }
            }
            Messages.Message("CrystalTech_MilestoneMessage".Translate(totalCrystalBuildingsBuilt), MessageTypeDefOf.PositiveEvent);
        }
    }
}