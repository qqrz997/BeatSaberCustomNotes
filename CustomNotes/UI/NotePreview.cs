using CustomNotes.Managers;
using CustomNotes.Models;
using UnityEngine;

namespace CustomNotes.UI;

internal class NotePreview
{
    private readonly GameResourcesManager gameResourcesManager;
    
    private static readonly Vector3 PreviewPosition = new(1.55f, 0.7f, 3.25f);
    private static readonly Vector3 PreviewRotation = new(0f, 28f, 0f);
    
    public NotePreview(GameResourcesManager gameResourcesManager)
    {
        this.gameResourcesManager = gameResourcesManager;
        
        var transform = new GameObject("NotePreviewContainer").transform;
        transform.SetPositionAndRotation(PreviewPosition, Quaternion.Euler(PreviewRotation));

        var leftNoteArrow = gameResourcesManager.CreateNormalGameNote();
        var leftNoteDot = gameResourcesManager.CreateNormalGameNote();
        var rightNoteArrow = gameResourcesManager.CreateNormalGameNote();
        var rightNoteDot = gameResourcesManager.CreateNormalGameNote();
        leftNoteDot.ShowDot = true;
        rightNoteDot.ShowDot = true;
        
        var noteBomb = gameResourcesManager.CreateBombNote();
        
        LeftNoteArrow = new(transform, leftNoteArrow, "LeftNoteArrow", new(0f, 0f, 0f));
        LeftNoteDot = new(transform, leftNoteDot, "LeftNoteDot", new(0.0f, 0.6f, 0.0f));
        RightNoteArrow = new(transform, rightNoteArrow, "RightNoteArrow", new(0.6f, 0.0f, 0.0f));
        RightNoteDot = new(transform, rightNoteDot, "RightNoteDot", new(0.6f, 0.6f, 0.0f));
        NoteBomb = new(transform, noteBomb, "NoteBomb", new(1.2f, 0.3f, 0.0f));
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

    public void ShowDefaultNotes()
    {
        LeftNoteArrow.ShowDefaultNote();
        LeftNoteDot.ShowDefaultNote();
        RightNoteArrow.ShowDefaultNote();
        RightNoteDot.ShowDefaultNote();
        NoteBomb.ShowDefaultNote();
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
        private readonly INoteModel noteModel;

        public PreviewNote(Transform parent, INoteModel noteModel, string name, Vector3 localPosition)
        {
            container = new GameObject(name).transform;
            container.SetParent(parent, false);
            container.localPosition = localPosition;
            
            this.noteModel = noteModel;
            noteModel.Transform.SetParent(container, false);
        }
        
        private NoteInstance note;
        
        public void SetNote(NoteInstance note)
        {
            ClearNote();
            this.note = note;
            
            if (note == null)
            {
                return;
            }

            note.transform.SetParent(container, false);
            note.transform.localPosition = Vector3.zero;
        }

        public void ShowDefaultNote()
        {
            if (note) note.gameObject.SetActive(false);
            noteModel.GameObject.SetActive(true);
        }

        public void SetColor(Color color, float multiplier)
        {
            if (note) note.SetColor(color * multiplier);
            noteModel.SetColor(color);
        }
        
        public void SetScale(float scale)
        {
            if (note) note.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * scale;
        }

        public void ClearNote()
        {
            noteModel.GameObject.SetActive(false);
            if (note) Object.Destroy(note.gameObject);
        }
    }
}