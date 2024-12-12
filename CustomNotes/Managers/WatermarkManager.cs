using CustomNotes.Settings.Utilities;
using CustomNotes.Utilities;
using System;
using BeatSaberMarkupLanguage;
using HMUI;
using TMPro;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CustomNotes.Managers
{
    // Displays text that shows that the player is using custom notes in HMD only mode
    public class WatermarkManager : IInitializable, IDisposable
    {
        private readonly PluginConfig pluginConfig;

        internal WatermarkManager(PluginConfig pluginConfig)
        {
            this.pluginConfig = pluginConfig;
        }
        
        private GameObject watermarkObject;

        public void Initialize()
        {
            Plugin.Log.Debug($"Initializing {nameof(WatermarkManager)}!");
            if (pluginConfig.HmdOnly || LayerUtils.ForceHmdOnly)
            {
                CreateWatermark();
            }
        }

        public void Dispose()
        {
            Plugin.Log.Debug($"Disposing {nameof(WatermarkManager)}!");
            DestroyWatermark();
        }
        
        public void CreateWatermark()
        {
            if (watermarkObject != null)
            {
                return;
            }

            watermarkObject = new("CustomNotes Watermark");
            watermarkObject.transform.localScale = new(0.05f, 0.05f, 0.05f);
            watermarkObject.transform.position = new(0f, 0.025f, 0.8f);
            watermarkObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            var watermarkCanvas = watermarkObject.AddComponent<Canvas>();
            watermarkCanvas.renderMode = RenderMode.WorldSpace;
            ((RectTransform)watermarkCanvas.transform).sizeDelta = new(100, 50);

            watermarkObject.AddComponent<CurvedCanvasSettings>().SetRadius(0f);

            var text = (CurvedTextMeshPro)BeatSaberUI.CreateText((RectTransform)watermarkCanvas.transform, "Custom Notes Enabled", new(0, 0));
            text.alignment = TextAlignmentOptions.Center;
            text.color = new(0.95f, 0.95f, 0.95f);

            watermarkObject.SetLayerRecursively(NoteLayer.ThirdPerson);
        }
        
        public void DestroyWatermark()
        {
            if (watermarkObject != null)
            {
                Object.Destroy(watermarkObject);
                watermarkObject = null;
            }
        }
    }
}
