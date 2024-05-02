using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ClockSpeed
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource StaticLogger;
        public static ConfigEntry<float> ClockTimeScale;
        public static GUIStyle Style;
        public static bool ModBroken = false;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! Applying patch...");
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            ClockTimeScale = Config.Bind("Clock Speed", "Clock Speed Scale", 780f / 7f, "How many times faster than real time the in-game clock should move.");
            StaticLogger = Logger;
        }
        private void OnGUI()
        {
                if (Style == null)
                {
                    Style = new GUIStyle(GUI.skin.label);
                    Style.alignment = TextAnchor.UpperCenter;
                    Style.fontSize = 30;
                    Style.normal.textColor = Color.red;
                }
                float x = (float)(Screen.width / 2 - 400);
                float y = (float)(Screen.height - 130);
                if (ModBroken)
                {
                    GUI.Label(new Rect(x, y, 800f, 170f), "Clock Speed Scale may not be set to 0 or a negative value, as this breaks the game. The value has not been applied.", Style);
                }
        }
    }
    [HarmonyPatch(typeof(DayCycleManager), "UpdateTimer")]
    public static class DayCycleManager_UpdateTimer_Patch
    {
        public static void Prefix(DayCycleManager __instance)
        {
            if(Plugin.ClockTimeScale.Value <= 0)
            {
                Plugin.ModBroken = true;
                return;
            }
            Plugin.ModBroken = false;
            __instance.m_GameTimeScale *= Plugin.ClockTimeScale.Value * 7f / 780f;
            //Plugin.StaticLogger.LogInfo("Set GameTimeScale to " + __instance.m_GameTimeScale);

        }
        public static void Postfix(DayCycleManager __instance)
        {
            if (Plugin.ModBroken) return;
            __instance.m_GameTimeScale /= Plugin.ClockTimeScale.Value * 7f / 780f;
            //Plugin.StaticLogger.LogInfo("Set GameTimeScale to " + __instance.m_GameTimeScale);
        }
    }
}
