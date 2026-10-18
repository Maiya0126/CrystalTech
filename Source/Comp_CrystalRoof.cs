using System;
using Verse;
using RimWorld;

namespace CrystalTech
{
    public class CompProperties_CrystalRoof : CompProperties
    {
        public float lightTransmission = 0.85f;

        public CompProperties_CrystalRoof()
        {
            compClass = typeof(Comp_CrystalRoof);
        }
    }

    public class Comp_CrystalRoof : ThingComp
    {
        private static RoofDef cachedRoofDef;

        public static RoofDef CrystalRoofDef
        {
            get
            {
                if (cachedRoofDef == null)
                {
                    cachedRoofDef = DefDatabase<RoofDef>.GetNamed("CrystalRoofConstructed");
                }
                return cachedRoofDef;
            }
        }

        private bool roofPlacedByThis;
        private RoofDef prevRoofDef;

        // True when the cell's roof is a crystal roof (light-transmitting)
        public static bool IsUnderCrystalRoof(IntVec3 c, Map map)
        {
            if (map == null || !c.InBounds(map)) return false;
            return map.roofGrid.RoofAt(c) == CrystalRoofDef;
        }

        public float LightTransmission
        {
            get
            {
                CompProperties_CrystalRoof props = parent.def.GetCompProperties<CompProperties_CrystalRoof>();
                return props != null ? props.lightTransmission : 0.85f;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TryPlaceRoof(parent.MapHeld);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            TryRemoveRoof(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            TryRemoveRoof(previousMap);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref roofPlacedByThis, "roofPlacedByThis", false);
            Scribe_Defs.Look(ref prevRoofDef, "prevRoofDef");
        }

        private void TryPlaceRoof(Map map)
        {
            if (map == null) return;
            IntVec3 c = parent.Position;
            if (!c.InBounds(map)) return;
            RoofDef cur = map.roofGrid.RoofAt(c);
            if (cur == CrystalRoofDef) return;
            // Only take over cells with no roof or plain player-built roofs; never replace mountain rock
            if (cur != null && cur != RoofDefOf.RoofConstructed) return;
            prevRoofDef = cur;
            map.roofGrid.SetRoof(c, CrystalRoofDef);
            roofPlacedByThis = true;
        }

        private void TryRemoveRoof(Map map)
        {
            if (map == null) return;
            IntVec3 c = parent.Position;
            if (c.InBounds(map) && map.roofGrid.RoofAt(c) == CrystalRoofDef)
            {
                if (prevRoofDef != null)
                {
                    map.roofGrid.SetRoof(c, prevRoofDef);
                }
                else
                {
                    map.roofGrid.RemoveRoofUnsafe(map.cellIndices.CellToIndex(c));
                }
                map.regionGrid.GetValidRegionAt_NoRebuild(c)?.District.Notify_RoofChanged();
                map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Roofs);
                map.glowGrid.DirtyCell(c);
                map.events.Notify_RoofChanged(c);
            }
            roofPlacedByThis = false;
            prevRoofDef = null;
        }
    }
}
