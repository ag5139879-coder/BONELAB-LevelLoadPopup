using MelonLoader;
using BoneLib;
using UnityEngine;
using System;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.0.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            // Hook the level loading events provided by BoneLib
            Hooking.OnLevelLoading += OnLevelLoading;
            Hooking.OnLevelLoaded += OnLevelLoaded;

            LoggerInstance.Msg("Level Load Popup loaded! Waiting for level transitions...");
        }

        private void OnLevelLoading(LevelInfo info)
        {
            try
            {
                if (info == null)
                {
                    LoggerInstance.Warning("OnLevelLoading received null LevelInfo");
                    return;
                }

                string title = GetLevelTitle(info);

                // Simple, reliable popup using BoneLib notifications
                // This works well in VR and is a good first step toward the full Patch 1 style UI
                BoneLib.Notifications.Notifier.Send(new BoneLib.Notifications.Notification
                {
                    Title = "Loading Level",
                    Message = title,
                    Type = BoneLib.Notifications.NotificationType.Info,
                    PopupLength = 5f,
                    ShowTitleOnPopup = true
                });

                LoggerInstance.Msg($"Showing load popup for: {title}");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Error in OnLevelLoading: {ex}");
            }
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            // Optional: clean up any custom UI here once the level is fully ready
            // For the notification version we don't need to do anything
        }

        private string GetLevelTitle(LevelInfo info)
        {
            try
            {
                // Prefer the nice display title from the crate
                if (info.levelReference != null && info.levelReference.Crate != null)
                {
                    string crateTitle = info.levelReference.Crate.Title;
                    if (!string.IsNullOrEmpty(crateTitle))
                        return crateTitle;
                }

                // Fallback to barcode if title is missing
                if (!string.IsNullOrEmpty(info.barcode))
                    return info.barcode;

                return "Unknown Level";
            }
            catch
            {
                return info?.barcode ?? "Unknown Level";
            }
        }
    }
}