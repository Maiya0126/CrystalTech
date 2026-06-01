using RimWorld;
using Verse;
using UnityEngine;

namespace CrystalTech
{
    public class CompPowerPlantTransparentWall : CompPowerPlant
    {
        protected override float DesiredPowerOutput
        {
            get
            {
                if (!parent.Spawned) return 0f;

                float skyGlow = parent.Map.skyManager.CurSkyGlow;
                float lightFactor = Mathf.Lerp(0f, 1f, skyGlow);

                bool isRoofed = parent.Map.roofGrid.Roofed(parent.Position);
                if (isRoofed)
                {
                    lightFactor *= 0.3f;
                }

                return -Props.PowerConsumption * lightFactor;
            }
        }
    }

    public class CompProperties_PowerPlantTransparentWall : CompProperties_Power
    {
        public CompProperties_PowerPlantTransparentWall()
        {
            compClass = typeof(CompPowerPlantTransparentWall);
        }
    }
}
