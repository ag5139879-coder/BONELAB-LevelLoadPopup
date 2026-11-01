using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.6.0", "ag5139879-coder")]
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
            LoggerInstance.Msg("Level Load Popup v1.6.0 initializing...");

            try
            {
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

                // Always show the text notification
                Notifier.Send(new Notification
                {
                    Title = "LEVEL LOADED",
                    Message = displayTitle,
                    Type = NotificationType.Information,
                    PopupLength = 6.5f,
                    ShowTitleOnPopup = true
                });

                // Try to show image if one exists
                Texture2D tex = TryLoadImage(rawTitle, displayTitle);
                if (tex != null)
                {
                    ShowImagePopup(tex);
                    LoggerInstance.Msg("Image popup shown for: " + displayTitle);
                }
                else
                {
                    LoggerInstance.Msg("No image found for: " + rawTitle);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("OnLevelLoaded error: " + ex);
            }
        }

        private Texture2D TryLoadImage(string rawTitle, string displayTitle)
        {
            if (string.IsNullOrEmpty(_imageFolder) || !Directory.Exists(_imageFolder))
                return null;

            // Try several possible filenames
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
                if (!File.Exists(path)) continue;

                try
                {
                    byte[] data = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(tex, data))
                    {
                        LoggerInstance.Msg("Loaded image: " + path);
                        return tex;
                    }
                }
                catch (Exception ex)
                {
                    LoggerInstance.Warning("Failed to load " + path + ": " + ex.Message);
                }
            }

            return null;
        }

        private void ShowImagePopup(Texture2D tex)
        {
            try
            {
                EnsurePopupUI();
                if (_popupRoot == null || _previewImage == null) return;

                _previewImage.texture = tex;
                _previewImage.color = Color.white;

                // Place in front of the player
                var cam = Camera.main;
                if (cam != null)
                {
                    Transform t = _popupRoot.transform;
                    t.position = cam.transform.position + cam.transform.forward * 2.2f;
                    t.rotation = Quaternion.LookRotation(t.position - cam.transform.position);
                }

                _popupRoot.SetActive(true);
                _hideTime = Time.time + 6.5f;
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("ShowImagePopup error: " + ex);
            }
        }

        private void EnsurePopupUI()
        {
            if (_popupRoot != null) return;

            try
            {
                _popupRoot = new GameObject("LevelLoadPopup_Image");
                GameObject.DontDestroyOnLoad(_popupRoot);

                var canvas = _popupRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 1000;

                _popupRoot.AddComponent<CanvasScaler>();
                _popupRoot.AddComponent<GraphicRaycaster>();

                var imageGO = new GameObject("Preview");
                imageGO.transform.SetParent(_popupRoot.transform, false);
                _previewImage = imageGO.AddComponent<RawImage>();
                _previewImage.color = Color.white;

                var rect = imageGO.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(800, 450);

                _popupRoot.transform.localScale = Vector3.one * 0.0035f;
                _popupRoot.SetActive(false);

                LoggerInstance.Msg("Image popup UI created");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to create image UI: " + ex);
                _popupRoot = null;
            }
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