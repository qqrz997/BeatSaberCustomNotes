using CustomNotes.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomNotes.Models;

internal class CustomNote : ICustomNote
{
    private readonly AssetBundle assetBundle;
    
    public CustomNote(string fileName,
        AssetBundle assetBundle,
        NoteDescriptor descriptor,
        NoteObjects leftNotes,
        NoteObjects rightNotes,
        GameObject bomb)
    {
        this.assetBundle = assetBundle;
        FileName = fileName;
        Descriptor = descriptor;
        Left = leftNotes;
        Right = rightNotes;
        Bomb = bomb;
    }
    
    public string FileName { get; }
    public NoteDescriptor Descriptor { get; }
    public NoteObjects Left { get; }
    public NoteObjects Right { get; }
    public GameObject Bomb { get; }

    public bool HasArrows => Descriptor.DisableBaseNoteArrows;

    public string ErrorMessage { get; private set; } = string.Empty;

    public static CustomNote Default { get; } = new();
    
    public void Destroy()
    {
        if (assetBundle != null) assetBundle.Unload(true);
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