using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomNotes.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using Utils = CustomNotes.Utilities.Utils;
using Object = UnityEngine.Object;

namespace CustomNotes.Managers;

internal class NoteAssetLoader : IInitializable, IDisposable
{
    private readonly PluginConfig config;

    private NoteAssetLoader(PluginConfig config)
    {
        this.config = config;
    }

    private bool isLoaded;

    public static string NotesDirectory { get; } = Path.Combine(UnityGame.InstallPath, "CustomNotes");

    public List<CustomNote> CustomNoteObjects { get; private set; } = [];
    public List<string> CustomNoteFiles { get; private set; } = [];

    public int SelectedNoteIdx { get; set; }
    public bool CustomNoteSelected => SelectedNoteIdx != 0;

    /// <summary>
    /// Load all CustomNotes 
    /// </summary>
    public void Initialize()
    {
        if (isLoaded)
        {
            return;
        }

        Directory.CreateDirectory(NotesDirectory);

        CustomNoteFiles = Utils
            .GetFileNames(NotesDirectory, ["*.bloq", "*.note"], SearchOption.AllDirectories, true)
            .ToList();
        Plugin.Log.Notice($"{CustomNoteFiles.Count} external notes found. Preparing to load.");
            
        CustomNoteObjects = LoadCustomNotes(CustomNoteFiles);
        Plugin.Log.Notice($"{CustomNoteObjects.Count - 1} total custom notes loaded.");

        SelectedNoteIdx = GetSelectedNoteIndex();
        isLoaded = true;
    }

    /// <summary>
    /// Clear all loaded CustomNotes
    /// </summary>
    public void Dispose()
    {
        foreach (var customNote in CustomNoteObjects)
        {
            customNote.Destroy();
        }
        isLoaded = false;
        SelectedNoteIdx = 0;
        CustomNoteObjects.Clear();
        CustomNoteFiles.Clear();
    }

    /// <summary>
    /// Reload all CustomNotes
    /// </summary>
    internal void Reload()
    {
        Plugin.Log.Debug("Reloading the NoteAssetLoader");

        Dispose();
        Initialize();
    }

    private int GetSelectedNoteIndex()
    {
        if (string.IsNullOrWhiteSpace(config.LastNote))
        {
            return 0;
        }
            
        for (int i = 0; i < CustomNoteObjects.Count; i++)
        {
            if (CustomNoteObjects[i].FileName == config.LastNote)
            {
                return i;
            }
        }
            
        return 0;
    }
    
    public static CustomNote LoadInternal(byte[] noteData, string name)
    {
        try
        {
            if (noteData is null or []) throw new ArgumentNullException(nameof(noteData), "noteData is null.");
            if (name is null or []) throw new ArgumentNullException(nameof(name), "note name is null.");
            
            var assetBundle = AssetBundle.LoadFromMemory(noteData);
            return LoadFromBundle(assetBundle, name);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn("Problem encountered getting the AssetBundle from a resource");
            Plugin.Log.Warn(ex);

            return new("DefaultNotes",
                $@"File: 'internalResource\\{name}'" +
                "\n\nAn internal asset has failed to load." +
                "\n\nThis shouldn't have happened and should be reported!" +
                " Remember to include the log related to this incident.");
        }
    }
        
    public static GameObject LoadNotePrefab(AssetBundle assetBundle) => 
        assetBundle.LoadAsset<GameObject>("assets/_customnote.prefab");

    private static List<CustomNote> LoadCustomNotes(IEnumerable<string> customNoteFiles) => 
        customNoteFiles.Select(LoadCustomNote).Prepend(CustomNote.Default).ToList();
    
    private static CustomNote LoadCustomNote(string fileName)
    {
        try
        {
            var filePath = Path.Combine(NotesDirectory, fileName);
            var assetBundle = AssetBundle.LoadFromFile(filePath);

            return LoadFromBundle(assetBundle, fileName);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Problem encountered when loading '{Path.GetFileNameWithoutExtension(fileName)}'");
            Plugin.Log.Warn(ex);

            return new("DefaultNotes",
                $"File: '{fileName}'" +
                "\n\nThis file failed to load." +
                "\n\nThis may have been caused by having duplicated files, another note with the" +
                " same name already exists or that the custom note is simply just broken." +
                "\n\nThe best thing is probably just to delete it!");
        }
    }

    private static CustomNote LoadFromBundle(AssetBundle assetBundle, string fileName)
    {
        var noteObject = LoadNotePrefab(assetBundle);

        var descriptor = noteObject.GetComponent<NoteDescriptor>();
        descriptor.Icon ??= Utils.GetDefaultCustomIcon();

        var noteLeft = noteObject.transform.Find("NoteLeft").gameObject;
        var noteRight = noteObject.transform.Find("NoteRight").gameObject;
        var noteDotLeftTransform = noteObject.transform.Find("NoteDotLeft");
        var noteDotRightTransform = noteObject.transform.Find("NoteDotRight");
        var noteDotLeft = noteDotLeftTransform != null ? noteDotLeftTransform.gameObject : noteLeft;
        var noteDotRight = noteDotRightTransform != null ? noteDotRightTransform.gameObject : noteRight;
        var noteBomb = noteObject.transform.Find("NoteBomb")?.gameObject;

        var burstSliderLeft = GetBurstSlider(noteObject, noteDotLeft, "BurstSliderLeft");
        var burstSliderRight = GetBurstSlider(noteObject, noteDotRight, "BurstSliderRight");

        var burstSliderHeadLeftT = noteObject.transform.Find("BurstSliderHeadLeft");
        var burstSliderHeadRightT = noteObject.transform.Find("BurstSliderHeadRight");
        var burstSliderHeadLeft = burstSliderHeadLeftT != null ? burstSliderHeadLeftT.gameObject : noteLeft;
        var burstSliderHeadRight = burstSliderHeadRightT != null ? burstSliderHeadRightT.gameObject : noteRight;
        
        var burstSliderHeadDotLeftT = noteObject.transform.Find("BurstSliderHeadDotLeft");
        var burstSliderHeadDotRightT = noteObject.transform.Find("BurstSliderHeadDotRight");
        var burstSliderHeadDotLeft = 
            burstSliderHeadDotLeftT != null ? burstSliderHeadDotLeftT.gameObject 
            : burstSliderHeadLeft != null ? burstSliderHeadLeft.gameObject 
            : noteDotLeft;
        var burstSliderHeadDotRight = 
            burstSliderHeadDotRightT != null ? burstSliderHeadDotRightT.gameObject 
            : burstSliderHeadRight != null ? burstSliderHeadRight.gameObject 
            : noteDotRight;

        return new(fileName,
            assetBundle,
            descriptor,
            new(noteLeft, noteDotLeft, burstSliderHeadLeft, burstSliderHeadDotLeft, burstSliderLeft),
            new(noteRight, noteDotRight, burstSliderHeadRight, burstSliderHeadDotRight, burstSliderRight),
            noteBomb);
    }
    
    private static GameObject GetBurstSlider(GameObject prefab, GameObject dotPrefab, string sliderPrefabName)
    {
        var burstSlider = prefab.transform.Find(sliderPrefabName)?.gameObject;
        if (burstSlider != null)
        {
            return burstSlider;
        }

        burstSlider = new(sliderPrefabName);
            
        var burstSliderDot = Object.Instantiate(dotPrefab, burstSlider.transform, true);
        burstSliderDot.transform.localPosition = Vector3.zero;

        var sliderScale = burstSliderDot.transform.localScale;
        burstSliderDot.transform.localScale = sliderScale with { y = sliderScale.y / 4 };
            
        burstSlider.SetActive(false);
        return burstSlider;
    }
}