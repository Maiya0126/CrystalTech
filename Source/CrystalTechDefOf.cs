using RimWorld;
using Verse;

namespace CrystalTech
{
    [DefOf]
    public static class CrystalTechDefOf
    {
        public static HediffDef CrystalRegenerationHediff;

        static CrystalTechDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CrystalTechDefOf));
        }
    }
}
