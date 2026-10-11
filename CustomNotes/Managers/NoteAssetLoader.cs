using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomNotes.Models;
using IPA.Utilities;
using SiraUtil.Zenject;
using UnityEngine;
using Object = UnityEngine.Object;
using Utils = CustomNotes.Utilities.Utils;

namespace CustomNotes.Managers;

internal class NoteAssetLoader : IAsyncInitializable, IDisposable
{
    private readonly PluginConfig config;
    private readonly NoteFileManager noteFileManager;

    private NoteAssetLoader(PluginConfig config, NoteFileManager noteFileManager)
    {
        this.config = config;
        this.noteFileManager = noteFileManager;
    }

    private bool isLoaded;
    private NoteFileInfo[] customNoteFiles = [];

    public static string NotesDirectory { get; } = Path.Combine(UnityGame.InstallPath, "CustomNotes");

    public Dictionary<string, CustomNoteData> LoadedCustomNotes { get; private set; } = [];

    public event Action<IEnumerable<CustomNoteData>> CustomNotesChanged; 

    /// <summary>
    /// Load all CustomNotes 
    /// </summary>
    public async Task InitializeAsync(CancellationToken token)
    {
        if (isLoaded)
        {
            return;
        }

        Directory.CreateDirectory(NotesDirectory);

        customNoteFiles = await noteFileManager.ReloadAllFiles(token, null);
        Plugin.Log.Notice($"{customNoteFiles.Length} external notes found. Preparing to load.");
            
        LoadedCustomNotes = LoadCustomNotes(customNoteFiles).ToDictionary(x => x.Metadata.File.Hash);
        Plugin.Log.Notice($"{LoadedCustomNotes.Count} total custom notes loaded.");
        
        CustomNotesChanged?.Invoke(LoadedCustomNotes.Values);

        isLoaded = true;
    }

    /// <summary>
    /// Clear all loaded CustomNotes
    /// </summary>
    public void Dispose()
    {
        foreach (var (_, customNote) in LoadedCustomNotes)
            customNote.Dispose();
        LoadedCustomNotes.Clear();
        CustomNotesChanged?.Invoke(LoadedCustomNotes.Values);
        isLoaded = false;
    }

    /// <summary>
    /// Reload all CustomNotes
    /// </summary>
    internal async Task Reload()
    {
        Plugin.Log.Debug("Reloading the NoteAssetLoader");

        Dispose();
        await InitializeAsync(CancellationToken.None);
    }

    public static GameObject LoadNotePrefab(AssetBundle assetBundle) => 
        assetBundle.LoadAsset<GameObject>("assets/_customnote.prefab");

    private static IEnumerable<CustomNoteData> LoadCustomNotes(IEnumerable<NoteFileInfo> customNoteFiles) => 
        customNoteFiles.Select(LoadCustomNote);
    
    private static CustomNoteData LoadCustomNote(NoteFileInfo file)
    {
        try
        {
            var assetBundle = AssetBundle.LoadFromFile(file.FileInfo.FullName);
            return LoadFromBundle(assetBundle, file);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Problem encountered when loading '{file.FileInfo.Name}'\n{ex}");

            var message = $"File: '{file.FileInfo.Name}'" +
                          "\n\nThis file failed to load." +
                          "\n\nThis may have been caused by having duplicated files, another note with the" +
                          " same name already exists or that the custom note is simply just broken." +
                          "\n\nThe best thing is probably just to delete it!";
            return CustomNoteData.Error(file, message);
        }
    }

    private static CustomNoteData LoadFromBundle(AssetBundle assetBundle, NoteFileInfo file)
    {
        var noteObject = LoadNotePrefab(assetBundle);

        var descriptor = noteObject.GetComponent<NoteDescriptor>();
        descriptor.Icon ??= Utils.GetDefaultCustomIcon();

        var noteArrowLeft = noteObject.transform.Find("NoteLeft").gameObject;
        var noteArrowRight = noteObject.transform.Find("NoteRight").gameObject;
        var noteDotLeftTransform = noteObject.transform.Find("NoteDotLeft");
        var noteDotRightTransform = noteObject.transform.Find("NoteDotRight");
        var noteDotLeft = noteDotLeftTransform != null ? noteDotLeftTransform.gameObject : noteArrowLeft;
        var noteDotRight = noteDotRightTransform != null ? noteDotRightTransform.gameObject : noteArrowRight;
        var noteBomb = noteObject.transform.Find("NoteBomb")?.gameObject;

        var chainSegmentLeft = GetBurstSlider(noteObject, noteDotLeft, "BurstSliderLeft");
        var chainSegmentRight = GetBurstSlider(noteObject, noteDotRight, "BurstSliderRight");

        var burstSliderHeadLeftT = noteObject.transform.Find("BurstSliderHeadLeft");
        var burstSliderHeadRightT = noteObject.transform.Find("BurstSliderHeadRight");
        var chainArrowLeft = burstSliderHeadLeftT != null ? burstSliderHeadLeftT.gameObject : noteArrowLeft;
        var chainArrowRight = burstSliderHeadRightT != null ? burstSliderHeadRightT.gameObject : noteArrowRight;
        
        var burstSliderHeadDotLeftT = noteObject.transform.Find("BurstSliderHeadDotLeft");
        var burstSliderHeadDotRightT = noteObject.transform.Find("BurstSliderHeadDotRight");
        var chainDotLeft = 
            burstSliderHeadDotLeftT != null ? burstSliderHeadDotLeftT.gameObject 
            : chainArrowLeft != null ? chainArrowLeft.gameObject 
            : noteDotLeft;
        var chainDotRight = 
            burstSliderHeadDotRightT != null ? burstSliderHeadDotRightT.gameObject 
            : chainArrowRight != null ? chainArrowRight.gameObject 
            : noteDotRight;

        var noteInfo = new NoteInfo(descriptor.NoteName, descriptor.AuthorName, descriptor.Icon);
        var metadata = new NoteMetadata(file, noteInfo, string.Empty);
        
        var leftPrefabs = new NotePrefabs(
            noteArrowLeft, noteDotLeft, chainArrowLeft, chainDotLeft, chainSegmentLeft, 
            !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        
        var rightPrefabs = new NotePrefabs(
            noteArrowRight, noteDotRight, chainArrowRight, chainDotRight, chainSegmentRight, 
            !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        
        var customNotePrefab = new CustomNotePrefab(
            leftPrefabs, rightPrefabs, noteBomb, descriptor.UsesNoteColor, descriptor.NoteColorStrength);
        
        return new(assetBundle, metadata, customNotePrefab);
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