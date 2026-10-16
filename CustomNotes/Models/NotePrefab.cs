namespace CustomNotes.Models;

internal class NotePrefab : INotePrefab
{
    public NotePrefab(INoteSubPrefab leftNoteArrow,
        INoteSubPrefab leftNoteDot,
        INoteSubPrefab leftChainArrow,
        INoteSubPrefab leftChainDot,
        INoteSubPrefab leftChainSegment,
        INoteSubPrefab rightNoteArrow,
        INoteSubPrefab rightNoteDot,
        INoteSubPrefab rightChainArrow,
        INoteSubPrefab rightChainDot,
        INoteSubPrefab rightChainSegment,
        INoteSubPrefab bomb)
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
}