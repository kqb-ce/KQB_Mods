using HarmonyLib;
using UnityEngine;
using System;

namespace FixEmotes
{

    [HarmonyPatch(typeof(CustomControlMapping))]
    [HarmonyPatch("Load")]
    public static class CCMLoad_Patch
    {
        public static void Postfix(CustomControlMapping __instance)
        {
            try
            {
                GameObject[] gameObject = GameObject.FindObjectsOfType<GameObject>(true);

                foreach (GameObject go in gameObject)
                {
                    if (go.GetName() == "SettingAndMapCategoriesGroup")
                    {
                        go.SetActive(true);

                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log("caught exception in FixEmotes patch");
            }
        }

    }
}
