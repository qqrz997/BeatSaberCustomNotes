using CustomNotes.Managers;
using CustomNotes.Models;
using UnityEngine;

namespace CustomNotes.UI;

internal class NotePreview
{
    private static readonly Vector3 PreviewPosition = new(1.55f, 0.7f, 3.25f);
    private static readonly Vector3 PreviewRotation = new(0f, 42f, 0f);

    private readonly PreviewNote leftNoteArrow;
    private readonly PreviewNote leftNoteDot;
    private readonly PreviewNote leftChainArrow;
    private readonly PreviewNote leftChainDot;
    private readonly PreviewNote leftChainSegment;
    private readonly PreviewNote rightNoteArrow;
    private readonly PreviewNote rightNoteDot;
    private readonly PreviewNote rightChainArrow;
    private readonly PreviewNote rightChainDot;
    private readonly PreviewNote rightChainSegment;
    private readonly PreviewNote noteBomb;
    
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
        
        this.leftNoteArrow = new(transform, leftNoteArrow, "LeftNoteArrow", new(0f, 0.6f, 0f));
        this.leftNoteDot = new(transform, leftNoteDot, "LeftNoteDot", new(0.0f, 0.0f, 0.0f));
        this.leftChainArrow = new(transform, leftChainArrow, "LeftChainArrow", new(1.2f, 0.6f, 0f));
        this.leftChainDot = new(transform, leftChainDot, "LeftChainDot", new(1.2f, 0.175f, 0f));
        this.leftChainSegment = new(transform, leftChainSegment, "LeftChainSegment", new(1.2f, -0.15f, 0f));
        this.rightNoteArrow = new(transform, rightNoteArrow, "RightNoteArrow", new(0.6f, 0.6f, 0.0f));
        this.rightNoteDot = new(transform, rightNoteDot, "RightNoteDot", new(0.6f, 0.0f, 0.0f));
        this.rightChainArrow = new(transform, rightChainArrow, "RightChainArrow", new(1.8f, 0.6f, 0f));
        this.rightChainDot = new(transform, rightChainDot, "RightChainDot", new(1.8f, 0.175f, 0f));
        this.rightChainSegment = new(transform, rightChainSegment, "RightChainSegment", new(1.8f, -0.15f, 0f));
        this.noteBomb = new(transform, noteBomb, "NoteBomb", new(2.4f, 0.3f, 0.0f));
    }

    public void SetNotes(CustomNotePrefab prefab)
    {
        leftNoteArrow.SetNote(prefab.Left.NoteArrow.Instantiate());
        leftNoteDot.SetNote(prefab.Left.NoteDot.Instantiate());
        leftChainArrow.SetNote(prefab.Left.ChainArrow.Instantiate());
        leftChainDot.SetNote(prefab.Left.ChainDot.Instantiate());
        leftChainSegment.SetNote(prefab.Left.ChainSegment.Instantiate());
        rightNoteArrow.SetNote(prefab.Right.NoteArrow.Instantiate());
        rightNoteDot.SetNote(prefab.Right.NoteDot.Instantiate());
        rightChainArrow.SetNote(prefab.Right.ChainArrow.Instantiate());
        rightChainDot.SetNote(prefab.Right.ChainDot.Instantiate());
        rightChainSegment.SetNote(prefab.Right.ChainSegment.Instantiate());
        noteBomb.SetNote(prefab.Bomb.Instantiate());
    }
    
    public void SetScale(float scale)
    {
        leftNoteArrow.SetScale(scale);
        leftNoteDot.SetScale(scale);
        leftChainArrow.SetScale(scale);
        leftChainDot.SetScale(scale);
        leftChainSegment.SetScale(scale);
        rightNoteArrow.SetScale(scale);
        rightNoteDot.SetScale(scale);
        rightChainArrow.SetScale(scale);
        rightChainDot.SetScale(scale);
        rightChainSegment.SetScale(scale);
        noteBomb.SetScale(scale);
    }

    public void SetColors(Color leftColor, Color rightColor, float multiplier)
    {
        leftNoteArrow.SetColor(leftColor, multiplier);
        leftNoteDot.SetColor(leftColor, multiplier);
        leftChainArrow.SetColor(leftColor, multiplier);
        leftChainDot.SetColor(leftColor, multiplier);
        leftChainSegment.SetColor(leftColor, multiplier);
        rightNoteArrow.SetColor(rightColor, multiplier);
        rightNoteDot.SetColor(rightColor, multiplier);
        rightChainArrow.SetColor(rightColor, multiplier);
        rightChainDot.SetColor(rightColor, multiplier);
        rightChainSegment.SetColor(rightColor, multiplier);
    }

    public void ClearNotes()
    {
        leftNoteArrow.ClearNote();
        leftNoteDot.ClearNote();
        leftChainArrow.ClearNote();
        leftChainDot.ClearNote();
        leftChainSegment.ClearNote();
        rightNoteArrow.ClearNote();
        rightNoteDot.ClearNote();
        rightChainArrow.ClearNote();
        rightChainDot.ClearNote();
        rightChainSegment.ClearNote();
        noteBomb.ClearNote();
    }

    public void ShowDefaultNotes()
    {
        leftNoteArrow.SetNote(null);
        leftNoteDot.SetNote(null);
        leftChainArrow.SetNote(null);
        leftChainDot.SetNote(null);
        leftChainSegment.SetNote(null);
        rightNoteArrow.SetNote(null);
        rightNoteDot.SetNote(null);
        rightChainArrow.SetNote(null);
        rightChainDot.SetNote(null);
        rightChainSegment.SetNote(null);
        noteBomb.SetNote(null);
    }

    private class PreviewNote
    {
        private readonly Transform container;
        private readonly INoteModel defaultNote;
        private readonly DefaultNoteModel? defaultNoteModel;

        public PreviewNote(Transform parent, INoteModel defaultNote, string name, Vector3 localPosition)
        {
            container = new GameObject(name).transform;
            container.SetParent(parent, false);
            container.localPosition = localPosition;
            
            this.defaultNote = defaultNote;
            if (defaultNote is DefaultNoteModel defaultNoteModel) 
                this.defaultNoteModel = defaultNoteModel;
            defaultNote.Transform.SetParent(container, false);
        }
        
        private NoteInstance? note;
        
        public void SetNote(NoteInstance? note)
        {
            ClearNote();
            this.note = note;
            
            if (!note)
            {
                defaultNote.GameObject.SetActive(true);
                defaultNoteModel?.ShowNoteMesh = true;
                return;
            }

            note.transform.SetParent(container, false);
            note.transform.localPosition = Vector3.zero;

            if (note.UseBaseArrows && defaultNoteModel != null)
            {
                defaultNoteModel.ShowNoteMesh = false;
                defaultNoteModel.GameObject.SetActive(true);
            }
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