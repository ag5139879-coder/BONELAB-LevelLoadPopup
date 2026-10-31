using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using System;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.0.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            try
            {
                Hooking.OnLevelLoading += OnLevelLoading;
                Hooking.OnLevelLoaded += OnLevelLoaded;
                LoggerInstance.Msg("Level Load Popup loaded!");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to hook level events: " + ex);
            }
        }

        private void OnLevelLoading(LevelInfo info)
        {
            try
            {
                string title = GetLevelTitle(info);

                Notifier.Send(new Notification
                {
                    Title = "Loading Level",
                    Message = title,
                    Type = NotificationType.Information,
                    PopupLength = 5f,
                    ShowTitleOnPopup = true
                });

                LoggerInstance.Msg("Showing load popup for: " + title);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Error in OnLevelLoading: " + ex);
            }
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            // Nothing needed for the simple notification version
        }

        private string GetLevelTitle(LevelInfo info)
        {
            try
            {
                // Try the nice display title first
                if (info.levelReference != null && info.levelReference.Crate != null)
                {
                    string crateTitle = info.levelReference.Crate.Title;
                    if (!string.IsNullOrEmpty(crateTitle))
                        return crateTitle;
                }

                // Fallback to barcode
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