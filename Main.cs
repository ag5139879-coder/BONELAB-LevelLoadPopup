using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using UnityEngine;
using System;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.3.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Level Load Popup v1.3.0 initializing...");

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
                string title = GetLevelTitle(info);
                LoggerInstance.Msg("[Loading] " + title);

                // Strong notification - this is the reliable part
                Notifier.Send(new Notification
                {
                    Title = "LOADING",
                    Message = title,
                    Type = NotificationType.Information,
                    PopupLength = 7f,
                    ShowTitleOnPopup = true
                });

                LoggerInstance.Msg("Notification sent for: " + title);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("OnLevelLoading error: " + ex);
            }
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            try
            {
                string title = GetLevelTitle(info);
                LoggerInstance.Msg("[Loaded] " + title);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("OnLevelLoaded error: " + ex);
            }
        }

        private string GetLevelTitle(LevelInfo info)
        {
            try
            {
                // Prefer the title field if BoneLib fills it
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