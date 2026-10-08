using UnityEngine;

namespace CustomNotes.Models;

internal interface ICustomNote
{
    NoteObjects Left { get; }
    NoteObjects Right { get; }
    GameObject Bomb { get; }
    bool HasArrows { get; }
}