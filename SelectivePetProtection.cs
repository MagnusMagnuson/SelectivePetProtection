using HarmonyLib;
using System;
using Vector3 = UnityEngine.Vector3;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using System.Xml.Schema;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Bootstrap;
using Settlers.Behaviors;
using static Settlers.Behaviors.Companion;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;


namespace SelectivePetProtection
{

    public enum ShieldState
    {
        None,
        Stun,
        Immortal
    }

    public static class ShieldColors
    {
        public static Dictionary<ShieldState, string> Colors = new Dictionary<ShieldState, string>
        {
            { ShieldState.None, "#FFFFFF" },
            { ShieldState.Stun, "#95c9da" },
            { ShieldState.Immortal, "#d8bf58" }
        };

        public static void UpdateColors(string stunColor, string immortalColor)
        {
            Colors[ShieldState.Stun] = stunColor;
            Colors[ShieldState.Immortal] = immortalColor;
        }
    }
    [BepInDependency("RustyMods.VikingNPC", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class SelectivePetProtection : BaseUnityPlugin
    {
        internal const string ModName = "SelectivePetProtection";
        internal const string ModVersion = "0.2.1";
        internal const string Author = "magnus";
        private const string ModGUID = Author + "." + ModName;
        public static float stunRecoveryTime { get; internal set; } = 120f;
        public static string stunShieldColor { get; internal set; } = "#95c9da";
        public static string immortalShieldColor { get; internal set; } = "#d8bf58";
        public static ManualLogSource Log;

        public static bool isVikingNPCLoaded = false;
        BaseUnityPlugin vikingNPCPlugin;


        public static Harmony harmony = new Harmony("mod.selective_pet_protection");

        // configuration
        private ConfigEntry<int> stunRecoveryTimeConfig;
        private ConfigEntry<string> stunShieldColorConfig;
        private ConfigEntry<string> immortalShieldColorConfig;

        public static ConfigEntry<KeyboardShortcut> ToggleProtectionShortcut { get; set; }


        private void Start()
        {
            Log = base.Logger;
            foreach (var kvp in Chainloader.PluginInfos)
            {
                if (kvp.Key == "RustyMods.VikingNPC")
                {
                    SelectivePetProtection.Log.LogWarning("VikingNPC is loaded");
                    isVikingNPCLoaded = true;
                    vikingNPCPlugin = kvp.Value.Instance;
                    //ModConfig.SetShareSuiteReference(kvp.Value.Instance);
                    break;
                }
            }
        }


        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            //Log = base.Logger;


            // Configuration
            stunRecoveryTimeConfig = Config.Bind("General", "StunRecoveryTime", 120, "Time in seconds it takes for a pet to recover from being stunned after receiving a deadly hit");
            stunRecoveryTime = (float)stunRecoveryTimeConfig.Value;
            stunShieldColorConfig = Config.Bind("General", "StunShieldColor", "#95c9da", "Color as hex code of the shield icon that appears when a pet is protected and gets stunned when receiving a deadly hit (equivalent to essential in V+/PetProtection)");
            stunShieldColor = stunShieldColorConfig.Value;
            immortalShieldColorConfig = Config.Bind("General", "ImmortalShieldColor", "#d8bf58", "Color as hex code of the shield icon that appears when a pet is immortal (equivalent to immortal in V+/PetProtection)");

            // Update ShieldColors with config values
            ShieldColors.UpdateColors(stunShieldColorConfig.Value, immortalShieldColorConfig.Value);

            harmony.PatchAll();

            ToggleProtectionShortcut = Config.Bind("General", "ToggleProtectionShortcut", new KeyboardShortcut(KeyCode.T, KeyCode.LeftShift), "Toggle the protection of tamed creatures with the shield icon. Press Left Shift + T to toggle.");

        }

    }

    /// <summary>
    /// Toggles the shield icon when the player presses the hotkey on a tamed character.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateHover))] 
    public static class Player_UpdateHover_Patch
    {
        public static void Postfix(ref Player __instance)
        {
            if (SelectivePetProtection.ToggleProtectionShortcut.Value.IsDown())
            {
                Interactable componentInParent = __instance.m_hovering?.GetComponentInParent<Interactable>();
                if (componentInParent != null)
                {
                    GameObject hoveringObject = __instance.m_hovering.gameObject;
                    Character tame = hoveringObject.GetComponentInParent<Character>();

                    if (tame != null && tame.IsTamed())
                    {
                        ZDO zdo = tame.m_nview.GetZDO();
                        string tamedName = zdo.GetString(ZDOVars.s_tamedName);
                        string tamedNameNoTags = tamedName.RemoveRichTextTags().Replace("🛡️", "");

                        if (tamedName.Contains($"<color={ShieldColors.Colors[ShieldState.Stun]}>🛡️"))
                        {
                            zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags + $"<color={ShieldColors.Colors[ShieldState.Immortal]}>🛡️</color>");
                        }
                        else if (tamedName.Contains($"<color={ShieldColors.Colors[ShieldState.Immortal]}>🛡️"))
                        {
                            zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags);
                        }
                        else
                        {
                            zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags + $"<color={ShieldColors.Colors[ShieldState.Stun]}>🛡️</color>");
                        }
                        tame.m_name = zdo.GetString(ZDOVars.s_tamedName);

                    }
                }
            }
        }

    }


    /// <summary>
    /// Adds the shield icon to the tamed creature's name when the player names it "god". Otherwise executes the original method.
    /// </summary>
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.RPC_SetName))]
    public static class Tameable_RPC_SetName_Patch
    {
        public static bool Prefix(ref Tameable __instance, ref string name)
        {
            ZDO zdo = __instance.m_character.m_nview.GetZDO();
            string tamedName = zdo.GetString(ZDOVars.s_tamedName);
            string tamedNameNoTags = tamedName.RemoveRichTextTags().Replace("🛡️", "");

            if (name.Equals("!stun"))
            {
                zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags + $"<color={ShieldColors.Colors[ShieldState.Stun]}>🛡️</color>");
                return false;
            } 
            else if(name.Equals("!god"))
            {
                zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags + $"<color={ShieldColors.Colors[ShieldState.Immortal]}>🛡️</color>");
                return false;
            }
            else if(name.Equals("!none")) {
                zdo.Set(ZDOVars.s_tamedName, tamedNameNoTags);
                return false;
            }
            return true;
        }
    }    

    /// <summary>
    /// Temporarily removes the shield icon from the renamne window.
    /// </summary>
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.SetName))]
    public static class Tameable_SetName_Patch
    {
        public static bool Prefix(ref Tameable __instance)
        {
            ZDO zdo = __instance.m_character.m_nview.GetZDO();
            string tamedName = zdo.GetString(ZDOVars.s_tamedName);

            if (tamedName.Contains("🛡️"))
            {
                TextInput.instance.m_queuedSign = __instance;
                TextInput.instance.Show("$hud_rename", tamedName.RemoveRichTextTags().Replace("🛡️", ""), 10);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Prevent Rich Text Tags from being removed when displaying the hover name.
    /// </summary>
    [HarmonyPatch(typeof(Tameable), nameof(Character.GetHoverName))]
    public static class Tameable_GetHoverName_Patch
    {
        public static void Postfix(ref Tameable __instance, ref string __result)
        {
            ZDO zdo = __instance.m_character.m_nview.GetZDO();
            string tamedName = zdo.GetString(ZDOVars.s_tamedName); 

            if (tamedName.Contains("🛡️</color>"))
                __result = tamedName;
        }
    }

    /// <summary>
    /// Determines what happens when a tamed creature takes damage.
    /// </summary>
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
    public static class Character_ApplyDamage_Patch
    {
        public static void Postfix(ref Character __instance, ref HitData hit, ref bool showDamageText, ref bool triggerEffects, ref HitData.DamageModifier mod)
        {
            // Network & Tameable component
            ZDO zdo = __instance.m_nview.GetZDO();
            string tamedName = zdo.GetString(ZDOVars.s_tamedName);

            if (!__instance.IsTamed() || zdo == null)
                return;
            
            if (tamedName.Contains("🛡️"))
            {  
                // if killed on this hit
                if (__instance.GetHealth() <= 5f)
                {
                              
                    // Allow players to kill the tamed creature with ownerDamageOverride
                    if (ShouldIgnoreDamage(__instance, hit, zdo))
                    {
                        //stunned
                        if (tamedName.Contains(ShieldColors.Colors[ShieldState.Stun])) {

                            __instance.SetHealth(__instance.GetMaxHealth());
                            __instance.m_animator.SetBool("sleeping", true);
                            zdo.Set("sleeping", true);
                            zdo.Set("isRecoveringFromStun", true);

                            __instance.GetComponent<MonsterAI>().m_sleeping = true;
                            __instance.GetComponent<MonsterAI>().SetAlerted(false);
                            __instance.m_disableWhileSleeping = true;
                            __instance.GetComponent<MonsterAI>().m_nview.GetZDO().Set(ZDOVars.s_sleeping, value: true);
                            //if (__instance.GetComponent<MonsterAI>().m_character.m_moveDir != Vector3.zero)
                            //    __instance.GetComponent<MonsterAI>().StopMoving();

                            //__instance.GetComponent<MonsterAI>().SetAggravated(false, BaseAI.AggravatedReason.Theif);
                            //__instance.m_baseAI.m_alerted = false;
                            //__instance.m_baseAI.m_aggravated = false;
                            //__instance.m_baseAI.m_passiveAggresive = false;
                            //__instance.m_baseAI.m_nview.GetZDO().Set(ZDOVars.s_alert, false);

                        } 
                        else
                        {
                            __instance.SetHealth(__instance.GetMaxHealth());
                        }

                    }
                }
            }
        }

        private static bool ShouldIgnoreDamage(Character __instance, HitData hit, ZDO zdo)
        {
            if (hit == null)
                return true;
            Character attacker = hit.GetAttacker();
            if (attacker == null)
            {
                return true;
            }

            // deactivated since you can just remove the shield instead and then kill the pet
            // Attacker is player
            //if (attacker == __instance.GetComponent<Tameable>().GetPlayer(attacker.GetZDOID()))
            //    return false;
            return true;
        }
    }


    //[HarmonyPatch(typeof(TameableCompanion), nameof(TameableCompanion.GetStatus))]
    //public static class TameableCompanion_GetStatus_Patch
    //{
    //    public static void Postfix(ref TameableCompanion __instance, ref string __result)
    //    {
    //        SelectivePetProtection.Log.LogWarning("in getstatus postfix");
    //        if (__instance.m_companion.IsTamed())
    //        {
    //            ZDO zdo = __instance.m_nview.GetZDO();
    //            if(zdo.GetBool("isRecoveringFromStun"))
    //            {
    //                __result = "Recovering";
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Forces a tamed creature to stay asleep if it's recovering from being stunned.
    /// </summary>
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateSleep))]
    public static class MonsterAI_UpdateSleep_Patch
    {
        public static bool Prefix(MonsterAI __instance, ref float dt)
        {

            if (!__instance.m_character.IsTamed())
                return true;

            MonsterAI monsterAI = __instance;
            ZDO zdo = monsterAI.m_nview.GetZDO();
            if (zdo == null || !zdo.GetBool("isRecoveringFromStun"))
                return true;

            //if (monsterAI.m_character.m_moveDir != Vector3.zero)
            //    monsterAI.StopMoving();

            if (monsterAI.m_sleepTimer != 0f)
                monsterAI.m_sleepTimer = 0f;

            float timeSinceStun = zdo.GetFloat("timeSinceStun") + dt;
            zdo.Set("timeSinceStun", timeSinceStun);

            if (timeSinceStun >= SelectivePetProtection.stunRecoveryTime)
            {
                zdo.Set("timeSinceStun", 0f);
                monsterAI.m_sleepTimer = 0.5f;
                monsterAI.m_character.m_animator.SetBool("sleeping", false);
                zdo.Set("sleeping", false);

                zdo.Set("isRecoveringFromStun", false);
                monsterAI.Wakeup();

                //if (SelectivePetProtection.isVikingNPCLoaded)
                //{
                //    SelectivePetProtection.Log.LogWarning("in viking npc patch: " + __instance.m_character.GetComponent<TameableCompanion>());
                //    __instance.m_character.GetComponent<TameableCompanion>().m_companionAI.m_resting = false;
                //    __instance.m_character.GetComponent<TameableCompanion>().m_companionAI.
                //}
            }

            dt = 0f;

            return false;
        }
    }

    /// <summary>
    /// Adds a text indicator so player's know when an animal they've tamed has been stunned.
    /// </summary>
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    public static class Tameable_GetHoverText_Patch
    {
        public static void Postfix(Tameable __instance, ref string __result)
        {
            Tameable tameable = __instance;

            if(tameable.m_character.IsTamed())
                __result += "\n[<color=yellow><b>Shift + T</b></color>] Toggle Protection";

            // If tamed creature is recovering from a stun, then add Stunned to hover text.
            if (tameable.m_character.m_nview.GetZDO().GetBool("isRecoveringFromStun"))
                __result = __result.Insert(__result.IndexOf(" )"), ", Recovering");

        }
    }

    /// <summary>
    /// 
    /// </summary>
    //[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
}