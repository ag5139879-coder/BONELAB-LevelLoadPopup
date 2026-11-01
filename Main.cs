using MelonLoader;
using MelonLoader.Utils;
using BoneLib;
using BoneLib.Notifications;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.6.1", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        private static string _imageFolder;
        private static GameObject _popupRoot;
        private static RawImage _previewImage;
        private static float _hideTime;

        private static readonly Dictionary<string, string> LevelThemes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
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
            { "Arena", "ARENA" },
            { "Moonbase", "MOONBASE" },
            { "Tuscany", "TUSCANY" },
            { "Tunnel", "THE TUNNEL" },
            { "Void", "THE VOID" },
            { "Halfway Park", "HALFWAY PARK" },
            { "Mythopolis", "MYTHOPOLIS" },
            { "15 - Void G114", "VOID G114" },
            { "Void G114", "VOID G114" }
        };

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Level Load Popup v1.6.1 initializing...");

            try
            {
                // MelonEnvironment lives in MelonLoader.Utils
                _imageFolder = Path.Combine(MelonEnvironment.UserDataDirectory, "LevelLoadPopup", "Images");
                if (!Directory.Exists(_imageFolder))
                {
                    Directory.CreateDirectory(_imageFolder);
                    LoggerInstance.Msg("Created image folder: " + _imageFolder);
                }
                else
                {
                    LoggerInstance.Msg("Image folder: " + _imageFolder);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Could not create image folder: " + ex.Message);
                _imageFolder = null;
            }

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

        public override void OnUpdate()
        {
            if (_popupRoot != null && _popupRoot.activeSelf && Time.time >= _hideTime)
            {
                _popupRoot.SetActive(false);
            }
        }

        private void OnLevelLoading(LevelInfo info)
        {
            try { LoggerInstance.Msg("[Loading] " + GetLevelTitle(info)); }
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

                // Image support is prepared but requires UnityEngine.ImageConversionModule
                // For now we only log whether a matching file exists
                if (!string.IsNullOrEmpty(_imageFolder) && Directory.Exists(_imageFolder))
                {
                    string found = FindImagePath(rawTitle, displayTitle);
                    if (found != null)
                        LoggerInstance.Msg("Image file found (display not yet enabled): " + found);
                    else
                        LoggerInstance.Msg("No image file for: " + rawTitle);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("OnLevelLoaded error: " + ex);
            }
        }

        private string FindImagePath(string rawTitle, string displayTitle)
        {
            string[] candidates =
            {
                displayTitle + ".png",
                displayTitle + ".jpg",
                rawTitle + ".png",
                rawTitle + ".jpg",
                displayTitle.Replace(" ", "_") + ".png",
                rawTitle.Replace(" ", "_") + ".png"
            };

            foreach (string name in candidates)
            {
                string path = Path.Combine(_imageFolder, name);
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        private string GetThemedTitle(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "UNKNOWN LEVEL";

            if (LevelThemes.TryGetValue(raw, out string themed))
                return themed;

            foreach (var kvp in LevelThemes)
            {
                if (raw.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kvp.Value;
            }

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