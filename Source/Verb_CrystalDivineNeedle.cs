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

            // 麻痹：清醒但不能动（敌我通用）
            pawn.health.AddHediff(CrystalTechDefOf.CrystalNeedleParalysis);

            if (isAlly)
            {
                // 针灸：倒计时结束痊愈（已有则不重复施加 = 天然冷却）
                if (!pawn.health.hediffSet.HasHediff(CrystalTechDefOf.CrystalAcupuncture))
                {
                    pawn.health.AddHediff(CrystalTechDefOf.CrystalAcupuncture);
                }
            }
            else
            {
                // 血崩：持续出血，封顶不致死
                if (!pawn.health.hediffSet.HasHediff(CrystalTechDefOf.CrystalHemorrhage))
                {
                    pawn.health.AddHediff(CrystalTechDefOf.CrystalHemorrhage);
                }
            }

            return result;
        }

        public override bool IsUsableOn(Thing target)
        {
            return target is Pawn;
        }
    }
}
