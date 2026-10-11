using CustomNotes.Models;
using UnityEngine;

namespace CustomNotes.UI;

internal class NotePreview
{
    private static readonly Vector3 PreviewPosition = new(1.55f, 0.7f, 3.25f);
    private static readonly Vector3 PreviewRotation = new(0f, 28f, 0f);
    
    public NotePreview()
    {
        var gameObject = new GameObject("NotePreviewContainer");
        gameObject.transform.SetPositionAndRotation(PreviewPosition, Quaternion.Euler(PreviewRotation));
        
        LeftNoteArrow = new("LeftNoteArrow", new(0f, 0f, 0f), gameObject.transform);
        LeftNoteDot = new("LeftNoteDot", new(0.0f, 0.6f, 0.0f), gameObject.transform);
        RightNoteArrow = new("RightNoteArrow", new(0.6f, 0.0f, 0.0f), gameObject.transform);
        RightNoteDot = new("RightNoteDot", new(0.6f, 0.6f, 0.0f), gameObject.transform);
        NoteBomb = new("NoteBomb", new(1.2f, 0.3f, 0.0f), gameObject.transform);
    }
    
    public PreviewNote LeftNoteArrow { get; }
    public PreviewNote LeftNoteDot { get; }
    public PreviewNote RightNoteArrow { get; }
    public PreviewNote RightNoteDot { get; }
    public PreviewNote NoteBomb { get; }

    public void SetScale(float scale)
    {
        LeftNoteArrow.SetScale(scale);
        LeftNoteDot.SetScale(scale);
        RightNoteArrow.SetScale(scale);
        RightNoteDot.SetScale(scale);
        NoteBomb.SetScale(scale);
    }

    public void ClearNotes()
    {
        LeftNoteArrow.ClearNote();
        LeftNoteDot.ClearNote();
        RightNoteArrow.ClearNote();
        RightNoteDot.ClearNote();
        NoteBomb.ClearNote();
    }
    
    public class PreviewNote
    {
        private readonly Transform container;

        public PreviewNote(string name, Vector3 localPosition, Transform parent)
        {
            container = new GameObject(name).transform;
            container.SetParent(parent, false);
            container.localPosition = localPosition;
        }
        
        private NoteInstance note;

        public void SetNote(NoteInstance note, float scale)
        {
            ClearNote();
            this.note = note;
            if (!note) return;
            note.transform.SetParent(container, false);
            note.transform.localPosition = Vector3.zero;
            note.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) *  scale;
        }
        
        public void SetScale(float scale)
        {
            if (note) note.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * scale;
        }

        public void ClearNote()
        {
            if (note) Object.Destroy(note.gameObject);
        }
    }
}