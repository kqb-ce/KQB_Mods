using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;

namespace PanAudioMod
{
    [HarmonyPatch(typeof(SoundManager))]
    [HarmonyPatch("PlaySFX")]
    class PanAudioMod
    {
        static bool Prefix(AudioEvent whichAudioEvent, float panPosition = 0f, MatchManager.EntityRelation entityRelation = MatchManager.EntityRelation.None, GameObject whichGameObject = null, Action callback = null) {
            if (whichAudioEvent is null) {
                return true;
            }
            if (String.Equals("snd_dash", whichAudioEvent.name) || String.Equals("snd_attackMorningStar", whichAudioEvent.name))
            {
                SimpleAudioEvent simpleAudioEvent = (SimpleAudioEvent)whichAudioEvent;
                if (simpleAudioEvent.delayBeforeStart < 0.01f)
                {
                    System.Object[] parametersArray = new object[] { whichAudioEvent, panPosition, entityRelation, null };
                    typeof(SoundManager).GetMethod("PlayNow", BindingFlags.NonPublic | BindingFlags.Static).Invoke(new SoundManager(), parametersArray);
                }
                return false;
            }
            return true;
        }
    }
}
