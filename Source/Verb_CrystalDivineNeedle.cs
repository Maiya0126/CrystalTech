using RimWorld;
using Verse;
using Verse.AI;

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
            DamageWorker.DamageResult result;

            if (isAlly)
            {
                // 友军：零伤害治疗性针刺，不产生瘀伤
                result = new DamageWorker.DamageResult();
                // 一针即止：限制当前近战 job 只挥击一次
                Job job = CasterPawn.CurJob;
                if (job != null && job.def == JobDefOf.AttackMelee && job.maxNumMeleeAttacks > 1)
                {
                    job.maxNumMeleeAttacks = 1;
                }
            }
            else
            {
                // 敌军：保留轻微冲击伤害与标准近战循环
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, 1f, 0f, -1f, caster, null, EquipmentSource?.def, DamageInfo.SourceCategory.ThingOrUnknown, null, true);
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                result = pawn.TakeDamage(dinfo);
            }

            // 麻痹：清醒但不能动（敌我通用，重复命中刷新倒计时不叠实例）
            Hediff paralysis = pawn.health.hediffSet.GetFirstHediffOfDef(CrystalTechDefOf.CrystalNeedleParalysis);
            if (paralysis != null)
            {
                paralysis.TryGetComp<HediffComp_Disappears>()?.ResetElapsedTicks();
            }
            else
            {
                pawn.health.AddHediff(CrystalTechDefOf.CrystalNeedleParalysis);
            }

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
                // 血崩：持续出血，封顶不致死（已有则不叠加）
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
