using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CrystalTech
{
    // ============ Job Driver: skygazing under crystal roof ============
    public class JobDriver_CrystalSkygaze : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
            };
            toil.tickIntervalAction = delegate (int delta)
            {
                Map map = pawn.Map;
                float extraJoyGainFactor = Traverse.Create(map.gameConditionManager).Method("AggregateSkyGazeJoyGainFactor", map).GetValue<float>();
                JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = job.def.joyDuration;
            toil.FailOn(() => pawn.Map.weatherManager.curWeather.preventSkygaze);
            toil.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
            // crystal roof removed -> stop
            toil.FailOn(() => !Comp_CrystalRoof.IsUnderCrystalRoof(pawn.Position, pawn.Map));
            yield return toil;
        }

        public override string GetReport()
        {
            if (base.Map != null && base.Map.TileInfo.Layer.Def.isSpace)
            {
                return "Stargazing".Translate();
            }
            if (base.Map != null && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
            {
                return "WatchingEclipse".Translate();
            }
            if (base.Map != null && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Aurora))
            {
                return "WatchingAurora".Translate();
            }
            float num = GenCelestial.CurCelestialSunGlow(base.Map);
            if (num < 0.1f)
            {
                return "Stargazing".Translate();
            }
            if (num < 0.65f)
            {
                if (GenLocalDate.DayPercent(pawn) < 0.5f)
                {
                    return "WatchingSunrise".Translate();
                }
                return "WatchingSunset".Translate();
            }
            return "CloudWatching".Translate();
        }
    }

    // ============ Joy Giver: sends pawns to skygaze under crystal roof panels ============
    public class JoyGiver_CrystalSkygaze : JoyGiver
    {
        private static ThingDef cachedPanelDef;

        private static ThingDef PanelDef
        {
            get
            {
                if (cachedPanelDef == null)
                {
                    cachedPanelDef = DefDatabase<ThingDef>.GetNamed("CrystalRoofPanel");
                }
                return cachedPanelDef;
            }
        }

        public override float GetChance(Pawn pawn)
        {
            Map map = pawn.Map;
            if (map == null) return 0f;
            List<Thing> panels = map.listerThings.ThingsOfDef(PanelDef);
            if (panels.Count == 0) return 0f;
            float num = Traverse.Create(map.gameConditionManager).Method("AggregateSkyGazeChanceFactor", map).GetValue<float>();
            return base.GetChance(pawn) * num;
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            Map map = pawn.Map;
            if (map == null) return null;
            if (!JoyUtility.EnjoyableOutsideNow(pawn)
                || map.weatherManager.curWeather.rainRate > 0.1f
                || map.weatherManager.curWeather.preventSkygaze
                || (ModsConfig.AnomalyActive && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.UnnaturalDarkness)))
            {
                return null;
            }

            List<Thing> panels = map.listerThings.ThingsOfDef(PanelDef);
            if (panels.Count == 0) return null;

            List<IntVec3> candidates = new List<IntVec3>();
            for (int i = 0; i < panels.Count; i++)
            {
                IntVec3 c = panels[i].Position;
                if (!Comp_CrystalRoof.IsUnderCrystalRoof(c, map)) continue;
                if (c.IsForbidden(pawn)) continue;
                if (!c.Standable(map)) continue;
                if (c.Fogged(map)) continue;
                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.None)) continue;
                candidates.Add(c);
            }
            if (candidates.Count == 0) return null;

            IntVec3 dest = candidates.RandomElement();
            return JobMaker.MakeJob(def.jobDef, dest);
        }
    }

    // ============ Thought: enjoying sky view under crystal dome ============
    public class ThoughtWorker_CrystalSkygaze : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.jobs?.curJob == null) return false;
            if (p.jobs.curJob.def.defName != "CrystalSkygaze") return false;
            return ThoughtState.ActiveDefault;
        }
    }

    // ============ Aurora thought also counts under crystal roof ============
    [HarmonyPatch(typeof(ThoughtWorker_Aurora), "CurrentStateInternal")]
    public static class ThoughtWorker_Aurora_CrystalRoofPatch
    {
        static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (__result.Active || p?.MapHeld == null) return;
            if (Comp_CrystalRoof.IsUnderCrystalRoof(p.PositionHeld, p.MapHeld))
            {
                __result = ThoughtState.ActiveDefault;
            }
        }
    }

    // ============ UnroofedOnly joy buildings (e.g. Telescope) usable under crystal roof ============
    [HarmonyPatch(typeof(JoyGiver_InteractBuilding), "CanInteractWith")]
    public static class JoyGiver_InteractBuilding_CrystalRoofPatch
    {
        static void Prefix(JoyGiver_InteractBuilding __instance, Thing t, ref bool __state)
        {
            __state = false;
            if (__instance.def == null || !__instance.def.unroofedOnly) return;
            if (t?.Map == null) return;
            if (t.Position.Roofed(t.Map) && Comp_CrystalRoof.IsUnderCrystalRoof(t.Position, t.Map))
            {
                // temporarily lift unroofedOnly so the vanilla check passes
                __instance.def.unroofedOnly = false;
                __state = true;
            }
        }

        static void Finalizer(Exception __exception, bool __state, JoyGiver_InteractBuilding __instance)
        {
            if (__state && __instance?.def != null)
            {
                __instance.def.unroofedOnly = true;
            }
        }
    }

    // ============ Allow placing unroofed-only buildings under crystal roof ============
    [HarmonyPatch(typeof(PlaceWorker_NotUnderRoof), "AllowsPlacing")]
    public static class PlaceWorker_NotUnderRoof_CrystalRoofPatch
    {
        static void Postfix(IntVec3 loc, Map map, ref AcceptanceReport __result)
        {
            if (__result.Accepted || map == null) return;
            // only crystal roof counts as "sky-visible" for placement
            if (Comp_CrystalRoof.IsUnderCrystalRoof(loc, map))
            {
                __result = true;
            }
        }
    }
}
