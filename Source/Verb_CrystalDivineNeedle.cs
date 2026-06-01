using RimWorld;
using Verse;

namespace CrystalTech
{
    public class Verb_CrystalDivineNeedle : Verb_MeleeAttack
    {
        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null || pawn.Dead)
            {
                return new DamageWorker.DamageResult();
            }

            bool isAlly = !CasterPawn.HostileTo(pawn);
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, 1f, 0f, -1f, caster, null, EquipmentSource?.def, DamageInfo.SourceCategory.ThingOrUnknown, null, !isAlly);
            dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            DamageWorker.DamageResult result = pawn.TakeDamage(dinfo);

            pawn.health.AddHediff(HediffDefOf.Anesthetic);

            if (isAlly)
            {
                if (!pawn.health.hediffSet.HasHediff(CrystalTechDefOf.CrystalRegenerationHediff))
                {
                    pawn.health.AddHediff(CrystalTechDefOf.CrystalRegenerationHediff);
                }
            }
            else
            {
                Hediff bloodLoss = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                bloodLoss.Severity = 0.3f;
                pawn.health.AddHediff(bloodLoss);
            }

            return result;
        }

        public override bool IsUsableOn(Thing target)
        {
            return target is Pawn;
        }
    }
}
