using CustomNotes.Managers;
using CustomNotes.Models;
using UnityEngine;

namespace CustomNotes.UI;

internal class NotePreview
{
    private static readonly Vector3 PreviewPosition = new(1.55f, 0.7f, 3.25f);
    private static readonly Vector3 PreviewRotation = new(0f, 42f, 0f);
    
    public NotePreview(GameResourcesManager gameResourcesManager)
    {
        var transform = new GameObject("NotePreviewContainer").transform;
        transform.SetPositionAndRotation(PreviewPosition, Quaternion.Euler(PreviewRotation));

        var leftNoteArrow = gameResourcesManager.CreateNormalGameNote(false);
        var leftNoteDot = gameResourcesManager.CreateNormalGameNote(false);
        var leftChainArrow = gameResourcesManager.CreateBurstSliderHead(false);
        var leftChainDot = gameResourcesManager.CreateBurstSliderHead(false);
        var leftChainSegment = gameResourcesManager.CreateBurstSlider(false);
        leftNoteDot.ShowDot = true;
        leftChainDot.ShowDot = true;
        var rightNoteArrow = gameResourcesManager.CreateNormalGameNote(false);
        var rightNoteDot = gameResourcesManager.CreateNormalGameNote(false);
        var rightChainArrow = gameResourcesManager.CreateBurstSliderHead(false);
        var rightChainDot = gameResourcesManager.CreateBurstSliderHead(false);
        var rightChainSegment = gameResourcesManager.CreateBurstSlider(false);
        rightNoteDot.ShowDot = true;
        rightChainDot.ShowDot = true;
        
        var noteBomb = gameResourcesManager.CreateBombNote(false);
        
        LeftNoteArrow = new(transform, leftNoteArrow, "LeftNoteArrow", new(0f, 0.6f, 0f));
        LeftNoteDot = new(transform, leftNoteDot, "LeftNoteDot", new(0.0f, 0.0f, 0.0f));
        LeftChainArrow = new(transform, leftChainArrow, "LeftChainArrow", new(1.2f, 0.6f, 0f));
        LeftChainDot = new(transform, leftChainDot, "LeftChainDot", new(1.2f, 0.175f, 0f));
        LeftChainSegment = new(transform, leftChainSegment, "LeftChainSegment", new(1.2f, -0.15f, 0f));
        RightNoteArrow = new(transform, rightNoteArrow, "RightNoteArrow", new(0.6f, 0.6f, 0.0f));
        RightNoteDot = new(transform, rightNoteDot, "RightNoteDot", new(0.6f, 0.0f, 0.0f));
        RightChainArrow = new(transform, rightChainArrow, "RightChainArrow", new(1.8f, 0.6f, 0f));
        RightChainDot = new(transform, rightChainDot, "RightChainDot", new(1.8f, 0.175f, 0f));
        RightChainSegment = new(transform, rightChainSegment, "RightChainSegment", new(1.8f, -0.15f, 0f));
        NoteBomb = new(transform, noteBomb, "NoteBomb", new(2.4f, 0.3f, 0.0f));
    }
    
    public PreviewNote LeftNoteArrow { get; }
    public PreviewNote LeftNoteDot { get; }
    public PreviewNote LeftChainArrow { get; }
    public PreviewNote LeftChainDot { get; }
    public PreviewNote LeftChainSegment { get; }
    public PreviewNote RightNoteArrow { get; }
    public PreviewNote RightNoteDot { get; }
    public PreviewNote RightChainArrow { get; }
    public PreviewNote RightChainDot { get; }
    public PreviewNote RightChainSegment { get; }
    public PreviewNote NoteBomb { get; }

    public void SetScale(float scale)
    {
        LeftNoteArrow.SetScale(scale);
        LeftNoteDot.SetScale(scale);
        LeftChainArrow.SetScale(scale);
        LeftChainDot.SetScale(scale);
        LeftChainSegment.SetScale(scale);
        RightNoteArrow.SetScale(scale);
        RightNoteDot.SetScale(scale);
        RightChainArrow.SetScale(scale);
        RightChainDot.SetScale(scale);
        RightChainSegment.SetScale(scale);
        NoteBomb.SetScale(scale);
    }

    public void ClearNotes()
    {
        LeftNoteArrow.ClearNote();
        LeftNoteDot.ClearNote();
        LeftChainArrow.ClearNote();
        LeftChainDot.ClearNote();
        LeftChainSegment.ClearNote();
        RightNoteArrow.ClearNote();
        RightNoteDot.ClearNote();
        RightChainArrow.ClearNote();
        RightChainDot.ClearNote();
        RightChainSegment.ClearNote();
        NoteBomb.ClearNote();
    }
    
    public class PreviewNote
    {
        private readonly Transform container;
        private readonly INoteModel defaultNote;

        public PreviewNote(Transform parent, INoteModel defaultNote, string name, Vector3 localPosition)
        {
            container = new GameObject(name).transform;
            container.SetParent(parent, false);
            container.localPosition = localPosition;
            
            this.defaultNote = defaultNote;
            defaultNote.Transform.SetParent(container, false);
        }
        
        private NoteInstance note;
        
        public void SetNote(NoteInstance note)
        {
            ClearNote();
            this.note = note;
            
            if (note == null)
            {
                defaultNote.GameObject.SetActive(true);
                return;
            }

            note.transform.SetParent(container, false);
            note.transform.localPosition = Vector3.zero;
        }

        public void SetColor(Color color, float multiplier)
        {
            if (note) note.SetColor(color * multiplier);
            defaultNote.SetColor(color);
        }
        
        public void SetScale(float scale)
        {
            if (note) note.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * scale;
            defaultNote.Transform.localScale = Vector3.one * scale;
        }

        public void ClearNote()
        {
            defaultNote.GameObject.SetActive(false);
            if (note) Object.Destroy(note.gameObject);
        }
    }
}