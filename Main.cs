using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using System;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.5.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        // Simple theme data for official-looking titles
        private static readonly Dictionary<string, string> LevelThemes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Main campaign / hubs
            { "Descent", "DESCENT" },
            { "Hub", "BONELAB HUB" },
            { "Museum Basement", "MUSEUM BASEMENT" },
            { "Street Puncher", "STREET PUNCHER" },
            { "Sprint Bridge", "SPRINT BRIDGE 04" },
            { "Hangar", "HANGAR" },
            { "Pillar Climb", "PILLAR CLIMB" },
            { "Big Anomaly", "BIG ANOMALY" },
            { "Ascent", "ASCENT" },
            { "Outro Park", "OUTRO PARK" },

            // Common arenas / maps
            { "Arena", "ARENA" },
            { "Moonbase", "MOONBASE" },
            { "Tuscany", "TUSCANY" },
            { "Tunnel", "THE TUNNEL" },
            { "Void", "THE VOID" },
            { "Halfway Park", "HALFWAY PARK" },
            { "Mythopolis", "MYTHOPOLIS" },
            { "District 02 Container Yard", "CONTAINER YARD" },
            { "District 03 Roof Tops", "ROOF TOPS" },
            { "District 04 Metro", "METRO" },

            // Fallbacks people often see
            { "15 - Void G114", "VOID G114" },
            { "Void G114", "VOID G114" }
        };

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Level Load Popup v1.5.0 initializing...");

            try
            {
                Hooking.OnLevelLoading += OnLevelLoading;
                Hooking.OnLevelLoaded += OnLevelLoaded;
                LoggerInstance.Msg("Hooks registered successfully");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to register hooks: " + ex);
            }

            LoggerInstance.Msg("Level Load Popup ready.");
        }

        private void OnLevelLoading(LevelInfo info)
        {
            try
            {
                LoggerInstance.Msg("[Loading] " + GetLevelTitle(info));
            }
            catch { }
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            try
            {
                string rawTitle = GetLevelTitle(info);
                string displayTitle = GetThemedTitle(rawTitle);

                LoggerInstance.Msg("[Loaded] " + rawTitle + " -> " + displayTitle);

                Notifier.Send(new Notification
                {
                    Title = "LEVEL LOADED",
                    Message = displayTitle,
                    Type = NotificationType.Information,
                    PopupLength = 6.5f,
                    ShowTitleOnPopup = true
                });

                LoggerInstance.Msg("Notification sent: " + displayTitle);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("OnLevelLoaded error: " + ex);
            }
        }

        private string GetThemedTitle(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "UNKNOWN LEVEL";

            // Exact match
            if (LevelThemes.TryGetValue(raw, out string themed))
                return themed;

            // Partial match (e.g. "15 - Void G114" contains "Void")
            foreach (var kvp in LevelThemes)
            {
                if (raw.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kvp.Value;
            }

            // Default: uppercase the original name for a cleaner look
            return raw.ToUpperInvariant();
        }

        private string GetLevelTitle(LevelInfo info)
        {
            try
            {
                if (!string.IsNullOrEmpty(info.title))
                    return info.title;

                if (info.levelReference != null && info.levelReference.Crate != null)
                {
                    string crateTitle = info.levelReference.Crate.Title;
                    if (!string.IsNullOrEmpty(crateTitle))
                        return crateTitle;
                }

                if (!string.IsNullOrEmpty(info.barcode))
                    return info.barcode;

                return "Unknown Level";
            }
            catch
            {
                return "Unknown Level";
            }
        }
    }
}