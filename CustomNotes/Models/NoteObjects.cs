using UnityEngine;

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