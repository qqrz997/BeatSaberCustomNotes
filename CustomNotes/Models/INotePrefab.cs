namespace CustomNotes.Models;

internal interface INotePrefab
{
    INoteSubPrefab LeftNoteArrow { get; }
    INoteSubPrefab LeftNoteDot { get; }
    INoteSubPrefab LeftChainArrow { get; }
    INoteSubPrefab LeftChainDot { get; }
    INoteSubPrefab LeftChainSegment { get; }
    
    INoteSubPrefab RightNoteArrow { get; }
    INoteSubPrefab RightNoteDot { get; }
    INoteSubPrefab RightChainArrow { get; }
    INoteSubPrefab RightChainDot { get; }
    INoteSubPrefab RightChainSegment { get; }
    
    INoteSubPrefab Bomb { get; }
}