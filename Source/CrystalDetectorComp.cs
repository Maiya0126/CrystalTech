using RimWorld;
using Verse;
using UnityEngine;

namespace CrystalTech
{
    public class CompProperties_CrystalDetector : CompProperties
    {
        public float detectionRadius = 10f;

        public CompProperties_CrystalDetector()
        {
            compClass = typeof(CrystalDetectorComp);
        }
    }

    public class CrystalDetectorComp : ThingComp
    {
        private CompProperties_CrystalDetector Props => (CompProperties_CrystalDetector)props;

        private bool triggered;
        private int lastAlertTick;

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned) return;
            if (Main.Settings != null && !Main.Settings.enableAntiStealth) return;
            if (!parent.IsHashIntervalTick(300)) return;

            bool found = false;
            var pawns = parent.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].Position.InHorDistOf(parent.Position, Props.detectionRadius)
                    && pawns[i].IsPsychologicallyInvisible())
                {
                    found = true;
                    break;
                }
            }

            if (found != triggered)
            {
                triggered = found;
                if (triggered && Find.TickManager.TicksGame > lastAlertTick + 1200)
                {
                    lastAlertTick = Find.TickManager.TicksGame;
                    if (ModLister.CheckAnomaly("Crystal detector"))
                    {
                        Messages.Message("CrystalTech_DetectorTriggered".Translate(), parent, MessageTypeDefOf.ThreatSmall, false);
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, "triggered", false);
            Scribe_Values.Look(ref lastAlertTick, "lastAlertTick", 0);
        }

        public override string CompInspectStringExtra()
        {
            if (triggered)
            {
                return "CrystalTech_CloakedDetected".Translate().Colorize(ColorLibrary.RedReadable);
            }
            return null;
        }
    }
}
