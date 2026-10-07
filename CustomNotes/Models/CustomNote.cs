using System;
using System.IO;
using CustomNotes.Managers;
using CustomNotes.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomNotes.Models;

internal class NoteObjects
{
    public NoteObjects(GameObject noteArrow, 
        GameObject noteDot,
        GameObject chainArrow, 
        GameObject chainDot,
        GameObject chainSegment)
    {
        NoteArrow = noteArrow;
        NoteDot = noteDot;
        ChainArrow = chainArrow;
        ChainDot = chainDot;
        ChainSegment = chainSegment;
    }

    public GameObject NoteArrow { get; }
    public GameObject NoteDot { get; }
    public GameObject ChainArrow { get; }
    public GameObject ChainDot { get; }
    public GameObject ChainSegment { get; }
}

internal class CustomNote
{
    public CustomNote(string fileName,
        AssetBundle assetBundle,
        NoteDescriptor descriptor,
        NoteObjects leftNotes,
        NoteObjects rightNotes,
        GameObject bomb)
    {
        FileName = fileName;
        AssetBundle = assetBundle;
        Descriptor = descriptor;
        NoteLeft = leftNotes.NoteArrow;
        NoteRight = rightNotes.NoteArrow;
        NoteDotLeft = leftNotes.NoteDot;
        NoteDotRight = rightNotes.NoteDot;
        BurstSliderHeadLeft = leftNotes.ChainArrow;
        BurstSliderHeadRight = rightNotes.ChainArrow;
        BurstSliderHeadDotLeft = leftNotes.ChainDot;
        BurstSliderHeadDotRight = rightNotes.ChainDot;
        BurstSliderLeft = leftNotes.ChainSegment;
        BurstSliderRight = rightNotes.ChainSegment;
    }
    
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

    public static CustomNote Default { get; } = new();
    
    public void Destroy()
    {
        if (AssetBundle != null) AssetBundle.Unload(true);
        if (Descriptor != null) Object.Destroy(Descriptor);
    }

    public CustomNote(string fileName, string errorMessage)
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
}