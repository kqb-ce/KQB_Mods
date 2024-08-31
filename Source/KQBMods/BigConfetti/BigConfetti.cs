using HarmonyLib;
using UnityEngine;
using GameLogic;
using UnityEngine.UI;
using LiquidBit.KillerQueenX;
using System.Reflection;
using static UnityEngine.ParticleSystem;

namespace BigConfetti
{
    [HarmonyPatch(typeof(DeathEffectGradients))]
    [HarmonyPatch("Play")]
    public static class Play_Patch
    {
        public static bool AdjustParticles = false;
        public static bool Prefix(ParticleSystem ___littleParticleSystem, ParticleSystem ___bigParticleSystem)
        {

                EmissionModule littleEmission = ___littleParticleSystem.emission;
                littleEmission.rateOverTimeMultiplier = 15f;
                MainModule littleMain = ___littleParticleSystem.main;
                littleMain.startSizeX = 5f;
                littleMain.startSizeY = 10f;

                NoiseModule littleNoise = ___littleParticleSystem.noise;
                littleNoise.enabled = true;

                EmissionModule bigEmission = ___bigParticleSystem.emission;
                bigEmission.rateOverTimeMultiplier = 15f;
                MainModule bigMain = ___bigParticleSystem.main;
                bigMain.startSizeX = 5f;
                bigMain.startSizeY = 10f;

                NoiseModule bigNoise = ___bigParticleSystem.noise;
                bigNoise.enabled = true;

            return true;
        }
    }

}
