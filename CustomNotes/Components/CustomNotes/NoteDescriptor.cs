using UnityEngine;

// Class has to be in this namespace due to compatibility
// ReSharper disable once CheckNamespace
namespace CustomNotes;

public class NoteDescriptor : MonoBehaviour
{
    // Casing must match
    // ReSharper disable InconsistentNaming
    public string NoteName = "Note";
    public string AuthorName = "Author";
    public string Description = string.Empty;
    public Texture2D Icon;
    public bool DisableBaseNoteArrows;
    public bool UsesNoteColor;
    public float NoteColorStrength = 1.0f;
}