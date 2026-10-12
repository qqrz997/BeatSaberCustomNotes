using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    private readonly NoteFileManager noteFileManager;

    private readonly string notesDirectory = Path.Combine(UnityGame.InstallPath, "CustomNotes");
    private readonly Dictionary<string, CustomNoteData> loadedCustomNotes = [];

    private NoteAssetLoader(NoteFileManager noteFileManager)
    {
        this.noteFileManager = noteFileManager;
    }

    private bool isLoaded;
    private NoteFileInfo[] customNoteFiles = [];

    public IEnumerable<CustomNoteData> LoadedCustomNotes => loadedCustomNotes.Values;

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

        Directory.CreateDirectory(notesDirectory);

        customNoteFiles = await noteFileManager.ReloadAllFiles(token, null);
        Plugin.Log.Notice($"{customNoteFiles.Length} external notes found. Preparing to load.");
            
        foreach (var note in LoadCustomNotes(customNoteFiles))
        {
            loadedCustomNotes.Add(note.Metadata.File.Hash, note);
        }
        Plugin.Log.Notice($"{loadedCustomNotes.Count} total custom notes loaded.");
        
        CustomNotesChanged?.Invoke(LoadedCustomNotes);

        isLoaded = true;
    }

    /// <summary>
    /// Clear all loaded CustomNotes
    /// </summary>
    public void Dispose()
    {
        foreach (var customNote in LoadedCustomNotes)
            customNote.Dispose();
        loadedCustomNotes.Clear();
        CustomNotesChanged?.Invoke(LoadedCustomNotes);
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
    
    public bool TryGetCustomNote(string hash, [NotNullWhen(true)] out CustomNoteData note)
    {
        note = null;
        return !string.IsNullOrEmpty(hash) && loadedCustomNotes.TryGetValue(hash, out note);
    }

    private static GameObject LoadNotePrefab(AssetBundle assetBundle) => 
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
        var prefab = LoadNotePrefab(assetBundle);

        var descriptor = prefab.GetComponent<NoteDescriptor>();
        descriptor.Icon ??= Utils.GetDefaultCustomIcon();

        var noteArrowLeft = prefab.transform.Find("NoteLeft").gameObject;
        var noteArrowRight = prefab.transform.Find("NoteRight").gameObject;
        var noteDotLeftTransform = prefab.transform.Find("NoteDotLeft");
        var noteDotRightTransform = prefab.transform.Find("NoteDotRight");
        var noteDotLeft = noteDotLeftTransform != null ? noteDotLeftTransform.gameObject : noteArrowLeft;
        var noteDotRight = noteDotRightTransform != null ? noteDotRightTransform.gameObject : noteArrowRight;
        var noteBomb = prefab.transform.Find("NoteBomb")?.gameObject;

        var chainSegmentLeft = GetSubPrefab(noteDotLeft, "BurstSliderLeft", Vector3.zero, new(1f, 0.225f, 1f));
        var chainSegmentRight = GetSubPrefab(noteDotLeft, "BurstSliderRight", Vector3.zero, new(1f, 0.225f, 1f));

        var chainArrowLeft = GetSubPrefab(noteArrowLeft, "BurstSliderHeadLeft", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        var chainArrowRight = GetSubPrefab(noteArrowRight, "BurstSliderHeadRight", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        
        var chainDotLeft = GetSubPrefab(noteDotLeft, "BurstSliderHeadDotLeft", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        var chainDotRight = GetSubPrefab(noteDotRight, "BurstSliderHeadDotRight", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));

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

        GameObject GetSubPrefab(GameObject fallback, string subPrefabName, Vector3 position, Vector3 scale)
        {
            var subPrefab = prefab.transform.Find(subPrefabName)?.gameObject;
            if (subPrefab != null) return subPrefab;

            subPrefab = new(subPrefabName);
            subPrefab.transform.SetParent(prefab.transform);
            
            var model = Object.Instantiate(fallback, subPrefab.transform, true);
            model.transform.localPosition = position;
            model.transform.localScale = scale;
        
            return subPrefab;
        }
    }
}