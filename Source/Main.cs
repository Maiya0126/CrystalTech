using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace CrystalTech
{
    public class CrystalTechSettings : ModSettings
    {
        public float transparencyAlpha = 0.25f;
        public bool enableMoodEffect = true;
        public bool enableLightTransmission = true;
        public float craftingYieldMultiplier = 1f;
        public bool enableAntiStealth = true;
        public bool enableItemGlow = true;
        public bool enableWallGlow = true;
        public bool enableFurnitureGlow = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref transparencyAlpha, "transparencyAlpha", 0.25f);
            Scribe_Values.Look(ref enableMoodEffect, "enableMoodEffect", true);
            Scribe_Values.Look(ref enableLightTransmission, "enableLightTransmission", true);
            Scribe_Values.Look(ref craftingYieldMultiplier, "craftingYieldMultiplier", 1f);
            Scribe_Values.Look(ref enableAntiStealth, "enableAntiStealth", true);
            Scribe_Values.Look(ref enableItemGlow, "enableItemGlow", true);
            Scribe_Values.Look(ref enableWallGlow, "enableWallGlow", true);
            Scribe_Values.Look(ref enableFurnitureGlow, "enableFurnitureGlow", true);
        }
    }

    public class Main : Mod
    {
        public static CrystalTechSettings Settings;

        public Main(ModContentPack content) : base(content)
        {
            Settings = GetSettings<CrystalTechSettings>();
            var harmony = new Harmony("Maiya.CrystalTech");
            harmony.PatchAll();
            Log.Message("[Crystal Tech] Initialized");
        }

        public override string SettingsCategory()
        {
            return "Crystal Tech 透晶科技";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("CrystalTech_SettingTransparency".Translate(Settings.transparencyAlpha.ToString("F2")));
            float oldAlpha = Settings.transparencyAlpha;
            Settings.transparencyAlpha = listing.Slider(Settings.transparencyAlpha, 0.05f, 0.8f);
            if (Mathf.Abs(oldAlpha - Settings.transparencyAlpha) > 0.001f)
            {
                foreach (var map in Find.Maps)
                {
                    CrystalTechCore.FlushAllCrystalGraphics(map);
                }
            }

            listing.Gap(12f);
            bool oldMood = Settings.enableMoodEffect;
            bool oldLight = Settings.enableLightTransmission;
            bool oldWallGlow = Settings.enableWallGlow;
            bool oldFurnitureGlow = Settings.enableFurnitureGlow;
            bool oldItemGlow = Settings.enableItemGlow;
            listing.CheckboxLabeled("CrystalTech_SettingMood".Translate(), ref Settings.enableMoodEffect);
            listing.CheckboxLabeled("CrystalTech_SettingLight".Translate(), ref Settings.enableLightTransmission);
            if (oldMood != Settings.enableMoodEffect || oldLight != Settings.enableLightTransmission)
            {
                foreach (var map in Find.Maps)
                {
                    CrystalTechCore.FlushAllCrystalGraphics(map);
                }
            }

            listing.Gap(12f);
            listing.Label("CrystalTech_SettingYield".Translate(Settings.craftingYieldMultiplier.ToString("F1")));
            Settings.craftingYieldMultiplier = listing.Slider(Settings.craftingYieldMultiplier, 0.5f, 5f);

            listing.Gap(12f);
            listing.CheckboxLabeled("CrystalTech_SettingAntiStealth".Translate(), ref Settings.enableAntiStealth);
            listing.CheckboxLabeled("CrystalTech_SettingWallGlow".Translate(), ref Settings.enableWallGlow);
            listing.CheckboxLabeled("CrystalTech_SettingFurnitureGlow".Translate(), ref Settings.enableFurnitureGlow);
            listing.CheckboxLabeled("CrystalTech_SettingItemGlow".Translate(), ref Settings.enableItemGlow);
            if (oldWallGlow != Settings.enableWallGlow || oldFurnitureGlow != Settings.enableFurnitureGlow || oldItemGlow != Settings.enableItemGlow)
            {
                foreach (var map in Find.Maps)
                {
                    CrystalTechCore.RefreshAllCrystalGlowers(map);
                }
            }

            listing.End();
            Settings.Write();
        }
    }

    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class RecipeWorker_MakeRecipeProducts_YieldPatch
    {
        static void Postfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef)
        {
            if (Main.Settings == null || Main.Settings.craftingYieldMultiplier <= 0f)
                return;

            bool isCrystalRecipe = recipeDef.defName == "Make_CrystalMaterial" || recipeDef.defName == "Make_CrystalMaterialBulk";
            if (!isCrystalRecipe)
                return;

            float mult = Main.Settings.craftingYieldMultiplier;
            if (mult <= 1f)
                return;

            var adjusted = new List<Thing>();
            foreach (var thing in __result)
            {
                if (thing.def.defName == "CrystalMaterial")
                {
                    thing.stackCount = Mathf.Max(1, Mathf.RoundToInt(thing.stackCount * mult));
                }
                adjusted.Add(thing);
            }
            __result = adjusted;
        }
    }
}