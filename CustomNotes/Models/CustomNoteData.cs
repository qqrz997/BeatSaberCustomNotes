using System;
using UnityEngine;

namespace CustomNotes.Models;

internal class CustomNoteData : IDisposable
{
    private readonly AssetBundle assetBundle;
    
    public CustomNoteData(AssetBundle assetBundle, NoteMetadata metadata, CustomNotePrefab prefab)
    {
        this.assetBundle = assetBundle;
        Metadata = metadata;
        Prefab = prefab;
    }

    public NoteMetadata Metadata { get; }
    public CustomNotePrefab Prefab { get; }

    public static CustomNoteData Error(NoteFileInfo file, string message) => 
        new(null, new(file, new("", "", null), message), null);

    public void Dispose()
    {
        if (assetBundle) assetBundle.Unload(true);
    }
}