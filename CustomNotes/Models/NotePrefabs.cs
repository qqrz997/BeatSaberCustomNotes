using UnityEngine;

namespace CustomNotes.Models;

internal class NotePrefabs
{
    public NotePrefabs(GameObject noteArrowPrefab,
        GameObject noteDotPrefab,
        GameObject chainArrowPrefab,
        GameObject chainDotPrefab, 
        GameObject chainSegmentPrefab,
        bool useBaseArrows,
        float colorMultiplier)
    {
        NoteArrow = new(noteArrowPrefab, useBaseArrows, colorMultiplier);
        NoteDot = new(noteDotPrefab, useBaseArrows, colorMultiplier);
        ChainArrow = new(chainArrowPrefab, useBaseArrows, colorMultiplier);
        ChainDot = new(chainDotPrefab, useBaseArrows, colorMultiplier);
        ChainSegment = new(chainSegmentPrefab, useBaseArrows, colorMultiplier);
    }

    public NotePrefab NoteArrow { get; }
    public NotePrefab NoteDot { get; }
    public NotePrefab ChainArrow { get; }
    public NotePrefab ChainDot { get; }
    public NotePrefab ChainSegment { get; }
}