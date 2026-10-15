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

        var noteArrowLeft = left.noteArrow;
        var noteArrowRight = right.noteArrow ? right.noteArrow : noteArrowLeft;

        var noteDotLeft = left.noteDot ? left.noteDot : noteArrowLeft;
        var noteDotRight = right.noteDot ? right.noteDot : noteArrowRight;

        var noteBomb = descriptor.bomb;

        var chainSegmentLeft = left.chainSegment ? left.chainSegment : noteDotLeft;
        var chainSegmentRight = right.chainSegment ? right.chainSegment : noteDotRight;

        var chainArrowLeft = left.chainArrow ? left.chainArrow : noteArrowLeft;
        var chainArrowRight = right.chainArrow ? right.chainArrow : noteArrowRight;

        var chainDotLeft = left.chainDot ? left.chainDot : noteDotLeft;
        var chainDotRight = right.chainDot ? right.chainDot : noteDotRight;
        
        var noteInfo = new NoteInfo(descriptor.noteName, descriptor.authorName, descriptor.icon);
        var metadata = new NoteMetadata(file, noteInfo, string.Empty);
        
        var leftPrefabs = NotePrefabs.Note2(
            noteArrowLeft, noteDotLeft, chainArrowLeft, chainDotLeft, chainSegmentLeft);
        var rightPrefabs = NotePrefabs.Note2(
            noteArrowRight, noteDotRight, chainArrowRight, chainDotRight, chainSegmentRight);
        
        var bombPrefab = new Note2Prefab(noteBomb);
        
        var customNotePrefab = new CustomNotePrefab(leftPrefabs, rightPrefabs, bombPrefab);
        
        return new(assetBundle, metadata, customNotePrefab);
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