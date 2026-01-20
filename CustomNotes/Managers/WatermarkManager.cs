using System;
using BeatSaberMarkupLanguage;
using CameraUtils.Core;
using HMUI;
using TMPro;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CustomNotes.Managers;

// Displays text that shows that the player is using custom notes in HMD only mode
internal class WatermarkManager : IInitializable, IDisposable
{
    private readonly PluginConfig config;

    private WatermarkManager(PluginConfig config)
    {
        this.config = config;
    }
        
    private GameObject watermarkObject;

    public void Initialize()
    {
        if (config.HmdOnly)
        {
            CreateWatermark();
        }
    }

    public void Dispose()
    {
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

        watermarkObject.SetLayerRecursively(VisibilityLayer.DesktopOnlyAndReflected);
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