using GameLogic;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using System.Reflection;
using UnityEngine;

namespace MoreConfetti
{
    [HarmonyPatch(typeof(DeathEffectGradients))]
    [HarmonyPatch("Init")]
    public static class ParticleSystems_Patch
    {
        public static void Postfix(ref ParticleSystem ___bigParticleSystem, ref ParticleSystem ___littleParticleSystem)
        {

            ParticleSystem.MainModule main = ___bigParticleSystem.main;
            ParticleSystem.MainModule main2 = ___littleParticleSystem.main;
            ParticleSystem.TrailModule trails = ___littleParticleSystem.trails;
            Debug.Log("max big: " + main.maxParticles);
            Debug.Log("Max radius: " + ___bigParticleSystem.shape.radius);

            main.maxParticles = 2147483647;
            main2.maxParticles = 2147483647;

            ParticleSystem.EmissionModule bigEmission = ___bigParticleSystem.emission;
            ParticleSystem.EmissionModule littleEmission = ___littleParticleSystem.emission;

            littleEmission.rateOverTime = new ParticleSystem.MinMaxCurve(1000.0f);
            bigEmission.rateOverTime = new ParticleSystem.MinMaxCurve(1000.0f);

            ParticleSystem.ShapeModule bigShape = ___bigParticleSystem.shape;
            ParticleSystem.ShapeModule lilShape = ___littleParticleSystem.shape;

            bigShape.radius = 3f;
            lilShape.radius = 3f;


        }

    }

    [HarmonyPatch(typeof(QueenVisuals))]
    [HarmonyPatch("Awake")]
    public static class QueenTrail_Patch
    {
        public static bool has_ran = false;
        public static void Postfix(ref ParticleSystem ___particalSystem)
        {

                ParticleSystem.MainModule main = ___particalSystem.main;
                main.maxParticles = 2147483647;
                Debug.Log("duration: " + main.duration);
                main.duration = 10f;
                ParticleSystem.EmissionModule bigEmission = ___particalSystem.emission;
                bigEmission.rateOverTime = new ParticleSystem.MinMaxCurve(1000.0f);
                
        }
    }
}