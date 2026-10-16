using UnityEngine;

namespace CustomNotes.Models;

internal class LegacyNotePrefab : INotePrefab
{
    public LegacyNotePrefab(INoteSubPrefab leftNoteArrow,
        INoteSubPrefab leftNoteDot,
        INoteSubPrefab leftChainArrow,
        INoteSubPrefab leftChainDot,
        INoteSubPrefab leftChainSegment,
        INoteSubPrefab rightNoteArrow,
        INoteSubPrefab rightNoteDot,
        INoteSubPrefab rightChainArrow,
        INoteSubPrefab rightChainDot,
        INoteSubPrefab rightChainSegment,
        INoteSubPrefab bomb,
        bool colorable,
        float colorMultiplier)
    {
        LeftNoteArrow = leftNoteArrow;
        LeftNoteDot = leftNoteDot;
        LeftChainArrow = leftChainArrow;
        LeftChainDot = leftChainDot;
        LeftChainSegment = leftChainSegment;
        RightNoteArrow = rightNoteArrow;
        RightNoteDot = rightNoteDot;
        RightChainArrow = rightChainArrow;
        RightChainDot = rightChainDot;
        RightChainSegment = rightChainSegment;
        Bomb = bomb;
        Colorable = colorable;
        ColorMultiplier = colorMultiplier;
    }
    
    public INoteSubPrefab LeftNoteArrow { get; }
    public INoteSubPrefab LeftNoteDot { get; }
    public INoteSubPrefab LeftChainArrow { get; }
    public INoteSubPrefab LeftChainDot { get; }
    public INoteSubPrefab LeftChainSegment { get; }
    
    public INoteSubPrefab RightNoteArrow { get; }
    public INoteSubPrefab RightNoteDot { get; }
    public INoteSubPrefab RightChainArrow { get; }
    public INoteSubPrefab RightChainDot { get; }
    public INoteSubPrefab RightChainSegment { get; }
    
    public INoteSubPrefab Bomb { get; }
    
    public bool Colorable { get; }
    public float ColorMultiplier { get; }
}