using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CrystalTech
{
    /// <summary>
    /// 透晶空投系统：
    /// 1. 新游戏开局：陨石陨晶舱坠落在玩家地图附近（40 透晶材料 + 4 母晶碎块），附叙事信件
    /// 2. 研究完成奖励：三阶透晶研究各触发一次样器空投（制作+惊喜双轨）
    /// 3. 老存档兼容：读档时检测已研究项目，补发未领取的研究奖励（不重复）
    /// </summary>
    public class GameComponent_CrystalDrop : GameComponent
    {
        private bool startDropDone;
        private bool rewardCrystalTech;
        private bool rewardCrystalAdvanced;
        private bool rewardCrystalReinforced;

        public GameComponent_CrystalDrop(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startDropDone, "startDropDone");
            Scribe_Values.Look(ref rewardCrystalTech, "rewardCrystalTech");
            Scribe_Values.Look(ref rewardCrystalAdvanced, "rewardCrystalAdvanced");
            Scribe_Values.Look(ref rewardCrystalReinforced, "rewardCrystalReinforced");
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            TryDoStartDrop();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            // 老存档：开局空投不再补（避免突兀），但已研究的奖励要补发
            TryCatchUpRewards();
        }

        public void TryDoStartDrop()
        {
            if (startDropDone) return;
            startDropDone = true;

            Map map = Find.CurrentMap ?? FirstPlayerMap();
            if (map == null)
            {
                startDropDone = false; // 还没有地图，下次再试
                return;
            }

            List<Thing> things = new List<Thing>();
            things.Add(MakeThing("CrystalMaterial", 40));
            things.Add(MakeThing("CrystalShard", 4));
            DropAtMapEdge(map, things, delegate (IntVec3 cell)
            {
                Find.LetterStack.ReceiveLetter(
                    "CrystalTech_StartDropLabel".Translate(),
                    "CrystalTech_StartDropText".Translate(),
                    LetterDefOf.PositiveEvent, new TargetInfo(cell, map));
            });
        }

        /// <summary>研究完成 hook（由 Harmony patch 调用）</summary>
        public void Notify_ResearchFinished(ResearchProjectDef proj)
        {
            Map map = Find.CurrentMap ?? FirstPlayerMap();
            if (map == null) return;

            if (proj.defName == "CrystalTech" && !rewardCrystalTech)
            {
                rewardCrystalTech = true;
                DropReward(map, "CrystalTech_RewardTechLabel", "CrystalTech_RewardTechText",
                    new string[] { "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle" });
            }
            else if (proj.defName == "CrystalAdvanced" && !rewardCrystalAdvanced)
            {
                rewardCrystalAdvanced = true;
                DropReward(map, "CrystalTech_RewardAdvancedLabel", "CrystalTech_RewardAdvancedText",
                    new string[] { "Apparel_CrystalShield", "Apparel_CrystalGoggles", "Apparel_CrystalContactLens" });
            }
            else if (proj.defName == "CrystalReinforced" && !rewardCrystalReinforced)
            {
                rewardCrystalReinforced = true;
                DropReward(map, "CrystalTech_RewardReinforcedLabel", "CrystalTech_RewardReinforcedText",
                    new string[] { "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle",
                                   "Apparel_CrystalShield", "Apparel_CrystalGoggles", "Apparel_CrystalContactLens",
                                   "Apparel_CrystalMagnifier", "Apparel_CrystalAmulet", "Apparel_CrystalTelescope" });
            }
        }

        private void DropReward(Map map, string labelKey, string textKey, string[] defNames)
        {
            List<Thing> things = new List<Thing>();
            foreach (string defName in defNames)
            {
                things.Add(MakeThing(defName, 1));
            }
            DropAtMapEdge(map, things, delegate (IntVec3 cell)
            {
                Find.LetterStack.ReceiveLetter(
                    labelKey.Translate(), textKey.Translate(),
                    LetterDefOf.PositiveEvent, new TargetInfo(cell, map));
            });
        }

        private void TryCatchUpRewards()
        {
            // 老存档：已研究但未领奖励 → 补发（一次性，标记后不再触发）
            ResearchManager rm = Find.ResearchManager;
            if (rm == null) return;

            if (IsResearched("CrystalTech") && !rewardCrystalTech)
            {
                Map map = Find.CurrentMap ?? FirstPlayerMap();
                if (map != null)
                {
                    rewardCrystalTech = true;
                    DropReward(map, "CrystalTech_RewardTechLabel", "CrystalTech_RewardTechOldSaveText",
                        new string[] { "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle" });
                }
            }
            if (IsResearched("CrystalAdvanced") && !rewardCrystalAdvanced)
            {
                Map map = Find.CurrentMap ?? FirstPlayerMap();
                if (map != null)
                {
                    rewardCrystalAdvanced = true;
                    DropReward(map, "CrystalTech_RewardAdvancedLabel", "CrystalTech_RewardAdvancedOldSaveText",
                        new string[] { "Apparel_CrystalShield", "Apparel_CrystalGoggles", "Apparel_CrystalContactLens" });
                }
            }
            if (IsResearched("CrystalReinforced") && !rewardCrystalReinforced)
            {
                Map map = Find.CurrentMap ?? FirstPlayerMap();
                if (map != null)
                {
                    rewardCrystalReinforced = true;
                    DropReward(map, "CrystalTech_RewardReinforcedLabel", "CrystalTech_RewardReinforcedOldSaveText",
                        new string[] { "CrystalLongSword", "CrystalSpear", "CrystalGreatBow", "CrystalDivineNeedle",
                                       "Apparel_CrystalShield", "Apparel_CrystalGoggles", "Apparel_CrystalContactLens",
                                       "Apparel_CrystalMagnifier", "Apparel_CrystalAmulet", "Apparel_CrystalTelescope" });
                }
            }
        }

        private static bool IsResearched(string defName)
        {
            ResearchProjectDef proj = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(defName);
            return proj != null && proj.IsFinished;
        }

        private static Map FirstPlayerMap()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome) return maps[i];
            }
            return maps.Count > 0 ? maps[0] : null;
        }

        private static Thing MakeThing(string defName, int count)
        {
            ThingDef def = ThingDefDatabase(defName);
            if (def == null) return null;
            Thing thing = Verse.ThingMaker.MakeThing(def);
            thing.stackCount = count;
            return thing;
        }

        private static ThingDef ThingDefDatabase(string defName)
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        }

        private static void DropAtMapEdge(Map map, List<Thing> things, System.Action<IntVec3> letter)
        {
            // 过滤掉 null（def 缺失时不炸）
            List<Thing> valid = things.FindAll(t => t != null);
            if (valid.Count == 0) return;

            IntVec3 dropCenter = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(dropCenter, map, valid, 110, canInstaDropDuringInit: false, leaveSlag: true);
            letter?.Invoke(dropCenter);
        }
    }

    /// <summary>研究完成 → 通知空投组件（只对本 mod 的研究项目生效）</summary>
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    public static class Patch_ResearchFinish_CrystalDrop
    {
        static void Postfix(ResearchProjectDef proj)
        {
            if (proj == null) return;
            if (proj.defName != "CrystalTech" && proj.defName != "CrystalAdvanced" && proj.defName != "CrystalReinforced") return;
            GameComponent_CrystalDrop comp = Current.Game?.GetComponent<GameComponent_CrystalDrop>();
            comp?.Notify_ResearchFinished(proj);
        }
    }
}
