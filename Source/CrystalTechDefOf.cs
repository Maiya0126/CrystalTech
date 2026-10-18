using RimWorld;
using Verse;

namespace CrystalTech
{
    [DefOf]
    public static class CrystalTechDefOf
    {
        public static HediffDef CrystalRegenerationHediff;
        public static HediffDef CrystalNeedleParalysis;
        public static HediffDef CrystalAcupuncture;
        public static HediffDef CrystalHemorrhage;

        static CrystalTechDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CrystalTechDefOf));
        }
    }
}
