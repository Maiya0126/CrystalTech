using RimWorld;
using UnityEngine;
using Verse;

namespace CrystalTech
{
    /// <summary>
    /// 透晶血崩：持续出血。每 2500 ticks（1 游戏小时）给 BloodLoss +0.04，
    /// 封顶 0.55（severe 级，意识-0.4 但不致死）。
    /// </summary>
    public class Hediff_CrystalHemorrhage : HediffWithComps
    {
        private const int IntervalTicks = 2500;
        private const float BloodLossPerInterval = 0.04f;
        private const float BloodLossCap = 0.55f;

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            if (pawn == null || pawn.Dead || !pawn.IsHashIntervalTick(IntervalTicks, delta))
            {
                return;
            }

            Hediff bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss == null)
            {
                bloodLoss = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                bloodLoss.Severity = 0f;
                pawn.health.AddHediff(bloodLoss);
            }

            if (bloodLoss.Severity < BloodLossCap)
            {
                bloodLoss.Severity = Mathf.Min(bloodLoss.Severity + BloodLossPerInterval, BloodLossCap);
                pawn.health.Notify_HediffChanged(bloodLoss);
            }
        }
    }
}
