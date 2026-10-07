using System;
using System.IO;
using CustomNotes.Managers;
using CustomNotes.Utilities;
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

    public static CustomNote Load(string fileName)
    {
        if (fileName == "DefaultNotes")
        {
            return new();
        }

        try
        {
            string filePath = Path.Combine(NoteAssetLoader.NotesDirectory, fileName);
            var assetBundle = AssetBundle.LoadFromFile(filePath);
            
            return new(assetBundle, fileName);
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
            return new(assetBundle, name);
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
    
    private CustomNote(AssetBundle assetBundle, string fileName)
    {
        FileName = fileName;
        AssetBundle = assetBundle;
        
        var noteObject = NoteAssetLoader.LoadNotePrefab(assetBundle, fileName);

        Descriptor = noteObject.GetComponent<NoteDescriptor>();
        Descriptor.Icon ??= Utils.GetDefaultCustomIcon();

        NoteLeft = noteObject.transform.Find("NoteLeft").gameObject;
        NoteRight = noteObject.transform.Find("NoteRight").gameObject;
        var noteDotLeftTransform = noteObject.transform.Find("NoteDotLeft");
        var noteDotRightTransform = noteObject.transform.Find("NoteDotRight");
        NoteDotLeft = noteDotLeftTransform != null ? noteDotLeftTransform.gameObject : NoteLeft;
        NoteDotRight = noteDotRightTransform != null ? noteDotRightTransform.gameObject : NoteRight;
        NoteBomb = noteObject.transform.Find("NoteBomb")?.gameObject;

        BurstSliderLeft = GetBurstSlider(noteObject, NoteDotLeft, "BurstSliderLeft");
        BurstSliderRight = GetBurstSlider(noteObject, NoteDotRight, "BurstSliderRight");

        var burstSliderHeadLeft = noteObject.transform.Find("BurstSliderHeadLeft");
        var burstSliderHeadRight = noteObject.transform.Find("BurstSliderHeadRight");
        BurstSliderHeadLeft = burstSliderHeadLeft != null ? burstSliderHeadLeft.gameObject : NoteLeft;
        BurstSliderHeadRight = burstSliderHeadRight != null ? burstSliderHeadRight.gameObject : NoteRight;
        
        var burstSliderHeadDotLeft = noteObject.transform.Find("BurstSliderHeadDotLeft");
        var burstSliderHeadDotRight = noteObject.transform.Find("BurstSliderHeadDotRight");
        BurstSliderHeadDotLeft = 
            burstSliderHeadDotLeft != null ? burstSliderHeadDotLeft.gameObject 
            : burstSliderHeadLeft != null ? burstSliderHeadLeft.gameObject 
            : NoteDotLeft;
        BurstSliderHeadDotRight = 
            burstSliderHeadDotRight != null ? burstSliderHeadDotRight.gameObject 
            : burstSliderHeadRight != null ? burstSliderHeadRight.gameObject 
            : NoteDotRight;
    }

    private CustomNote()
    {
        FileName = "DefaultNotes";
        Descriptor = new NoteDescriptor
        {
            AuthorName = "Beat Games",
            NoteName = "Default",
            Description = "This is the default notes. (No preview available)",
            Icon = Utils.GetDefaultIcon()
        };
    }

    private CustomNote(string fileName, string errorMessage)
    {
        FileName = fileName;
        Descriptor = new NoteDescriptor
        {
            NoteName = "Error - Check Description",
            AuthorName = string.Empty,
            Icon = Utils.GetErrorIcon()
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
}