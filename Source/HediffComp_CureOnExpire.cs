using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CrystalTech
{
    public class HediffCompProperties_CureOnExpire : HediffCompProperties
    {
        public int cureDelayTicks = 60000;

        public HediffCompProperties_CureOnExpire()
        {
            compClass = typeof(HediffComp_CureOnExpire);
        }
    }

    /// <summary>
    /// 透晶针灸：倒计时结束后按生物雕塑规则结算痊愈。
    /// 排除：慢性病、义体/植入物、永久疤痕、缺失肢体。
    /// 治疗：疾病（含可致命感染）、失血、非永久伤口。
    /// </summary>
    public class HediffComp_CureOnExpire : HediffComp
    {
        private int ticksToCure;

        public HediffCompProperties_CureOnExpire Props => (HediffCompProperties_CureOnExpire)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ticksToCure = Props.cureDelayTicks;
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (ticksToCure < 2500)
                {
                    return ticksToCure.ToStringSecondsFromTicks("F0");
                }
                return ticksToCure.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true);
            }
        }

        public override bool CompShouldRemove => ticksToCure <= 0;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            ticksToCure -= delta;
            if (ticksToCure <= 0)
            {
                ticksToCure = 0;
                DoCure();
            }
        }

        private void DoCure()
        {
            Pawn pawn = base.Pawn;
            if (pawn == null || pawn.Dead || pawn.health == null)
            {
                return;
            }

            List<Hediff> hediffs = new List<Hediff>(pawn.health.hediffSet.hediffs);
            int curedCount = 0;
            foreach (Hediff hediff in hediffs)
            {
                if (hediff == parent || !WillCure(hediff))
                {
                    continue;
                }
                HealthUtility.Cure(hediff);
                curedCount++;
            }

            if (curedCount > 0 && PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message("CrystalTech_AcupunctureCured".Translate(pawn.Named("PAWN"), curedCount.Named("COUNT")), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        /// <summary>生物雕塑 WillHeal 同款规则</summary>
        private static bool WillCure(Hediff hediff)
        {
            if (!hediff.def.everCurableByItem)
            {
                return false;
            }
            if (hediff.def.chronic)
            {
                return false;
            }
            if (hediff.def.countsAsAddedPartOrImplant)
            {
                return false;
            }
            if (hediff is Hediff_MissingPart)
            {
                return false;
            }
            if (hediff.def == HediffDefOf.BloodLoss)
            {
                return true;
            }
            if (hediff is Hediff_Injury && !hediff.IsPermanent())
            {
                return true;
            }
            // 疾病类（带免疫组件的可治愈病症）
            if (hediff.TryGetComp<HediffComp_Immunizable>() != null)
            {
                return true;
            }
            return false;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksToCure, "ticksToCure", 0);
        }
    }
}
