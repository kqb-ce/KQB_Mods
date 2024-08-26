using HarmonyLib;
namespace KQB_o_ween
{
	[HarmonyPatch(typeof(AssetSystem), MethodType.Constructor)]
	internal class AssetSystemMod
	{
		private static void Postfix(AssetSystem __instance)
		{
			AssetSystem.bundlePlayerSpritesName = "../../BepInEx/plugins/halloween/assets_playersprites-halloween";
			AssetSystem.bundleSharedUIName = "../../BepInEx/plugins/halloween/assets_sharedui-halloween";
		}
	}
}
