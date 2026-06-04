using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CrystalTech
{
    [StaticConstructorOnStartup]
    public static class CrystalTechCore
    {
        public static readonly List<string> BasicTransparentDefNames = new List<string>
        {
            "TransparentWall", "TransparentDoor", "TransparentFence",
            "TransparentFenceGate", "TransparentVent",
            "TransparentTableRect", "TransparentTableSquare",
            "TransparentDiningChair", "TransparentArmchair",
            "TransparentBed", "TransparentDoubleBed",
            "TransparentBarricade", "TransparentShelf", "TransparentSmallShelf",
            "CrystalFloorWindow", "TransparentBridge"
        };

        public static readonly List<string> AdvancedTransparentDefNames = new List<string>
        {
            "AdvancedTransparentWall", "AdvancedTransparentDoor", "AdvancedTransparentAutodoor",
            "AdvancedTransparentFence", "AdvancedTransparentFenceGate", "TransparentCeilingLamp"
        };

        public static readonly List<string> ReinforcedTransparentDefNames = new List<string>
        {
            "ReinforcedTransparentWall", "ReinforcedTransparentDoor",
            "ReinforcedTransparentFence", "ReinforcedTransparentFenceGate"
        };

        public static readonly List<string> TransparentWallDefNames = new List<string>();

        public static readonly List<string> CrystalApparelDefNames = new List<string>
        {
            "Apparel_CrystalContactLens", "Apparel_CrystalGoggles", "Apparel_CrystalMagnifier",
            "Apparel_CrystalAmulet", "Apparel_CrystalTelescope", "Apparel_CrystalShield"
        };

        public static readonly List<string> CrystalWeaponDefNames = new List<string>
        {
            "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle"
        };

        public static readonly List<string> CrystalItemDefNames = new List<string>
        {
            "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle",
            "Apparel_CrystalContactLens", "Apparel_CrystalGoggles", "Apparel_CrystalMagnifier",
            "Apparel_CrystalAmulet", "Apparel_CrystalTelescope", "Apparel_CrystalShield", "CrystalMaterial"
        };

        static CrystalTechCore()
        {
            TransparentWallDefNames.AddRange(BasicTransparentDefNames);
            TransparentWallDefNames.AddRange(AdvancedTransparentDefNames);
            TransparentWallDefNames.AddRange(ReinforcedTransparentDefNames);

            Log.Message("[Crystal Tech] Initialized");
        }

        public static readonly List<string> CrystalFurnitureDefNames = new List<string>
        {
            "TransparentTableRect", "TransparentTableSquare",
            "TransparentDiningChair", "TransparentArmchair",
            "TransparentBed", "TransparentDoubleBed"
        };

        public static bool IsTransparentWall(ThingDef def)
        {
            if (def == null) return false;
            return TransparentWallDefNames.Contains(def.defName);
        }

        public static bool IsCrystalApparel(ThingDef def)
        {
            return def != null && CrystalApparelDefNames.Contains(def.defName);
        }

        public static bool IsCrystalFurniture(ThingDef def)
        {
            return def != null && CrystalFurnitureDefNames.Contains(def.defName);
        }

        public static bool HasCrystalItemEquipped(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.equipment?.Primary != null && CrystalItemDefNames.Contains(pawn.equipment.Primary.def.defName))
                return true;
            if (pawn.apparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (CrystalItemDefNames.Contains(apparel.def.defName))
                        return true;
                }
            }
            return false;
        }

        public static HediffDef CrystalGlowHediffDef => HediffDef.Named("CrystalItemGlowHediff");

        public static void UpdateCrystalGlow(Pawn pawn)
        {
            if (pawn == null || pawn.health == null) return;
            if (Main.Settings != null && !Main.Settings.enableItemGlow) return;
            bool hasItem = HasCrystalItemEquipped(pawn);
            bool hasHediff = pawn.health.hediffSet.HasHediff(CrystalGlowHediffDef);
            if (hasItem && !hasHediff)
            {
                pawn.health.AddHediff(CrystalGlowHediffDef);
            }
            else if (!hasItem && hasHediff)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(CrystalGlowHediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }

        static float Alpha => Main.Settings?.transparencyAlpha ?? 0.25f;

        public static Color GetTintColor(ThingDef def)
        {
            if (CrystalItemDefNames.Contains(def.defName))
                return new Color(1f, 1f, 1f, 0.6f);
            float a = Alpha;
            if (BasicTransparentDefNames.Contains(def.defName))
                return new Color(1f, 1f, 1f, a);
            if (AdvancedTransparentDefNames.Contains(def.defName))
                return new Color(0.75f, 0.85f, 1f, a);
            if (ReinforcedTransparentDefNames.Contains(def.defName))
                return new Color(1f, 0.85f, 0.6f, a);
            return new Color(1f, 1f, 1f, a);
        }

        public static float GetGlowBoost(ThingDef def)
        {
            if (BasicTransparentDefNames.Contains(def.defName))
                return 0.9f;
            if (AdvancedTransparentDefNames.Contains(def.defName))
                return 0.85f;
            if (ReinforcedTransparentDefNames.Contains(def.defName))
                return 0.75f;
            return 0.9f;
        }

        public static bool RoomHasTransparentWalls(Room room)
        {
            if (room == null || !room.ProperRoom) return false;
            foreach (var district in room.Districts)
            {
                foreach (var region in district.Regions)
                {
                    foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                    {
                        if (IsTransparentWall(thing.def)) return true;
                    }
                }
            }
            return false;
        }

        public static float GetTransparentWallRatio(Room room)
        {
            if (room == null || !room.ProperRoom) return 0f;
            int transparentCount = 0;
            int totalWallCount = 0;
            foreach (var district in room.Districts)
            {
                foreach (var region in district.Regions)
                {
                    foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                    {
                        if (thing.def.IsBuildingArtificial && thing.def.building != null)
                        {
                            if (thing.def.building.isWall == true || thing.def.passability == Traversability.Impassable)
                            {
                                totalWallCount++;
                                if (IsTransparentWall(thing.def)) transparentCount++;
                            }
                        }
                    }
                }
            }
            if (totalWallCount == 0) return 0f;
            return (float)transparentCount / totalWallCount;
        }

        // For Wear patch - bypass PawnCanWear during crystal apparel wear
        public static bool ForceAllowCrystalWear = false;

        public static void FlushGraphicCache(Thing thing)
        {
            if (thing == null) return;
            try
            {
                var fi = typeof(Thing).GetField("graphicInt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(thing, null);
                var fi2 = typeof(Thing).GetField("styleGraphicInt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fi2 != null) fi2.SetValue(thing, null);
            }
            catch { }
        }

        public static void FlushAllCrystalGraphics(Map map)
        {
            if (map == null) return;
            int count = 0;
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing?.def != null && CrystalTechCore.IsTransparentWall(thing.def))
                {
                    FlushGraphicCache(thing);
                    map.mapDrawer.MapMeshDirty(thing.Position, MapMeshFlagDefOf.Things);
                    count++;
                }
            }
            Log.Message("[Crystal Tech] Flushed graphic cache for " + count + " crystal things");
        }
    }

    // ========== Transparent wall rendering ==========

    [HarmonyPatch(typeof(Thing), "DrawColor", MethodType.Getter)]
    public static class Thing_DrawColor_TransparentPatch
    {
        static void Postfix(Thing __instance, ref Color __result)
        {
            if (__instance?.def != null && CrystalTechCore.IsTransparentWall(__instance.def))
            {
                Color tint = CrystalTechCore.GetTintColor(__instance.def);
                __result = new Color(tint.r, tint.g, tint.b, tint.a);
            }
        }
    }

    [HarmonyPatch(typeof(Need_Outdoors), "NeedInterval")]
    public static class Need_Outdoors_CabinFeverPatch
    {
        static void Postfix(Need_Outdoors __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null || !pawn.Spawned) return;
            Room room = pawn.GetRoom();
            if (room == null || room.PsychologicallyOutdoors) return;
            bool inCrystalRoom = false;
            foreach (var cell in room.Cells)
            {
                var edifice = cell.GetEdifice(pawn.Map);
                if (edifice != null && CrystalTechCore.IsTransparentWall(edifice.def))
                {
                    inCrystalRoom = true;
                    break;
                }
            }
            if (!inCrystalRoom) return;
            float cur = __instance.CurLevel;
            if (cur < 0.6f)
            {
                __instance.CurLevel = Mathf.Min(cur + 0.0025f * 3f, 0.8f);
            }
        }
    }

    [HarmonyPatch(typeof(StuffProperties), "CanMake")]
    public static class StuffProperties_CanMake_BlockBuildingPatch
    {
        static bool Prefix(StuffProperties __instance, BuildableDef t, ref bool __result)
        {
            if (__instance.parent?.defName != "CrystalMaterial") return true;
            if (t == null) return true;
            if (t is ThingDef thingDef && thingDef.category == ThingCategory.Building)
            {
                if (!CrystalTechCore.IsTransparentWall(thingDef) && !CrystalTechCore.IsCrystalFurniture(thingDef))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GenSpawn), "Spawn", new[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool) })]
    public static class GenSpawn_Spawn_CrystalMilestonePatch
    {
        static void Postfix(Thing newThing)
        {
            if (newThing?.def != null && CrystalTechCore.IsTransparentWall(newThing.def))
            {
                var comp = Current.Game?.GetComponent<CrystalMilestones>();
                if (comp != null)
                {
                    comp.Notify_CrystalBuildingBuilt();
                }
            }
        }
    }

    [HarmonyPatch(typeof(GlowGrid), "GroundGlowAt")]
    public static class GlowGrid_GroundGlowAt_TransparentPatch
    {
        static void Postfix(GlowGrid __instance, IntVec3 c, ref float __result)
        {
            if (Main.Settings != null && !Main.Settings.enableLightTransmission) return;
            if (__result > 0.3f) return;
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (map == null) return;
            if (!c.Roofed(map)) return;
            float skyGlow = map.skyManager.CurSkyGlow;
            if (skyGlow < 0.1f) return;
            float bestBoost = 0f;
            foreach (IntVec3 neighbor in GenAdj.CardinalDirections)
            {
                IntVec3 adj = c + neighbor;
                if (!adj.InBounds(map)) continue;
                List<Thing> things = map.thingGrid.ThingsListAt(adj);
                for (int i = 0; i < things.Count; i++)
                {
                    if (CrystalTechCore.IsTransparentWall(things[i].def))
                    {
                        float boost = CrystalTechCore.GetGlowBoost(things[i].def);
                        if (boost > bestBoost) bestBoost = boost;
                        break;
                    }
                }
            }
            if (bestBoost > 0f)
            {
                __result = Mathf.Max(__result, skyGlow * bestBoost);
            }
        }
    }

    // ========== Alien Race Compatibility ==========
    // Same pattern as "I Can Wear This" mod:
    // 1. Custom FloatMenuOptionProvider to show "Wear" option (bypasses HAR's EquipmentUtility.CanEquip check)
    // 2. Pawn_ApparelTracker.Wear patch to set a flag during wear
    // 3. Apparel.PawnCanWear patch to return true when flag is set

    public class FloatMenuOptionProvider_CrystalWear : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return context.FirstSelectedPawn?.apparel != null;
        }

        public override bool TargetThingValid(Thing thing, FloatMenuContext context)
        {
            return base.TargetThingValid(thing, context) && thing is Apparel && CrystalTechCore.IsCrystalApparel(((Apparel)thing).def);
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Apparel apparel = clickedThing as Apparel;
            if (apparel == null)
            {
                return null;
            }
            if (!CrystalTechCore.IsCrystalApparel(apparel.def))
            {
                return null;
            }

            Pawn pawn = context.FirstSelectedPawn;
            string key2 = apparel.def.apparel.LastLayer.IsUtilityLayer ? "ForceEquipApparel" : "ForceWear";

            if (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }
            if (apparel.IsBurning())
            {
                return new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null);
            }
            if (pawn.apparel.WouldReplaceLockedApparel(apparel))
            {
                return new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), null);
            }
            if (pawn.IsMutant && pawn.mutant.Def.disableApparel)
            {
                return new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + ": " + pawn.mutant.Def.LabelCap, null);
            }
            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
            {
                return new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), null);
            }
            // Skip EquipmentUtility.CanEquip - this is where HAR blocks wearing

            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                key2.Translate(apparel.LabelShort, apparel),
                delegate
                {
                    apparel.SetForbidden(value: false);
                    Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                },
                MenuOptionPriority.High
            ), pawn, apparel);
        }
    }

    [HarmonyPatch(typeof(Apparel), "PawnCanWear", new[] { typeof(Pawn), typeof(bool) })]
    internal static class Apparel_PawnCanWear_CrystalPatch
    {
        static bool Prefix(Apparel __instance, ref bool __result, Pawn pawn, bool ignoreGender)
        {
            if (CrystalTechCore.ForceAllowCrystalWear && CrystalTechCore.IsCrystalApparel(__instance.def))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Wear", new[] { typeof(Apparel), typeof(bool), typeof(bool) })]
    internal static class Pawn_ApparelTracker_Wear_CrystalPatch
    {
        static bool Prefix(Apparel newApparel)
        {
            if (CrystalTechCore.IsCrystalApparel(newApparel.def))
            {
                CrystalTechCore.ForceAllowCrystalWear = true;
            }
            return true;
        }

        static void Postfix(Apparel newApparel)
        {
            if (CrystalTechCore.IsCrystalApparel(newApparel.def))
            {
                CrystalTechCore.ForceAllowCrystalWear = false;
            }
            CrystalTechCore.UpdateCrystalGlow(newApparel.Wearer);
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    internal static class ApparelTracker_Removed_GlowPatch
    {
        static void Postfix(Pawn_ApparelTracker __instance)
        {
            CrystalTechCore.UpdateCrystalGlow(__instance.pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    internal static class EquipTracker_Added_GlowPatch
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            CrystalTechCore.UpdateCrystalGlow(__instance.pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    internal static class EquipTracker_Removed_GlowPatch
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            CrystalTechCore.UpdateCrystalGlow(__instance.pawn);
        }
    }
}
