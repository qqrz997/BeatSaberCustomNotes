using System;
using System.IO;
using System.IO.Compression;
using AssetComponents.Models;
using CustomNotes.Models;
using CustomNotes.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomNotes.Managers;

internal class Note2Loader
{
    private readonly GameResourcesManager gameResourcesManager;

    public Note2Loader(GameResourcesManager gameResourcesManager)
    {
        this.gameResourcesManager = gameResourcesManager;
    }

    public CustomNoteData LoadNote2(NoteFileInfo file)
    {
        try
        {
            return LoadFromBundle(file);
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

    private CustomNoteData LoadFromBundle(NoteFileInfo file)
    {
        Plugin.Log.Debug($"Attempting to load note2 file - {file.FileInfo.Name}");

        using var fileStream = file.FileInfo.OpenRead();
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        var jsonEntry = archive.GetEntry("metadata.json")
            ?? throw new("Unable to find metadata entry");
        using var jsonStream = jsonEntry.Open();
        var note2 = DeserializeStream<Note2Model>(jsonStream)
            ?? throw new("Failed to deserialize metadata");
        var assetMetadata = note2.Assets[AssetPlatform.PC];
        
        var bundleEntry = archive.GetEntry(assetMetadata.FilePath)
            ?? throw new("Unable to find asset bundle");
        using var bundleStream = bundleEntry.Open();
        var assetBundle = LoadBundle(bundleStream);
        
        var prefab = assetBundle.LoadAsset<GameObject>(AssetBundleDefinition.NoteAssetName);
        
        var descriptor = prefab.GetComponent<AssetComponents.Components.Notes.NoteDescriptor>();
        descriptor.icon ??= Utils.GetDefaultCustomIcon();
        
        var left = descriptor.leftNotes;
        var right = descriptor.rightNotes ? descriptor.rightNotes : descriptor.leftNotes;

        var noteArrowLeft = new Note2SubPrefab(left.noteArrow);
        var noteDotLeft = GetSubPrefab(left.noteDot, noteArrowLeft);
        var chainArrowLeft = GetOrCreateSubPrefab(left.chainArrow, new(0f, 0.255f, 0f), new(1f, 0.59f, 1f), noteArrowLeft);
        var chainDotLeft = GetSubPrefab(left.chainDot, chainArrowLeft);
        var chainSegmentLeft = GetOrCreateSubPrefab(left.chainSegment, Vector3.zero, new(1f, 0.225f, 1f), noteDotLeft);
        
        var noteArrowRight = GetSubPrefab(right.noteArrow, noteArrowLeft);
        var noteDotRight = GetSubPrefab(right.noteDot, noteArrowRight);
        var chainArrowRight = GetOrCreateSubPrefab(right.chainArrow, new(0f, 0.255f, 0f), new(1f, 0.59f, 1f), noteArrowRight);
        var chainDotRight = GetSubPrefab(right.chainDot, chainArrowRight);
        var chainSegmentRight = GetOrCreateSubPrefab(right.chainSegment, Vector3.zero, new(1f, 0.225f, 1f), noteDotRight);

        var bomb = GetBomb(descriptor.bomb);
        
        var noteInfo = new NoteInfo(descriptor.noteName, descriptor.authorName, descriptor.icon);
        var metadata = new NoteMetadata(file, noteInfo, string.Empty);
        
        var customNotePrefab = new NotePrefab(
            noteArrowLeft, noteDotLeft, chainArrowLeft, chainDotLeft, chainSegmentLeft,
            noteArrowRight, noteDotRight, chainArrowRight, chainDotRight, chainSegmentRight,
            bomb);
        
        return new(assetBundle, metadata, customNotePrefab);

        static Note2SubPrefab GetSubPrefab(GameObject? prefabObject, Note2SubPrefab fallback) => 
            prefabObject != null ? new(prefabObject) : fallback;
        
        Note2SubPrefab GetOrCreateSubPrefab(GameObject? prefabObject,
            Vector3 position,
            Vector3 scale,
            Note2SubPrefab fallback)
        {
            if (prefabObject != null) return new(prefabObject);
            
            var subPrefab = new GameObject("PlaceholderSubPrefab");
            subPrefab.transform.SetParent(prefab.transform);

            var model = fallback.Instantiate().GameObject;
            model.transform.SetParent(subPrefab.transform);
            model.transform.localPosition = position;
            model.transform.localScale = scale;

            return new(subPrefab);
        }

        Note2SubPrefab GetBomb(GameObject? prefabObject)
        {
            if (prefabObject != null) return new(prefabObject);
            
            var subPrefab = new GameObject("PlaceholderSubPrefab");
            subPrefab.transform.SetParent(prefab.transform);

            var model = gameResourcesManager.CreateBombNote(true);
            model.Transform.SetParent(subPrefab.transform, true);
            model.Transform.localPosition = Vector3.zero;
            model.Transform.localScale = Vector3.one;

            return new(subPrefab);
        }
    }

    private static AssetBundle LoadBundle(Stream stream)
    {
        if (!stream.CanRead || !stream.CanSeek)
            return CopyStreamAndLoadBundle(stream);

        return AssetBundle.LoadFromStream(stream);
        //return await AssetBundleExtensions.LoadFromStreamAsync(stream);
    }
    
    private static AssetBundle CopyStreamAndLoadBundle(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return AssetBundle.LoadFromStream(memoryStream);
        // return await AssetBundleExtensions.LoadFromStreamAsync(memoryStream);
    }
    
    private static T? DeserializeStream<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(streamReader);
        return new JsonSerializer().Deserialize<T>(jsonTextReader);
    }
}