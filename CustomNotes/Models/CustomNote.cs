using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using AssetComponents.Components.Notes;
using AssetComponents.Models;
using CustomNotes.Managers;
using CustomNotes.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomNotes.Models;

internal class CustomNote
{
    public string FileName { get; private set; }
    public AssetBundle AssetBundle { get; }
    public NoteDescriptor Descriptor { get; private set; }
    public GameObject NoteLeft { get; }
    public GameObject NoteRight { get; }
    public GameObject NoteDotLeft { get; }
    public GameObject NoteDotRight { get; }
    public GameObject NoteBomb { get; }
    public GameObject BurstSliderLeft { get; }
    public GameObject BurstSliderRight { get; }
    public GameObject BurstSliderHeadLeft { get; }
    public GameObject BurstSliderHeadRight { get; }
    public GameObject BurstSliderHeadDotLeft { get; }
    public GameObject BurstSliderHeadDotRight { get; }

    public string ErrorMessage { get; private set; } = string.Empty;

    public static CustomNote GetDefault()
    {
        return new();
    }
    
    public static CustomNote Load(string fileName)
    {
        try
        {
            string filePath = Path.Combine(NoteAssetLoader.NotesDirectory, fileName);
            var fileInfo = new FileInfo(filePath);

            return LoadBloq2(fileInfo);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Problem encountered when loading '{Path.GetFileNameWithoutExtension(fileName)}'");
            Plugin.Log.Warn(ex);
        }

        return new("DefaultNotes",
            $"File: '{fileName}'" +
            "\n\nThis file failed to load." +
            "\n\nThis may have been caused by having duplicated files, another note with the" +
            " same name already exists or that the custom note is simply just broken." +
            "\n\nThe best thing is probably just to delete it!");
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
        }

        return new("DefaultNotes",
            $@"File: 'internalResource\\{name}'" +
            "\n\nAn internal asset has failed to load." +
            "\n\nThis shouldn't have happened and should be reported!" +
            " Remember to include the log related to this incident.");
    }
    
    public void Destroy()
    {
        if (AssetBundle != null) AssetBundle.Unload(true);
        if (Descriptor != null) Object.Destroy(Descriptor);
    }
    
    private static CustomNote LoadBloq2(FileInfo file)
    {
        Plugin.Log.Debug($"Attempting to load bloq2 file - {file.Name}");

        using var fileStream = file.OpenRead();
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        var jsonEntry = archive.GetEntry("metadata.json");
        using var jsonStream = jsonEntry.Open();
        var bloq2 = DeserializeStream<Bloq2Model>(jsonStream);
        var assetMetadata = bloq2.Assets[AssetPlatform.PC];
        
        var bundleEntry = archive.GetEntry(assetMetadata.FilePath);
        using var bundleStream = bundleEntry.Open();
        var bundle = LoadBundle(bundleStream);

        return LoadFromBundle(bundle, file.Name);
    }

    private static CustomNote LoadFromBundle(AssetBundle bundle, string fileName)
    {
        var notePrefab = bundle.LoadAsset<GameObject>(AssetBundleDefinition.NoteAssetName);
        var descriptor = notePrefab.GetComponent<NoteDescriptor>();

        return new(descriptor, fileName, bundle);
    }

    private CustomNote(NoteDescriptor descriptor, string fileName, AssetBundle assetBundle)
    {
        Descriptor = descriptor;
        Descriptor.icon ??= Utils.GetDefaultCustomIcon();
        
        FileName = fileName;
        AssetBundle = assetBundle;

        var left = Descriptor.leftNotes;
        var right = Descriptor.rightNotes ? Descriptor.rightNotes : Descriptor.leftNotes;

        NoteLeft = left.noteArrow;
        NoteRight = right.noteArrow ? right.noteArrow : NoteLeft;
        
        NoteDotLeft = left.noteDot ? left.noteDot : NoteLeft;
        NoteDotRight = right.noteDot ? right.noteDot : NoteRight;
        
        NoteBomb = Descriptor.bomb;

        BurstSliderLeft = left.chainSegment ? left.chainSegment : NoteDotLeft;
        BurstSliderRight = right.chainSegment ? right.chainSegment : NoteDotRight;

        BurstSliderHeadLeft = left.chainArrow ? left.chainArrow : NoteLeft;
        BurstSliderHeadRight = right.chainArrow ? right.chainArrow : NoteRight;

        BurstSliderHeadDotLeft = left.chainDot ? left.noteDot : NoteDotLeft;
        BurstSliderHeadDotRight = right.chainDot ? right.chainDot : NoteDotRight;
    }
    
    public static T DeserializeStream<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(streamReader);
        return new JsonSerializer().Deserialize<T>(jsonTextReader);
    }

    private CustomNote()
    {
        FileName = "DefaultNotes";
        Descriptor = new NoteDescriptor
        {
            authorName = "Beat Games",
            noteName = "Default",
            icon = Utils.GetDefaultIcon()
        };
    }

    private CustomNote(string fileName, string errorMessage)
    {
        FileName = fileName;
        Descriptor = new NoteDescriptor
        {
            noteName = "Error - Check Description",
            authorName = string.Empty,
            icon = Utils.GetErrorIcon()
        };
        ErrorMessage = errorMessage;
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
    
    public static AssetBundle LoadBundle(Stream stream)
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
}

internal class Bloq2Model
{
    [JsonConstructor]
    public Bloq2Model(
        string iconPath,
        string modelName,
        string authorName,
        Dictionary<AssetPlatform, AssetModel> assets)
    {
        IconPath = iconPath;
        ModelName = modelName;
        AuthorName = authorName;
        Assets = assets;
    }
    
    public string IconPath { get; }
    public string ModelName { get; }
    public string AuthorName { get; }
    public Dictionary<AssetPlatform, AssetModel> Assets { get; }
}

internal enum AssetPlatform
{
    PC
}

internal class AssetModel
{
    [JsonConstructor]
    public AssetModel(
        string filePath)
    {
        FilePath = filePath;
    }
    
    public string FilePath { get; }
}