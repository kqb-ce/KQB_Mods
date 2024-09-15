using GameLogic;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;

namespace MoreConfetti
{
    [HarmonyPatch(typeof(DeathEffectGradients))]
    [HarmonyPatch("SetParticleSystems")]
    public static class ParticleSystems_Patch
    {
        public static int count = 0;
        public static int ogMax1;
        public static int ogMax2;
        public static float duration2;
        public static float emission1;
        public static float emission2;

        public static void Postfix(ref ParticleSystem ___bigParticleSystem, ref ParticleSystem ___littleParticleSystem, Team team)
        {
            ParticleSystem.MainModule main = ___bigParticleSystem.main;
            ParticleSystem.MainModule main2 = ___littleParticleSystem.main;
            ParticleSystem.TrailModule trails = ___littleParticleSystem.trails;

            if (ParticleSystems_Patch.count % 2 == 0)
            {
                Debug.Log("BIG CONFEttI");
                ParticleSystems_Patch.ogMax1 = main.maxParticles;
                ParticleSystems_Patch.ogMax2 = main2.maxParticles;

                main.maxParticles = main.maxParticles * 20;
                main2.maxParticles = main2.maxParticles * 20;

                ParticleSystems_Patch.duration2 = main2.duration;
                //main.duration = main.duration + 1f;
                main2.duration = main2.duration + 1f;

                ParticleSystems_Patch.emission1 = ___bigParticleSystem.emissionRate;
                ParticleSystems_Patch.emission2 = ___littleParticleSystem.emissionRate;
                ___bigParticleSystem.emissionRate = 60f;
                ___littleParticleSystem.emissionRate = 60f;
            }
            else {
                Debug.Log("little");

                main.maxParticles = ParticleSystems_Patch.ogMax1;
                main2.maxParticles = ParticleSystems_Patch.ogMax2;

                main2.duration = ParticleSystems_Patch.duration2;

                ___bigParticleSystem.emissionRate = ParticleSystems_Patch.emission1;
                ___littleParticleSystem.emissionRate = ParticleSystems_Patch.emission2;

            }
            ParticleSystems_Patch.count++;

        }

    }

}
