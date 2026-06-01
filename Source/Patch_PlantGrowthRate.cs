using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace CrystalTech
{
    [HarmonyPatch(typeof(Plant), "GrowthRate", MethodType.Getter)]
    public static class Plant_GrowthRate_TransparentBonus
    {
        private const float TransparentRoomGrowthBonus = 0.1f;

        static void Postfix(Plant __instance, ref float __result)
        {
            if (__instance == null || !__instance.Spawned) return;

            Room room = __instance.GetRoom();
            if (room == null || !room.ProperRoom) return;

            float transparentRatio = CrystalTechCore.GetTransparentWallRatio(room);
            if (transparentRatio > 0f)
            {
                float bonus = TransparentRoomGrowthBonus * transparentRatio;
                __result *= (1f + bonus);
            }
        }
    }
}
