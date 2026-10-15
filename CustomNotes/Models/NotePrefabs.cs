using UnityEngine;

namespace CustomNotes.Models;

internal class NotePrefabs
{
    public static NotePrefabs Note2(GameObject noteArrowPrefab,
        GameObject noteDotPrefab,
        GameObject chainArrowPrefab,
        GameObject chainDotPrefab,
        GameObject chainSegmentPrefab)
    {
        var noteArrow = new Note2Prefab(noteArrowPrefab);
        var noteDot = new Note2Prefab(noteDotPrefab);
        var chainArrow = new Note2Prefab(chainArrowPrefab);
        var chainDot = new Note2Prefab(chainDotPrefab);
        var chainSegment = new Note2Prefab(chainSegmentPrefab);
        return new(noteArrow, noteDot, chainArrow, chainDot, chainSegment);
    }
    
    public static NotePrefabs LegacyNote(GameObject noteArrowPrefab,
        GameObject noteDotPrefab,
        GameObject chainArrowPrefab,
        GameObject chainDotPrefab, 
        GameObject chainSegmentPrefab,
        bool useBaseArrows,
        float colorMultiplier)
    {
        var noteArrow = new LegacyNotePrefab(noteArrowPrefab, useBaseArrows, colorMultiplier);
        var noteDot = new LegacyNotePrefab(noteDotPrefab, useBaseArrows, colorMultiplier);
        var chainArrow = new LegacyNotePrefab(chainArrowPrefab, useBaseArrows, colorMultiplier);
        var chainDot = new LegacyNotePrefab(chainDotPrefab, useBaseArrows, colorMultiplier);
        var chainSegment = new LegacyNotePrefab(chainSegmentPrefab, useBaseArrows, colorMultiplier);
        return new(noteArrow, noteDot, chainArrow, chainDot, chainSegment);
    }

    private NotePrefabs(INotePrefab noteArrow,
        INotePrefab noteDot,
        INotePrefab chainArrow,
        INotePrefab chainDot,
        INotePrefab chainSegment)
    {
        NoteArrow = noteArrow;
        NoteDot = noteDot;
        ChainArrow = chainArrow;
        ChainDot = chainDot;
        ChainSegment = chainSegment;
    }

    public INotePrefab NoteArrow { get; }
    public INotePrefab NoteDot { get; }
    public INotePrefab ChainArrow { get; }
    public INotePrefab ChainDot { get; }
    public INotePrefab ChainSegment { get; }
}