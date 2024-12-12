using CustomNotes.Utilities;
using System;
using System.IO;
using CustomNotes.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomNotes.Data;

public class CustomNote
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
            return new()
            {
                Descriptor = new NoteDescriptor
                {
                    AuthorName = "Beat Saber",
                    NoteName = "Default",
                    Description = "This is the default notes. (No preview available)",
                    Icon = Utils.GetDefaultIcon()
                }
            };
        }

        try
        {
            string filePath = Path.Combine(NoteAssetLoader.NotesDirectory, fileName);
            var assetBundle = AssetBundle.LoadFromFile(filePath);
            
            return new(assetBundle, fileName);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Something went wrong getting the AssetBundle for '{fileName}'!");
            Plugin.Log.Warn(ex);
        }

        return new()
        {
            FileName = "DefaultNotes",
            Descriptor = new NoteDescriptor
            {
                NoteName = "Invalid Note (Delete it!)",
                AuthorName = fileName,
                Icon = Utils.GetErrorIcon()
            },
            ErrorMessage = $"File: '{fileName}'" +
                           "\n\nThis file failed to load." +
                           "\n\nThis may have been caused by having duplicated files," +
                           " another note with the same name already exists or " +
                           " that the custom note is simply just broken." +
                           "\n\nThe best thing is probably just to delete it!"
        };
    }

    public static CustomNote LoadInternal(byte[] noteData, string name)
    {
        try
        {
            if (noteData == null) throw new ArgumentNullException(nameof(noteData), "noteData is null.");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "note name is null.");
            
            var assetBundle = AssetBundle.LoadFromMemory(noteData);
            return new(assetBundle, name);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Something went wrong getting the AssetBundle from a resource!");
            Plugin.Log.Warn(ex);
        }

        return new()
        {
            FileName = "DefaultNotes",
            Descriptor = new NoteDescriptor
            {
                NoteName = "Internal Error (Report it!)",
                AuthorName = name,
                Icon = Utils.GetErrorIcon()
            },
            ErrorMessage = $@"File: 'internalResource\\{name}'" +
                           "\n\nAn internal asset has failed to load." +
                           "\n\nThis shouldn't have happened and should be reported!" +
                           " Remember to include the log related to this incident."
        };
    }

    private CustomNote() { }
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

        // burst slider stuff
        var burstSliderLeftTransform = noteObject.transform.Find("BurstSliderLeft");
        if (burstSliderLeftTransform == null)
        {
            BurstSliderLeft = new();
            var innerBurstSliderLeft = Object.Instantiate(NoteDotLeft, BurstSliderLeft.transform, true);
            innerBurstSliderLeft.transform.localPosition = Vector3.zero;
            innerBurstSliderLeft.transform.localScale = new Vector3(
                innerBurstSliderLeft.transform.localScale.x,
                innerBurstSliderLeft.transform.localScale.y / 4,
                innerBurstSliderLeft.transform.localScale.z);
            BurstSliderLeft.SetActive(false);
        }
        else
        {
            BurstSliderLeft = burstSliderLeftTransform.gameObject;
        }

        var burstSliderRightTransform = noteObject.transform.Find("BurstSliderRight");
        if (burstSliderRightTransform == null)
        {
            BurstSliderRight = new GameObject();
            var innerBurstSliderRight = Object.Instantiate(NoteDotRight, BurstSliderRight.transform, true);
            innerBurstSliderRight.transform.localPosition = Vector3.zero;
            innerBurstSliderRight.transform.localScale = new Vector3(
                innerBurstSliderRight.transform.localScale.x,
                innerBurstSliderRight.transform.localScale.y / 4,
                innerBurstSliderRight.transform.localScale.z);
            BurstSliderRight.SetActive(false);
        }
        else
        {
            BurstSliderRight = burstSliderRightTransform.gameObject;
        }

        // burst slider head stuff
        var burstSliderHeadLeftTransform = noteObject.transform.Find("BurstSliderHeadLeft");
        var burstSliderHeadRightTransform = noteObject.transform.Find("BurstSliderHeadRight");
        var burstSliderHeadDotLeftTransform = noteObject.transform.Find("BurstSliderHeadDotLeft");
        var burstSliderHeadDotRightTransform = noteObject.transform.Find("BurstSliderHeadDotRight");
        BurstSliderHeadLeft = burstSliderHeadLeftTransform != null ? burstSliderHeadLeftTransform.gameObject : NoteLeft;
        BurstSliderHeadRight = burstSliderHeadRightTransform != null ? burstSliderHeadRightTransform.gameObject : NoteRight;
        BurstSliderHeadDotLeft = burstSliderHeadDotLeftTransform != null ? burstSliderHeadDotLeftTransform.gameObject : burstSliderHeadLeftTransform != null ? burstSliderHeadLeftTransform.gameObject : NoteDotLeft;
        BurstSliderHeadDotRight = burstSliderHeadDotRightTransform != null ? burstSliderHeadDotRightTransform.gameObject : burstSliderHeadRightTransform != null ? burstSliderHeadRightTransform.gameObject : NoteDotRight;
    }

    public void Destroy()
    {
        if (AssetBundle != null)
        {
            AssetBundle.Unload(true);
        }
        else
        {
            Object.Destroy(Descriptor);
        }
    }
}