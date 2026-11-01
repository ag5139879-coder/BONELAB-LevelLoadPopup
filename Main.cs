using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using System;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.1.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Level Load Popup v1.1.0 initializing...");

            try
            {
                Hooking.OnLevelLoading += OnLevelLoading;
                LoggerInstance.Msg("Hooked OnLevelLoading");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to hook OnLevelLoading: " + ex);
            }

            try
            {
                Hooking.OnLevelLoaded += OnLevelLoaded;
                LoggerInstance.Msg("Hooked OnLevelLoaded");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to hook OnLevelLoaded: " + ex);
            }

            LoggerInstance.Msg("Level Load Popup ready.");
        }

        private void OnLevelLoading(LevelInfo info)
        {
            LoggerInstance.Msg("OnLevelLoading fired!");
            ShowPopup(info, "Loading");
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            LoggerInstance.Msg("OnLevelLoaded fired!");
            ShowPopup(info, "Loaded");
        }

        private void ShowPopup(LevelInfo info, string prefix)
        {
            try
            {
                string title = GetLevelTitle(info);
                LoggerInstance.Msg(prefix + " level: " + title);

                // Try BoneLib notification
                try
                {
                    Notifier.Send(new Notification
                    {
                        Title = prefix + " Level",
                        Message = title,
                        Type = NotificationType.Information,
                        PopupLength = 6f,
                        ShowTitleOnPopup = true
                    });
                    LoggerInstance.Msg("Notification sent");
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error("Notifier.Send failed: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("ShowPopup error: " + ex);
            }
        }

        private string GetLevelTitle(LevelInfo info)
        {
            try
            {
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