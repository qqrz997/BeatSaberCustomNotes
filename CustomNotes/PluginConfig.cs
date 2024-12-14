using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomNotes;

internal class PluginConfig
{
    public virtual bool Enabled { get; set; } = true;
    public virtual string LastNote { get; set; }
    public virtual float NoteSize { get; set; } = 1;
    public virtual bool HmdOnly { get; set; }
    public virtual bool AutoDisable { get; set; }
    public virtual bool DisableAprilFools { get; set; }
        
    public float GetNoteSize() => DisableAprilFools || !Plugin.IsAprilFirst ? NoteSize : Random.Range(0.25f, 1.5f);
    public bool NoteSizeEquals(float noteSize) => Mathf.Approximately(GetNoteSize(), noteSize);
        
    public static bool ForceHmdOnly { private get; set; }
    public bool UseHmdOnly() => HmdOnly || ForceHmdOnly;
}