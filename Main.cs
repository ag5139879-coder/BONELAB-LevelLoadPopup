using MelonLoader;
using BoneLib;
using BoneLib.Notifications;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[assembly: MelonInfo(typeof(LevelLoadPopup.Main), "Level Load Popup", "1.2.0", "ag5139879-coder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace LevelLoadPopup
{
    public class Main : MelonMod
    {
        private static GameObject _popupRoot;
        private static Text _titleText;
        private static RawImage _previewImage;
        private static float _hideTime;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Level Load Popup v1.2.0 initializing...");

            try
            {
                Hooking.OnLevelLoading += OnLevelLoading;
                Hooking.OnLevelLoaded += OnLevelLoaded;
                LoggerInstance.Msg("Hooks registered");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Hook failed: " + ex);
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
            LoggerInstance.Msg("OnLevelLoading: " + GetLevelTitle(info));
            ShowPopup(info);
        }

        private void OnLevelLoaded(LevelInfo info)
        {
            // Keep popup visible a bit into the level, then hide via OnUpdate timer
        }

        private void ShowPopup(LevelInfo info)
        {
            try
            {
                string title = GetLevelTitle(info);

                // Also keep the simple notification as backup
                try
                {
                    Notifier.Send(new Notification
                    {
                        Title = "Loading Level",
                        Message = title,
                        Type = NotificationType.Information,
                        PopupLength = 5f,
                        ShowTitleOnPopup = true
                    });
                }
                catch { }

                EnsurePopupUI();

                if (_titleText != null)
                    _titleText.text = title;

                // Try to get a preview texture from the level crate
                Texture preview = TryGetPreviewTexture(info);
                if (_previewImage != null)
                {
                    if (preview != null)
                    {
                        _previewImage.texture = preview;
                        _previewImage.color = Color.white;
                        LoggerInstance.Msg("Preview texture applied");
                    }
                    else
                    {
                        _previewImage.texture = null;
                        _previewImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
                        LoggerInstance.Msg("No preview texture found, using solid background");
                    }
                }

                if (_popupRoot != null)
                {
                    _popupRoot.SetActive(true);
                    _hideTime = Time.time + 6f;

                    // Position in front of the player camera if possible
                    try
                    {
                        var cam = Camera.main;
                        if (cam != null)
                        {
                            Transform t = _popupRoot.transform;
                            t.position = cam.transform.position + cam.transform.forward * 2.5f;
                            t.rotation = Quaternion.LookRotation(t.position - cam.transform.position);
                        }
                    }
                    catch { }
                }

                LoggerInstance.Msg("Custom popup shown for: " + title);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("ShowPopup error: " + ex);
            }
        }

        private void EnsurePopupUI()
        {
            if (_popupRoot != null) return;

            try
            {
                _popupRoot = new GameObject("LevelLoadPopup_Root");
                GameObject.DontDestroyOnLoad(_popupRoot);

                var canvas = _popupRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 999;

                var scaler = _popupRoot.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 10f;

                _popupRoot.AddComponent<GraphicRaycaster>();

                // Background panel
                var bgGO = new GameObject("Background");
                bgGO.transform.SetParent(_popupRoot.transform, false);
                var bgImage = bgGO.AddComponent<Image>();
                bgImage.color = new Color(0f, 0f, 0f, 0.75f);
                var bgRect = bgGO.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(800, 500);

                // Preview image
                var previewGO = new GameObject("Preview");
                previewGO.transform.SetParent(_popupRoot.transform, false);
                _previewImage = previewGO.AddComponent<RawImage>();
                _previewImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
                var previewRect = previewGO.GetComponent<RectTransform>();
                previewRect.sizeDelta = new Vector2(700, 350);
                previewRect.anchoredPosition = new Vector2(0, 40);

                // Title text
                var textGO = new GameObject("Title");
                textGO.transform.SetParent(_popupRoot.transform, false);
                _titleText = textGO.AddComponent<Text>();
                _titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _titleText.fontSize = 42;
                _titleText.alignment = TextAnchor.MiddleCenter;
                _titleText.color = Color.white;
                _titleText.text = "Loading...";
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(760, 80);
                textRect.anchoredPosition = new Vector2(0, -180);

                // Scale the whole thing for world space
                _popupRoot.transform.localScale = Vector3.one * 0.004f;

                _popupRoot.SetActive(false);
                LoggerInstance.Msg("Popup UI created");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Failed to create popup UI: " + ex);
            }
        }

        private Texture TryGetPreviewTexture(LevelInfo info)
        {
            try
            {
                if (info.levelReference == null) return null;
                var crate = info.levelReference.Crate;
                if (crate == null) return null;

                // Try common property names via reflection (different BoneLib / game versions)
                var type = crate.GetType();
                string[] candidates = { "Preview", "PreviewTexture", "Icon", "Thumbnail", "MainTexture", "Texture", "Sprite" };

                foreach (var name in candidates)
                {
                    var prop = type.GetProperty(name);
                    if (prop != null)
                    {
                        var val = prop.GetValue(crate);
                        if (val is Texture tex) return tex;
                        if (val is Sprite spr && spr.texture != null) return spr.texture;
                    }

                    var field = type.GetField(name);
                    if (field != null)
                    {
                        var val = field.GetValue(crate);
                        if (val is Texture tex) return tex;
                        if (val is Sprite spr && spr.texture != null) return spr.texture;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("TryGetPreviewTexture: " + ex.Message);
            }
            return null;
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